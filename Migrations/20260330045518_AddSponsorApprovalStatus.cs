using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Sponsors",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Sponsors");
        }
    }
}
