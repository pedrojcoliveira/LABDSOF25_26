
using System.ComponentModel.DataAnnotations;

namespace MindCarePlus.Shared.DTOs.TarefaTerapeutica;

public class CriarTarefaTerapeuticaDto
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 100 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 500 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O prazo é obrigatório")]
    public DateTime Prazo { get; set; }

    [Range(0, 100, ErrorMessage = "Os pontos devem estar entre 0 e 100")]
    public int PontosRecompensa { get; set; } = 20;

    [Required(ErrorMessage = "O ID do paciente é obrigatório")]
    public Guid PacienteId { get; set; }
}

public class TarefaTerapeuticaDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataAtribuicao { get; set; }
    public DateTime Prazo { get; set; }
    public DateTime? DataConclusao { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int PontosRecompensa { get; set; }
    public string PsicologoNome { get; set; } = string.Empty;
    public bool Atrasada => DateTime.UtcNow > Prazo && Estado != "Concluido";
}