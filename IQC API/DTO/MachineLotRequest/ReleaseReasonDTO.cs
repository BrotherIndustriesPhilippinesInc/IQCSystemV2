using IQC_API.Models;

namespace IQC_API.DTO.MachineLotRequest
{
    public class ReleaseReasonDTO:BaseModel
    {
        public string ReleaseReasonCode { get; set; }
        public string ReleaseReasonName { get; set; }
    }
}
