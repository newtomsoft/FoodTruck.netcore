using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FoodTruck.DAL
{
    public class CreerAdminDAL
    {
        internal CreerAdmin Details(string identifiant)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                DateTime maintenant = DateTime.Now;
                var creerAdmin = (from u in db.CreerAdmin
                                       where u.CodeVerification == identifiant && DbFunctions.DiffMinutes(maintenant, u.DateFinValidite) >= 0
                                       select u).FirstOrDefault();
                return creerAdmin;
            }
        }
        internal void Ajouter(Client client, string codeVerification, DateTime dateFinValidite)
        {

            Supprimer(client.Email);
            using (foodtruckEntities db = new foodtruckEntities())
            {
                CreerAdmin creerAdmin = new CreerAdmin
                {
                    Email = client.Email,
                    Nom = client.Nom,
                    Prenom = client.Prenom,
                    CodeVerification = codeVerification,
                    DateFinValidite = dateFinValidite
                };
                db.CreerAdmin.Add(creerAdmin);
                db.SaveChanges();
            }
        }

        internal CreerAdmin Verifier(string identifiant)
        {
            CreerAdmin creerAdmin = Details(identifiant);
            if (creerAdmin != null)
            {
                Supprimer(creerAdmin.Email);
            }
            return creerAdmin;
        }
        internal void Supprimer(string email)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<CreerAdmin> listeAdminTemporaire =
                    (from a in db.CreerAdmin
                     where a.Email == email
                     select a).ToList();
                db.CreerAdmin.RemoveRange(listeAdminTemporaire);
                db.SaveChanges();
            }
        }
    }
}