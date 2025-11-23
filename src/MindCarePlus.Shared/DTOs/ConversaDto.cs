namespace MindCarePlus.Shared.DTOs;


/// DTO para listar conversas (preview)
public class ConversaDto
{
    public Guid OutroUtilizadorId { get; set; }
    public string OutroUtilizadorNome { get; set; } = string.Empty;
    public string OutroUtilizadorAvatar { get; set; } = "👤";
    public string OutroUtilizadorTipo { get; set; } = string.Empty; // "Paciente" ou "Psicologo"
    public string UltimaMensagem { get; set; } = string.Empty;
    public DateTime UltimaMensagemData { get; set; }
    public int MensagensNaoLidas { get; set; }
    public bool UltimaMensagemPropria { get; set; }
}