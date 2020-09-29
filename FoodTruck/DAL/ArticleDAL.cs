using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoodTruck.DAL
{
    public class ArticleDAL
    {
        internal Article Details(int id)
        {
            Article lArticle;
            using (foodtruckEntities db = new foodtruckEntities())
            {
                lArticle = (from article in db.Article
                            where article.Id == id
                            select article).FirstOrDefault();
            }
            return lArticle;
        }
        internal Article Details(string nom)
        {
            Article lArticle;
            using (foodtruckEntities db = new foodtruckEntities())
            {
                lArticle = (from article in db.Article
                            where article.Nom == nom
                            select article).FirstOrDefault();
            }
            return lArticle;
        }
        /// <summary>
        /// retourne la liste des familles d'articles. Trié par Id
        /// </summary>
        /// <returns></returns>
        public List<FamilleArticle> FamillesArticle()
        {
            List<FamilleArticle> famillesArticle;
            using (foodtruckEntities db = new foodtruckEntities())
            {
                famillesArticle = (from fa in db.FamilleArticle
                                   orderby fa.Id
                                   select fa).ToList();
            }
            return famillesArticle;
        }

        internal void AugmenterQuantiteVendue(int id, int nbre)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Article article = (from a in db.Article
                                   where a.Id == id
                                   select a).FirstOrDefault();
                article.NombreVendus += nbre;
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Ajoute l'article lArticle en base
        /// </summary>
        /// <param name="article"></param>
        internal void Ajouter(Article article)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                db.Article.Add(article);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string messageErreur = DALExceptions.HandleException(ex);
                    throw new Exception(messageErreur);
                }
            }
        }

        public List<Article> Random(int nombreRetour, int nombreTop)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<Article> articles = (from article in db.Article
                                          where article.DansCarte == true && article.FamilleId <= 3
                                          orderby article.NombreVendus descending
                                          select article)
                                          .Take(nombreTop)
                                          .OrderBy(random => Guid.NewGuid())
                                          .Take(nombreRetour)
                                          .ToList();
                return articles;
            }
        }

        public List<Article> Articles(bool dansCarteSeulement)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<Article> articles = (from article in db.Article
                                          where article.DansCarte || article.DansCarte == dansCarteSeulement
                                          orderby article.FamilleId, article.DansCarte descending, article.Nom
                                          select article)
                                          .ToList();
                return articles;
            }
        }

        internal bool NomExiste(string nom, int id = 0)
        {
            Article article;
            using (foodtruckEntities db = new foodtruckEntities())
            {
                article = (from a in db.Article
                           where a.Nom == nom && a.Id != id
                           select a).FirstOrDefault();
            }
            return article != null ? true : false;
        }

        public List<Article> Tous(int nombreMax = 1000)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<Article> articles = (from article in db.Article
                                          orderby article.FamilleId, article.Nom
                                          select article)
                                          .Take(nombreMax)
                                          .ToList();
                return articles;
            }
        }

        internal void Modifier(Article article)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Article articleAModifier = (from art in db.Article
                                            where art.Id == article.Id
                                            select art).FirstOrDefault();

                articleAModifier.Nom = article.Nom;
                articleAModifier.Description = article.Description;
                articleAModifier.PrixTTC = article.PrixTTC;
                articleAModifier.Allergenes = article.Allergenes;
                articleAModifier.DansCarte = article.DansCarte;
                articleAModifier.FamilleId = article.FamilleId;
                articleAModifier.Grammage = article.Grammage;
                articleAModifier.Litrage = article.Litrage;
                articleAModifier.Image = article.Image;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string messageErreur = DALExceptions.HandleException(ex);
                    throw new Exception(messageErreur);
                }
            }
        }
    }
}
