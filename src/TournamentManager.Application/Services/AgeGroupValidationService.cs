using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class AgeGroupValidationService
{
    public void Validate(AgeGroup ageGroup) => ageGroup.Validate();
}
