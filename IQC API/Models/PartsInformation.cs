using System.Collections.Generic;

namespace IQC_API.Models
{
    public class PartsInformation
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public string? StandardTaktTime { get; set; }
        public string? N1N2 { get; set; }
        public string? VendorCode { get; set; }
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public string? Plant { get; set; }
        public string? SupplierName { get; set; }
        public string? OverseasManufacturer { get; set; }
        public string? Model { get; set; }
        public string? ModelCategory { get; set; }
        public string? QMLotCategory { get; set; }
        public string? SAPMasterRegistration { get; set; }
        public string? QualityLevel { get; set; }
        public string? STS_N1 { get; set; }
        public string? InspectionStatus { get; set; }
        public string? FutureInspectionStatus { get; set; }
        public string? SFCategory { get; set; }
        public string? PSMark{ get; set; }
        public string? Markings { get; set; }
        public string? CriticalComponentSafety { get; set; }
        public string? SupplierData { get; set; }
        public string? CommonGroup { get; set; }
        public string? N_Z { get; set; }
        public string? EOL { get; set; }
        public string? EOLDate { get; set; }
        public string? Remarks { get; set; }
        public string? Reference_STS_Checking { get; set; }
        public string? STS_Normal_N1 { get; set; }
        public string? QM_Lot_Category { get; set; }
        public string? JIT { get; set; }
        public string? Sloc { get; set; }
        public string? Size { get; set; }
        public string? VisualTT { get; set; }
        public string? DimensionTT { get; set; }
        public string? VisualDimension { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? UpdatedBy { get; set; }

    }
}
