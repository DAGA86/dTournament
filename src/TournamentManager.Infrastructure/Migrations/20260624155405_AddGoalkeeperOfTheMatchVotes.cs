using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalkeeperOfTheMatchVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId_TeamId",
                table: "PlayerOfTheMatchVotes");

            migrationBuilder.AddColumn<bool>(
                name: "IsGoalkeeperVote",
                table: "PlayerOfTheMatchVotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId",
                table: "PlayerOfTheMatchVotes",
                column: "MatchId",
                unique: true,
                filter: "[IsGoalkeeperVote] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId_TeamId",
                table: "PlayerOfTheMatchVotes",
                columns: new[] { "MatchId", "TeamId" },
                unique: true,
                filter: "[IsGoalkeeperVote] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId",
                table: "PlayerOfTheMatchVotes");

            migrationBuilder.DropIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId_TeamId",
                table: "PlayerOfTheMatchVotes");

            migrationBuilder.DropColumn(
                name: "IsGoalkeeperVote",
                table: "PlayerOfTheMatchVotes");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerOfTheMatchVotes_MatchId_TeamId",
                table: "PlayerOfTheMatchVotes",
                columns: new[] { "MatchId", "TeamId" },
                unique: true);
        }
    }
}
