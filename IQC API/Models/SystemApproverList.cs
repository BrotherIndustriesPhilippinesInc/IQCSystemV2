using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.Models
{
    [Table("Tbl_System_Approver_list")]
    public class SystemApproverList
    {
        [Column("ID")]
        public int Id { get; set; }

        [Column("SYSTEM ID")]
        public long SystemID { get; set; }

        [Column("SYSTEM NAME")]
        public string? SystemName { get; set; }

        [Column("APPROVER NUMBER")]
        public long ApproverNumber { get; set; }

        [Column("FULL NAME")]
        public string? FullName { get; set; }

        [Column("EMAIL ADDRESS")]
        public string? EmailAddress { get; set; }

        [Column("SECTION")]
        public string? Section { get; set; }

        [Column("POSITION")]
        public string? Position { get; set; }

        [Column("ADID")]
        public string? ADID { get; set; }

        [Column("EMPLOYEE NUMBER")]
        public string? EmployeeNumber { get; set; }
    }
}
