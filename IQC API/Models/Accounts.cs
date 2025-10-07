using System.ComponentModel.DataAnnotations;

namespace IQC_API.Models
{
    public class Accounts
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string MesName { get; set; }
    }
}
