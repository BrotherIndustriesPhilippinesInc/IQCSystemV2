using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddExportsMachineLotRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpoortedBy",
                table: "MachineLotRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportDate",
                table: "MachineLotRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotNumber",
                table: "MachineLotRequest",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpoortedBy",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "ExportDate",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "LotNumber",
                table: "MachineLotRequest");
        }
    }
}
