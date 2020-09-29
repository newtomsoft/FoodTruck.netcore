using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FoodTruck.Models
{
    public class PlageHoraireRetrait
    {
        internal TimeSpan Pas { get; private set; }
        public List<DateTime> Dates { get; private set; }

        public PlageHoraireRetrait(DateTime premierCreneau, DateTime dernierCreneau)
        {
            int pasMinutes = int.Parse(ConfigurationManager.AppSettings["PasCreneauxHoraire"]);
            Pas = new TimeSpan(0, pasMinutes, 0);
            DateTime creneauCourant = premierCreneau;
            Dates = new List<DateTime>();
            while (creneauCourant <= dernierCreneau)
            {
                Dates.Add(creneauCourant);
                creneauCourant = ObtenirCreneauSuivant(creneauCourant);
            }
        }

        public bool Contient(DateTime date)
        {
            if (Dates.First() <= date && date <= Dates.Last())
                return true;
            else
                return false;
        }

        private DateTime ObtenirCreneauCourant(DateTime date)
        {
            int minute = date.Minute;
            int minuteCreneauCourant = minute / (int)Pas.TotalMinutes * (int)Pas.TotalMinutes;
            return date.AddMinutes(minuteCreneauCourant - minute);
        }
        private DateTime ObtenirCreneauSuivant(DateTime date)
        {
            return ObtenirCreneauCourant(date) + Pas;
        }

        internal void Rogner(DateTime date)
        {
            if (Contient(date))
            {
                int indexMin = -1;
                int compteur = 0;
                foreach (DateTime creneau in Dates)
                {
                    if (creneau < date)
                    {
                        compteur++;
                        if (indexMin == -1)
                            indexMin = Dates.IndexOf(creneau);
                    }
                }
                if (compteur > 0)
                    Dates.RemoveRange(indexMin, compteur);
            }
        }
    }
}