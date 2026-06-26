using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Admin;

public sealed class OperatorPasswordViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova password é obrigatória.")]
    [DataType(DataType.Password)]
    [MinLength(12, ErrorMessage = "A password deve ter pelo menos 12 caracteres.")]
    [Display(Name = "Nova password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "As passwords não coincidem.")]
    [Display(Name = "Confirmar password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
