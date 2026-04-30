using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class MakeSponsorIdOptionalForStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Sponsors_SponsorId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "SponsorId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Sponsors_SponsorId",
                table: "Students",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "SponsorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Sponsors_SponsorId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "SponsorId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Sponsors_SponsorId",
                table: "Students",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "SponsorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
