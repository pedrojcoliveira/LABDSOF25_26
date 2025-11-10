using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public abstract class Utilizador
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string PasswordHash { get; set; } = string.Empty; 
        public DateTime dataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<DesafioAtribuido> DesafiosAtribuidos { get; set; } = new List<DesafioAtribuido>();
        public ICollection<BadgeAtribuida> BadgesAtribuidas { get; set; } = new List<BadgeAtribuida>();
    }
}
