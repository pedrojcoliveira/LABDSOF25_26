using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class DesafioAtribuido
    {
        public Guid Id { get; set; }
        public Guid DesafioId { get; set; }
        public Desafio Desafio { get; set; } = null!;
        public Guid UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; } = null!;
        public DateTime DataAtribuicao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string Estado { get; set; } = string.Empty; // TODO: criar enum com os estados possiveis (atribuido, em progresso, concluido, falhado, etc.) Ou usar bool Concluido???

        // Campos para Tarefas Terapêuticas
        public bool IsTarefaTerapeutica { get; set; } = false;
        public Guid? PsicologoId { get; set; } // Psicólogo que atribuiu a tarefa
        public DateTime? Prazo { get; set; } // Data limite para conclusão
    }
}
