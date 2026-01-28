using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineLotRequestsAndWhatFor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatFor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    whatForCode = table.Column<string>(type: "text", nullable: false),
                    whatForName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatFor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineLotRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WhatForId = table.Column<int>(type: "integer", nullable: false),
                    PartCode = table.Column<string>(type: "text", nullable: false),
                    PartName = table.Column<string>(type: "text", nullable: false),
                    VendorName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ReleaseReason = table.Column<string>(type: "text", nullable: false),
                    ReleaseNo = table.Column<string>(type: "text", nullable: false),
                    YellowCard = table.Column<string>(type: "text", nullable: false),
                    DCIOtherNo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineLotRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineLotRequest_WhatFor_WhatForId",
                        column: x => x.WhatForId,
                        principalTable: "WhatFor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineLotRequest_WhatForId",
                table: "MachineLotRequest",
                column: "WhatForId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineLotRequest");

            migrationBuilder.DropTable(
                name: "WhatFor");
        }
    }
}
