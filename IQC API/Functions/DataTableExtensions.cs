using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace IQC_API.Functions
{
    #region DTOs
    public class DataTableRequest
    {
        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("search")]
        public DataTableSearch Search { get; set; } = new();

        [JsonPropertyName("order")]
        public List<DataTableOrder> Order { get; set; } = new();

        [JsonPropertyName("columns")]
        public List<DataTableColumn> Columns { get; set; } = new();
    }

    public class DataTableSearch
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
        public List<object>? Fixed { get; set; }
    }

    public class DataTableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; } = "asc";
        public string? Name { get; set; }
    }

    public class DataTableColumn
    {
        public string? Data { get; set; }
        public string? Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTableSearch? Search { get; set; }
    }

    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; } = new();
    }
    #endregion

    #region Extension Methods
    public static class DataTableExtensions
    {
        // 🔍 Global search across multiple fields
        public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query,
            DataTableRequest request,
            Expression<Func<T, object>>[] searchableFields)
        {
            if (string.IsNullOrWhiteSpace(request.Search?.Value))
                return query;

            var searchValue = request.Search.Value.Trim().ToLower();
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? predicate = null;
            var searchValueExpr = Expression.Constant(searchValue);

            foreach (var field in searchableFields)
            {
                var member = Expression.Convert(
                    Expression.Invoke(field, parameter),
                    typeof(string)
                );

                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                var toLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
                var contains = Expression.Call(
                    toLower,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                    searchValueExpr
                );

                var condition = Expression.AndAlso(notNull, contains);
                predicate = predicate == null ? condition : Expression.OrElse(predicate, condition);
            }

            if (predicate == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return query.Where(lambda);
        }

        // 📊 Dynamic ordering
        public static IQueryable<T> ApplyOrdering<T>(
            this IQueryable<T> query,
            DataTableRequest request)
        {
            if (request.Order == null || request.Order.Count == 0)
                return query;

            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var order in request.Order)
            {
                var columnName = request.Columns.ElementAtOrDefault(order.Column)?.Data;
                if (string.IsNullOrEmpty(columnName)) continue;

                var property = typeof(T).GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                if (property == null) continue;

                bool ascending = order.Dir.Equals("asc", StringComparison.OrdinalIgnoreCase);
                columnName = property.Name;

                if (orderedQuery == null)
                {
                    orderedQuery = ascending
                        ? query.OrderBy(e => EF.Property<object>(e, columnName))
                        : query.OrderByDescending(e => EF.Property<object>(e, columnName));
                }
                else
                {
                    orderedQuery = ascending
                        ? orderedQuery.ThenBy(e => EF.Property<object>(e, columnName))
                        : orderedQuery.ThenByDescending(e => EF.Property<object>(e, columnName));
                }
            }

            return orderedQuery ?? query;
        }

        // 📄 Pagination
        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> query,
            DataTableRequest request)
        {
            return query.Skip(request.Start).Take(request.Length);
        }
    }
    #endregion

    #region DataTable Helper
    public static class DataTableHelper
    {
        /// <summary>
        /// Builds a standardized DataTables-compatible response asynchronously.
        /// </summary>
        public static async Task<DataTableResponse<T>> BuildResponseAsync<T>(
            IQueryable<T> baseQuery,
            DataTableRequest request,
            Expression<Func<T, object>>[] searchableColumns)
            where T : class
        {
            var filteredQuery = baseQuery
                .ApplySearch(request, searchableColumns)
                .ApplyOrdering(request);

            var totalRecords = await baseQuery.CountAsync();
            var filteredRecords = await filteredQuery.CountAsync();
            var pagedData = await filteredQuery.ApplyPaging(request).ToListAsync();

            return new DataTableResponse<T>
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = filteredRecords,
                Data = pagedData
            };
        }

        /// <summary>
        /// Auto-detects searchable columns based on DataTableRequest.Columns.
        /// </summary>
        public static async Task<DataTableResponse<T>> BuildResponseAutoAsync<T>(
        IQueryable<T> baseQuery,
        DataTableRequest request)
        where T : class
        {
            var searchableColumns = request.Columns
                .Where(c => c.Searchable && !string.IsNullOrEmpty(c.Data))
                .Select(c =>
                {
                    var param = Expression.Parameter(typeof(T), "x");

                    // Lookup property by ignoring case
                    var propertyInfo = typeof(T).GetProperty(c.Data!,
                        System.Reflection.BindingFlags.IgnoreCase |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance);

                    if (propertyInfo == null)
                        throw new ArgumentException($"'{c.Data}' is not a member of type '{typeof(T).Name}'");

                    var property = Expression.Property(param, propertyInfo);
                    return Expression.Lambda<Func<T, object>>(
                        Expression.Convert(property, typeof(object)), param);
                })
                .ToArray();

            return await BuildResponseAsync(baseQuery, request, searchableColumns);
        }

    }
    #endregion
}
