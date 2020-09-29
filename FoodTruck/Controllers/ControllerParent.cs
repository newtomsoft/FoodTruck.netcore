using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.ViewModels;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FoodTruck.Controllers
{
    public class ControllerParent : Controller
    {
        protected string ActionNom { get; set; }
        protected string ControllerNom { get; set; }
        protected Client Client { get; set; }
        protected string ProspectGuid { get; set; }
        protected PanierViewModel PanierViewModel { get; set; }
        protected bool AdminArticle { get; set; }
        protected bool AdminCommande { get; set; }
        protected bool AdminClient { get; set; }
        protected bool AdminPlanning { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ActionNom = RouteData.Values["action"].ToString();
            ControllerNom = RouteData.Values["controller"].ToString();
            MettrelUrlEnSession();

            if (Session["ClientId"] == null || (int)Session["ClientId"] == 0)
            {
                HttpCookie cookie = Request.Cookies.Get("GuidClient");
                if (cookie != null && (Client = new ClientDAL().ConnexionCookies(cookie.Value)) != null)
                {
                    ViewBag.Client = Client;
                    Session["ClientId"] = Client.Id;
                    PanierViewModel = new PanierViewModel(); //Todo effacer
                    AgregerPanierEnBase();
                    RecupererPanierEnBase();
                }
                else
                {
                    Session["ClientId"] = 0;
                    ViewBag.Client = Client = new Client();
                }
            }
            else
                ViewBag.Client = Client = new ClientDAL().Details((int)Session["ClientId"]);

            new PanierDAL(Client.Id).SupprimerArticlesPasDansCarte();
            VisiteDAL.Enregistrer(Client.Id);
            if (Client.Id != 0)
            {
                DonnerLesDroitsdAcces();
                PanierViewModel = new PanierViewModel(new PanierDAL(Client.Id).ListerPanierClient());
            }
            else
            {
                RetirerLesDroitsdAcces();
                if (Session["ProspectGuid"] != null)
                {
                    ProspectGuid = Session["ProspectGuid"].ToString();
                }
                else
                {
                    HttpCookie cookie = Request.Cookies.Get("Prospect");
                    if (cookie != null)
                    {
                        Session["ProspectGuid"] = ProspectGuid = cookie.Value;
                        List<PanierProspect> paniers = new PanierProspectDAL(ProspectGuid).ListerPanierProspect();
                        if (paniers.Count > 0)
                            RecupererPanierEnBase();
                    }
                    else
                    {
                        string guid = Guid.NewGuid().ToString();
                        Session["ProspectGuid"] = ProspectGuid = guid;
                        cookie = new HttpCookie("Prospect")
                        {
                            Value = guid,
                            Expires = DateTime.Now.AddDays(30)
                        };
                        Response.Cookies.Add(cookie);
                    }
                }
                new PanierProspectDAL(ProspectGuid).SupprimerArticlesPasDansCarte();
                PanierViewModel = new PanierViewModel(new PanierProspectDAL(ProspectGuid).ListerPanierProspect());
            }
            PanierViewModel.Trier();
            ViewBag.Panier = PanierViewModel;

            if (Client.AdminArticle || Client.AdminCommande || Client.AdminClient || Client.AdminPlanning)
                ViewBag.MenuAdmin = true;
            if (Client.AdminArticle)
                ViewBag.AdminArticle = true;
        }

        protected void InitialiserSession()
        {
            RetirerLesDroitsdAcces();
            Session["ClientId"] = 0;
            Client = new Client();
            PanierViewModel = new PanierViewModel();
            string guid = Guid.NewGuid().ToString();
            Session["ProspectGuid"] = guid;
            HttpCookie cookie = new HttpCookie("Prospect")
            {
                Value = guid,
                Expires = DateTime.Now.AddDays(30)
            };
            Request.Cookies.Add(cookie);
        }

        private void AgregerPanierEnBase()
        {
            if (Client != null && Client.Id != 0)
            {
                PanierDAL lePanierDal = new PanierDAL(Client.Id);
                foreach (ArticleViewModel article in PanierViewModel.ArticlesDetailsViewModel)
                {
                    Panier panier = lePanierDal.ListerPanierClient().Find(pan => pan.ArticleId == article.Article.Id);
                    if (panier == null)
                        lePanierDal.Ajouter(article.Article, article.Quantite);
                    else
                        lePanierDal.ModifierQuantite(article.Article, article.Quantite);
                }
            }
        }
        protected void RecupererPanierEnBase()
        {
            if (Client.Id != 0)
                PanierViewModel = new PanierViewModel(new PanierDAL(Client.Id).ListerPanierClient());
            else
                PanierViewModel = new PanierViewModel(new PanierProspectDAL(ProspectGuid).ListerPanierProspect());
        }

        private void DonnerLesDroitsdAcces()
        {
            //todo mettre dans Getter ?
            if (Client.AdminArticle) AdminArticle = true;
            if (Client.AdminCommande) AdminCommande = true;
            if (Client.AdminClient) AdminClient = true;
            if (Client.AdminPlanning) AdminPlanning = true;
        }

        private void RetirerLesDroitsdAcces()
        {
            AdminArticle = AdminCommande = AdminClient = AdminPlanning = false;
        }
        private void MettrelUrlEnSession()
        {
            if (Session["Url"] == null)
                Session["Url"] = new List<string>(2);
            if (ControllerNom != "Compte" && Request.HttpMethod == "GET")
            {
                if ((Session["Url"] as List<string>).Count == 2)
                    (Session["Url"] as List<string>).RemoveAt(0);
                (Session["Url"] as List<string>).Add(Request.Url.LocalPath); // = Request.RawUrl
            }
            if ((Session["Url"] as List<string>).Count == 0)
                (Session["Url"] as List<string>).Add("~/");
        }

        protected string UrlCourante()
        {
            int index = ((List<string>)Session["Url"]).Count - 1;
            return ((List<string>)Session["Url"])[index];
        }

        protected string UrlPrecedente()
        {
            return ((List<string>)Session["Url"])[0];
        }
    }
}