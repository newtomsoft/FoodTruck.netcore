using FoodTruck.ViewModels;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace FoodTruck.Controllers
{
    public class HomeController : ControllerParent
    {
        public HomeController()
        {
            ViewBag.PanierLatteralDesactive = true;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new HomeViewModel());
        }

        [HttpGet]
        public ActionResult APropos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.MailEnvoye = "";
            ViewBag.MailErreur = "";
            return View();
        }

        [HttpGet]
        public ActionResult Github()
        {
            return Redirect("https://github.com/newtom69/FoodTruckLyon");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Contact(string nom, string prenom, string email, string comments)
        {
            string nomOk = Server.HtmlEncode(nom);
            string prenomOk = Server.HtmlEncode(prenom);
            string commentsOk = Server.HtmlEncode(comments);
            string mailFoodTruck = ConfigurationManager.AppSettings["MailFoodTruck"];
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(mailFoodTruck);
                    message.To.Add(email);
                    message.Subject = "Message à partir du formulaire de contact";
                    StringBuilder mastringbuilder = new StringBuilder();
                    mastringbuilder.Append(
                        "<html lang=\"en\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    mastringbuilder.Append(
                        "<meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\"><title>Mail du site</title></head><body><h3>De : ");
                    mastringbuilder.Append(prenomOk + " " + nomOk);
                    mastringbuilder.Append("</h3><h3>Message : </h3><p>");
                    mastringbuilder.Append(commentsOk);
                    mastringbuilder.Append("<br/>");
                    mastringbuilder.Append("<br/>Ceci est une copie du message envoyé à votre FoodTruck. Vous recevrez bientôt une réponse. Merci");
                    mastringbuilder.Append("</p></body></html>");
                    message.Body = mastringbuilder.ToString();
                    message.IsBodyHtml = true;
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.EnableSsl = false;
                        client.Send(message);
                    }
                }
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(mailFoodTruck);
                    message.To.Add(mailFoodTruck);
                    message.Subject = "Message à partir du formulaire de contact";
                    message.ReplyToList.Add(email);
                    StringBuilder stringbuilder = new StringBuilder();
                    stringbuilder.Append("<html lang=\"en\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    stringbuilder.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\"><title>Mail du site</title></head><body><h3>De : ");
                    stringbuilder.Append($"{prenomOk} {nomOk}");
                    stringbuilder.Append("</h3><h3>Message : </h3><p>");
                    stringbuilder.Append(commentsOk);
                    stringbuilder.Append("<br/>");
                    stringbuilder.Append("</p></body></html>");
                    message.Body = stringbuilder.ToString();
                    message.IsBodyHtml = true;
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.EnableSsl = false;
                        client.Send(message);
                    }
                }
                TempData["message"] = new Message("Votre message a bien été envoyé.\nNous vous répondrons dès que possible.", TypeMessage.Info);
            }
            catch (Exception)
            {
                Response.StatusCode = 400;
                TempData["message"] = new Message("Erreur dans l'envoi du mail.\nMerci de réessayer plus tard.", TypeMessage.Info);
            }
            return View();
        }
    }
}
