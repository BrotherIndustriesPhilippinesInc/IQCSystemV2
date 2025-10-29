using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class PartsInformationAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "PartsInformation",
                newName: "VisualTT");

            migrationBuilder.RenameColumn(
                name: "PSMarking",
                table: "PartsInformation",
                newName: "SupplierData");

            migrationBuilder.RenameColumn(
                name: "Marking",
                table: "PartsInformation",
                newName: "STS_Normal_N1");

            migrationBuilder.RenameColumn(
                name: "Classification",
                table: "PartsInformation",
                newName: "STS_N1");

            migrationBuilder.AddColumn<string>(
                name: "CommonGroup",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DimensionTT",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FutureInspectionStatus",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InspectionStatus",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Markings",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "N_Z",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PSMark",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QM_Lot_Category",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QualityLevel",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reference_STS_Checking",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SAPMasterRegistration",
                table: "PartsInformation",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommonGroup",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "DimensionTT",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "FutureInspectionStatus",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "InspectionStatus",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "Markings",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "N_Z",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "PSMark",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "QM_Lot_Category",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "QualityLevel",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "Reference_STS_Checking",
                table: "PartsInformation");

            migrationBuilder.DropColumn(
                name: "SAPMasterRegistration",
                table: "PartsInformation");

            migrationBuilder.RenameColumn(
                name: "VisualTT",
                table: "PartsInformation",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "SupplierData",
                table: "PartsInformation",
                newName: "PSMarking");

            migrationBuilder.RenameColumn(
                name: "STS_Normal_N1",
                table: "PartsInformation",
                newName: "Marking");

            migrationBuilder.RenameColumn(
                name: "STS_N1",
                table: "PartsInformation",
                newName: "Classification");
        }
    }
}
