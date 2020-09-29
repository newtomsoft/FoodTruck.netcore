using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck.Controllers
{
    public class CompteController : ControllerParent
    {
        public CompteController()
        {
            ViewBag.PanierLatteralDesactive = true;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Client.Id == 0)
                return RedirectToAction("Connexion", "Compte");
            else
                return RedirectToAction("Profil");
        }

        [HttpGet]
        public ActionResult Profil()
        {
            ViewBag.RemiseTotalClient = new CommandeDAL().RemiseTotaleClient(Client.Id);
            return View(Client);
        }

        [HttpPost]
        public ActionResult Profil(string ancienEmail, string login, string email, string ancienMdp, string nom, string prenom, string telephone, string mdp, string mdp2)
        {
            ViewBag.Login = login;
            ViewBag.Email = email;
            ViewBag.Nom = nom;
            ViewBag.Prenom = prenom;
            ViewBag.Telephone = telephone;
            ViewBag.Mdp = mdp;
            ViewBag.AncienMdp = ancienMdp;

            StringBuilder messageFabrique = new StringBuilder();

            ClientDAL clientDAL = new ClientDAL();
            Client client = clientDAL.Connexion(ancienEmail, ancienMdp);
            bool erreur = false;
            if (client == null)
            {
                messageFabrique.Append("L'ancien mot de passe n'est pas correct.");
                erreur = true;
                ViewBag.AncienMdp = "";
            }
            else
            {
                int verifClientId = clientDAL.ExisteLogin(login);
                if (verifClientId !=0 && verifClientId != Client.Id)
                {
                    ViewBag.Login = "";
                    messageFabrique.Append("Le nouveau nom d'utilisateur est déjà utilisé.\n");
                    erreur = true;
                }
                verifClientId = clientDAL.ExisteEmail(email);
                if (verifClientId != 0 && verifClientId != Client.Id)
                {
                    ViewBag.Email = "";
                    messageFabrique.Append("Le nouvel Email est déjà utilisé.\n");
                    erreur = true;
                }
                string nouveauMdp = "";
                if (mdp == "" && mdp2 == "")
                {
                    nouveauMdp = ancienMdp;
                }
                else if (VerifMdp(mdp, mdp2))
                {
                    nouveauMdp = mdp;
                }
                else
                {
                    ViewBag.Mdp = "";
                    messageFabrique.Append("Mauvais choix de mots de passe. (minimum 8 caractères et identiques)");
                    erreur = true;
                }
                if (!erreur)
                {
                    if (clientDAL.Modification(client.Id, nouveauMdp, login, email, nom, prenom, telephone) == 1)
                    {
                        messageFabrique.Append("La modification du profil a bien été prise en compte.");
                        Client = clientDAL.Connexion(email, nouveauMdp);
                    }
                    else
                    {
                        messageFabrique.Append("Une erreur s'est produite lors de la mise à jour de vos données");
                        erreur = true;
                    }
                }
            }
            string message = messageFabrique.ToString();
            if (erreur)
                TempData["message"] = new Message(message, TypeMessage.Erreur);
            else
                TempData["message"] = new Message(message, TypeMessage.Ok);

            CommandeDAL commandeDAL = new CommandeDAL();
            ViewBag.RemiseTotalClient = commandeDAL.RemiseTotaleClient(Client.Id); //todo vérifier si code utile
            return View(Client);
        }

        [HttpGet]
        public ActionResult Commandes()
        {
            if (Client.Id != 0)
                return View();
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult AnnulerCommande(int commandeId)
        {
            CommandeDAL commandeDAL = new CommandeDAL();
            Commande commande = commandeDAL.Detail(commandeId);
            if (commande != null && commande.ClientId == Client.Id)
            {
                commandeDAL.Annuler(commandeId);
            }
            return RedirectToAction("Commandes", "Compte");
        }

        [HttpGet]
        public ActionResult Connexion()
        {
            if (Client.Id == 0)
                return View();
            else
                return RedirectToAction("Profil");
        }

        [HttpPost]
        public ActionResult Connexion(string loginEmail, string mdp, bool connexionAuto)
        {
            Client = new ClientDAL().Connexion(loginEmail, mdp);
            if (Client != null)
            {
                ViewBag.Client = Client;
                Session["ClientId"] = Client.Id;
                if (connexionAuto)
                    ConnexionAutomatique();

                RecupererPanierProspectPuisSupprimer();
                SupprimerCookieProspect();
                string message = $"Bienvenue {Client.Prenom} {Client.Nom}\nVous avez {Client.Cagnotte} € sur votre cagnotte fidélité\nDepuis votre inscription du {Client.Inscription.ToString("dd MMMM yyyy")}, vous avez eu {new CommandeDAL().RemiseTotaleClient(Client.Id).ToString("C2", new CultureInfo("fr-FR"))} de remises sur vos commandes";
                TempData["message"] = new Message(message, TypeMessage.Ok);
                return Redirect(UrlCourante());
            }
            else
            {
                ViewBag.Client = new Client();
                TempData["message"] = new Message("Email ou mot de passe incorrect.\nVeuillez réessayer.", TypeMessage.Erreur);
                return View();
            }
        }

        [HttpGet]
        public ActionResult Deconnexion()
        {
            if (Client.Id != 0)
            {
                HttpCookie newCookie = new HttpCookie("GuidClient")
                {
                    Expires = DateTime.Now.AddDays(-30)
                };
                Response.Cookies.Add(newCookie);
                InitialiserSession();
                ViewBag.Panier = null; // todo
            }
            TempData["message"] = new Message("Vous êtes maintenant déconnecté.\nMerci de votre visite.\nA bientôt.", TypeMessage.Info);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Creation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Creation(string email, string login, string mdp, string mdp2, string nom, string prenom, string telephone, bool connexionAuto)
        {
            ViewBag.Nom = nom;
            ViewBag.Prenom = prenom;
            ViewBag.Email = email;
            ViewBag.Login = login;
            ViewBag.Telephone = telephone;
            ViewBag.Mdp = mdp;
            ViewBag.Mdp2 = mdp2;

            Client client = Client;
            if (Client.Id == 0)
            {
                StringBuilder messageConstruction = new StringBuilder();
                bool erreur = false;
                ClientDAL clientDAL = new ClientDAL();
                if (clientDAL.ExisteEmail(email) != 0)
                {
                    erreur = true;
                    ViewBag.ErreurEmail = true;
                    messageConstruction.Append("- Un compte existe déjà avec cet Email.\n");
                }
                if (clientDAL.ExisteLogin(login) != 0)
                {
                    erreur = true;
                    ViewBag.ErreurLogin = true;
                    messageConstruction.Append("- Un compte existe déjà avec ce nom d'utilisateur.\n");
                }
                if (!VerifMdp(mdp, mdp2))
                {
                    erreur = true;
                    ViewBag.ErreurMdp = true;
                    ViewBag.Mdp = "";
                    ViewBag.Mdp2 = "";
                    messageConstruction.Append("- Mauvais choix de mots de passe. (minimum 8 caractères et identiques)");
                }
                if (erreur)
                {
                    TempData["message"] = new Message(messageConstruction.ToString(), TypeMessage.Erreur);
                    return View();
                }

                client = new ClientDAL().Creation(email, login, mdp, nom, prenom, telephone);
            }

            if (client != null)
            {
                Connexion(email, mdp, connexionAuto);
                return RedirectToAction("Profil");
            }
            else
            {
                ViewBag.Client = new Client();
                TempData["message"] = new Message("Compte déjà existant.\nVeuillez saisir une autre adresse mail ou vous <a href=\"/Compte/Connexion\">connecter</a>", TypeMessage.Erreur);
                return View();
            }
        }

        [HttpGet]
        public ActionResult OubliMotDePasse(string codeVerification)
        {
            ClientDAL clientDAL = new ClientDAL();
            int clientId = new OubliMotDePasseDAL().Verifier(codeVerification);
            if (clientId != 0)
            {
                Client = clientDAL.Details(clientId);
                ViewBag.Client = Client;
                Session["ClientId"] = Client.Id;
                RecupererPanierProspectPuisSupprimer();
                SupprimerCookieProspect();
                return View();
            }
            else
            {
                TempData["message"] = new Message("Le lien de redéfinition du mot de passe n'est plus valide.\nVeuillez refaire une demande", TypeMessage.Interdit);
                return RedirectToAction("Connexion", "Compte");
            }
        }

        [HttpPost]
        public ActionResult OubliMotDePasse(string action, string email, string mdp, string mdp2)
        {
            if (action == "generationMail")
            {
                int dureeValidite = int.Parse(ConfigurationManager.AppSettings["DureeValiditeLienReinitialisationMotDePasse"]);
                string codeVerification = Guid.NewGuid().ToString("n") + email.GetHash();
                string url = HttpContext.Request.Url.ToString() + '/' + codeVerification;
                Client client = new ClientDAL().Details(email);
                if (client != null)
                {
                    DateTime finValidite = DateTime.Now.AddMinutes(dureeValidite);
                    string stringFinValidite = finValidite.ToString("dddd dd MMMM yyyy à HH:mm").Replace(":", "h");
                    new OubliMotDePasseDAL().Ajouter(client.Id, codeVerification, DateTime.Now.AddMinutes(dureeValidite));
                    string sujetMail = "Procédure de réinitialisation de votre mot de passe";
                    string message = "Bonjour\n" +
                        "Vous avez oublié votre mot de passe et avez demandé à le réinitialiser.\n" +
                        "Si vous êtes bien à l'origine de cette demande, veuillez cliquer sur le lien suivant ou recopier l'adresse dans votre navigateur :\n" +
                        "\n" +
                        url +
                        "\n\nVous serez alors redirigé vers une page de réinitialisation de votre mot de passe.\n" +
                        $"Attention, ce lien expirera le {stringFinValidite} et n'est valable qu'une seule fois";

                    if (Utilitaire.EnvoieMail(email, sujetMail, message))
                        TempData["message"] = new Message($"Un email de réinitialisation de votre mot de passe vient de vous être envoyé.\nIl expirera dans {dureeValidite} minutes.", TypeMessage.Info);
                    else
                        TempData["message"] = new Message("Erreur dans l'envoi du mail.\nVeuillez réessayer dans quelques instants", TypeMessage.Erreur);
                }
                else
                    TempData["message"] = new Message("Nous n'avons pas de compte client avec cette adresse email.\nMerci de vérifier votre saisie", TypeMessage.Erreur);

                return RedirectToAction("Connexion", "Compte");
            }
            else if (action == "changementMotDePasse")
            {
                if (VerifMdp(mdp, mdp2))
                {
                    ClientDAL clientDAL = new ClientDAL();
                    if (clientDAL.Modification(Client.Id, mdp) == 1)
                    {
                        TempData["message"] = new Message("La modification de votre mot de passe a bien été prise en compte", TypeMessage.Ok);
                    }
                }
                else
                {
                    TempData["message"] = new Message("Mauvais choix de mots de passe.\nVeuillez réessayer (minimum 8 caractères et identiques)", TypeMessage.Erreur);
                    return View();
                }

                CommandeDAL commandeDAL = new CommandeDAL();
                ViewBag.RemiseTotalClient = commandeDAL.RemiseTotaleClient(Client.Id);
                return RedirectToAction("Connexion", "Compte");
            }
            else
                return RedirectToAction("Connexion", "Compte");
        }

        [HttpGet]
        public ActionResult ObtenirDroitsAdmin(string codeVerification)
        {
            ClientDAL clientDAL = new ClientDAL();
            CreerAdmin creerAdmin = new CreerAdminDAL().Verifier(codeVerification);
            if (creerAdmin != null)
            {
                Client = clientDAL.Details(creerAdmin.Email);
                string mdp = "";
                if (Client == null)
                {
                    mdp = Utilitaire.StringAleatoire(12);
                    string telephone = "";
                    Client = clientDAL.Creation(creerAdmin.Email, creerAdmin.Email, mdp, creerAdmin.Nom, creerAdmin.Prenom, telephone);
                }
                clientDAL.DonnerDroitAdmin(Client.Id);
                {
                    string mailFoodTruck = ConfigurationManager.AppSettings["MailFoodTruck"];
                    string objet = $"{Client.Prenom.Trim()} {Client.Nom.Trim()} a abtenu les droit admin";
                    string messageMail = $"le client {Client.Prenom.Trim()} {Client.Nom.Trim()} a obtenu les droits admin";
                    Utilitaire.EnvoieMail(mailFoodTruck, objet, messageMail);
                }
                ViewBag.Client = Client;
                Session["ClientId"] = Client.Id;
                RecupererPanierProspectPuisSupprimer();
                SupprimerCookieProspect();
                ConnexionAutomatique();
                string message = "Félicitation ! Vous êtes maintenant administrateur du site.\nVous pouvez accéder au menu Administration";
                if (mdp != "")
                    message += $"\nVeuillez noter votre mot de passe ou bien le changer : {mdp}";
                TempData["message"] = new Message(message, TypeMessage.Info);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["message"] = new Message("Le lien d'obtention des droits d'administration n'est plus valable.\nMerci de refaire une demande", TypeMessage.Interdit);
                return RedirectToAction("Index", "Home");
            }
        }

        private bool VerifMdp(string mdp1, string mdp2)
        {
            if (mdp1 != mdp2 || mdp1.Length < 8)
                return false;
            else
                return true;
        }

        private void SupprimerCookieProspect()
        {
            HttpCookie cookie = new HttpCookie("Prospect")
            {
                Expires = DateTime.Now.AddDays(-30)
            };
            Response.Cookies.Add(cookie);
        }

        private void RecupererPanierProspectPuisSupprimer()
        {
            PanierProspectDAL panierProspectDAL = new PanierProspectDAL(ProspectGuid);
            PanierViewModel panierViewModelSauv = new PanierViewModel(panierProspectDAL.ListerPanierProspect());
            if (panierViewModelSauv != null && Client.Id != 0)
            {
                PanierDAL panierDal = new PanierDAL(Client.Id);
                foreach (ArticleViewModel article in (panierViewModelSauv).ArticlesDetailsViewModel)
                {
                    Panier panier = panierDal.ListerPanierClient().Find(pan => pan.ArticleId == article.Article.Id);
                    if (panier == null)
                        panierDal.Ajouter(article.Article, article.Quantite);
                    else
                        panierDal.ModifierQuantite(article.Article, article.Quantite);
                }
            }
            panierProspectDAL.Supprimer();
        }

        private void ConnexionAutomatique()
        {
            HttpCookie cookie = new HttpCookie("GuidClient")
            {
                Value = Client.Guid,
                Expires = DateTime.Now.AddDays(30)
            };
            Response.Cookies.Add(cookie);
        }
    }
}
