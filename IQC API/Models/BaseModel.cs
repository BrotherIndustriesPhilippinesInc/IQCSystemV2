using System.ComponentModel.DataAnnotations.Schema;

namespace IQC_API.Models
{
    public class BaseModel
    {
        public string CreatedBy { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }
    }
}
