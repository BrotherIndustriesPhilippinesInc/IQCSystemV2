using System.ComponentModel.DataAnnotations;

namespace IQC_API.Models
{
    public class Accounts
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string MesName { get; set; }

        public bool IsAdmin { get; set; } = false;

        public bool IsSuperAdmin { get; set; } = false;

        public string? Theme { get; set; } = "#005CAB";
    }
}