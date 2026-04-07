using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SponsorChangeRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestField = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmittedByUserDisplay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedByUserDisplay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppliedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AppliedByUserDisplay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AppliedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppliedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorChangeRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_SponsorChangeRequests_AspNetUsers_AppliedByUserId",
                        column: x => x.AppliedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorChangeRequests_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorChangeRequests_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SponsorChangeRequests_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorChangeRequests_AppliedByUserId",
                table: "SponsorChangeRequests",
                column: "AppliedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorChangeRequests_ReviewedByUserId",
                table: "SponsorChangeRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorChangeRequests_SponsorId_Status",
                table: "SponsorChangeRequests",
                columns: new[] { "SponsorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorChangeRequests_SubmittedByUserId",
                table: "SponsorChangeRequests",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorChangeRequests_SubmittedOn",
                table: "SponsorChangeRequests",
                column: "SubmittedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorChangeRequests");
        }
    }
}
