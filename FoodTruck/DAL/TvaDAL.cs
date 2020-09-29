using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodTruck.DAL
{
    public class TvaDAL
    {
        public Tva Details(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Tva tva = (from t in db.Tva
                           where t.Id == id
                           select t).FirstOrDefault();

                return tva;
            }
        }

        public float Taux(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                double taux = (from t in db.Tva
                               where t.Id == id
                               select t.Taux).FirstOrDefault();

                return (float)Math.Round(taux, 1);
            }
        }

        public float TauxFamilleArticle(int familleArticleId)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                var taux = (from t in db.Tva
                            join f in db.FamilleArticle on t.Id equals f.TvaId
                            where f.Id == familleArticleId
                            select t.Taux).FirstOrDefault();

                return (float)Math.Round(taux, 1);
            }
        }

        public float TauxArticle(int ArticleId)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                var taux = (from t in db.Tva
                            join f in db.FamilleArticle on t.Id equals f.TvaId
                            join a in db.Article on f.Id equals a.FamilleId
                            where a.Id == ArticleId
                            select t.Taux).FirstOrDefault();

                return (float)Math.Round(taux, 1);
            }
        }
    }
}