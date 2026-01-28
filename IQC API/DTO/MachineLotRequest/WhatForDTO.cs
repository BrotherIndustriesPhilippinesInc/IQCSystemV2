using IQC_API.Models;

namespace IQC_API.DTO.MachineLotRequest
{
    public class WhatForDTO: BaseModel
    {
        public string WhatForCode { get; set; }
        public string WhatForName { get; set; }
        public string WhatForDetails { get; set; }
    }
}
