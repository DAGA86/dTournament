using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeGroupDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "AgeGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AgeGroups_TournamentId_DisplayOrder",
                table: "AgeGroups",
                columns: new[] { "TournamentId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgeGroups_TournamentId_DisplayOrder",
                table: "AgeGroups");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "AgeGroups");
        }
    }
}
