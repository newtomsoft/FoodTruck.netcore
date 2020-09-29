using FoodTruck.DAL;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace FoodTruck.Areas.Administrer.Controllers
{
    public class DonneeController : ControllerParent
    {
        [HttpGet]
        public ActionResult Purger()
        {
            if (AdminClient)
            {
                ViewBag.PanierProspectSupprimes = $"Nombre d'enregistrements de PanierProspect supprimés :  {new PanierProspectDAL("").Purger(30)}";
                ViewBag.OubliMotDePasseSupprimes = $"Nombre d'enregistrements de OubliMotDePasse supprimés : {new OubliMotDePasseDAL().Purger()}";
                ViewBag.JourExceptionnelSupprimes = $"Nombre d'enregistrements de JourExceptionnel supprimés : {new JourExceptionnelDAL().Purger()}";
                ViewBag.ImagesSupprimees = $"Nombre d'images supprimées : {new ImageDAL().Purger(Server.MapPath(ConfigurationManager.AppSettings["PathImagesArticles"]))}";
            }
            return View();
        }
    }
}