using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class DivideCheckItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CheckItems",
                table: "InspectionDetails",
                newName: "VisualCheckItems");

            migrationBuilder.AddColumn<string>(
                name: "DimensionCheckItems",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensionCheckItems",
                table: "InspectionDetails");

            migrationBuilder.RenameColumn(
                name: "VisualCheckItems",
                table: "InspectionDetails",
                newName: "CheckItems");
        }
    }
}
