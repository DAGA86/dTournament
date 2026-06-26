namespace TournamentManager.Web.ViewModels.Admin;

public sealed record OperatorAccountViewModel(string UserId, string Email, string DisplayName, bool IsLockedOut);

public sealed record OperatorAccountsViewModel(IReadOnlyList<OperatorAccountViewModel> Operators, OperatorPasswordViewModel PasswordForm);