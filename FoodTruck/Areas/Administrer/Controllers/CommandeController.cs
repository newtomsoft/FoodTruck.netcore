using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace FoodTruck.Areas.Administrer.Controllers
{
    public class CommandeController : ControllerParent
    {
        [HttpGet]
        public ActionResult EnCours()
        {
            const int fouchetteHeures = 4;
            if (AdminCommande)
            {
                List<Commande> commandes = new CommandeDAL().CommandesEnCours(fouchetteHeures);
                if (commandes.Count == 0)
                {
                    TempData["message"] = new Message("Vous n'avez aucune commande en cours", TypeMessage.Info);
                }
                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);

            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult EnCours(int id, string statut)
        {
            if (AdminCommande)
            {
                if (statut == "retire")
                    new CommandeDAL().Retirer(id);
                else if (statut == "annule")
                    new CommandeDAL().Annuler(id);
                return RedirectToAction(ActionNom);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        public ActionResult AStatuer()
        {
            if (AdminCommande)
            {
                List<Commande> commandes = new CommandeDAL().CommandesAStatuer();
                if (commandes.Count == 0)
                    TempData["message"] = new Message("Vous n'avez aucune commande à statuer", TypeMessage.Info);
                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        public ActionResult Recherche()
        {
            if (AdminCommande)
            {
                ViewBag.DateDebut = DateTime.Today;
                ViewBag.DateFin = DateTime.Today;
                List<Commande> commandes = new CommandeDAL().CommandesRecherche("", DateTime.Today, DateTime.Today.AddDays(3));
                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult Recherche(string recherche, DateTime? dateDebut, DateTime? dateFin)
        {
            if (AdminCommande)
            {
                ViewBag.Recherche = recherche;
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                string[] tabRecherche = recherche.Split(' ');
                List<Commande>[] tabCommandes = new List<Commande>[tabRecherche.Length];

                for (int i = 0; i < tabRecherche.Length; i++)
                    tabCommandes[i] = new CommandeDAL().CommandesRecherche(tabRecherche[i], dateDebut, dateFin);

                List<Commande> commandes = tabCommandes[0];
                for (int i = 1; i < tabCommandes.Length; i++)
                    commandes = commandes.Intersect(tabCommandes[i], new CommandeEqualityComparer()).ToList();
                if (commandes.Count == 0)
                    TempData["message"] = new Message("Aucune commande ne correspond à votre recherche.\nVeuillez élargir vos critères de recherche", TypeMessage.Avertissement);

                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        public ActionResult Futures()
        {
            if (AdminCommande)
            {
                List<Commande> commandes = new CommandeDAL().CommandesFutures();
                if (commandes.Count == 0)
                    TempData["message"] = new Message("Vous n'avez aucune commande à venir", TypeMessage.Info);
                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        public ActionResult PendantFermetures()
        {
            if (AdminCommande)
            {
                List<Commande> commandes = new CommandeDAL().CommandesPendantFermetures();
                if (TempData["message"] == null)
                {
                    if (commandes.Count == 0)
                        TempData["message"] = new Message("Vous n'avez aucune commande pendant des fermetures", TypeMessage.Ok);
                    else
                        TempData["message"] = new Message($"Il y a {commandes.Count} commande(s) pendant des fermetures.\nVous pouvez les annuler et prévenir les clients automatiquement par mail", TypeMessage.Ok);
                }

                List<CommandeViewModel> listeCommandesViewModel = new List<CommandeViewModel>();
                foreach (Commande commande in commandes)
                {
                    listeCommandesViewModel.Add(new CommandeViewModel(commande));
                }
                return View(listeCommandesViewModel);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult PendantFermetures(int id)
        {
            if (AdminCommande && id == 0)
            {
                CommandeDAL commandeDAL = new CommandeDAL();
                List<Commande> commandes = commandeDAL.CommandesPendantFermetures();
                List<int> commandesIdMailInconnu = new List<int>();
                foreach (Commande commande in commandes)
                {
                    int commandeId = commande.Id;
                    commandeDAL.Annuler(commandeId);
                    int clientId = commande.ClientId;
                    if (clientId != 0)
                    {
                        Client client = new ClientDAL().Details(clientId);
                        string objetMail = $"Problème commande {commandeId} : Fermeture de votre foodtruck";
                        string corpsMessage = $"Bonjour {client.Prenom}\n\n" +
                            $"Vous avez passé la commande numéro {commandeId} pour le {commande.DateRetrait.ToString("dddd dd MMMM yyyy à HH:mm").Replace(":", "h")} et nous vous en remercions.\n\n" +
                            $"Malheureusement nous ne sommes plus ouvert pendant votre horaire de retrait et nous avons été contraint de l'annuler.\n\n" +
                            $"Nous vous invitons à choisir un autre créneau de retrait (vous pouvez dupliquer votre commande annulée dans votre espace client).\n\n" +
                            $"Nous vous prions de nous excuser pour la gène occasionnée.\n\n" +
                            $"Bien cordialement\n" +
                            $"Votre équipe Foodtrucklyon";
                        string adresseMailClient = client.Email;
                        Utilitaire.EnvoieMail(adresseMailClient, objetMail, corpsMessage);
                    }
                    else
                    {
                        commandesIdMailInconnu.Add(commandeId);
                    }
                }
                string message = $"Les {commandes.Count} commande(s) ont bien été annulées et les clients ont reçu un mail";
                if (commandesIdMailInconnu.Count != 0)
                {
                    message += "\nAttention :\nLes clients des commandes qui suivent n'ont pas pu être prévenus car ils n'ont pas rensigné les adresses mail :\n";
                    foreach (int commandeId in commandesIdMailInconnu)
                    {
                        message += $"-{commandeId}-  ";
                    }
                }
                TempData["message"] = new Message(message, TypeMessage.Ok);
                return RedirectToAction(ActionNom);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}