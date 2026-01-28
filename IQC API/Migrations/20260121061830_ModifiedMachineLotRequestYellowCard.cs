using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQC_API.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedMachineLotRequestYellowCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""MachineLotRequest"" 
                  ALTER COLUMN ""YellowCard"" TYPE boolean 
                  USING (CASE 
                            WHEN ""YellowCard"" ILIKE 'true' OR ""YellowCard"" = '1' THEN true 
                            ELSE false 
                         END);"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""MachineLotRequest"" 
                  ALTER COLUMN ""YellowCard"" TYPE text 
                  USING ""YellowCard""::text;"
            );
        }
    }
}
