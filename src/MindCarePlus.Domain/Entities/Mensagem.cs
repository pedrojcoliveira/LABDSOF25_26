using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Mensagem
    {
        public Guid Id { get; set; }
        public Guid RemetenteId { get; set; }
        public Utilizador Remetente { get; set; } = null!;
        public Guid DestinatarioId { get; set; }
        public Utilizador Destinatario { get; set; } = null!;
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public bool Lida { get; set; }
        public DateTime? DataLeitura { get; set; }

    }
}
