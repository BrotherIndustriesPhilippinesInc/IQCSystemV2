namespace IQC_API.DTO
{
    using System.Text.Json.Serialization;

    public class DataTableRequest
    {
        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("search")]
        public DataTableSearch Search { get; set; }

        [JsonPropertyName("order")]
        public List<DataTableOrder> Order { get; set; }

        [JsonPropertyName("columns")]
        public List<DataTableColumn> Columns { get; set; }
    }


    public class DataTableSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
        public List<object> Fixed { get; set; }  // ✅ changed to List<object> to handle []
    }

    public class DataTableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; } // "asc" or "desc"
        public string Name { get; set; }
    }

    public class DataTableColumn
    {
        public string? Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTableSearch Search { get; set; }
    }

    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; }
    }


}
