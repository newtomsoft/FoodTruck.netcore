using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FoodTruck.DAL
{
    public class OubliMotDePasseDAL
    {
        internal OubliMotDePasse Details(string identifiant)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                DateTime maintenant = DateTime.Now;
                var oubliMotDePasse = (from u in db.OubliMotDePasse
                                       where u.CodeVerification == identifiant && DbFunctions.DiffMinutes(maintenant, u.DateFinValidite) >= 0
                                       select u).FirstOrDefault();
                return oubliMotDePasse;
            }
        }
        internal void Ajouter(int clientId, string codeVerification, DateTime dateFinValidite)
        {
            Supprimer(clientId);
            using (foodtruckEntities db = new foodtruckEntities())
            {
                OubliMotDePasse oubliMotDePasse = new OubliMotDePasse
                {
                    ClientId = clientId,
                    CodeVerification = codeVerification,
                    DateFinValidite = dateFinValidite
                };
                db.OubliMotDePasse.Add(oubliMotDePasse);
                db.SaveChanges();
            }
        }

        internal void Supprimer(int clientId)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<OubliMotDePasse> listeOubliMotDePasse =
                    (from u in db.OubliMotDePasse
                     where u.ClientId == clientId
                     select u).ToList();
                db.OubliMotDePasse.RemoveRange(listeOubliMotDePasse);
                db.SaveChanges();
            }
        }

        internal int Verifier(string identifiant)
        {
            OubliMotDePasse oubliMotDePasse = Details(identifiant);
            if (oubliMotDePasse != null)
            {
                Supprimer(oubliMotDePasse.ClientId);
                return oubliMotDePasse.ClientId;
            }
            else
            {
                return 0;
            }
        }
        public int Purger()
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                IQueryable<OubliMotDePasse> listeOubliMotDePasse = from u in db.OubliMotDePasse
                                                                              where DbFunctions.DiffMinutes(u.DateFinValidite, DateTime.Now) > 0
                                                                              select u;

                db.OubliMotDePasse.RemoveRange(listeOubliMotDePasse);
                return db.SaveChanges();
            }
        }
    }
}
