using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IQC_API.Models;

namespace IQC_API.Data
{
    public class IQC_APIContext : DbContext
    {
        public IQC_APIContext (DbContextOptions<IQC_APIContext> options)
            : base(options)
        {
        }

        public DbSet<IQC_API.Models.PortalAccount> PortalAccount { get; set; } = default!;
        public DbSet<IQC_API.Models.SystemApproverList> SystemApproverList { get; set; } = default!;
    }
}
