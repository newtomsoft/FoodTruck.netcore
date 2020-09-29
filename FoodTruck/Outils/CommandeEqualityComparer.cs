using FoodTruck.Models;
using System.Collections.Generic;

namespace FoodTruck.Outils
{
    class CommandeEqualityComparer : IEqualityComparer<Commande>
    {
        public bool Equals(Commande c1, Commande c2)
        {
            if (c2 == null && c1 == null)
                return true;
            else if (c1 == null || c2 == null)
                return false;
            else if (c1.Id == c2.Id)
                return true;
            else
                return false;
        }
        public int GetHashCode(Commande c)
        {
            int hCode = c.Id;
            return hCode.GetHashCode();
        }
    }

    class ClientEqualityComparer : IEqualityComparer<Client>
    {
        public bool Equals(Client u1, Client u2)
        {
            if (u2 == null && u1 == null)
                return true;
            else if (u1 == null || u2 == null)
                return false;
            else if (u1.Id == u2.Id)
                return true;
            else
                return false;
        }
        public int GetHashCode(Client u)
        {
            int hCode = u.Id;
            return hCode.GetHashCode();
        }
    }
}