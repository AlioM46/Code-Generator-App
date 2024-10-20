using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess_Layer_Library
{


    public class ForeignKeyInfo
    {
        public string ColumnName { get; set; }
        public string ReferenceTableName { get; set; }
    }

    public class clsGetSchema
    {





        public static List<ForeignKeyInfo> GetForeignKeys(string TableName)
        {

            List<ForeignKeyInfo> ForeignKeys = new List<ForeignKeyInfo>();

            string query = "SELECT      kcu.COLUMN_NAME AS ColumnName,     ku.TABLE_NAME AS ReferencedTableName FROM      INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS fk JOIN      INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu      " +
                "ON fk.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME JOIN      INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku      ON fk.UNIQUE_CONSTRAINT_NAME = ku.CONSTRAINT_NAME     AND kcu.ORDINAL_POSITION = ku.ORDINAL_POSITION WHERE      " +
                "kcu.TABLE_NAME = @TableName  " +
                "-- Filter for the specific table " +
                "ORDER BY      kcu.ORDINAL_POSITION;";

            using (SqlConnection conn = new SqlConnection(clsDataAccessSettings.ConnectionString)) {

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableName", TableName);

                try
                {
                    conn.Open();    
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read()) {
                        ForeignKeyInfo foreignKeyInfo = new ForeignKeyInfo();
                        foreignKeyInfo.ReferenceTableName = reader["ReferencedTableName"].ToString();
                        foreignKeyInfo.ColumnName= reader["ColumnName"].ToString();

                        ForeignKeys.Add(foreignKeyInfo);
                    }
                    reader.Close();
                }
                catch (Exception ex) {
                    return null;
                }
                return ForeignKeys;

            }
        }


        public static string GetPrimaryKey(string tableName)
        {

            string PK = "";

            using (SqlConnection conn = new SqlConnection(clsDataAccessSettings.ConnectionString)) {

                string query = @"
SELECT 
    --tc.TABLE_NAME, 
    kcu.COLUMN_NAME
    --,tc.CONSTRAINT_NAME 
FROM 
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
JOIN 
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu
ON 
    tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE 
    tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
    AND tc.TABLE_NAME NOT IN ('sysdiagrams')  -- Exclude sysdiagrams and other system tables if needed
	and tc.TABLE_NAME = @TableName
ORDER BY 
    tc.TABLE_NAME, kcu.COLUMN_NAME;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableName", tableName);

                try
                {
                    conn.Open();
                   object PrimaryKey= cmd.ExecuteScalar();
                    
                    if (PrimaryKey != null) { 
                    
                        PK = PrimaryKey.ToString();

                    }


                }
                catch (Exception ex) {
                    return null;
                }

                return PK;

            }

        } 
            public static DataTable GetTableData(string TableName)
            {
            string query = @"SELECT 
    c.TABLE_NAME, 
    c.COLUMN_NAME, 
    c.DATA_TYPE, 
    c.IS_NULLABLE
	
FROM 
    INFORMATION_SCHEMA.COLUMNS AS c
JOIN 
    INFORMATION_SCHEMA.TABLES AS t
    ON c.TABLE_NAME = t.TABLE_NAME
WHERE 
    t.TABLE_TYPE = 'BASE TABLE'  -- Ensures only tables, not views
    AND t.TABLE_CATALOG = @DataBaseName
	AND t.TABLE_NAME NOT IN ('sysdiagrams')  -- Exclude sysdiagrams and other system tables if needed
	and t.TABLE_NAME = @TableName
ORDER BY 
    c.TABLE_NAME, c.ORDINAL_POSITION;";
            DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(clsDataAccessSettings.ConnectionString)) {

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DataBaseName", clsDataAccessSettings.Database);
                    cmd.Parameters.AddWithValue("@TableName", TableName);
                    try
                    {

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            dt.Load(reader);
                        }

                        
                        reader.Close();
                    }
                    catch (Exception ex) { 
                    Console.WriteLine(ex.Message);
                
                }
                }

                return dt;

            }

        public static List<string> GetTablesNames()
        {
            List<string> tableNames = new List<string>();

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE' 
            AND TABLE_CATALOG = @DataBaseName ;";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DataBaseName", clsDataAccessSettings.Database);

                try
                {
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        tableNames.Add(reader["TABLE_NAME"].ToString());
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return tableNames;

        }

    }
}
