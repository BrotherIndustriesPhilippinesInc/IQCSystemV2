namespace IQC_API.Models
{
    public class MachineLotRequest: BaseModel
    {
        public int Id { get; set; }

        public string CheckLot { get; set; }

        public int WhatForId { get; set; }
        public WhatFor WhatFor { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public string? ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }

        public int ReleaseReasonId { get; set; }
        public ReleaseReason ReleaseReason { get; set; }

        public string Remarks { get; set; }

        public string? ExportedBy { get; set; }
        public DateTime? ExportDate { get; set; }
        public string LotNumber { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }

    public class WhatFor: BaseModel
    {
        public int Id { get; set; }
        public string WhatForCode { get; set; }
        public string WhatForName { get; set; }

        public string WhatForDetails { get; set; }

        public ICollection<MachineLotRequest> MachineLotRequests { get; set; }
    }

    public class ReleaseReason: BaseModel
    {
        public int Id { get; set; }
        public string ReleaseReasonCode { get; set; }
        public string ReleaseReasonName { get; set; }

        public ICollection<MachineLotRequest> MachineLotRequests { get; set; }
    }
}
