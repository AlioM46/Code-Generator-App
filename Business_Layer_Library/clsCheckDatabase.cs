using DataAccess_Layer_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer_Library
{
    public class clsCheckDatabase
    {

        public static bool DoesDataBaseExist(string Database)
        {
            return clsDataAccessSettings.DoesDataBaseExist(Database);
        }
        public static bool IsValidCredentials(string Database, string username, string password)
        {
            return clsDataAccessSettings.IsValidCredentials(Database, username,password);
        }

    }
}
