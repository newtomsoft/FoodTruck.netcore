using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace FoodTruck.DAL
{
    public class DALExceptions
    {
        public static string HandleException(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException)
            {
                // A custom exception of yours for concurrency issues
                throw new ConcurrencyException();
            }
            else if (exception is DbUpdateException dbUpdateEx)
            {
                if (dbUpdateEx.InnerException != null && dbUpdateEx.InnerException.InnerException != null)
                {
                    if (dbUpdateEx.InnerException.InnerException is SqlException sqlException)
                    {
                        switch (sqlException.Number)
                        {
                            case 2627:  // Unique constraint error
                            case 547:   // Constraint check violation
                            case 2601:  // Duplicated key row error
                                return "Impossible de sauvegarder. Entrée déjà existante";
                            default:
                                throw new DatabaseAccessException(dbUpdateEx.Message, dbUpdateEx.InnerException);
                        }
                    }

                    throw new DatabaseAccessException(dbUpdateEx.Message, dbUpdateEx.InnerException);
                }
            }
            return "Erreur. Merci de réessayer\nSi le problème persiste, contactez un administrateur.";
        }
    }
}