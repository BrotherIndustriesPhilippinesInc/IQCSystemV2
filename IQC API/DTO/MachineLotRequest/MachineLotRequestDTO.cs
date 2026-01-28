using IQC_API.Models;

namespace IQC_API.DTO.MachineLotRequest
{
    public class MachineLotRequestDTO: BaseModel
    {
        public int WhatForId { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public string ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        public int ReleaseReasonId { get; set; }

        public string Remarks { get; set; }
    }
}
