using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.DTO.User
{
    public class UserDto
    {
        [JsonPropertyName("EmpNo")]
        public string EmpNo { get; set; }

        [JsonPropertyName("Full_Name")] // Matches the SQL Column exactly
        public string FullName { get; set; }

        [JsonPropertyName("Section")]
        public string Section { get; set; }

        [JsonPropertyName("Department")]
        public string Department { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("Company")]
        public string Company { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Expr1")]
        public string Expr1 { get; set; }

        [JsonPropertyName("ADID")]
        public string ADID { get; set; }

        [JsonPropertyName("RFID_AMS")]
        public string RFID_AMS { get; set; }

        [JsonPropertyName("Position")]
        public string Position { get; set; }

        [JsonPropertyName("Last_Name")] // Matches the SQL Column exactly
        public string LastName { get; set; }

        [JsonPropertyName("Gender")]
        public string Gender { get; set; }

        [JsonPropertyName("Date_Hired")] // Matches the SQL Column exactly
        public DateTime? DateHired { get; set; }

        [JsonPropertyName("CostCode")]
        public string CostCode { get; set; }


    }
}

