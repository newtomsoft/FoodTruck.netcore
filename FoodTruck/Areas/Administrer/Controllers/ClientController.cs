using System.Web.Mvc;
using System.Net;
using System.Configuration;
using FoodTruck.Outils;
using System;
using FoodTruck.DAL;
using FoodTruck.ViewModels;
using System.Collections.Generic;
using System.Linq;
using FoodTruck.Models;

namespace FoodTruck.Areas.Administrer.Controllers
{
    public class ClientController : ControllerParent
    {
        [HttpGet]
        public ActionResult Recherche()
        {
            if (AdminClient)
                return View(null as List<Client>);
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult Recherche(string recherche)
        {
            if (AdminClient)
            {
                ViewBag.Recherche = recherche;

                string[] tabRecherche = recherche.Split(' ');
                List<Client>[] tabClients = new List<Client>[tabRecherche.Length];

                for (int i = 0; i < tabRecherche.Length; i++)
                    tabClients[i] = new ClientDAL().Recherche(tabRecherche[i]);

                List<Client> clients = tabClients[0];
                for (int i = 1; i < tabClients.Length; i++)
                    clients = clients.Intersect(tabClients[i], new ClientEqualityComparer()).ToList();
                if (clients.Count == 0)
                    TempData["message"] = new Message("Aucun client ne correspond à votre recherche.\nVeuillez élargir vos critères de recherche", TypeMessage.Avertissement);
                return View(clients);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        public ActionResult DonnerDroitsAdmin()
        {
            if (AdminClient)
                return View();
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult DonnerDroitsAdmin(string email, string nom, string prenom)
        {
            if (AdminClient)
            {
                int dureeValidite = int.Parse(ConfigurationManager.AppSettings["DureeValiditeLienDroitsAdmin"]);
                string codeVerification = Guid.NewGuid().ToString("n");
                string url = $"{Request.Url.Scheme}://{Request.Url.Authority}/Compte/ObtenirDroitsAdmin/{codeVerification}";
                DateTime finValidite = DateTime.Now.AddMinutes(dureeValidite);
                string stringFinValidite = finValidite.ToString("dddd dd MMMM yyyy à HH:mm").Replace(":", "h");

                Client client = new Client
                {
                    Email = email,
                    Nom = nom,
                    Prenom = prenom,
                };
                new CreerAdminDAL().Ajouter(client, codeVerification, DateTime.Now.AddMinutes(dureeValidite));
                string sujetMail = "Vous avez les droits d'administration";
                string message = $"Bonjour {prenom} {nom}\n\n" +
                    $"{Client.Prenom} {Client.Nom}, de FoodTruckLyon vous a donné les droits d'accès administrateur au site.\n" +
                    "Veuillez cliquer sur le lien suivant ou recopier l'adresse dans votre navigateur afin de valider l'action:\n" +
                    "\n" +
                    url +
                    "\n\n" +
                    $"Attention, ce lien expirera le {stringFinValidite} et n'est valable qu'une seule fois\nvotre compte gardera l'accès administrateur sauf si un super-administrateur en décide autrement.";
                if (Utilitaire.EnvoieMail(email, sujetMail, message))
                    TempData["message"] = new Message($"Un email de confirmation vient d'être envoyé à l'adresse {email}.\nIl expirera le {stringFinValidite}", TypeMessage.Info);
                else
                    TempData["message"] = new Message("Erreur dans l'envoi du mail.\nVeuillez rééssayer plus tard", TypeMessage.Erreur);

                return View();
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}