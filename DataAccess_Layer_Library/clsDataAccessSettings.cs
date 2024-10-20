using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess_Layer_Library
{
    public class clsDataAccessSettings
    {


        public static string Database { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }

        public static string ConnectionString {  get; set; }

        public static bool IsValidCredentials(string database,string username, string password)
        {
            Database= database;
            Username = username;
            Password = password;


            ConnectionString = $"Server=.;Database={database};User Id={username}; Password={password};";
      
            using (var Connection = new SqlConnection(ConnectionString)) {

                try
                {
                    Connection.Open();
                    Database = database;
                    Username = username;
                    Password = password;

                    return true;
                }
                catch (Exception ex) {
                    return false; ;

                }
            }
        }



        public static bool DoesDataBaseExist(string Database)
        {
            string connectionString = $"Server=.;Integrated Security=true;";  // Change for your credentials

            string query = $"SELECT database_id FROM sys.databases WHERE Name = @databaseName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@databaseName", Database);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        // If database_id is returned, the database exists
                        return result != null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }
            }

        }


    }
}
