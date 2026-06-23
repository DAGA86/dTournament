using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchPeriodSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HalfTimeBreakMinutes",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlannedPeriodCount",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HalfTimeBreakMinutes",
                table: "AgeGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPeriods",
                table: "AgeGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HalfTimeBreakMinutes",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PlannedPeriodCount",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HalfTimeBreakMinutes",
                table: "AgeGroups");

            migrationBuilder.DropColumn(
                name: "NumberOfPeriods",
                table: "AgeGroups");
        }
    }
}
