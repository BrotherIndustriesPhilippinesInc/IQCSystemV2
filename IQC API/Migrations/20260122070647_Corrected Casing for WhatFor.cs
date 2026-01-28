using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedCasingforWhatFor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "whatForName",
                table: "WhatFor",
                newName: "WhatForName");

            migrationBuilder.RenameColumn(
                name: "whatForDetails",
                table: "WhatFor",
                newName: "WhatForDetails");

            migrationBuilder.RenameColumn(
                name: "whatForCode",
                table: "WhatFor",
                newName: "WhatForCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhatForName",
                table: "WhatFor",
                newName: "whatForName");

            migrationBuilder.RenameColumn(
                name: "WhatForDetails",
                table: "WhatFor",
                newName: "whatForDetails");

            migrationBuilder.RenameColumn(
                name: "WhatForCode",
                table: "WhatFor",
                newName: "whatForCode");
        }
    }
}
