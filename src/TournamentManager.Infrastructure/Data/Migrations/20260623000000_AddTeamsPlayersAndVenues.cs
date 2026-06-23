using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Infrastructure.Data.Migrations;

public partial class AddTeamsPlayersAndVenues : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Groups", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), AgeGroupId = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 80, nullable: false), DisplayOrder = table.Column<int>(nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_Groups", x => x.Id); table.ForeignKey("FK_Groups_AgeGroups_AgeGroupId", x => x.AgeGroupId, "AgeGroups", "Id", onDelete: ReferentialAction.Restrict); });

        migrationBuilder.CreateTable(name: "Venues", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 120, nullable: false), Description = table.Column<string>(maxLength: 1000, nullable: true), IsActive = table.Column<bool>(nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Venues", x => x.Id));

        migrationBuilder.CreateTable(name: "Teams", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), AgeGroupId = table.Column<Guid>(nullable: false), GroupId = table.Column<Guid>(nullable: true), Name = table.Column<string>(maxLength: 160, nullable: false), ShortName = table.Column<string>(maxLength: 32, nullable: false), Club = table.Column<string>(maxLength: 160, nullable: false), LogoPath = table.Column<string>(maxLength: 512, nullable: true), PrimaryColor = table.Column<string>(maxLength: 16, nullable: true), ResponsiblePerson = table.Column<string>(maxLength: 160, nullable: false), Contact = table.Column<string>(maxLength: 160, nullable: true), IsActive = table.Column<bool>(nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_Teams", x => x.Id); table.ForeignKey("FK_Teams_AgeGroups_AgeGroupId", x => x.AgeGroupId, "AgeGroups", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_Teams_Groups_GroupId", x => x.GroupId, "Groups", "Id", onDelete: ReferentialAction.Restrict); });

        migrationBuilder.CreateTable(name: "Players", columns: table => new
        {
            Id = table.Column<Guid>(nullable: false), TeamId = table.Column<Guid>(nullable: false), FullName = table.Column<string>(maxLength: 180, nullable: false), DisplayName = table.Column<string>(maxLength: 80, nullable: false), BirthDate = table.Column<DateOnly>(nullable: false), ShirtNumber = table.Column<int>(nullable: true), IsActive = table.Column<bool>(nullable: false), AgeOverrideApproved = table.Column<bool>(nullable: false), AgeOverrideApprovedByUserId = table.Column<string>(maxLength: 450, nullable: true), AgeOverrideReason = table.Column<string>(maxLength: 1000, nullable: true), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_Players", x => x.Id); table.ForeignKey("FK_Players_Teams_TeamId", x => x.TeamId, "Teams", "Id", onDelete: ReferentialAction.Restrict); });

        migrationBuilder.CreateIndex("IX_Groups_AgeGroupId_Name", "Groups", new[] { "AgeGroupId", "Name" }, unique: true);
        migrationBuilder.CreateIndex("IX_Venues_Name", "Venues", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_Teams_AgeGroupId_Name", "Teams", new[] { "AgeGroupId", "Name" }, unique: true);
        migrationBuilder.CreateIndex("IX_Teams_GroupId", "Teams", "GroupId");
        migrationBuilder.CreateIndex("IX_Players_DisplayName", "Players", "DisplayName");
        migrationBuilder.CreateIndex("IX_Players_TeamId_ShirtNumber", "Players", new[] { "TeamId", "ShirtNumber" }, unique: true, filter: "[ShirtNumber] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Players");
        migrationBuilder.DropTable("Venues");
        migrationBuilder.DropTable("Teams");
        migrationBuilder.DropTable("Groups");
    }
}
