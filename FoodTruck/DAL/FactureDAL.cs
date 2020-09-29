using FoodTruck.Models;
using FoodTruck.ViewModels;
using System;
using System.Linq;

namespace FoodTruck.DAL
{
    public class FactureDAL
    {
        public Facture Details(string guid)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Facture facture = (from f in db.Facture
                                   where f.Guid == guid
                                   select f).FirstOrDefault();

                return facture;
            }
        }
        public Facture DetailsCommande(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Facture facture = (from f in db.Facture
                                   where f.CommandeId == id
                                   select f).FirstOrDefault();
                if (facture == null)
                {
                    facture = new Facture
                    {
                        CommandeId = id,
                        Guid = Guid.NewGuid().ToString("n"),
                    };
                    db.Facture.Add(facture);
                    db.SaveChanges();
                }
                return facture;
            }
        }
    }
}