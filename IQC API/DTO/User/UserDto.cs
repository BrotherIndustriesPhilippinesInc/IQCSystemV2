using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.DTO.User
{
    public class UserDto
    {
        public string EmpNo { get; set; }
        public string FullName { get; set; }
        public string Section { get; set; } // This will hold the TRANSLATED value
        public string Department { get; set; } // This will hold the TRANSLATED value
        public string Status { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Expr1 { get; set; }
        public string ADID { get; set; }
        public string RFID_AMS { get; set; }
        public string Position { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateHired { get; set; }
        public string CostCode { get; set; }


    }
}

