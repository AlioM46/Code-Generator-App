using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer_Library
{
    public class clsAttributes
    {


        public  Dictionary<string, string> sqlToCSharpTypes = new Dictionary<string, string>
            {
            { "bigint", "long" },
            { "binary", "byte[]" },
            { "bit", "bool" },
            { "char", "string" },
            { "date", "DateTime" },
            { "datetime", "DateTime" },
            { "datetime2", "DateTime" },
            { "datetimeoffset", "DateTimeOffset" },
            { "decimal", "decimal" },
            { "float", "double" },
            { "image", "byte[]" },
            { "int", "int" },
            { "money", "decimal" },
            { "nchar", "string" },
            { "ntext", "string" },
            { "numeric", "decimal" },
            { "nvarchar", "string" },
            { "real", "float" },
            { "smalldatetime", "DateTime" },
            { "smallint", "short" },
            { "smallmoney", "decimal" },
            { "sql_variant", "object" },
            { "text", "string" },
            { "time", "TimeSpan" },
            { "tinyint", "byte" },
            { "uniqueidentifier", "Guid" },
            { "varbinary", "byte[]" },
            { "varchar", "string" },
            { "xml", "string" }
        };


        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string DefaultValue { get; set; }
        private string _CSharpType;
        public string CSharpType { get { return _CSharpType; } set { _CSharpType= value; DefaultValue = GetDefaultValue();   } }
        public string ReferenceName { get; set; }


        public clsAttributes(string Name, bool isNullable, string DataType, bool IsForeignKey, bool IsPrimaryKey, string ReferenceTableName)
        {
            this.Name = Name;
            this.IsNullable = isNullable;
            this.IsForeignKey = IsForeignKey;
            this.IsPrimaryKey = IsPrimaryKey;
            this.DataType = DataType;
           CSharpType= ConvertToCSharpType(DataType);
            this.ReferenceName = ReferenceTableName;
        }



        private string GetDefaultValue()
        {
            if (CSharpType == "int" || CSharpType == "float" ||
    CSharpType ==
    "double" || CSharpType == "decimal" ||
    CSharpType == "short"
    )
            {
                return "-1";
            }
            else if (CSharpType == "string")
            {
                return "\"\"";
            }
            else if (CSharpType == "DateTime")
            {
                return "DateTime.MaxValue";
            }
            else if (CSharpType == "byte")
            {
                return "0";
            }
            return "";
        }


        public string ConvertToCSharpType(string dbType)
        {
            if (sqlToCSharpTypes.ContainsKey(dbType))
            {
                return sqlToCSharpTypes[dbType];
            }
            return "object";

        }





}
}
