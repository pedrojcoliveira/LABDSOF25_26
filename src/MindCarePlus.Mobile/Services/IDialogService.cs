using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Services
{
    // O "Contrato" do que queremos fazer, para utilizar quando formos mostrar alertas
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string cancel);
    }
}