using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFontColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FontColor",
                table: "Accounts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontColor",
                table: "Accounts");
        }
    }
}
