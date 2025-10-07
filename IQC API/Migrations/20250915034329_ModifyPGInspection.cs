using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPGInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InspectionTime",
                table: "InspectionDetails");

            migrationBuilder.RenameColumn(
                name: "CheckItem",
                table: "InspectionDetails",
                newName: "TotalLotQty");

            migrationBuilder.AddColumn<string>(
                name: "ActualCheckTime",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AllowQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckLot",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckMonitor",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckUser",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassOne",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassTwo",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContinuedEligibility",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DesignNoticeNo",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DimenstionsMaxSamplingCheckQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FactoryCode",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstAppearance",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstSize",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FourMNumber",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IQCCheckDate",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "InspectionDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LotJudge",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelNo",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OccuredEngineer",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OutgoingInspectionReport",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartCode",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartName",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessMethod",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProficienceLevel",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedCheckLot",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SamplingCheckDefectiveQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SamplingCheckQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SamplingRejectQty",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondAppearance",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondSize",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Standard",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockInCollectDate",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Supervisor",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThreeCDataConfirm",
                table: "InspectionDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualCheckTime",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "AllowQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "CheckLot",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "CheckMonitor",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "CheckUser",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ClassOne",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ClassTwo",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ContinuedEligibility",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "DesignNoticeNo",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "DimenstionsMaxSamplingCheckQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "FactoryCode",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "FirstAppearance",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "FirstSize",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "FourMNumber",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "IQCCheckDate",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "LotJudge",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ModelNo",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "OccuredEngineer",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "OutgoingInspectionReport",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "PartCode",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "PartName",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ProcessMethod",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ProficienceLevel",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "RejectQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "RelatedCheckLot",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "SamplingCheckDefectiveQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "SamplingCheckQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "SamplingRejectQty",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "SecondAppearance",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "SecondSize",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "Standard",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "StockInCollectDate",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "Supervisor",
                table: "InspectionDetails");

            migrationBuilder.DropColumn(
                name: "ThreeCDataConfirm",
                table: "InspectionDetails");

            migrationBuilder.RenameColumn(
                name: "TotalLotQty",
                table: "InspectionDetails",
                newName: "CheckItem");

            migrationBuilder.AddColumn<int>(
                name: "InspectionTime",
                table: "InspectionDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
