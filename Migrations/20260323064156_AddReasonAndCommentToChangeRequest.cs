using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISMSponsor.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonAndCommentToChangeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "ChangeRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionComment",
                table: "ChangeRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "ResolutionComment",
                table: "ChangeRequests");
        }
    }
}
