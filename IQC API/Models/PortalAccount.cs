using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.Models
{
    [Table("Tbl_Accounts")]
    public class PortalAccount
    {
        public int Id { get; set; }
        [Column("EMPLOYEE NUMBER")]
        public string? EmployeeNumber { get; set; }

        [Column("EMPLOYEE NAME")]
        public string? EmployeeName { get; set; }

        public string? ADID { get; set; }

        [Column("EMAIL ADDRESS")]
        public string? EmailAddress { get; set; }

        [Column("DEPARTMENT")]
        public string? Department { get; set; }

        [Column("SECTION")]
        public string? Section { get; set; }

        [Column("POSITION")]
        public string? Position { get; set; }

        [Column("PASSWORD")]
        public string? Password { get; set; }

        [Column("ISADMIN")]
        public string? IsAdmin { get; set; }

        [Column("ISAPPROVER")]
        public string? IsApprover { get; set; }

        [Column("BPS_GROUP")]
        public string? BPSGroup { get; set; }

        [Column("STATUS")]
        public string? Status { get; set; }

        [Column("VERIFIED")]
        public bool? Verified { get; set; }

        [Column("AFFILIATES")]
        public string? Affiliates { get; set; }


    }
}
