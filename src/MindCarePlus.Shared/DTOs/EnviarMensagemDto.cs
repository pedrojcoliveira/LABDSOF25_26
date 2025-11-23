namespace MindCarePlus.Shared.DTOs;

public class EnviarMensagemDto
{
    public Guid DestinatarioId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}