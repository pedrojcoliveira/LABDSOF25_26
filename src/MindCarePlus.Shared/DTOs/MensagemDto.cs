namespace MindCarePlus.Shared.DTOs;

/// DTO para enviar/receber mensagens individuais
public class MensagemDto
{
    public Guid Id { get; set; }
    public Guid RemetenteId { get; set; }
    public string RemetenteNome { get; set; } = string.Empty;
    public Guid DestinatarioId { get; set; }
    public string DestinatarioNome { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public bool Lida { get; set; }
    public string TipoRemetente { get; set; } = string.Empty; // "Paciente" ou "Psicologo"
    public bool EhMinhaMensagem { get; set; }
}