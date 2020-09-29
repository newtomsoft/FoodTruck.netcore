using System.Configuration;
using System.IO;

namespace FoodTruck.ViewModels
{
    public class Message
    {
        public TypeMessage Type { get; }
        public string CheminImage { get; }
        public string Contenu { get; }

        public Message(string contenu, TypeMessage type)
        {
            string dossierImagesSysteme = ConfigurationManager.AppSettings["PathImagesSysteme"];
            const string iconeOk = "IconeOk.png";
            const string iconeErreur = "IconeErreur.png";
            const string iconeInfo = "IconeInfo.png";
            const string iconeAvertissement = "IconeAvertissement.png";
            const string iconeInterdit = "IconeInterdit.png";
            Type = type;
            Contenu = contenu;
            switch (type)
            {
                case TypeMessage.Ok:
                    CheminImage = Path.Combine(dossierImagesSysteme, iconeOk);
                    break;
                case TypeMessage.Info:
                    CheminImage = Path.Combine(dossierImagesSysteme, iconeInfo);
                    break;
                case TypeMessage.Avertissement:
                    CheminImage = Path.Combine(dossierImagesSysteme, iconeAvertissement);
                    break;
                case TypeMessage.Erreur:
                    CheminImage = Path.Combine(dossierImagesSysteme, iconeErreur);
                    break;
                case TypeMessage.Interdit:
                    CheminImage = Path.Combine(dossierImagesSysteme, iconeInterdit);
                    break;
            }
        }

        public string ToHtml()
        {
            return "<p>" + Contenu.Replace("\n", "</p><p>") + "</p>";
        }
    }

    public enum TypeMessage
    {
        Ok,
        Info,
        Avertissement,
        Erreur,
        Interdit,
    }
}