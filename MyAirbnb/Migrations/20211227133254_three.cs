using Microsoft.EntityFrameworkCore.Migrations;

namespace MyAirbnb.Migrations
{
    public partial class three : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Reservations",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Reservations",
                newName: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PostId",
                table: "Reservations",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_UserId",
                table: "Comment",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_PostId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Comment_UserId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Reservations",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Reservations",
                newName: "End");
        }
    }
}
