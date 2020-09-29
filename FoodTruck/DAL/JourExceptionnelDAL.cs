using FoodTruck.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace FoodTruck.DAL
{
    public class JourExceptionnelDAL
    {
        internal List<JourExceptionnel> FutursFermeturesExceptionnelles()
        {
            return FutursPeriodesExceptionnelles(false);
        }
        internal List<JourExceptionnel> FutursOuverturesExceptionnelles()
        {
            return FutursPeriodesExceptionnelles(true);
        }
        private List<JourExceptionnel> FutursPeriodesExceptionnelles(bool ouvert)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                DateTime date = DateTime.Now;
                List<JourExceptionnel> jours = (from j in db.JourExceptionnel
                                                where DbFunctions.DiffMinutes(date, j.DateFin) >= 0 && j.Ouvert == ouvert
                                                orderby j.DateDebut
                                                select j).ToList();
                return jours;
            }
        }

        internal JourExceptionnel AjouterFermeture(DateTime dateDebut, DateTime dateFin)
        {
            return AjouterPeriodeExceptionnelle(dateDebut, dateFin, false);
        }
        internal JourExceptionnel AjouterOuverture(DateTime dateDebut, DateTime dateFin)
        {
            return AjouterPeriodeExceptionnelle(dateDebut, dateFin, true);
        }
        private JourExceptionnel AjouterPeriodeExceptionnelle(DateTime dateDebut, DateTime dateFin, bool ouvert)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                JourExceptionnel chevauchement = (from j in db.JourExceptionnel
                                                  where DbFunctions.DiffMinutes(j.DateDebut, dateFin) >= 0 && DbFunctions.DiffMinutes(dateDebut, j.DateFin) >= 0
                                                  select j).FirstOrDefault();
                if (chevauchement == null)
                {
                    JourExceptionnel jour = new JourExceptionnel
                    {
                        DateDebut = dateDebut,
                        DateFin = dateFin,
                        Ouvert = ouvert,
                    };
                    db.JourExceptionnel.Add(jour);
                    db.SaveChanges();
                }
                return chevauchement;
            }
        }
        internal JourExceptionnel ModifierFermeture(DateTime dateId, DateTime dateDebut, DateTime dateFin)
        {
            return ModifierPeriodeExceptionnelle(dateId, dateDebut, dateFin, false);
        }
        internal JourExceptionnel ModifierOuverture(DateTime dateId, DateTime dateDebut, DateTime dateFin)
        {
            return ModifierPeriodeExceptionnelle(dateId, dateDebut, dateFin, true);
        }
        private JourExceptionnel ModifierPeriodeExceptionnelle(DateTime dateId, DateTime dateDebut, DateTime dateFin, bool ouvert)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                JourExceptionnel jourSelectionne = (from j in db.JourExceptionnel
                                                    where j.DateDebut == dateId && j.Ouvert == ouvert
                                                    select j).FirstOrDefault();

                JourExceptionnel chevauchement = (from j in db.JourExceptionnel
                                                  where j.DateDebut != jourSelectionne.DateDebut && DbFunctions.DiffMinutes(j.DateDebut, dateFin) > 0 && DbFunctions.DiffMinutes(dateDebut, j.DateFin) > 0
                                                  select j).FirstOrDefault();

                if (chevauchement == null && jourSelectionne != null)
                {
                    jourSelectionne.DateDebut = dateDebut;
                    jourSelectionne.DateFin = dateFin;
                    jourSelectionne.Ouvert = ouvert;
                    db.SaveChanges();
                }
                return chevauchement;
            }
        }

        internal bool SupprimerFermeture(DateTime dateId)
        {
            return SupprimerPeriodeExceptionnelle(dateId, false);
        }
        internal bool SupprimerOuverture(DateTime dateId)
        {
            return SupprimerPeriodeExceptionnelle(dateId, true);
        }
        private bool SupprimerPeriodeExceptionnelle(DateTime dateId, bool ouvert)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                JourExceptionnel jourSelectionne = (from j in db.JourExceptionnel
                                                    where j.DateDebut == dateId && j.Ouvert == ouvert
                                                    select j).FirstOrDefault();

                db.JourExceptionnel.Remove(jourSelectionne);
                if (db.SaveChanges() != 1)
                    return false;
                else
                    return true;
            }
        }

        private JourExceptionnel ProchaineOuvertureExceptionnelle(DateTime date)
        {
            return ProchainePeriodeExceptionnelle(date, true);
        }
        private JourExceptionnel ProchaineFermetureExceptionnelle(DateTime date)
        {
            return ProchainePeriodeExceptionnelle(date, false);
        }
        private JourExceptionnel ProchainePeriodeExceptionnelle(DateTime date, bool ouvert)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                JourExceptionnel jour = (from j in db.JourExceptionnel
                                         where DbFunctions.DiffSeconds(date, j.DateFin) > 0 && j.Ouvert == ouvert
                                         orderby j.DateDebut
                                         select j).FirstOrDefault();
                if (jour == null)
                {
                    jour = new JourExceptionnel
                    {
                        DateDebut = DateTime.MaxValue,
                        DateFin = DateTime.MaxValue,
                        Ouvert = ouvert
                    };
                }
                else if (jour.DateDebut < DateTime.Now)
                {
                    jour.DateDebut = jour.DateDebut.Add(RecalculHeureDebut(date));
                }
                return jour;
            }
        }

        internal PlageHoraireRetrait ProchainOuvert(DateTime date)
        {
            date = date.AddMinutes(int.Parse(ConfigurationManager.AppSettings["DelaiMinimumAvantRetraitCommande"]));
            bool faireRecherche;
            PlageHoraireRetrait plageHoraireRetrait;
            do
            {
                faireRecherche = false;
                OuvertureHebdomadaire prochainOuvertHabituellement = ProchainOuvertHabituellement(date);
                JourExceptionnel prochainOuvertExceptionnellement = ProchaineOuvertureExceptionnelle(date);
                JourExceptionnel prochainFermeExceptionnellement = ProchaineFermetureExceptionnelle(date);

                DateTime dateMinuit;
                int jourOuvertHabituellement = prochainOuvertHabituellement.JourSemaineId;
                int jourJ = (int)date.DayOfWeek;
                if (jourOuvertHabituellement < jourJ)
                {
                    dateMinuit = date.AddDays(7 - jourJ + jourOuvertHabituellement).Date;
                }
                else
                {
                    dateMinuit = date.AddDays(jourOuvertHabituellement - jourJ).Date;
                }
                if (date.Date == dateMinuit && prochainOuvertHabituellement.Debut < date.TimeOfDay)
                    prochainOuvertHabituellement.Debut = RecalculHeureDebut(date);

                plageHoraireRetrait = new PlageHoraireRetrait(dateMinuit + prochainOuvertHabituellement.Debut, dateMinuit + prochainOuvertHabituellement.Fin);

                // Test avec ouverture exceptionnelle
                //
                //cas plage ouverture exceptionnelle complètement avant plage ouverture habituelle
                if (prochainOuvertExceptionnellement.DateDebut < plageHoraireRetrait.Dates.First() && prochainOuvertExceptionnellement.DateFin < plageHoraireRetrait.Dates.First())
                {
                    plageHoraireRetrait = new PlageHoraireRetrait(prochainOuvertExceptionnellement.DateDebut, prochainOuvertExceptionnellement.DateFin);
                }
                // cas ouverture exceptionnelle commence avant plage
                else if (prochainOuvertExceptionnellement.DateDebut < plageHoraireRetrait.Dates.First())
                {
                    DateTime fin = prochainOuvertExceptionnellement.DateFin > plageHoraireRetrait.Dates.Last() ? prochainOuvertExceptionnellement.DateFin : plageHoraireRetrait.Dates.Last();
                    plageHoraireRetrait = new PlageHoraireRetrait(prochainOuvertExceptionnellement.DateDebut, fin);
                }
                // cas ouverture exceptionnelle fini après plage
                else if (prochainOuvertExceptionnellement.DateDebut < plageHoraireRetrait.Dates.Last() && prochainOuvertExceptionnellement.DateFin > plageHoraireRetrait.Dates.Last())
                {
                    DateTime debut = prochainOuvertExceptionnellement.DateDebut < plageHoraireRetrait.Dates.First() ? prochainOuvertExceptionnellement.DateDebut : plageHoraireRetrait.Dates.First();
                    plageHoraireRetrait = new PlageHoraireRetrait(debut, prochainOuvertExceptionnellement.DateFin);
                }

                //Test avec fermeture exceptionnelle
                //
                // fermeture englobe complètement l'ouverture => recherche à nouveau des plages
                if (prochainFermeExceptionnellement.DateDebut <= plageHoraireRetrait.Dates.First() && prochainFermeExceptionnellement.DateFin >= plageHoraireRetrait.Dates.Last())
                {
                    faireRecherche = true;
                    date = prochainFermeExceptionnellement.DateFin;
                }
                // fermeture à cheval sur ouverture
                else if (prochainFermeExceptionnellement.DateDebut <= plageHoraireRetrait.Dates.Last() && prochainFermeExceptionnellement.DateFin >= plageHoraireRetrait.Dates.First())
                {
                    DateTime debut;
                    DateTime fin;
                    if (prochainFermeExceptionnellement.DateFin < plageHoraireRetrait.Dates.Last())
                    {
                        debut = prochainFermeExceptionnellement.DateFin;
                        fin = plageHoraireRetrait.Dates.Last();
                    }
                    else
                    {
                        debut = plageHoraireRetrait.Dates.First();
                        fin = prochainFermeExceptionnellement.DateDebut;
                    }
                    plageHoraireRetrait = new PlageHoraireRetrait(debut, fin);
                }
            } while (faireRecherche);
            return plageHoraireRetrait;
        }

        private OuvertureHebdomadaire ProchainOuvertHabituellement(DateTime date)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                TimeSpan minuit = new TimeSpan(0, 0, 0);
                int totalSecondesDate = 24 * 60 * 60 * (int)date.DayOfWeek + (int)date.TimeOfDay.TotalSeconds;

                OuvertureHebdomadaire plage = (from ouverture in db.OuvertureHebdomadaire
                                               let totalSecondesOuverture = 24 * 60 * 60 * ouverture.JourSemaineId + DbFunctions.DiffSeconds(minuit, ouverture.Fin)
                                               where totalSecondesOuverture > totalSecondesDate
                                               orderby totalSecondesOuverture
                                               select ouverture).FirstOrDefault();

                DateTime maintenant = DateTime.Now;
                if (plage == null)
                {
                    plage = (from c in db.OuvertureHebdomadaire
                             orderby c.JourSemaineId, c.Debut
                             select c).First();
                }
                else if (date.Date == maintenant.Date && plage.JourSemaineId == (int)maintenant.DayOfWeek && plage.Debut < maintenant.TimeOfDay)
                {
                    plage.Debut = RecalculHeureDebut(date);
                }
                return plage;
            }
        }

        public int Purger()
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                IQueryable<JourExceptionnel> joursExceptionnels = from j in db.JourExceptionnel
                                                                  where DbFunctions.DiffMinutes(j.DateFin, DateTime.Now) > 0
                                                                  select j;
                db.JourExceptionnel.RemoveRange(joursExceptionnels);
                return db.SaveChanges();
            }
        }

        private TimeSpan RecalculHeureDebut(DateTime date)
        {
            int pasMinutes = int.Parse(ConfigurationManager.AppSettings["PasCreneauxHoraire"]);
            int minutes = (int)Math.Ceiling(date.TimeOfDay.TotalMinutes / pasMinutes) * pasMinutes;
            int heures = minutes / 60;
            minutes -= heures * 60;
            return new TimeSpan(heures, minutes, 0);
        }
    }
}