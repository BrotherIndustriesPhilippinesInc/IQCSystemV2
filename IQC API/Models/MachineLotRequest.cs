namespace IQC_API.Models
{
    public class MachineLotRequest
    {
        public int Id { get; set; }
        
        public int WhatForId { get; set; }
        public WhatFor WhatFor { get; set; }

        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public string ReleaseReason { get; set; }
        public string ReleaseNo { get; set; }
        public string YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        
    }

    public class WhatFor
    {
        public int Id { get; set; }
        public string whatForCode { get; set; }
        public string whatForName { get; set; }

        public ICollection<MachineLotRequest> MachineLotRequests { get; set; }
    }
}
