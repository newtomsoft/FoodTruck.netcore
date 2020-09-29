using FoodTruck.DAL;
using FoodTruck.Outils;
using System;
using System.Configuration;

namespace BatchFoodTruck
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Nombre d'enregistrements de PanierProspect supprimés :  {new PanierProspectDAL("").Purger(30)}");
            Console.WriteLine($"Nombre d'enregistrements de OubliMotDePasse supprimés : {new OubliMotDePasseDAL().Purger()}");
            Console.WriteLine($"Nombre d'enregistrements de JourExceptionnel supprimés : {new JourExceptionnelDAL().Purger()}");
            Console.WriteLine($"Nombre d'images supprimées : {new ImageDAL().Purger(ConfigurationManager.AppSettings["PathImagesArticles"])}");
        }
    }
}
