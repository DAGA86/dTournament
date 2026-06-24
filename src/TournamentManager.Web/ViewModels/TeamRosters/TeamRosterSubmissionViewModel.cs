using System.ComponentModel.DataAnnotations;
using TournamentManager.Application.Services;

namespace TournamentManager.Web.ViewModels.TeamRosters;

public sealed class TeamRosterSubmissionViewModel : IValidatableObject
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string AgeGroupName { get; set; } = string.Empty;
    public int? BirthYearFrom { get; set; }
    public int? BirthYearTo { get; set; }
    public string? MinimumBirthDate => BirthYearFrom.HasValue ? $"{BirthYearFrom.Value:D4}-01-01" : null;
    public string? MaximumBirthDate => BirthYearTo.HasValue ? $"{BirthYearTo.Value:D4}-12-31" : null;
    public List<TeamRosterPlayerViewModel> Players { get; set; } = [];
    public List<TeamRosterStaffMemberViewModel> StaffMembers { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var players = Players.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).ToList();
        var staffMembers = StaffMembers.Where(x => !string.IsNullOrWhiteSpace(x.FullName) || !string.IsNullOrWhiteSpace(x.Role)).ToList();
        if (players.Count > TeamRosterSubmissionService.MaximumPlayers) yield return new ValidationResult($"Só pode submeter até {TeamRosterSubmissionService.MaximumPlayers} jogadores.", [nameof(Players)]);
        if (staffMembers.Count > TeamRosterSubmissionService.MaximumStaffMembers) yield return new ValidationResult($"Só pode submeter até {TeamRosterSubmissionService.MaximumStaffMembers} elementos de staff.", [nameof(StaffMembers)]);
        foreach (var staffMember in staffMembers.Where(x => string.IsNullOrWhiteSpace(x.FullName) || string.IsNullOrWhiteSpace(x.Role))) yield return new ValidationResult("Cada elemento de staff deve ter nome e função.", [nameof(StaffMembers)]);
        var duplicateNumbers = players.Where(x => x.ShirtNumber.HasValue).GroupBy(x => x.ShirtNumber!.Value).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicateNumbers.Count > 0) yield return new ValidationResult("Os números dos jogadores não podem estar repetidos.", [nameof(Players)]);

        foreach (var player in players)
        {
            if (!player.BirthDate.HasValue)
            {
                yield return new ValidationResult("A data de nascimento é obrigatória para cada jogador preenchido.", [nameof(Players)]);
                continue;
            }

            var birthYear = player.BirthDate.Value.Year;
            if (BirthYearFrom.HasValue && birthYear < BirthYearFrom.Value)
            {
                yield return new ValidationResult($"O ano de nascimento dos jogadores não pode ser anterior a {BirthYearFrom.Value} para o escalão {AgeGroupName}.", [nameof(Players)]);
            }

            if (BirthYearTo.HasValue && birthYear > BirthYearTo.Value)
            {
                yield return new ValidationResult($"O ano de nascimento dos jogadores não pode ser posterior a {BirthYearTo.Value} para o escalão {AgeGroupName}.", [nameof(Players)]);
            }
        }
    }
}

public sealed class TeamRosterPlayerViewModel
{
    [MaxLength(180)] public string? FullName { get; set; }
    [Range(1, 999)] public int? ShirtNumber { get; set; }
    [DataType(DataType.Date)] public DateOnly? BirthDate { get; set; }
}

public sealed class TeamRosterStaffMemberViewModel
{
    [MaxLength(180)] public string? FullName { get; set; }
    [MaxLength(80)] public string? Role { get; set; }
}