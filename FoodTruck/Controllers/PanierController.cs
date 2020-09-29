using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web.Mvc;

namespace FoodTruck.Controllers
{
    public class PanierController : ControllerParent
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (PanierViewModel.ArticlesDetailsViewModel.Count != 0)
            {
                ViewBag.PanierLatteralDesactive = true;
                DateTime maintenant = DateTime.Now;
                List<PlageHoraireRetrait> plagesHorairesRetrait = maintenant.PlageHoraireRetrait();

                // obtention nombre de commandes à retirer dans chaque creneaux ouvert et desactivation si = nombre max
                int maxCommandesHeure = int.Parse(ConfigurationManager.AppSettings["NombreDeCommandesMaxParHeure"]);
                CommandeDAL commandeDAL = new CommandeDAL();
                PanierViewModel.Creneaux = new List<Creneau>();
                foreach (PlageHoraireRetrait plage in plagesHorairesRetrait)
                {
                    int maxCommandesCreneau = (int)Math.Ceiling(maxCommandesHeure * plage.Pas.TotalMinutes / 60);
                    foreach (DateTime date in plage.Dates)
                    {
                        Creneau creneau = new Creneau
                        {
                            DateRetrait = date,
                            CommandesPossiblesRestantes = maxCommandesCreneau - commandeDAL.NombreCommandes(date)
                        };
                        PanierViewModel.Creneaux.Add(creneau);
                    }
                }

                int index = ((List<string>)Session["Url"]).Count - 2;
                if (index < 0)
                    index = 0;
                if (Client.Id == 0 && PanierViewModel.ArticlesDetailsViewModel.Count > 0 && ((List<string>)Session["Url"])[index] != "/Panier/Index")
                    TempData["message"] = new Message("Vous n'êtes pas connecté à votre compte.\nVous pouvez commander mais\n- vous ne bénéficierez pas du programme de fidélité\n- votre commande ne sera pas dans votre historique\n- vous ne recevrez pas de confirmation de votre commande", TypeMessage.Info);
                return View(PanierViewModel);
            }
            else
            {
                TempData["message"] = new Message("Vous n'avez pas d'article dans votre panier\nVoyez notre carte pour faire votre choix.", TypeMessage.Info);
                return RedirectToAction("Index", "Article");
            }
        }

        [HttpPost]
        public ActionResult Index(string codePromo)
        {
            TempData["CodePromo"] = codePromo;
            TempData["RemiseCommercialeValide"] = false;
            TempData["RemiseCommercialeMontant"] = (double)0;
            ValiditeCodePromo code = new CodePromoDAL().Validite(codePromo, PanierViewModel.PrixTotalTTC, out double montantRemise);
            switch (code)
            {
                case ValiditeCodePromo.Valide:
                    TempData["RemiseCommercialeValide"] = true;
                    TempData["RemiseCommercialeMontant"] = montantRemise;
                    TempData["message"] = new Message($"Le code saisi est bien valide.\nIl vous donne droit à {montantRemise.ToString("C2", new CultureInfo("fr-FR"))}  de réduction sur votre commande", TypeMessage.Ok);
                    break;
                case ValiditeCodePromo.Inconnu:
                    TempData["message"] = new Message("Le code saisi est inconnu", TypeMessage.Erreur);
                    break;
                case ValiditeCodePromo.DateDepassee:
                    TempData["message"] = new Message("Le code saisi n'est plus valable", TypeMessage.Erreur);
                    break;
                case ValiditeCodePromo.DateFuture:
                    TempData["message"] = new Message("Le code saisi n'est pas encore valable", TypeMessage.Erreur);
                    break;
                case ValiditeCodePromo.MontantInsuffisant:
                    TempData["message"] = new Message("Le code saisi est valide mais le montant de la commande est insuffisant", TypeMessage.Erreur);
                    break;
            }
            return Redirect("~/Panier/Index#codePromoClient");
        }

        [HttpPost]
        public ActionResult Ajouter(string nom, string ancre, bool? retourPageArticleIndex)
        {
            Article article = new ArticleDAL().Details(nom);
            if (article != null && article.DansCarte)
            {
                PanierViewModel.Ajouter(article, 1, Client.Id, ProspectGuid);
                ViewBag.Panier = PanierViewModel;
            }
            if (retourPageArticleIndex ?? false)
                return Redirect("/Article" + ancre);
            else
                return Redirect(Request.UrlReferrer.AbsolutePath + ancre);
        }

        [HttpPost]
        public ActionResult Retirer(int id)
        {
            bool sauvPanierClient = Client.Id != 0 ? true : false;
            if (id < PanierViewModel.ArticlesDetailsViewModel.Count)
            {
                Article article = new ArticleDAL().Details(PanierViewModel.ArticlesDetailsViewModel[id].Article.Id);
                PanierDAL panierDAL;
                PanierProspectDAL panierProspectDAL;
                PanierViewModel.PrixTotalTTC = Math.Round(PanierViewModel.PrixTotalTTC - article.PrixTTC, 2);

                if (PanierViewModel.ArticlesDetailsViewModel[id].Quantite > 1)
                {
                    PanierViewModel.ArticlesDetailsViewModel[id].Quantite--;
                    PanierViewModel.ArticlesDetailsViewModel[id].PrixTotalTTC = Math.Round(PanierViewModel.ArticlesDetailsViewModel[id].PrixTotalTTC - PanierViewModel.ArticlesDetailsViewModel[id].Article.PrixTTC, 2);
                    if (sauvPanierClient)
                    {
                        panierDAL = new PanierDAL(Client.Id);
                        panierDAL.ModifierQuantite(article, -1);
                    }
                    else
                    {
                        panierProspectDAL = new PanierProspectDAL(ProspectGuid);
                        panierProspectDAL.ModifierQuantite(article, -1);
                    }
                }
                else
                {
                    PanierViewModel.ArticlesDetailsViewModel.RemoveAt(id);
                    if (sauvPanierClient)
                    {
                        panierDAL = new PanierDAL(Client.Id);
                        panierDAL.Supprimer(article);
                    }
                    else
                    {
                        panierProspectDAL = new PanierProspectDAL(ProspectGuid);
                        panierProspectDAL.Supprimer(article);
                    }
                }
                ViewBag.Panier = PanierViewModel;
            }
            return Redirect(Request.UrlReferrer.AbsolutePath);
        }
    }
}
