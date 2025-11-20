using MindCarePlus.Shared.DTOs.Paciente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Application.Interfaces;
public interface IPacienteService
{
    Task<DetalhePacienteDto?> ObterDetalhesCompletosAsync(Guid pacienteId, Guid psicologoId);

    Task<List<PacienteResumoDto>> ListarPacientesPorPsicologoAsync(Guid psicologoId);
}
