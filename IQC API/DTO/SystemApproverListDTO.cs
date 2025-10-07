using System.ComponentModel.DataAnnotations;

namespace IQC_API.DTO
{
    public class SystemApproverListDTOPost
    {
        public required string EmployeeNumber { get; set; }

        public required string MesName { get; set; }

    }
}
