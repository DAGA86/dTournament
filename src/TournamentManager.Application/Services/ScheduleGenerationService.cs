using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class ScheduleGenerationService
{
    public IReadOnlyList<GeneratedMatchDto> GenerateRoundRobin(IReadOnlyList<Team> teams, int legs)
    {
        if (teams.Count < 2) throw new InvalidOperationException("At least two teams are required to generate a round robin schedule.");
        if (legs is < 1 or > 2) throw new InvalidOperationException("Round robin legs must be one or two.");

        var workingTeams = teams.OrderBy(x => x.Name).ToList();
        var hasBye = workingTeams.Count % 2 == 1;
        if (hasBye) workingTeams.Add(new Team { Id = Guid.Empty, Name = "Bye", ShortName = "Bye", Club = "Bye" });

        var teamCount = workingTeams.Count;
        var roundsPerLeg = teamCount - 1;
        var matchesPerRound = teamCount / 2;
        var generated = new List<GeneratedMatchDto>();

        for (var leg = 0; leg < legs; leg++)
        {
            var rotation = workingTeams.ToList();
            for (var round = 1; round <= roundsPerLeg; round++)
            {
                var roundNumber = leg * roundsPerLeg + round;
                for (var index = 0; index < matchesPerRound; index++)
                {
                    var first = rotation[index];
                    var second = rotation[teamCount - 1 - index];
                    if (first.Id == Guid.Empty || second.Id == Guid.Empty) continue;
                    var swapHomeAway = (round + index + leg) % 2 == 0;
                    var home = swapHomeAway ? second : first;
                    var away = swapHomeAway ? first : second;
                    generated.Add(new GeneratedMatchDto(home.Id, home.Name, away.Id, away.Name, roundNumber));
                }

                var fixedTeam = rotation[0];
                var tail = rotation.Skip(1).ToList();
                tail.Insert(0, tail[^1]);
                tail.RemoveAt(tail.Count - 1);
                rotation = new[] { fixedTeam }.Concat(tail).ToList();
            }
        }

        return generated;
    }

    public IReadOnlyList<GeneratedMatchDto> GenerateFixedMatches(IReadOnlyList<Team> teams, int matchesPerTeam)
    {
        if (teams.Count < 2) throw new InvalidOperationException("At least two teams are required to generate a schedule.");
        if (matchesPerTeam <= 0) throw new InvalidOperationException("Matches per team must be greater than zero.");
        if ((teams.Count * matchesPerTeam) % 2 != 0) throw new InvalidOperationException("This configuration is impossible because the total number of team match slots is odd.");
        if (matchesPerTeam > teams.Count - 1) throw new InvalidOperationException("Fixed matches per team cannot exceed the number of available opponents without repeats.");

        var allPairs = GenerateRoundRobin(teams, 1).ToList();
        var counts = teams.ToDictionary(x => x.Id, _ => 0);
        var selected = new List<GeneratedMatchDto>();

        foreach (var match in allPairs.OrderBy(x => x.RoundNumber).ThenBy(x => x.HomeTeamName))
        {
            if (counts[match.HomeTeamId] >= matchesPerTeam || counts[match.AwayTeamId] >= matchesPerTeam) continue;
            selected.Add(match with { RoundNumber = selected.Count / Math.Max(1, teams.Count / 2) + 1 });
            counts[match.HomeTeamId]++;
            counts[match.AwayTeamId]++;
            if (counts.Values.All(x => x == matchesPerTeam)) break;
        }

        if (counts.Values.Any(x => x != matchesPerTeam)) throw new InvalidOperationException("The schedule could not satisfy the requested number of matches for every team without repeated opponents.");
        return selected;
    }
}
