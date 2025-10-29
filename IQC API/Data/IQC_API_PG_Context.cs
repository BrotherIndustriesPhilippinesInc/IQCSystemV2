using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IQC_API.Models;

namespace IQC_API.Data
{
    public class IQC_API_PG_Context : DbContext
    {
        public IQC_API_PG_Context(DbContextOptions<IQC_API_PG_Context> options)
            : base(options)
        {
        }

        public DbSet<IQC_API.Models.InspectionDetails> InspectionDetails { get; set; } = default!;

        public DbSet<IQC_API.Models.Accounts> Accounts { get; set; } = default!;
        public DbSet<IQC_API.Models.PartsInformation> PartsInformation { get; set; } = default!;
    }
}
