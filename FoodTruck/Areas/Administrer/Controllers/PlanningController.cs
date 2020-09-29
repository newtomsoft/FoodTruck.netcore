using FoodTruck.DAL;
using FoodTruck.Models;
using FoodTruck.ViewModels;
using System;
using System.Net;
using System.Web.Mvc;

namespace FoodTruck.Areas.Administrer.Controllers
{
    public class PlanningController : ControllerParent
    {
        [HttpGet]
        public ActionResult FermeturesExceptionnelles()
        {
            if (AdminPlanning)
                return View(new JourExceptionnelDAL().FutursFermeturesExceptionnelles());
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
        [HttpPost]
        public ActionResult FermeturesExceptionnelles(string action, DateTime dateId, DateTime dateDebut, TimeSpan heureDebut, DateTime dateFin, TimeSpan heureFin)
        {
            if (AdminPlanning)
            {
                DateTime maintenant = DateTime.Now;
                DateTime dateDebutComplete = dateDebut + heureDebut;
                DateTime dateFinComplete = dateFin + heureFin;
                JourExceptionnelDAL ouvertureDAL = new JourExceptionnelDAL();
                if (action != "Supprimer" && (dateFinComplete <= dateDebutComplete || dateDebutComplete < maintenant))
                {
                    TempData["message"] = new Message("Impossible de valider votre demande. Les dates sont incorrectes.\nMerci de corriger votre saisie", TypeMessage.Erreur);
                }
                else
                {
                    JourExceptionnel chevauchement;
                    if (action == "Ajouter")
                    {
                        chevauchement = ouvertureDAL.AjouterFermeture(dateDebutComplete, dateFinComplete);
                        if (chevauchement == null)
                        {
                            TempData["message"] = new Message("La fermeture a bien été ajoutée", TypeMessage.Ok);
                        }
                        else
                        {
                            string ouvertureFermeture;
                            if (chevauchement.Ouvert)
                                ouvertureFermeture = "ouverture";
                            else
                                ouvertureFermeture = "fermeture";
                            TempData["message"] = new Message($"Impossible d'ajouter la fermeture.\nElle se chevauche avec une autre {ouvertureFermeture} :\n{chevauchement.DateDebut.ToString()} - {chevauchement.DateFin.ToString()}", TypeMessage.Erreur);
                        }
                    }
                    else if (action == "Modifier")
                    {
                        chevauchement = ouvertureDAL.ModifierFermeture(dateId, dateDebutComplete, dateFinComplete);
                        if (chevauchement == null)
                        {
                            TempData["message"] = new Message("La fermeture a bien été modifiée", TypeMessage.Ok);
                        }
                        else
                        {
                            string ouvertureFermeture;
                            if (chevauchement.Ouvert)
                                ouvertureFermeture = "ouverture";
                            else
                                ouvertureFermeture = "fermeture";
                            TempData["message"] = new Message($"Impossible de modifier la fermeture.\nElle se chevauche avec une autre {ouvertureFermeture} :\n{chevauchement.DateDebut.ToString()} - {chevauchement.DateFin.ToString()}", TypeMessage.Erreur);
                        }
                    }
                    else if (action == "Supprimer")
                    {
                        if (ouvertureDAL.SupprimerFermeture(dateId))
                            TempData["message"] = new Message("La suppression de la fermeture a bien été prise en compte", TypeMessage.Ok);
                        else
                            TempData["message"] = new Message("Une erreur est survenue lors de la supression de la fermeture.\nVeuillez réessayer plus tard", TypeMessage.Erreur);
                    }
                }
                return View(ouvertureDAL.FutursFermeturesExceptionnelles());
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        public ActionResult OuverturesExceptionnelles()
        {
            if (AdminPlanning)
                return View(new JourExceptionnelDAL().FutursOuverturesExceptionnelles());
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public ActionResult OuverturesExceptionnelles(string action, DateTime dateId, DateTime dateDebut, TimeSpan heureDebut, TimeSpan heureFin)
        {
            if (AdminPlanning)
            {
                DateTime maintenant = DateTime.Now;
                DateTime dateDebutComplete = dateDebut + heureDebut;
                DateTime dateFinComplete = dateDebut + heureFin;
                JourExceptionnelDAL ouvertureDAL = new JourExceptionnelDAL();
                if (heureFin <= heureDebut || dateDebutComplete < maintenant)
                {
                    TempData["message"] = new Message("Impossible de valider votre demande. Les dates sont incorrectes.\nMerci de corriger votre saisie", TypeMessage.Erreur);

                }
                else
                {
                    JourExceptionnel chevauchement;
                    if (action == "Ajouter")
                    {
                        chevauchement = ouvertureDAL.AjouterOuverture(dateDebutComplete, dateFinComplete);
                        if (chevauchement == null)
                        {
                            TempData["message"] = new Message("L'ouverture a bien été ajoutée", TypeMessage.Ok);
                        }
                        else
                        {
                            string ouvertureFermeture;
                            if (chevauchement.Ouvert)
                                ouvertureFermeture = "ouverture";
                            else
                                ouvertureFermeture = "fermeture";
                            TempData["message"] = new Message($"Impossible d'ajouter l'ouverture.\nElle se chevauche avec une autre {ouvertureFermeture} :\n{chevauchement.DateDebut.ToString()} - {chevauchement.DateFin.TimeOfDay.ToString()}", TypeMessage.Erreur);
                        }
                    }
                    else if (action == "Modifier")
                    {
                        chevauchement = ouvertureDAL.ModifierOuverture(dateId, dateDebutComplete, dateFinComplete);
                        if (chevauchement == null)
                        {
                            TempData["message"] = new Message("L'ouverture a bien été modifiée", TypeMessage.Ok);
                        }
                        else
                        {
                            string ouvertureFermeture;
                            if (chevauchement.Ouvert)
                                ouvertureFermeture = "ouverture";
                            else
                                ouvertureFermeture = "fermeture";
                            TempData["message"] = new Message($"Impossible de modifier l'ouverture.\nElle se chevauche avec une autre {ouvertureFermeture} :\n{chevauchement.DateDebut.ToString()} - {chevauchement.DateFin.ToString()}", TypeMessage.Erreur);
                        }
                    }
                    else if (action == "Supprimer")
                    {
                        if (ouvertureDAL.SupprimerOuverture(dateId))
                            TempData["message"] = new Message("La suppression de l'ouverture a bien été prise en compte", TypeMessage.Ok);
                        else
                            TempData["message"] = new Message("Une erreur est survenue lors de la supression de l'ouverture.\nVeuillez réessayer plus tard", TypeMessage.Erreur);
                    }
                }
                return View(ouvertureDAL.FutursOuverturesExceptionnelles());
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        public ActionResult OuverturesHebdomadaires()
        {
            if (AdminPlanning)
                return View(new OuvertureHebdomadaireDAL().OuverturesHebdomadaires());
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
        [HttpPost]
        public ActionResult OuverturesHebdomadaires(int id, string action, int jourId, TimeSpan heureDebut, TimeSpan heureFin)
        {
            if (AdminPlanning)
            {
                OuvertureHebdomadaireDAL ouvertureDAL = new OuvertureHebdomadaireDAL();
                if (heureFin <= heureDebut)
                {
                    TempData["message"] = new Message("Impossible de valider votre demande. L'heure de fin est avant l'heure de début.\nMerci de corriger votre saisie", TypeMessage.Erreur);
                }
                else
                {
                    OuvertureHebdomadaire chevauchement;
                    if (action == "Ajouter")
                    {
                        chevauchement = ouvertureDAL.AjouterOuverture(jourId, heureDebut, heureFin);
                        if (chevauchement == null)
                            TempData["message"] = new Message("L'ouverture a bien été ajoutée", TypeMessage.Ok);
                        else
                            TempData["message"] = new Message($"Impossible d'ajouter l'ouverture.\nElle se chevauche avec une autre ouverture :\n{chevauchement.Debut.ToString()} - {chevauchement.Fin.ToString()}", TypeMessage.Erreur);
                    }
                    else if (action == "Modifier")
                    {
                        chevauchement = ouvertureDAL.ModifierOuverture(id, jourId, heureDebut, heureFin);
                        if (chevauchement == null)
                            TempData["message"] = new Message("L'ouverture a bien été modifiée", TypeMessage.Ok);
                        else
                            TempData["message"] = new Message($"Impossible de modifier l'ouverture.\nElle se chevauche avec une autre ouverture :\n{chevauchement.Debut.ToString()} - {chevauchement.Fin.ToString()}", TypeMessage.Erreur);

                    }
                    else if (action == "Supprimer")
                    {
                        if (ouvertureDAL.SupprimerOuverture(id))
                            TempData["message"] = new Message("La suppression de l'ouverture a bien été prise en compte", TypeMessage.Ok);
                        else
                            TempData["message"] = new Message("Une erreur est survenue lors de la supression de l'ouverture.\nVeuillez réessayer plus tard", TypeMessage.Erreur);
                    }
                }
                return View(ouvertureDAL.OuverturesHebdomadaires());
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}