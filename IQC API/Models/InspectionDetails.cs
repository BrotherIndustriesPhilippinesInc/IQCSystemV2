using System.ComponentModel.DataAnnotations;

namespace IQC_API.Models
{
    public class InspectionDetails
    {
        public int Id { get; set; }

        [Required]
        public string CheckLot { get; set; }

        public string DimenstionsMaxSamplingCheckQty { get; set; }

        public string ContinuedEligibility { get; set; }




        public string RelatedCheckLot { get; set; }





        public string StockInCollectDate { get; set; }

        public string PartCode { get; set; }

        public string SamplingCheckQty { get; set; }





        public string FactoryCode { get; set; }

        public string PartName { get; set; }

        public string AllowQty { get; set; }




       
        public string Standard { get; set; }

        public string TotalLotQty { get; set; }

        public string SamplingRejectQty { get; set; }






        public string IQCCheckDate { get; set; }

        public string ClassOne { get; set; }

        public string SamplingCheckDefectiveQty { get; set; }

        public string LotJudge { get; set; }

        public string OccuredEngineer { get; set; }

        public string CheckMonitor { get; set; }

        public string LotNo { get; set; }

        public string ClassTwo { get; set; }

        public string RejectQty { get; set; }

        public string ProcessMethod { get; set; }

        public string CheckUser { get; set; }

        public string ProficienceLevel { get; set; }

        public string FirstSize { get; set; }

        public string SecondSize { get; set; }

        public string Supervisor { get; set; }

        public string ModelNo { get; set; }

        public string DesignNoticeNo { get; set; }

        public string FirstAppearance { get; set; }

        public string SecondAppearance { get; set; }

        public string ActualCheckTime { get; set; }

        public string FourMNumber { get; set; }

        public string Remarks { get; set; }

        public string OutgoingInspectionReport { get; set; }

        public string ThreeCDataConfirm { get; set; }       





        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; }

        public string? Approver { get; set; }

        public string VisualCheckItems { get; set; }

        public string DimensionCheckItems { get; set; }
    }
}
