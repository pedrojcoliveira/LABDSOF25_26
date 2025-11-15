using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs.Auth
{
    public class RegisterPsicologoDto: RegisterBaseDto
    {
        [Required(ErrorMessage = "O número da cédula profissional é obrigatório")]
        [StringLength(50, ErrorMessage = "O número da cédula profissional não pode exceder 50 caracteres")]
        public string NumeroCedulaProfissional { get; set; } = string.Empty;

        [Required(ErrorMessage = "A especialidade é obrigatória")]
        [StringLength(100, ErrorMessage = "A especialidade não pode exceder 100 caracteres")]
        public string Especialidade { get; set; } = string.Empty;
    }
}
