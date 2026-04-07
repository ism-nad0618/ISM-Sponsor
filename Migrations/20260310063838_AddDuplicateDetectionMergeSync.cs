using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateDetectionMergeSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMerged",
                table: "Sponsors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MergeOperationId",
                table: "Sponsors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MergedIntoSponsorId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MergedOn",
                table: "Sponsors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnlineBillingSystemId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentChargingPortalId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MergeOperations",
                columns: table => new
                {
                    MergeOperationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurvivingSponsorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MergedSponsorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InitiatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitiatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InitiatedByUserDisplay = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MergeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FieldSelections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SurvivorBeforeSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MergedSponsorSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildRecordsReassigned = table.Column<int>(type: "int", nullable: false),
                    UsersReassigned = table.Column<int>(type: "int", nullable: false),
                    LogsReassigned = table.Column<int>(type: "int", nullable: false),
                    RequestsReassigned = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MergeOperations", x => x.MergeOperationId);
                    table.ForeignKey(
                        name: "FK_MergeOperations_AspNetUsers_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MergeOperations_Sponsors_MergedSponsorId",
                        column: x => x.MergedSponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MergeOperations_Sponsors_SurvivingSponsorId",
                        column: x => x.SurvivingSponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                columns: table => new
                {
                    SyncLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayloadVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSucceededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogs", x => x.SyncLogId);
                });

            migrationBuilder.CreateTable(
                name: "SponsorDuplicateCandidates",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimarySponsorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DuplicateSponsorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MatchReasons = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MatchExplanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetectedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetectedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedByUserDisplay = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MergeOperationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorDuplicateCandidates", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_SponsorDuplicateCandidates_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorDuplicateCandidates_MergeOperations_MergeOperationId",
                        column: x => x.MergeOperationId,
                        principalTable: "MergeOperations",
                        principalColumn: "MergeOperationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorDuplicateCandidates_Sponsors_DuplicateSponsorId",
                        column: x => x.DuplicateSponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorDuplicateCandidates_Sponsors_PrimarySponsorId",
                        column: x => x.PrimarySponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MergeOperations_InitiatedByUserId",
                table: "MergeOperations",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MergeOperations_InitiatedOn",
                table: "MergeOperations",
                column: "InitiatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MergeOperations_MergedSponsorId",
                table: "MergeOperations",
                column: "MergedSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_MergeOperations_Status",
                table: "MergeOperations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MergeOperations_SurvivingSponsorId",
                table: "MergeOperations",
                column: "SurvivingSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_DuplicateSponsorId",
                table: "SponsorDuplicateCandidates",
                column: "DuplicateSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_MatchScore",
                table: "SponsorDuplicateCandidates",
                column: "MatchScore");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_MergeOperationId",
                table: "SponsorDuplicateCandidates",
                column: "MergeOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_PrimarySponsorId_DuplicateSponsorId",
                table: "SponsorDuplicateCandidates",
                columns: new[] { "PrimarySponsorId", "DuplicateSponsorId" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_ReviewedByUserId",
                table: "SponsorDuplicateCandidates",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDuplicateCandidates_Status",
                table: "SponsorDuplicateCandidates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_AttemptedAt",
                table: "SyncLogs",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_CorrelationId",
                table: "SyncLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_EntityType_EntityId",
                table: "SyncLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_Status",
                table: "SyncLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_TargetSystem",
                table: "SyncLogs",
                column: "TargetSystem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorDuplicateCandidates");

            migrationBuilder.DropTable(
                name: "SyncLogs");

            migrationBuilder.DropTable(
                name: "MergeOperations");

            migrationBuilder.DropColumn(
                name: "IsMerged",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "MergeOperationId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "MergedIntoSponsorId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "MergedOn",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "OnlineBillingSystemId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "StudentChargingPortalId",
                table: "Sponsors");
        }
    }
}
