using MindCarePlus.Mobile.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Services
{
    // A classe que estabelece o contrato usando o código do MAUI, para permitir testes unitários mais simples
    public class MauiDialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            // Garante que o alerta corre sempre na thread principal da UI
            return MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert(title, message, cancel)
            );
        }
    }
}