using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.Models
{
    [Table("tbl_EMSVIEW")]
    public class EmsView
    {
        // Add all columns that the View returns here
        [Column("EmpNo")]
        public string EmpNo { get; set; }

        [Column("Full_Name")]
        public string FullName { get; set; }

        // Example: Add other columns likely present in your view
        [Column("Section")]
        public string Section { get; set; }
        
        [Column("Department")]
        public string Department { get; set; }
        [Column("Status")]
        public string Status { get; set; }
        [Column("Company")]
        public string Company { get; set; }
        [Column("Email")]
        public string Email { get; set; }
        [Column("Expr1")]
        public string Expr1 { get; set; }
        [Column("ADID")]
        public string ADID { get; set; }
        [Column("RFID_AMS")]
        public string RFID_AMS { get; set; }
        [Column("Position")]
        public string Position { get; set; }
        [Column("Last_Name")]
        public string LastName { get; set; }
        [Column("Gender")]
        public string Gender { get; set; }
        [Column("Date_Hired")]
        public DateTime? DateHired { get; set; }
        [Column("CostCode")]
        public string CostCode { get; set; }

    }
}
