using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FoodTruck.Controllers
{
    public class ArticleController : ControllerParent
    {
        [HttpGet]
        public ActionResult Index()
        {
            List<ArticleViewModel> articlesVM = new List<ArticleViewModel>();
            if (AdminArticle)
            {
                foreach (Article article in new ArticleDAL().Articles(false))
                {
                    articlesVM.Add(new ArticleViewModel(article));
                }
                return View(articlesVM);
            }
            else
            {
                foreach (Article article in new ArticleDAL().Articles(true))
                {
                    articlesVM.Add(new ArticleViewModel(article));
                }
                return View(articlesVM);
            }
        }

        [HttpGet]
        public ActionResult Details(string nom)
        {
            nom = nom.UrlVersNom();
            ArticleDAL lArticleDAL = new ArticleDAL();
            Article articleCourant;
            articleCourant = lArticleDAL.Details(nom);
            if (articleCourant == null)
            {
                TempData["message"] = new Message("L'article que vous demandez n'existe pas !", TypeMessage.Erreur);
                return RedirectToAction("Index", "Article");
            }
            else if (!articleCourant.DansCarte)
            {
                TempData["message"] = new Message("L'article choisi n'est plus disponible dans votre foodtruck", TypeMessage.Avertissement);
            }
            return View(new ArticleViewModel(articleCourant));
        }
    }
}
