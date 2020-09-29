using FoodTruck.DAL;
using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoodTruck.ViewModels
{
    public class PanierViewModel
    {
        public List<ArticleViewModel> ArticlesDetailsViewModel { get; set; }
        public double PrixTotalTTC { get; set; }
        public List<Creneau> Creneaux { get; set; }

        internal PanierViewModel()
        {
            ArticlesDetailsViewModel = new List<ArticleViewModel>();
        }

        internal PanierViewModel(List<Panier> panierClient)
        {
            ArticlesDetailsViewModel = new List<ArticleViewModel>();
            foreach (Panier panier in panierClient)
            {
                PrixTotalTTC += panier.PrixTotal;
                ArticlesDetailsViewModel.Add(new ArticleViewModel(new ArticleDAL().Details(panier.ArticleId), panier.Quantite));
            }
        }

        internal PanierViewModel(List<PanierProspect> panierProspect)
        {
            ArticlesDetailsViewModel = new List<ArticleViewModel>();
            foreach (PanierProspect panier in panierProspect)
            {
                PrixTotalTTC += panier.PrixTotal;
                ArticlesDetailsViewModel.Add(new ArticleViewModel(new ArticleDAL().Details(panier.ArticleId), panier.Quantite));
            }
        }

        internal void Initialiser()
        {
            ArticlesDetailsViewModel = new List<ArticleViewModel>();
            Creneaux = new List<Creneau>();
            PrixTotalTTC = 0;
        }

        internal void Trier()
        {
            ArticlesDetailsViewModel = ArticlesDetailsViewModel.OrderBy(x => x.Article.FamilleId).ThenBy(x => x.Article.Nom).ToList();
        }

        internal bool Ajouter(Article article, int quantite = 1, int clientId = 0, string prospectGuid = "")
        {
            bool ajout = article.DansCarte ? true : false;
            if (ajout)
            {
                bool sauvPanierClient = clientId != 0 ? true : false;
                ArticleViewModel artcl = ArticlesDetailsViewModel.Find(art => art.Article.Id == article.Id);
                if (artcl == null)
                {
                    ArticleViewModel articleViewModel = new ArticleViewModel(article);
                    ArticlesDetailsViewModel.Add(articleViewModel);
                    if (sauvPanierClient)
                        new PanierDAL(clientId).Ajouter(article, quantite);
                    else
                        new PanierProspectDAL(prospectGuid).Ajouter(article, quantite);
                }
                else
                {
                    artcl.Quantite += quantite;
                    artcl.PrixTotalTTC = Math.Round(artcl.PrixTotalTTC + quantite * artcl.Article.PrixTTC, 2);
                    if (sauvPanierClient)
                        new PanierDAL(clientId).ModifierQuantite(article, quantite);
                    else
                        new PanierProspectDAL(prospectGuid).ModifierQuantite(article, quantite);
                }
                PrixTotalTTC += quantite * article.PrixTTC;
            }
            return ajout;
        }
    }
}