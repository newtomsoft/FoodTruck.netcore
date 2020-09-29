using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodTruck.ViewModels
{
    public class FactureViewModel
    {
        public CommandeViewModel CommandeViewModel { get; }

        public FactureViewModel(CommandeViewModel commandeViewModel)
        {
            CommandeViewModel = commandeViewModel;
        }
    }
}