using FoodTruck.DAL;
using FoodTruck.Models;
using SelectPdf;
using System;
using System.Collections.Generic;

namespace FoodTruck.ViewModels
{
    public class CommandeViewModel
    {
        public Commande Commande { get; set; }
        public Client Client { get; set; }
        public List<ArticleViewModel> ListArticlesVM { get; set; }

        public CommandeViewModel(Commande commande)
        {
            Commande = commande;
            if (commande != null)
            {
                Client = new ClientDAL().Details(commande.ClientId);
                ListArticlesVM = new CommandeDAL().Articles(commande.Id);
            }
        }
    }
}