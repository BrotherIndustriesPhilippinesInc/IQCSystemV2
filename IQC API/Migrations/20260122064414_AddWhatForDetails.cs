using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatForDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "whatForDetails",
                table: "WhatFor",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whatForDetails",
                table: "WhatFor");
        }
    }
}
