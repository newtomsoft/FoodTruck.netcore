using FoodTruck.DAL;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web.Mvc;
using System.Linq;
using FoodTruck.Models;
using System.IO;

namespace FoodTruck.Controllers
{
    public class CommandeController : ControllerParent
    {
        [ChildActionOnly]
        public ActionResult Liste(string id)
        {
            CommandeDAL commandeDAL = new CommandeDAL();
            List<Commande> commandes = null;
            ViewBag.ModeAdmin = false;
            ViewBag.ModeClient = true;
            switch (id)
            {
                case "dernieres":
                    const int nombreDernieresCommandes = 3;
                    List<Commande> commandesenCours = commandeDAL.CommandesEnCoursClient(Client.Id);
                    TempData["MessageCommandes"] = "Vos dernières commandes";
                    commandes = commandeDAL.CommandesClient(Client.Id).Except(commandesenCours, new CommandeEqualityComparer()).Take(nombreDernieresCommandes).ToList();
                    break;

                case "enCours":
                    TempData["MessageCommandes"] = "Vos commandes en cours";
                    commandes = commandeDAL.CommandesEnCoursClient(Client.Id);
                    break;

                case "toutes":
                    TempData["MessageCommandes"] = "Toutes vos commandes";
                    commandes = commandeDAL.CommandesClient(Client.Id);
                    break;
            }
            if (commandes != null && commandes.Count != 0)
            {
                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return PartialView(listeCommandesViewModel);
            }
            else
                return null;
        }

        [HttpPost]
        public ActionResult ReprendreArticles(int commandeId, bool viderPanier)
        {
            CommandeDAL commandeDAL = new CommandeDAL();
            Commande commande = commandeDAL.Detail(commandeId);
            if (commande != null && commande.ClientId == Client.Id)
            {
                List<ArticleViewModel> articles = commandeDAL.Articles(commandeId);
                if (viderPanier)
                {
                    new PanierDAL(Client.Id).Supprimer();
                    PanierViewModel.Initialiser();
                    ViewBag.Panier = null; //todo
                }
                List<Article> articlesKo = new List<Article>();
                foreach (var a in articles)
                {
                    if (!PanierViewModel.Ajouter(a.Article, a.Quantite, Client.Id, ProspectGuid))
                        articlesKo.Add(a.Article);
                }
                ViewBag.Panier = PanierViewModel;
                TempData["ArticlesNonAjoutes"] = articlesKo;
                if (articlesKo.Count > 0)
                {
                    string dossierImagesArticles = ConfigurationManager.AppSettings["PathImagesArticles"];
                    string message = "Les articles suivants ne peuvent pas être repris car ils ne sont plus disponibles :" +
                    "<div class=\"gestionCommandeArticle\">" +
                    "<section class=\"imagesGestionCommande\">";
                    foreach (Article article in articlesKo)
                    {
                        message += "<div class=\"indexArticle\">" +
                        $"<img src=\"{dossierImagesArticles}/{article.Image}\" alt=\"{article.Nom}\" /> " +
                        $"<p>{article.Nom}</p>" +
                        $"</div>";
                    }
                    message += "</section>" +
                    "</div>";
                    TempData["message"] = new Message(message, TypeMessage.Info); // TODO faire plus propre et ailleurs (formatage html propre à la vue)
                }
                else
                {
                    TempData["message"] = new Message($"La reprise des {articles.Count} articles de votre commande s'est correctement réalisée", TypeMessage.Ok);
                }
            }
            RecupererPanierEnBase();
            ViewBag.Panier = PanierViewModel;
            return RedirectToAction("Index", "Panier");
        }

        [HttpPost]
        public ActionResult Annuler(int commandeId)
        {
            CommandeDAL commandeDAL = new CommandeDAL();
            Commande commande = commandeDAL.Detail(commandeId);
            if (commande != null && (commande.ClientId == Client.Id || AdminCommande))
            {
                commandeDAL.Annuler(commandeId);
            }
            return Redirect(UrlCourante());
        }

        [HttpPost]
        public ActionResult Retirer(int commandeId)
        {
            CommandeDAL commandeDAL = new CommandeDAL();
            Commande commande = commandeDAL.Detail(commandeId);
            if (commande != null && (commande.ClientId == Client.Id || AdminCommande))
            {
                commandeDAL.Retirer(commandeId);
            }
            return Redirect(UrlCourante());
        }

