namespace MindCarePlus.Shared.DTOs.Profile;

// <summary>
/// DTO com resultado da operação de eliminação de conta
/// User Story: UM.P.05 e UM.PS.20
/// </summary>
public class DeleteAccountResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalhes dos dados eliminados (para auditoria/debug)
    /// </summary>
    public DeletedDataSummaryDto? DeletedData { get; set; }
}

/// <summary>
/// Resumo dos dados que foram eliminados
/// </summary>
public class DeletedDataSummaryDto
{
    public int RegistosDiariosEliminados { get; set; }
    public int RelatoriosIAEliminados { get; set; }
    public int MensagensEliminadas { get; set; }
    public int SessoesEliminadas { get; set; }
    public int AvaliacoesEliminadas { get; set; }
    public int DesafiosAtribuidosEliminados { get; set; }
    public int BadgesAtribuidasEliminadas { get; set; }
}



