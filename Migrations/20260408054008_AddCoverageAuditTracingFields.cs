using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverageAuditTracingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "CoverageEvaluationAudits",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ParentPercent",
                table: "CoverageEvaluationAudits",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleSnapshot",
                table: "CoverageEvaluationAudits",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SponsorPercent",
                table: "CoverageEvaluationAudits",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "CoverageEvaluationAudits");

            migrationBuilder.DropColumn(
                name: "ParentPercent",
                table: "CoverageEvaluationAudits");

            migrationBuilder.DropColumn(
                name: "RuleSnapshot",
                table: "CoverageEvaluationAudits");

            migrationBuilder.DropColumn(
                name: "SponsorPercent",
                table: "CoverageEvaluationAudits");
        }
    }
}
