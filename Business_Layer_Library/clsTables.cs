using DataAccess_Layer_Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer_Library
{
    public class clsTables
    {
        public string TableName { get; set; }
        public List<clsAttributes> AttributesList { get; set; }


        public static string GetPrimaryKey(string tableName)
        {
            return clsGetSchema.GetPrimaryKey(tableName);
        }
        public  string GetPrimaryKeyName()
        {
            return clsTables.GetPrimaryKey(this.TableName);
        }
        public string GetFullPrimaryKey()
        {
            return GetPrimaryKeyType() + " " + clsGetSchema.GetPrimaryKey(this.TableName);
        }
        public string GetPrimaryKeyType()
        {
            string Type = "";
            foreach (clsAttributes attr in AttributesList)
            {
                if (attr.Name == GetPrimaryKey(this.TableName))
                {
                    Type = attr.CSharpType;
                }
                else
                {
           Type= "int";
                }
            }
            return Type;
            
        }

        public static clsTables GetTable(string tableName)
        {


            clsTables newTable = new clsTables();

            newTable.AttributesList = new List<clsAttributes>();
            newTable.TableName = tableName;


            DataTable dt = new DataTable();

            dt = clsGetSchema.GetTableData(tableName);


            string PK = clsGetSchema.GetPrimaryKey(tableName);
            List<ForeignKeyInfo> ForeignKeys = clsGetSchema.GetForeignKeys(tableName);
            List<clsAttributes> AllKeys = new List<clsAttributes>();


            foreach (DataRow row in dt.Rows) {

                string ColumnName = row["COLUMN_NAME"].ToString();
                string DataType = row["DATA_TYPE"].ToString();
                bool IsNullable = row["IS_NULLABLE"].ToString() == "YES";
                bool IsPrimaryKey = PK == ColumnName;
                string ReferenceTableName = "";
                bool IsForeignKey = false;

                foreach (ForeignKeyInfo foreignKeyInfo in ForeignKeys) {
                    if (foreignKeyInfo.ColumnName == ColumnName) {
                        ReferenceTableName  = foreignKeyInfo.ReferenceTableName;
                        IsForeignKey = true;
                        break;
                    }
                }

                clsAttributes column = new clsAttributes(ColumnName, IsNullable, DataType, IsForeignKey, IsPrimaryKey, ReferenceTableName);
                AllKeys.Add(column);
            }

            newTable.AttributesList = AllKeys;
            return newTable;

        }

        public static List<clsTables> GetAllTables() {

            List<string> TablesNames = new List<string>();
            List<clsTables> Tables = new List<clsTables>();

            TablesNames = clsGetSchema.GetTablesNames();


            // Iterate On each Table name then Create a New Table class 
            // Fill Info = Columns Info;
            for (int i = 0; i < TablesNames.Count; i++)
            {
                Tables.Add(clsTables.GetTable(TablesNames[i]));    
            }

            return Tables;





        }


    }
}
