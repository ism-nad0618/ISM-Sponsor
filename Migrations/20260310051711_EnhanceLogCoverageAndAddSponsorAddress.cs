using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceLogCoverageAndAddSponsorAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_Sponsors_SponsorId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_Students_SchoolYearId_StudentId",
                table: "LogCoverages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogCoverages",
                table: "LogCoverages");

            migrationBuilder.AlterColumn<string>(
                name: "Tin",
                table: "Sponsors",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SponsorName",
                table: "Sponsors",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Sponsors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystemId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sponsors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "Sponsors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetSuiteId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowerSchoolId",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogId",
                table: "LogCoverages",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ActivatedByUserId",
                table: "LogCoverages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedOn",
                table: "LogCoverages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "LogCoverages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "LogCoverages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeactivatedByUserId",
                table: "LogCoverages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOn",
                table: "LogCoverages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeactivationReason",
                table: "LogCoverages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "LogCoverages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "LogCoverages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                table: "LogCoverages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "LogCoverages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "LogCoverages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewComments",
                table: "LogCoverages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogCoverages",
                table: "LogCoverages",
                column: "LogId");

            migrationBuilder.CreateTable(
                name: "SponsorAddresses",
                columns: table => new
                {
                    SponsorAddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateProvince = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorAddresses", x => x.SponsorAddressId);
                    table.ForeignKey(
                        name: "FK_SponsorAddresses_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_SponsorName",
                table: "Sponsors",
                column: "SponsorName");

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_Tin",
                table: "Sponsors",
                column: "Tin");

            migrationBuilder.CreateIndex(
                name: "IX_LogCoverages_ActivatedByUserId",
                table: "LogCoverages",
                column: "ActivatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogCoverages_CreatedByUserId",
                table: "LogCoverages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogCoverages_DeactivatedByUserId",
                table: "LogCoverages",
                column: "DeactivatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogCoverages_ModifiedByUserId",
                table: "LogCoverages",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogCoverages_SchoolYearId_StudentId",
                table: "LogCoverages",
                columns: new[] { "SchoolYearId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SponsorAddresses_SponsorId",
                table: "SponsorAddresses",
                column: "SponsorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_AspNetUsers_ActivatedByUserId",
                table: "LogCoverages",
                column: "ActivatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_AspNetUsers_CreatedByUserId",
                table: "LogCoverages",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_AspNetUsers_DeactivatedByUserId",
                table: "LogCoverages",
                column: "DeactivatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_AspNetUsers_ModifiedByUserId",
                table: "LogCoverages",
                column: "ModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_Sponsors_SponsorId",
                table: "LogCoverages",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "SponsorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_Students_SchoolYearId_StudentId",
                table: "LogCoverages",
                columns: new[] { "SchoolYearId", "StudentId" },
                principalTable: "Students",
                principalColumns: new[] { "SchoolYearId", "StudentId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_AspNetUsers_ActivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_AspNetUsers_CreatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_AspNetUsers_DeactivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_AspNetUsers_ModifiedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_Sponsors_SponsorId",
                table: "LogCoverages");

            migrationBuilder.DropForeignKey(
                name: "FK_LogCoverages_Students_SchoolYearId_StudentId",
                table: "LogCoverages");

            migrationBuilder.DropTable(
                name: "SponsorAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Sponsors_SponsorName",
                table: "Sponsors");

            migrationBuilder.DropIndex(
                name: "IX_Sponsors_Tin",
                table: "Sponsors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogCoverages",
                table: "LogCoverages");

            migrationBuilder.DropIndex(
                name: "IX_LogCoverages_ActivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropIndex(
                name: "IX_LogCoverages_CreatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropIndex(
                name: "IX_LogCoverages_DeactivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropIndex(
                name: "IX_LogCoverages_ModifiedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropIndex(
                name: "IX_LogCoverages_SchoolYearId_StudentId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ExternalSystemId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "NetSuiteId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "PowerSchoolId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "LogId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "ActivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "ActivatedOn",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "DeactivatedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "DeactivatedOn",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "LogCoverages");

            migrationBuilder.DropColumn(
                name: "ReviewComments",
                table: "LogCoverages");

            migrationBuilder.AlterColumn<string>(
                name: "Tin",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "SponsorName",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogCoverages",
                table: "LogCoverages",
                columns: new[] { "SchoolYearId", "StudentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_Sponsors_SponsorId",
                table: "LogCoverages",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "SponsorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogCoverages_Students_SchoolYearId_StudentId",
                table: "LogCoverages",
                columns: new[] { "SchoolYearId", "StudentId" },
                principalTable: "Students",
                principalColumns: new[] { "SchoolYearId", "StudentId" });
        }
    }
}
