using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Infrastructure.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Tournaments", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 160, nullable: false), Description = table.Column<string>(maxLength: 2000, nullable: true), Location = table.Column<string>(maxLength: 160, nullable: false), StartsOn = table.Column<DateOnly>(nullable: false), EndsOn = table.Column<DateOnly>(nullable: false), Status = table.Column<int>(nullable: false), LogoPath = table.Column<string>(maxLength: 512, nullable: true), RulesNotes = table.Column<string>(maxLength: 4000, nullable: true), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Tournaments", x => x.Id));
        migrationBuilder.CreateTable(name: "AgeGroups", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), TournamentId = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 120, nullable: false), BirthYearFrom = table.Column<int>(nullable: true), BirthYearTo = table.Column<int>(nullable: true), MatchDurationMinutes = table.Column<int>(nullable: false), PointsPerWin = table.Column<int>(nullable: false), PointsPerDraw = table.Column<int>(nullable: false), PointsPerLoss = table.Column<int>(nullable: false), CompetitionFormat = table.Column<int>(nullable: false), RoundRobinLegs = table.Column<int>(nullable: false), FixedMatchesPerTeam = table.Column<int>(nullable: true), AdvancingTeamsPerGroup = table.Column<int>(nullable: false), TieBreakerOrder = table.Column<string>(maxLength: 256, nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_AgeGroups", x => x.Id); table.ForeignKey("FK_AgeGroups_Tournaments_TournamentId", x => x.TournamentId, "Tournaments", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateIndex("IX_Tournaments_Name", "Tournaments", "Name");
        migrationBuilder.CreateIndex("IX_AgeGroups_TournamentId_Name", "AgeGroups", new[] { "TournamentId", "Name" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AgeGroups");
        migrationBuilder.DropTable("Tournaments");
    }
}
