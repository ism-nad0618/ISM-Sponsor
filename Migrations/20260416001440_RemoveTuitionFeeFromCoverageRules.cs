using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTuitionFeeFromCoverageRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TuitionFeePHP",
                table: "LoGCoverageRules");

            migrationBuilder.DropColumn(
                name: "TuitionFeeUSD",
                table: "LoGCoverageRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TuitionFeePHP",
                table: "LoGCoverageRules",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TuitionFeeUSD",
                table: "LoGCoverageRules",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
