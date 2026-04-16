using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddTuitionFeesPhpUsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TuitionFeePHP",
                table: "Items",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TuitionFeeUSD",
                table: "Items",
                type: "decimal(18,2)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TuitionFeePHP",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TuitionFeeUSD",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TuitionFeePHP",
                table: "LoGCoverageRules");

            migrationBuilder.DropColumn(
                name: "TuitionFeeUSD",
                table: "LoGCoverageRules");
        }
    }
}
