using System.ComponentModel.DataAnnotations;

namespace MindCarePlus.Shared.DTOs.Profile;

/// <summary>
/// DTO para pedido de eliminação de conta
/// User Story: UM.P.05 e UM.PS.20
/// </summary>
public class DeleteAccountRequestDto
{
    /// <summary>
    /// Password para confirmar a eliminação
    /// </summary>
    [Required(ErrorMessage = "A password é obrigatória para confirmar a eliminação")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmação explícita de que o utilizador entende que a ação é irreversível
    /// </summary>
    [Required(ErrorMessage = "Deve confirmar que entende que a ação é irreversível")]
    public bool ConfirmIrreversible { get; set; }
}
