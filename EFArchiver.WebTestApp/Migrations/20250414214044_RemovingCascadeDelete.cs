using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFArchiver.WebTestApp.Migrations
{
    /// <inheritdoc />
    public partial class RemovingCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_People_PersonId",
                table: "Profiles");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_People_PersonId",
                table: "Profiles",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_People_PersonId",
                table: "Profiles");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_People_PersonId",
                table: "Profiles",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
