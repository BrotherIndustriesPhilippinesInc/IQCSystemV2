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

        public DbSet<EmsView> EmsViews { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // KEY CONCEPT: Configuring a Keyless Entity (View)
            modelBuilder.Entity<EmsView>(entity =>
            {
                // Tells EF "Don't look for a Primary Key, just read the data"
                entity.HasNoKey();

                // Explicitly mapping to the View name
                entity.ToView("tbl_EMSVIEW");

                // If you need to map column names that are different in C# vs SQL
                // entity.Property(e => e.EmpNo).HasColumnName("EmpNo");
            });
        }
    }
}
