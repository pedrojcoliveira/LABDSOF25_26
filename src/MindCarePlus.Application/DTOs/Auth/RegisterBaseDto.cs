using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Application.DTOs.Auth
{
    public class RegisterBaseDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A palavra-passe é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A palavra-passe deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da palavra-passe é obrigatória")]
        [Compare("Password", ErrorMessage = "As palavras-passe não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A idade é obrigatória")]
        [Range(13, 120, ErrorMessage = "A idade deve estar entre 13 e 120 anos")]
        public int Idade { get; set; }
    }
}
