using FoodTruck.Models;
using FoodTruck.Outils;
using System;
using System.Configuration;
using System.Linq;

namespace FoodTruck.DAL
{
    class VisiteDAL
    {
        public static void Enregistrer(int clientId)
        {
            string adresseIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string ipsNonTracees = ConfigurationManager.AppSettings["ListIpDoNotTrack"];
            string[] tabIpsNonTracees = ipsNonTracees.Split(';');
            if (!tabIpsNonTracees.Any(ip => adresseIP == ip))
            {
                string url = System.Web.HttpContext.Current.Request.Url.ToString();
                string navigateur = System.Web.HttpContext.Current.Request.Browser.Browser;
                string UrlOrigine = "";
                if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
                    UrlOrigine = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
                Visite visite = new Visite
                {
                    Url = url,
                    Date = DateTime.Now,
                    AdresseIp = adresseIP,
                    ClientId = clientId,
                    Navigateur = navigateur,
                    UrlOrigine = UrlOrigine,
                    NavigateurMobile = Utilitaire.NavigateurMobile(),
                };
                using (foodtruckEntities db = new foodtruckEntities())
                {
                    db.Visite.Add(visite);
                    db.SaveChanges();
                }
            }
        }
    }
}
