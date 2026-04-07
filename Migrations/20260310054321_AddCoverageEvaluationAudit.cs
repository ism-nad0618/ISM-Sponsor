using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverageEvaluationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoverageEvaluationAudits",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EvaluatedByUserDisplay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EvaluatedByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolYearId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SponsorId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LogId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChargeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillTo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SponsorAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ParentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MatchedRuleId = table.Column<int>(type: "int", nullable: true),
                    RuleVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverageEvaluationAudits", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_CoverageEvaluationAudits_AspNetUsers_EvaluatedByUserId",
                        column: x => x.EvaluatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoverageEvaluationAudits_LoGCoverageRules_MatchedRuleId",
                        column: x => x.MatchedRuleId,
                        principalTable: "LoGCoverageRules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoverageEvaluationAudits_LogCoverages_LogId",
                        column: x => x.LogId,
                        principalTable: "LogCoverages",
                        principalColumn: "LogId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoverageEvaluationAudits_EvaluatedByUserId",
                table: "CoverageEvaluationAudits",
                column: "EvaluatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoverageEvaluationAudits_LogId",
                table: "CoverageEvaluationAudits",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_CoverageEvaluationAudits_MatchedRuleId",
                table: "CoverageEvaluationAudits",
                column: "MatchedRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoverageEvaluationAudits");
        }
    }
}
