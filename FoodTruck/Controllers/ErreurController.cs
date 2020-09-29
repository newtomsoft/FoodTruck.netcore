using FoodTruck.ViewModels;
using System;
using System.Web.Mvc;

namespace FoodTruck.Controllers
{
    public class ErreurController : Controller
    {
        public ActionResult Index()
        {
            TempData["message"] = new Message("Une erreur s'est produite pouir accéder à votre demande.\nContactez un administrateur si l'erreur persiste.\nNous vous redirigeons sur la page d'accueil.", TypeMessage.Erreur);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Erreur403()
        {
            TempData["message"] = new Message("Vous n'avez pas les droits d'accès à la page demandée.\nContactez un administrateur si vous pensez qu'il s'agit d'une erreur\nNous vous redirigeons sur la page d'accueil.", TypeMessage.Interdit);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Erreur404()
        {
            TempData["message"] = new Message("La page que vous demandez n'existe pas.\nContactez un administrateur si vous pensez qu'il s'agit d'une erreur.\nNous vous redirigeons sur la page d'accueil.", TypeMessage.Erreur);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Erreur500()
        {
            TempData["message"] = new Message("La ressource demandée par le serveur n'existe pas.\nCela peut venir d'une inactivité trop longue.\nVeuillez réessayer.\nSi le problème persiste, merci de contacter un administrateur.\nNous vous redirigeons sur la page d'accueil.", TypeMessage.Erreur);
            return RedirectToAction("Index", "Home");
        }
    }
}