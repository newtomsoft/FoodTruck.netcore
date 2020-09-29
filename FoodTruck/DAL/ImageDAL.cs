using FoodTruck.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoodTruck.DAL
{
    public class ImageDAL
    {
        public int Purger(string dossier)
        {
            var nomsFichiers = (from fullFilename in Directory.EnumerateFiles(dossier)
                                select Path.GetFileName(fullFilename)).ToList();

            List<Article> tousArticles = new ArticleDAL().Tous();

            var nomsImages = (from art in tousArticles
                              select art.Image.Trim()).ToList();

            nomsFichiers.RemoveAll(nom => nomsImages.Contains(nom));
            foreach (string fichier in nomsFichiers)
            {
                File.Delete(Path.Combine(dossier, fichier));
            }
            return nomsFichiers.Count;
        }
    }
}