        [HttpPost]
        public ActionResult Index(string codePromo, DateTime dateRetrait, int? remiseFidelite)
        {
            if (PanierViewModel.ArticlesDetailsViewModel.Count == 0)
            {
                return View(new Commande());
            }
            else
            {
                #region commandes restantes
                //int maxCommandesHeure = int.Parse(ConfigurationManager.AppSettings["NombreDeCommandesMaxParHeure"]);
                //int pasCreneauxHoraire = int.Parse(ConfigurationManager.AppSettings["PasCreneauxHoraire"]);
                //int maxCommandesCreneau = (int)Math.Ceiling((double)maxCommandesHeure * pasCreneauxHoraire / 60);
                //int commandesPossiblesRestantes = maxCommandesCreneau - new CommandeDAL().NombreCommandes(dateRetrait);
                #endregion
                int montantRemiseFidelite = remiseFidelite ?? 0;
                new CodePromoDAL().Validite(codePromo, PanierViewModel.PrixTotalTTC, out double montantRemiseCommerciale);

                if (montantRemiseFidelite != 0 && Client.Id != 0)
                {
                    int soldeCagnotte = new ClientDAL().RetirerCagnotte(Client.Id, montantRemiseFidelite);
                    if (soldeCagnotte == -1)
                        montantRemiseFidelite = 0;
                }
                Commande commande = new Commande
                {
                    ClientId = Client.Id,
                    DateCommande = DateTime.Now,
                    DateRetrait = dateRetrait,
                    PrixTotalTTC = 0,
                    RemiseFidelite = montantRemiseFidelite,
                    RemiseCommerciale = montantRemiseCommerciale
                };
                foreach (ArticleViewModel article in PanierViewModel.ArticlesDetailsViewModel)
                {
                    commande.PrixTotalTTC = Math.Round(commande.PrixTotalTTC + article.Article.PrixTTC * article.Quantite, 2);
                    new ArticleDAL().AugmenterQuantiteVendue(article.Article.Id, 1);
                }
                if (commande.PrixTotalTTC > montantRemiseFidelite + montantRemiseCommerciale)
                {
                    commande.PrixTotalTTC = Math.Round(commande.PrixTotalTTC - montantRemiseFidelite - montantRemiseCommerciale, 2);
                }
                else
                {
                    commande.RemiseCommerciale = Math.Round(commande.PrixTotalTTC - montantRemiseFidelite, 2);
                    commande.PrixTotalTTC = 0;
                }

                new CommandeDAL().Ajouter(commande, PanierViewModel.ArticlesDetailsViewModel);
                MailCommande(Client, commande, PanierViewModel);
                new PanierDAL(Client.Id).Supprimer();
                ViewBag.Panier = null; //todo

                string stringDateRetrait = commande.DateRetrait.ToString("dddd dd MMMM yyyy pour HH:mm");
                string message = $"Commande numéro {commande.Id} confirmée\nVeuillez venir la chercher le {stringDateRetrait}.";
                TypeMessage typeMessage;
                if (Client.Id == 0)
                {
                    message += $"\nAttention : Vous n'avez pas utilisé de compte client.\nMerci de bien noter votre numéro de commande.";
                    typeMessage = TypeMessage.Avertissement;
                }
                else
                {
                    message += "\nMerci";
                    typeMessage = TypeMessage.Ok;
                }
                TempData["message"] = new Message(message, typeMessage);
                return RedirectToAction("Index", "Home");
            }
        }

        private void MailCommande(Client client, Commande commande, PanierViewModel panier)
        {
            string mailFoodTruck = ConfigurationManager.AppSettings["MailFoodTruck"];
            string lesArticlesDansLeMail = "";
            foreach (ArticleViewModel article in panier.ArticlesDetailsViewModel)
                lesArticlesDansLeMail += "\n" + article.Quantite + " x " + article.Article.Nom + " = " + (article.Quantite * article.Article.PrixTTC).ToString("C2", new CultureInfo("fr-FR"));

            CultureInfo cultureinfoFr = new CultureInfo("fr-FR");
            string nomClient = client.Nom ?? "non renseigné";
            string prenomClient = client.Prenom ?? "non renseigné";
            string emailClient = client.Email ?? "non@renseigne";
            string corpsDuMailEnCommunClientFoodtruck =
                $"Nom : {nomClient}\n" +
                $"Prénom : {prenomClient}\n" +
                $"Email : {emailClient}\n\n" +
                $"Articles :{lesArticlesDansLeMail}\n" +
                $"Total de la commande : {commande.PrixTotalTTC.ToString("C2", cultureinfoFr)}\n";
            if (commande.RemiseFidelite > 0)
                corpsDuMailEnCommunClientFoodtruck += $"\nRemise fidélité : {commande.RemiseFidelite.ToString("C2", cultureinfoFr)}";
            if (commande.RemiseCommerciale > 0)
                corpsDuMailEnCommunClientFoodtruck += $"\nRemise commerciale : {commande.RemiseCommerciale.ToString("C2", cultureinfoFr)}";

            string sujet = $"Nouvelle commande numéro {commande.Id}";
            string corpsMail = $"Nouvelle commande {commande.Id}. Merci de la préparer pour le {commande.DateRetrait.ToString("dddd dd MMMM HH:mm")}\n" + corpsDuMailEnCommunClientFoodtruck;


            Utilitaire.EnvoieMail(mailFoodTruck, sujet, corpsMail);

            if (client.Id != 0)
            {
                string sujetMailClient = $"Nouvelle commande numéro {commande.Id} prise en compte";
                string corpsMailClient = $"Bonjour {client.Prenom}\n" +
                                         $"Votre dernière commande a bien été prise en compte." +
                                         $"\nVous pourrez venir la chercher le {commande.DateRetrait.ToString("dddd dd MMMM")}" +
                                         $" à partir de {commande.DateRetrait.ToString("HH:mm").Replace(":", "h")}" +
                                         $"\nMerci de votre confiance\n\n" +
                                         "voici le récapitulatif : \n" + corpsDuMailEnCommunClientFoodtruck;
                string objetEvenement = "FoodTruckLyon";
                string descriptionEvenement = $"Chercher commande FoodTruckLyon {commande.Id}\n {corpsDuMailEnCommunClientFoodtruck}";
                string adresseEvenement = "17 Rue des Gones 69007 Lyon";
                DateTime dateDebutEvenement = commande.DateRetrait;
                DateTime dateFinEvenement = commande.DateRetrait.AddMinutes(int.Parse(ConfigurationManager.AppSettings["PasCreneauxHoraire"]));
                double lattitudeEvenement = 45.796386;
                double longitudeEvenement = 5.0379093;
                MemoryStream pieceJointe = Utilitaire.CreerEvenementCalendrier(objetEvenement, descriptionEvenement, adresseEvenement, dateDebutEvenement, dateFinEvenement, lattitudeEvenement, longitudeEvenement);
                Utilitaire.EnvoieMail(emailClient, sujetMailClient, corpsMailClient, pieceJointe);
            }
        }
    }
}
