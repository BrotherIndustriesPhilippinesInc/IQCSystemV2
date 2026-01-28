using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseReason",
                table: "MachineLotRequest");

            migrationBuilder.AddColumn<int>(
                name: "ReleaseReasonId",
                table: "MachineLotRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReleaseReason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReleaseReasonCode = table.Column<string>(type: "text", nullable: false),
                    ReleaseReasonName = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseReason", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineLotRequest_ReleaseReasonId",
                table: "MachineLotRequest",
                column: "ReleaseReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineLotRequest_ReleaseReason_ReleaseReasonId",
                table: "MachineLotRequest",
                column: "ReleaseReasonId",
                principalTable: "ReleaseReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineLotRequest_ReleaseReason_ReleaseReasonId",
                table: "MachineLotRequest");

            migrationBuilder.DropTable(
                name: "ReleaseReason");

            migrationBuilder.DropIndex(
                name: "IX_MachineLotRequest_ReleaseReasonId",
                table: "MachineLotRequest");

            migrationBuilder.DropColumn(
                name: "ReleaseReasonId",
                table: "MachineLotRequest");

            migrationBuilder.AddColumn<string>(
                name: "ReleaseReason",
                table: "MachineLotRequest",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
