using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.Outils;
using FoodTruck.ViewModels;
using System;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck.Areas.Administrer.Controllers
{
    public class ArticleController : ControllerParent
    {
        [HttpGet]
        public ActionResult Ajouter()
        {
            if (AdminArticle)
                return View();
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
        [HttpPost]
        public ActionResult Ajouter(string nom, string description, string prix, int? grammage, int? litrage, string allergenes, int familleId, bool dansCarte, HttpPostedFileBase file)
        {
            if (AdminArticle)
            {
                string nomOk = nom.NomAdmis();
                double prixOk = Math.Abs(Math.Round(float.Parse(prix, CultureInfo.InvariantCulture.NumberFormat), 2));
                int grammageOk = Math.Abs(grammage ?? 0);
                int litrageOk = Math.Abs(litrage ?? 0);
                string descriptionOk = description;
                string allergenesOk = allergenes ?? "";
                int familleIdOk = familleId;
                bool dansCarteOk = dansCarte;
                Article article = new Article
                {
                    Nom = nomOk,
                    Description = descriptionOk,
                    PrixTTC = prixOk,
                    Grammage = grammageOk,
                    Litrage = litrageOk,
                    Allergenes = allergenesOk,
                    FamilleId = familleIdOk,
                    DansCarte = dansCarteOk,
                };
                try
                {
                    string dossierImage = ConfigurationManager.AppSettings["PathImagesArticles"];
                    string fileName = Guid.NewGuid().ToString("n") + Path.GetExtension(file.FileName);
                    string chemin = Path.Combine(Server.MapPath(dossierImage), fileName);
                    Image image = Image.FromStream(file.InputStream);
                    int tailleImage = int.Parse(ConfigurationManager.AppSettings["ImagesArticlesSize"]);
                    int largeur = tailleImage;
                    int hauteur = tailleImage;
                    if (image.Height >= image.Width)
                        hauteur = tailleImage * image.Height / image.Width;
                    else
                        largeur = tailleImage * image.Width / image.Height;
                    Bitmap imageTemp = new Bitmap(image, largeur, hauteur);
                    Rectangle recadrage = new Rectangle((largeur - tailleImage) / 2, (hauteur - tailleImage) / 2, tailleImage, tailleImage);
                    Bitmap nouvelleImage = imageTemp.Clone(recadrage, imageTemp.PixelFormat);
                    imageTemp.Dispose();
                    nouvelleImage.Save(chemin);
                    nouvelleImage.Dispose();
                    image.Dispose();
                    article.Image = fileName;
                    new ArticleDAL().Ajouter(article);
                    TempData["message"] = new Message("Votre article a bien été ajouté", TypeMessage.Ok);
                }
                catch (Exception ex)
                {
                    TempData["message"] = new Message(ex.Message, TypeMessage.Erreur);
                }
                return View();
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        public ActionResult Modifier()
        {
            if (AdminArticle)
                return View(new ArticleDAL().Tous());
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult Modifier(int id)
        {
            if (AdminArticle)
            {
                ArticleDAL articleDAL = new ArticleDAL();
                TempData["articleAModifier"] = articleDAL.Details(id);
                return View(articleDAL.Tous());
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult ModifierEtape2(Article article, string prix, HttpPostedFileBase file)
        {
            if (AdminArticle)
            {
                double prixOk = Math.Abs(Math.Round(float.Parse(prix, CultureInfo.InvariantCulture.NumberFormat), 2));
                article.PrixTTC = prixOk;
                article.Nom = article.Nom.NomAdmis();
                article.Grammage = Math.Abs(article.Grammage);
                article.Litrage = Math.Abs(article.Litrage);
                article.Allergenes = article.Allergenes ?? "";

                ArticleDAL articleDAL = new ArticleDAL();
                if (articleDAL.NomExiste(article.Nom, article.Id))
                {
                    TempData["message"] = new Message("Ce nom d'article existe déjà.\nMerci de choisir un autre nom ou bien de renommer d'abord l'article en doublon.", TypeMessage.Erreur);
                    TempData["articleAModifier"] = articleDAL.Details(article.Id);
                }
                else
                {
                    try
                    {
                        if (file != null)
                        {
                            string dossierImage = ConfigurationManager.AppSettings["PathImagesArticles"];
                            string fileName = Guid.NewGuid().ToString("n") + Path.GetExtension(file.FileName);
                            string chemin = Path.Combine(Server.MapPath(dossierImage), fileName);
                            Image image = Image.FromStream(file.InputStream);
                            int tailleImage = int.Parse(ConfigurationManager.AppSettings["ImagesArticlesSize"]);
                            var nouvelleImage = new Bitmap(image, tailleImage, tailleImage);
                            nouvelleImage.Save(chemin);
                            nouvelleImage.Dispose();
                            image.Dispose();
                            article.Image = fileName;
                        }
                        else
                        {
                            Article ancienArticle = articleDAL.Details(article.Id);
                            article.Image = ancienArticle.Image;
                        }
                        articleDAL.Modifier(article);
                        TempData["message"] = new Message("Votre article a bien été modifié", TypeMessage.Ok);

                    }
                    catch (Exception ex)
                    {
                        TempData["message"] = new Message(ex.Message, TypeMessage.Erreur);
                    }
                }
                return RedirectToAction("Modifier", "Article", new { area = "Administrer" });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}