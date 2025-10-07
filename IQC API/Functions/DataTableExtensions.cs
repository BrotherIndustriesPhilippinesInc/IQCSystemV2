using IQC_API.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IQC_API.Functions
{
    public static class DataTableExtensions
    {
        // 🔍 Apply global search across multiple string fields
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
                // Get the actual member access (not the invoke)
                var member = Expression.Convert(
                    Expression.Invoke(field, parameter),
                    typeof(string)
                );

                // Build null check: member != null
                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));

                // Call: member.ToLower().Contains(searchValue)
                var toLower = Expression.Call(member, nameof(string.ToLower), Type.EmptyTypes);
                var contains = Expression.Call(
                    toLower,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                    searchValueExpr
                );

                // Combine null check and contains
                var condition = Expression.AndAlso(notNull, contains);

                predicate = predicate == null ? condition : Expression.OrElse(predicate, condition);
            }

            if (predicate == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return query.Where(lambda);
        }


        // 📊 Apply dynamic ordering based on DataTables request
        public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, DataTableRequest request)
        {
            if (request.Order == null || !request.Order.Any())
                return query;

            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var order in request.Order)
            {
                var columnName = request.Columns[order.Column].Data;
                if (string.IsNullOrEmpty(columnName)) continue;

                // 🔥 Normalize to actual EF property name (PascalCase)
                var property = typeof(T).GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (property == null) continue;
                columnName = property.Name;

                bool ascending = order.Dir == "asc";

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

        // 📄 Apply paging
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, DataTableRequest request)
        {
            return query.Skip(request.Start).Take(request.Length);
        }
    }
}
