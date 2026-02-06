using IQC_API.Models;

namespace IQC_API.DTO.MachineLotRequest
{
    public class MachineLotRequestDTO: BaseModel
    {
        public string CheckLot { get; set; }
        public int WhatForId { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        //public string ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        public int ReleaseReasonId { get; set; }

        public string Remarks { get; set; }

        public string? ReleaseNo { get; set; }
    }

    public class MachineLotRequestUpdateDTO : BaseModel
    {
        public string? ReleaseNo { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        //public string ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        public string ReleaseReasonId { get; set; }

        public string Remarks { get; set; }
    }

    public class MachineLotRequestGetDTO : BaseModel
    {
        public int Id { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public string? ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        public string Remarks { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        // Flattened or nested DTOs
        public string WhatForName { get; set; }
        public string WhatForCode { get; set; }

        public string ReleaseReasonName { get; set; }
        public string ReleaseReasonCode { get; set; }

        public string CheckLot { get; set; }

        public string CreatedByFullName { get; set; }
        public List<SystemApproverList>? ApproverList { get; set; }
    }
}
