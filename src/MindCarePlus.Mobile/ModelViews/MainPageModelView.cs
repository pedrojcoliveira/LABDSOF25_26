using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class MainPageModelView: ObservableObject
    {


        [ObservableProperty]
        private int counter;


        [RelayCommand]
        public void AddCounter()
        {
            Counter++;
        }

    }
}
