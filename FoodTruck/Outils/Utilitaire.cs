using FoodTruck.DAL;
using FoodTruck.Models;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FoodTruck.Outils
{
    public static class Utilitaire
    {
        public static string StringAleatoire(int longueur)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!?_-#$*%^";
            StringBuilder retour = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint) * longueur];
                rng.GetBytes(uintBuffer);
                for (int i = 0; i < longueur; i++)
                {
                    uint num = BitConverter.ToUInt32(uintBuffer, sizeof(uint) * i);
                    retour.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return retour.ToString();
        }

        // update : 5/April/2018
        static readonly Regex MobileCheck = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        static readonly Regex MobileVersionCheck = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        public static bool NavigateurMobile()
        {
            if (HttpContext.Current.Request != null && HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                string u = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                if (u.Length >= 4 && (MobileCheck.IsMatch(u) || MobileVersionCheck.IsMatch(u.Substring(0, 4))))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static List<PlageHoraireRetrait> PlageHoraireRetrait(this DateTime date)
        {
            JourExceptionnelDAL ouvertureDAL = new JourExceptionnelDAL();
            List<PlageHoraireRetrait> plagesHorairesRetrait = new List<PlageHoraireRetrait> { ouvertureDAL.ProchainOuvert(date) };
            bool autreCreneau = true;
            while (autreCreneau)
            {
                plagesHorairesRetrait.Add(ouvertureDAL.ProchainOuvert(plagesHorairesRetrait.Last().Dates.Last().AddMinutes(1)));
                TimeSpan diff = plagesHorairesRetrait.Last().Dates.First() - plagesHorairesRetrait.First().Dates.First();
                if (plagesHorairesRetrait.Count >= 4 && diff.Days > 1)
                    autreCreneau = false;
            }
            return plagesHorairesRetrait;
        }
        public static string UrlVersNom(this string url)
        {
            return url.Replace("-", " ").Replace("_", "-");
        }
        public static string ToUrl(this string nom)
        {
            return nom.TrimEnd(' ').Replace("-", "_").Replace(" ", "-");
        }
        public static string NomAdmis(this string nom)
        {
            const char espace = ' ';
            const string interdit = "@=&#_;%^";
            return nom.Replace(interdit, espace).Replace(Path.GetInvalidFileNameChars(), espace);
        }
        public static string Replace(this string orig, string to, char by)
        {
            foreach (char car in to)
            {
                orig = orig.Replace(car, by);
            }
            return orig;
        }
        public static string Replace(this string orig, char[] to, char by)
        {
            foreach (char car in to)
            {
                orig = orig.Replace(car, by);
            }
            return orig;
        }

        public static bool EnvoieMail(string destinataire, string objet, string corpsMessage, MemoryStream pieceJointe = null, string reponseA = "")
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    string mailFoodTruck = ConfigurationManager.AppSettings["MailFoodTruck"];
                    if (reponseA == "")
                        reponseA = mailFoodTruck;
                    message.From = new MailAddress(mailFoodTruck);
                    message.To.Add(destinataire);
                    message.Subject = objet;
                    message.ReplyToList.Add(reponseA);
                    message.Body = corpsMessage;
                    message.IsBodyHtml = false;
                    if (pieceJointe != null)
                    {
                        pieceJointe.Position = 0;
                        System.Net.Mail.Attachment data = new System.Net.Mail.Attachment(pieceJointe, "event.ics", "text/calendar");
                        message.Attachments.Add(data);
                    }
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.EnableSsl = false;
                        client.Send(message);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static MemoryStream CreerEvenementCalendrier(string objet, string description, string adresse, DateTime dateDebut, DateTime dateFin, double lattitude = 0, double longitude = 0)
        {
            GeographicLocation coordonneesGPS = null;
            if (lattitude != 0 && longitude != 0)
                coordonneesGPS = new GeographicLocation(45.756386, 4.8379093);
            Calendar calendar = new Calendar();
            CalendarEvent myEvent = new CalendarEvent
            {
                Start = new CalDateTime(dateDebut),
                End = new CalDateTime(dateFin),
                Summary = objet,
                Description = description,
                Location = adresse,
                GeographicLocation = coordonneesGPS,
            };
            calendar.Events.Add(myEvent);
            MemoryStream stream = new MemoryStream();
            CalendarSerializer serializer = new CalendarSerializer();
            serializer.Serialize(calendar, stream, Encoding.ASCII);
            return stream;
        }

        public static string GetHash(this string input)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] data = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }

        }
    }
}