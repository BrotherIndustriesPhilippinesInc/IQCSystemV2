using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPartsinformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartsInformation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Classification = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    StandardTaktTime = table.Column<string>(type: "text", nullable: false),
                    N1N2 = table.Column<string>(type: "text", nullable: false),
                    VendorCode = table.Column<string>(type: "text", nullable: false),
                    PartCode = table.Column<string>(type: "text", nullable: false),
                    PartName = table.Column<string>(type: "text", nullable: false),
                    Plant = table.Column<string>(type: "text", nullable: false),
                    SupplierName = table.Column<string>(type: "text", nullable: false),
                    OverseasManufacturer = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    ModelCategory = table.Column<string>(type: "text", nullable: false),
                    QMLotCategory = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SFCategory = table.Column<string>(type: "text", nullable: false),
                    PSMarking = table.Column<string>(type: "text", nullable: false),
                    Marking = table.Column<string>(type: "text", nullable: false),
                    CriticalComponentSafety = table.Column<string>(type: "text", nullable: false),
                    EOL = table.Column<string>(type: "text", nullable: false),
                    EOLDate = table.Column<string>(type: "text", nullable: false),
                    JIT = table.Column<string>(type: "text", nullable: false),
                    Sloc = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    VisualDimension = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartsInformation", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartsInformation");
        }
    }
}
