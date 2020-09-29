using FoodTruck.Models;
using FoodTruck.Outils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoodTruck.DAL
{
    class ClientDAL
    {
        public Client Details(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Client client = (from c in db.Client
                                 where c.Id == id
                                 select c).FirstOrDefault();
                Trim(ref client);
                return client;
            }
        }

        public Client Details(string email)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Client client = (from c in db.Client
                                 where c.Email == email
                                 select c).FirstOrDefault();
                Trim(ref client);
                return client;
            }
        }

        public int ExisteEmail(string email)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                int clientId = (from c in db.Client
                                where c.Email == email
                                select c.Id).FirstOrDefault();

                return clientId;
            }
        }
        public int ExisteLogin(string login)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                int clientId = (from c in db.Client
                                where c.Login == login
                                select c.Id).FirstOrDefault();

                return clientId;
            }
        }
        public List<Client> Recherche(string recherche)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                List<Client> clients = (from c in db.Client
                                        where c.Id != 0 && (c.Nom.Contains(recherche) || c.Prenom.Contains(recherche) || c.Email.Contains(recherche) || c.Telephone.Contains(recherche))
                                        select c).ToList();
                return clients;
            }
        }

        public Client Connexion(string loginEmail, string mdp)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                string mdpHash = mdp.GetHash();
                Client client = (from c in db.Client
                                 where (c.Email == loginEmail || c.Login == loginEmail) && c.Mdp == mdpHash
                                 select c).FirstOrDefault();
                Trim(ref client);
                return client;
            }
        }
        public Client ConnexionCookies(string guid)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Client client = (from c in db.Client
                                 where c.Guid == guid
                                 select c).FirstOrDefault();
                Trim(ref client);
                return client;
            }

        }

        /// <summary>
        /// Si le solde est suffisant, retire "montant" euros de la cagnotte de client d'id "id" et retourne le solde restant
        /// Si le solde de la cagnotte est insuffisant retourne -1 sans rien modifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="montant"></param>
        /// <returns></returns>
        internal int RetirerCagnotte(int id, int montant)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                var client = (from c in db.Client
                              where c.Id == id
                              select c).FirstOrDefault();
                if (montant <= client.Cagnotte)
                {
                    client.Cagnotte -= montant;
                    db.SaveChanges();
                    return client.Cagnotte;
                }
                else
                {
                    return -1;
                }
            }
        }

        public Client Creation(string email, string login, string mdp, string nom, string prenom, string telephone)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                string mdpHash = mdp.GetHash();
                string guid = Guid.NewGuid().ToString();
                int id = (from c in db.Client
                          where c.Email == email || c.Guid == guid
                          select c.Id).FirstOrDefault();
                if (id == 0)
                {
                    Client client = new Client
                    {
                        Guid = guid,
                        Email = email.Trim(),
                        Login = login.Trim(),
                        Mdp = mdpHash,
                        Nom = nom.Trim(),
                        Prenom = prenom.Trim(),
                        Telephone = telephone.Trim(),
                        Cagnotte = 0,
                        Inscription = DateTime.Today
                    };
                    db.Client.Add(client);
                    db.SaveChanges();
                    return Connexion(email, mdp);
                }
                else
                {
                    return null;
                }
            }
        }

        internal int Modification(int id, string mdp, string login = null, string email = null, string nom = null, string prenom = null, string telephone = null)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Client client = (from user in db.Client
                                 where user.Id == id
                                 select user).FirstOrDefault();

                client.Mdp = mdp.GetHash();
                if (email != null)
                    client.Email = email.Trim();
                if (login != null)
                    client.Login = login.Trim();
                if (nom != null)
                    client.Nom = nom.Trim();
                if (prenom != null)
                    client.Prenom = prenom.Trim();
                if (telephone != null)
                    client.Telephone = telephone.Trim();
                return db.SaveChanges();
            }
        }

        internal int DonnerDroitAdmin(int id)
        {
            using (foodtruckEntities db = new foodtruckEntities())
            {
                Client client = (from user in db.Client
                                 where user.Id == id
                                 select user).FirstOrDefault();

                client.AdminArticle = client.AdminCommande = client.AdminPlanning = true;
                return db.SaveChanges();
            }
        }

        private void Trim(ref Client client)
        {
            if (client != null)
            {
                client.Prenom = client.Prenom.Trim();
                client.Nom = client.Nom.Trim();
                client.Email = client.Email.Trim();
                client.Login = client.Login.Trim();
            }
        }
    }
}
