using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Application.DTOs.Auth
{
    public class RegisterPacienteDto : RegisterBaseDto

    {
        [StringLength(500, ErrorMessage = "O objetivo não pode exceder 500 caracteres")]
        public string? Objectivo { get; set; }
    }
}
