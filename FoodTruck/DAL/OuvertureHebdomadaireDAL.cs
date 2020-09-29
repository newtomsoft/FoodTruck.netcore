using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace FoodTruck.DAL
{
    public class OuvertureHebdomadaireDAL
    {
        internal List<OuvertureHebdomadaire> OuverturesHebdomadaires()
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<OuvertureHebdomadaire> ouvertures = (from p in db.OuvertureHebdomadaire
                                                          orderby p.JourSemaineId, p.Debut
                                                          select p).ToList();
                return ouvertures;
            }
        }

        internal OuvertureHebdomadaire AjouterOuverture(int jourId, TimeSpan debut, TimeSpan fin)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                OuvertureHebdomadaire chevauchement = (from p in db.OuvertureHebdomadaire
                                                       where p.JourSemaineId == jourId && DbFunctions.DiffMinutes(p.Debut, fin) > 0 && DbFunctions.DiffMinutes(debut, p.Fin) > 0
                                                       select p).FirstOrDefault();

                if (chevauchement == null)
                {
                    OuvertureHebdomadaire ouverture = new OuvertureHebdomadaire
                    {
                        JourSemaineId = jourId,
                        Debut = debut,
                        Fin = fin,
                    };
                    db.OuvertureHebdomadaire.Add(ouverture);
                    db.SaveChanges();
                }
                return chevauchement;
            }
        }

        internal OuvertureHebdomadaire ModifierOuverture(int id, int jourId, TimeSpan debut, TimeSpan fin)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                OuvertureHebdomadaire ouverture = (from p in db.OuvertureHebdomadaire
                                                   where p.Id == id
                                                   select p).FirstOrDefault();

                OuvertureHebdomadaire chevauchement = (from p in db.OuvertureHebdomadaire
                                                       where p.Id != id && p.JourSemaineId == jourId && DbFunctions.DiffMinutes(p.Debut, fin) > 0 && DbFunctions.DiffMinutes(debut, p.Fin) > 0
                                                       select p).FirstOrDefault();

                if (chevauchement == null && ouverture != null)
                {
                    ouverture.JourSemaineId = jourId;
                    ouverture.Debut = debut;
                    ouverture.Fin = fin;
                    db.SaveChanges();
                }
                return chevauchement;
            }
        }

        internal bool SupprimerOuverture(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                OuvertureHebdomadaire ouverture = (from p in db.OuvertureHebdomadaire
                                                   where p.Id == id
                                                   select p).FirstOrDefault();
                db.OuvertureHebdomadaire.Remove(ouverture);
                if (db.SaveChanges() != 1)
                    return false;
                else
                    return true;
            }
        }
    }
}