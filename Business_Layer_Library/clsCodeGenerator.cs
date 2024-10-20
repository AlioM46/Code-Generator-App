using DataAccess_Layer_Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Business_Layer_Library
{
    public class clsCodeGenerator
    {


        public string ClassName { get; set; }


        private clsTables _Table;
        public clsTables Table
        {
            get
            {
                return _Table;
            }

            set
            {
                _Table = value;
                ClassName = "cls" + Table.TableName;
                FileName = "cls" + Table.TableName + ".cs";
            }

        }



        public static string FileName { get; set; }
        public static string FolderPath { get; set; }



        public string BusinessHeaderContent()
        {
            string content = "using System;\nusing DataAccess_Layer;\nusing System.Data;\n";
            content += "\nnamespace Business_Layer { \n public class " + ClassName + " \n{ \n";
            return content;

        }
        public string BusinessFooterContent()
        {
            string content = "\n";
            content += "}\n" +
                "}\n";
            return content;
        }



        public string HandleForeignKeys(clsAttributes Prop)
        {
            string content = "";

            if (Prop.IsForeignKey)
            {
                content += "\n\t\t// Composition of " + Prop.ReferenceName + "\n";
                content += "\t\tpublic " + "cls" + Prop.ReferenceName + " " + Prop.ReferenceName + " ; \n";
            }
            return content;

        }


        public string BusinessProperties()
        {
            string content = "\n\n\n";
            content += "\t\tpublic enum enMode {AddNew = 1, Update=2};\n\t\tpublic enMode Mode {get;set;}\n\n";

            foreach (clsAttributes prop in Table.AttributesList)
            {
                // string IsNullable = prop.IsNullable ? "?" :"";

                content += $"\t\tpublic {prop.CSharpType} {prop.Name} {{get; set;}}\n";

            }
            foreach (clsAttributes prop in Table.AttributesList)
            {
                content += HandleForeignKeys(prop);
            }


            return content + "\n";
        }


        public string BusinessConstructers()
        {
            string content = "";

            content += $"\n\n\t\tpublic {ClassName}({ParametersList()}) {{\n \n";

            foreach (clsAttributes prop in Table.AttributesList)
            {
                content += "\t\t\tthis." + prop.Name + " = " + prop.Name + ";\n";
                if (prop.IsForeignKey)
                {
                    content += $"\n\t\t\t{prop.ReferenceName} = cls{prop.ReferenceName}.Find({prop.Name});\n\n";
                }
            }

            content += "\t\t\tMode= enMode.Update;\n";
            content += "\n\t\t}";
            return content;

        }

        public string ParametersList(bool IncludePK = true, bool IncludeTypes = true, bool AtMarkPrefix = false, bool ThisKeyWord = false)
        {
            var content = new StringBuilder();



            foreach (clsAttributes prop in Table.AttributesList)
            {
                if (prop.IsPrimaryKey && !IncludePK)
                {
                    continue;
                }

                string thisKeyword = ThisKeyWord ? "this." : "";
                string prefix = AtMarkPrefix ? "@" : "";
                string type = IncludeTypes ? prop.CSharpType + " " : "";

                content.Append($"{prefix}{type}{thisKeyword}{prop.Name}, ");


            }

            if (content.Length > 2)
            {
                content.Length -= 2; // Remove last ", "
            }

            return content.ToString();
        }


        public string DefaultValueParametersList(bool IncludePK = true)
        {
            string content = "";

            foreach (clsAttributes prop in Table.AttributesList)
            {
                if (prop.IsPrimaryKey && !IncludePK)
                {
                    continue;
                }

                // Or Any Class & Object
                if (prop.CSharpType == "DateTime")
                {
                    content += "\n\t\t" + prop.CSharpType + " " + prop.Name + $" = new {prop.CSharpType}();\n";
                    content += "\t\t" + prop.Name + " = " + prop.DefaultValue + ";\n\n";

                }
                else
                {
                    content += "\t\t" + prop.CSharpType + " " + prop.Name + $" = {prop.DefaultValue};\n";
                }
            }

            if (content.EndsWith(", "))
            {
                content = content.Substring(0, content.Length - 2);
            }
            return content;
        }


        public string AddNewAndUpdateParameterList(bool IncludePK = true, bool IncludeTypes = false, bool IncludeThisKeyword = false)
        {
            string content = "";

            foreach (clsAttributes prop in Table.AttributesList)
            {
                if (prop.IsPrimaryKey && !IncludePK)
                {
                    continue;
                }

                if (IncludeThisKeyword && IncludeTypes)
                {
                    content += $" ref {prop.CSharpType} this." + prop.Name + ", ";
                }
                else if (IncludeTypes)
                {
                    content += $" ref {prop.CSharpType} " + prop.Name + ", ";
                }
                else if (IncludeThisKeyword)
                {
                    content += " ref this." + prop.Name + ", ";
                }
                else
                {
                    content += " ref " + prop.Name + ", ";
                }

            }

            if (content.EndsWith(", "))
            {
                content = content.Substring(0, content.Length - 2);
            }
            return content;
        }


        public string BusinessNonParametarizedConstructers()
        {
            string content = "";

            content += $"\n\n\t\tpublic {ClassName}() {{\n \n";

            foreach (clsAttributes prop in Table.AttributesList)
            {
                content += "\t\t\tthis." + prop.Name + " = " + prop.DefaultValue + ";\n";
            }
            content += "\t\t\tMode= enMode.AddNew;\n";

            content += "\n\t\t}";
            return content;
        }


        public string AddNewMethod()
        {
            string content = "";
            content += $"\n\n\t\tprivate bool _AddNew{_Table.TableName}() {{ \n\n " +
                $" \t\t\tthis.{clsTables.GetPrimaryKey(Table.TableName)} = DataAccess_Layer.cls{Table.TableName}.AddNew{Table.TableName}({ParametersList(false, false, false, true)});\n\n" +
                $"\t\t\t return this.{clsTables.GetPrimaryKey(Table.TableName)} != -1;\n" +
                $"\t\t }}\n\n";
            return content;

        }
        public string UpdateMethod()
        {
            string content = "";
            content += $"\t\tprivate bool _Update{_Table.TableName}() {{ \n\n " +
                $"\t\t\t return DataAccess_Layer.cls{Table.TableName}.Update{Table.TableName}({ParametersList(true, false, false, true)});" +
                $"\n\t\t }}\n\n";
            return content;
        }
        public string Delete()
        {
            string content = "";
            content += $"\t\tprivate bool _Delete{_Table.TableName}() {{ \n\n " +
                $"\t\t\t return DataAccess_Layer.cls{Table.TableName}.Delete{Table.TableName}(this.{clsTables.GetPrimaryKey(Table.TableName)});" +
                $"\n\t\t }}\n\n";
            return content;
        }

        public string Find()
        {
            string content = "";
            content += $"\t\tpublic static {ClassName} Find({Table.GetPrimaryKeyType()} {clsTables.GetPrimaryKey(Table.TableName)}) {{ \n\n " +
                $"{DefaultValueParametersList(false)}\n" +
                $"\t\t if (DataAccess_Layer.{ClassName}.Find({clsTables.GetPrimaryKey(Table.TableName)}{", "}{AddNewAndUpdateParameterList(false, false)})) {{ \n\n " +
                $"\t\t return new {ClassName}({ParametersList(true, false)});\n" +
                $"\t\t }} else \n\t\t{{ \n\t\treturn null;\n}}" +
                $"\n\t\t }}\n\n";
            return content;
        }

        public string Save()
        {
            string content = "";
            content += $"\t\tpublic bool Save() {{ \n\n " +
                $"\t\t switch(Mode) {{ \n\t\t\t case enMode.Update:\n\t\t\t return _Update{Table.TableName}();\n\n" +
                $"\t\t\t case enMode.AddNew: \n\n" +
                $"\t\t\t\t if (_AddNew{Table.TableName}()) {{ \n" +
                $"\t\t\t\t\tMode = enMode.Update;\n" +
                $"\t\t\t\t\treturn true;\n" +
                $"\n\t\t\t}} else {{ \n " +
                $"\t\t\t\t\t return false; }} \n" +
                $"\t\t\t\t\tdefault: return false;\n\n }} \n\t\t}}\n\n";
            return content;
        }


        public string DoesExist()
        {
            string content = "";



            // Static
            content += $"\t\tpublic static bool Does{_Table.TableName}Exists({_Table.GetFullPrimaryKey()}) {{\n";
            content += $"\t\t\treturn DataAccess_Layer.{ClassName}.Does{_Table.TableName}Exists({_Table.GetPrimaryKeyName()});\n" +
                $"\t\t }}\n\n";

            // Non-Static;

            content += $"\t\tpublic  bool Does{_Table.TableName}Exists() {{\n";
            content += $"\t\t\treturn Does{_Table.TableName}Exists(this.{_Table.GetPrimaryKeyName()});\n" +
                $"\t\t }}\n\n";

            return content;
        }

        public string GetAllRecords()
        {
            string content = "";


            content += $"\tpublic static DataTable GetAll{_Table.TableName}() {{\n";
            content += $"\t\t return DataAccess_Layer.{ClassName}.GetAll{_Table.TableName}();\n";
            content += "\t}\n\n";

            return content;
        }


        public string CommonMethods()
        {
            string content = "";

            content += AddNewMethod();
            content += UpdateMethod();
            content += Delete();
            content += Find();
            content += Save();
            content += DoesExist();
            content += GetAllRecords();
            //content += AddNewMethod();

            return content;

        }

        public void GenerateBusinessLayerFiles()
        {


            string content = "";

            content += BusinessHeaderContent();
            content += BusinessProperties();
            content += BusinessConstructers();
            content += BusinessNonParametarizedConstructers();
            content += CommonMethods();
            content += BusinessFooterContent();
            HandleFolder("Business", content);
        }



        public void HandleFolder(string Layer, string content)
        {


            if (!Directory.Exists(Path.Combine(FolderPath, Layer + "_Layer")))
            {
                Directory.CreateDirectory(Path.Combine(FolderPath, Layer + "_Layer"));
            }

            string filePath = Path.Combine(FolderPath, Layer + "_Layer", FileName);

            if (!File.Exists(filePath))
            {
                using (FileStream fs = File.Create(filePath))
                {
                }
            }

            File.WriteAllText(filePath, content);

        }


        public string DataAccessHeaderContent(string OptionalClassName = "")
        {


            string className = OptionalClassName != "" ? OptionalClassName : ClassName;

            string content = "";
            content += "using System;\nusing Microsoft.Data.SqlClient;\n\n\n";
            content += "namespace DataAccess_Layer{\n\n";
            content += "public class " + className + " { \n\n";

            return content;

        }



        public string HandleNullableValues()
        {
            string content = "\n\n";

            foreach (clsAttributes Column in _Table.AttributesList)
            {
                if (!Column.IsPrimaryKey)
                {
                    if (Column.IsNullable)
                    {
                        content += $"\t\tif ({Column.Name} != {Column.DefaultValue} && {Column.Name} != null) {{\t ";
                        content += $"\n\n\t\t\tCommand.Parameters.AddWithValue(\"@{Column.Name}\", {Column.Name});\n";
                        content += $"\t\t}}\n";
                        content += "\t\telse {";
                        content += $"\n\n\t\t\tCommand.Parameters.AddWithValue(\"@{Column.Name}\", DBNull.Value);\n";
                        content += "\t\t}\n\n";
                    }
                    else
                    {
                        content += $"\t\tCommand.Parameters.AddWithValue(\"@{Column.Name}\", {Column.Name});\n\n";
                    }
                }
            }
            return content + "\n\n";
        }


        public string DataAccessAddNewMethod()
        {

            string content = "";
            content += $"\n\n\tpublic static int AddNew{_Table.TableName}({ParametersList(false, true, false, false)}) {{\n";

            content += $"\t\t{_Table.GetFullPrimaryKey()} = -1;\n";
            content += $"\t\tstring query = $\"INSERT INTO {_Table.TableName} ({ParametersList(false, false, false)})" +
                        $"VALUES ({ParametersList(false, false, true)}); SELECT SCOPE_IDENTITY();\";\n";


            content += "\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += HandleNullableValues();
            content += "\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tobject result = Command.ExecuteScalar();\n";

            content += "\t\t\t\t\t\tif (result != null && int.TryParse(result.ToString(), out int InsertedID))\n";
            content += "\t\t\t\t\t\t{\n";
            content += $"\t\t\t\t\t\t\t{_Table.GetPrimaryKeyName()} = InsertedID;\n";
            content += "\t\t\t\t\t\t}\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while adding a new record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";

            content += $"\n\t\t\treturn {_Table.GetPrimaryKeyName()};\n";
            content += "\t\t}\n";

            return content;
        }


        public string HandleUpdateQuery()
        {
            string content = $"\"UPDATE {_Table.TableName} ";
            string PK = clsTables.GetPrimaryKey(_Table.TableName);

            foreach (clsAttributes prop in _Table.AttributesList)
            {
                if (prop.IsPrimaryKey)
                {
                    continue;
                }
                content += $"SET {prop.Name} = @{prop.Name}, ";
            }
            if (content.EndsWith(", "))
            {
                // Length = Steps so it's Last 2 Chars
                content = content.Substring(0, content.Length - 2);
            }

            content += $" WHERE {PK} = @{PK};\"\n";
            return content;
        }

        public string DataAccessUpdateMethod()
        {

            string content = "";
            content += $"public static bool Update{_Table.TableName}({ParametersList(true, true, false)}) {{\n";

            content += "\t\tint RowsAffected = -1;\n";
            content += $"\t\tstring query = {HandleUpdateQuery()};\n";


            content += "\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += HandleNullableValues();
            content += "\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tRowsAffected = Command.ExecuteNonQuery();\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while Updating a record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";
            content += "\t\t return (RowsAffected > 0);\n";
            content += "\t}";
            return content;
        }
        public string DataAccessDeleteMethod()
        {

            string content = "";
            content += $"\n\n\n\tpublic static bool Delete{_Table.TableName}({_Table.GetFullPrimaryKey()}) {{\n";

            content += "\n\t\tint RowsAffected = -1;\n";
            content += $"\n\t\tstring query = \" DELETE FROM {_Table.TableName} WHERE {_Table.GetPrimaryKeyName()} = @{_Table.GetPrimaryKeyName()}\";\n";


            content += "\n\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += $"\t\t\tCommand.Parameters.AddWithValue(\"@{_Table.GetPrimaryKeyName()}\", {_Table.GetPrimaryKeyName()});\n";

            content += "\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tRowsAffected = Command.ExecuteNonQuery();\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while Deleting a record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";
            content += "\t\t return (RowsAffected > 0);\n";
            content += "\t}";

            return content;
        }

        public string HandleFindMethodFillInfo()
        {
            string content = "";
            foreach (clsAttributes prop in _Table.AttributesList)
            {
                if (prop.IsPrimaryKey)
                {
                    continue;
                }

                if (prop.IsNullable)
                {
                    content += $"\t\tif (Reader[\"{prop.Name}\"] != DBNull.Value) {{ \n" +
                        $"\t\t{prop.Name} = ({prop.CSharpType})Reader[{prop.Name}];\n" +
                        $"\t\t}}\n" +
                        $"\t\telse {{\n" +
                        $"\t\t{prop.Name} = {prop.DefaultValue};\n" +
                        $"\t\t}}\n";
                }
                else
                {
                    content += $"\t\t\t{prop.Name} = ({prop.CSharpType})Reader[\"{prop.Name}\"];\n";
                }

            }
            return content;
        }

        public string DataAccessFindMethod()
        {

            string content = "";
            content += $"\n\n\n\t\tpublic static bool Find({_Table.GetFullPrimaryKey()}, {AddNewAndUpdateParameterList(false, true, false)}) {{\n";

            content += "\t\tbool IsFound= false;\n";
            content += $"\t\tstring query = \"select * from {_Table.TableName} where {_Table.GetPrimaryKeyName()} = @{_Table.GetPrimaryKeyName()}\";\n";


            content += "\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += $"\t\t\t Command.Parameters.AddWithValue(\"@{_Table.GetPrimaryKeyName()}\", {_Table.GetPrimaryKeyName()});";
            content += "\n\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tSqlDataReader Reader = Command.ExecuteReader();\n";
            content += "\t\t\t\t\tif (Reader.Read()) {\n\n";
            content += "\t\t\t\t\tIsFound = true;\n";
            content += $"\t\t\t\t\t{HandleFindMethodFillInfo()}\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\tReader.Close();";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while Find a record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";
            content += "\t\t return IsFound;\n";
            content += "\t}";
            return content;
        }

        public string DataAccessExistMethod()
        {
            string content = "";

            content += $"\n\n\n\tpublic static bool Does{_Table.TableName}Exists({_Table.GetFullPrimaryKey()}) {{\n";

            content += "\n\t\tbool IsFound = false;\n";
            content += $"\n\t\tstring query = \" SELECT Found = 1 FROM {_Table.TableName} WHERE {_Table.GetPrimaryKeyName()} = @{_Table.GetPrimaryKeyName()} \";\n";


            content += "\n\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += $"\t\t\tCommand.Parameters.AddWithValue(\"@{_Table.GetPrimaryKeyName()}\", {_Table.GetPrimaryKeyName()});\n";

            content += "\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tobject result = Command.ExecuteScalar();\n";
            content += "\t\t\t\t\tif (result != null) {\n";
            content += "\t\t\t\tIsFound = true;\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while Deleting a record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";
            content += "\t\t return IsFound;\n";
            content += "\t}";

            return content;
        }

        public string DataAccessGetListMethod()
        {
            string content = "";


            content += $"\n\n\n\tpublic static DataTable GetAll{_Table.TableName}({_Table.GetFullPrimaryKey()}) {{\n";

            content += "\n\t\tDataTable dt = new DataTable();\n";
            content += $"\n\t\tstring query = \" SELECT * FROM {_Table.TableName}\";\n";


            content += "\n\n\t\tusing (SqlConnection Connection = new SqlConnection(clsDataAccessSettings.ConnectionString))\n";
            content += "\t\t{\n";
            content += "\t\t\tusing (SqlCommand Command = new SqlCommand(query, Connection))\n";
            content += "\t\t\t{\n";
            content += "\t\t\t\ttry\n";
            content += "\t\t\t\t{\n";
            content += "\t\t\t\t\tConnection.Open();\n";
            content += "\t\t\t\t\tSqlDataReader Reader = Command.ExecuteReader();\n";
            content += "\t\t\t\t\tif (Reader.HasRows) {\n";
            content += "\t\t\t\t\tdt.Load(Reader);";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t\t\tcatch (Exception ex)\n";
            content += "\t\t\t\t\t{\n";
            content += "\t\t\t\t\t\t// Log exception or handle accordingly\n";
            content += "\t\t\t\t\t\tthrow new ApplicationException(\"An error occurred while Deleting a record.\", ex);\n";
            content += "\t\t\t\t\t}\n";
            content += "\t\t\t\t}\n";
            content += "\t\t\t}\n";
            content += "\t\t return dt;\n";
            content += "\t}";

            return content;
        }



        public string DataAccessCommonMethods()
        {




            string content = "";
            content += DataAccessAddNewMethod();
            content += DataAccessUpdateMethod();
            content += DataAccessDeleteMethod();
            content += DataAccessFindMethod();
            content += DataAccessExistMethod();
            content += DataAccessGetListMethod();
            return content;
        }

        public string DataAccessFooterContent()
        {

            string content = "\t\t}\n\t}";
            return content;


        }


        public string WriteSettingsFile()
        {
            string content = "";
            content += DataAccessHeaderContent("clsDataAccessSettings");
            content += $"\n\n\t\tpublic static string ConnectionString {{get;set;}} = \"Server=.;Database={clsDataAccessSettings.Database};User Id={clsDataAccessSettings.Username}; Password={clsDataAccessSettings.Password}\"; \n";
            content += DataAccessFooterContent();
            return content;
        }





        public void GenerateDataAccessLayerFiles()
        {



            string content = "";
            // Complete Here....
            content += DataAccessHeaderContent();
            content += DataAccessCommonMethods();
            content += DataAccessFooterContent();


            // Generating DataAccess Files.

            string SettingsFileContent = "";

            SettingsFileContent = WriteSettingsFile();

            HandleFolder("DataAccess", content);

            if (!File.Exists(Path.Combine(FolderPath, "DataAccess", "_Layer", "clsDataAccessSettings")))
            {

                using (FileStream fs = File.Create(Path.Combine(FolderPath, "DataAccess_Layer", "clsDataAccessSettings.cs"))) { }

            }
            File.WriteAllText(Path.Combine(FolderPath, "DataAccess_Layer", "clsDataAccessSettings.cs"), SettingsFileContent);

        }

        public void Generate(string folderPath)
        {
            FolderPath = folderPath;

            // Create the solution

            string solutionName = "CodeGenerator";
            string dataAccessProjectName = "DataAccess_Layer";
            string businessProjectName = "Business_Layer";

            // Create a new solution
            RunCommand("dir", FolderPath);
            RunCommand("dotnet new sln --name MySolution", FolderPath);

            RunCommand($"dotnet new classlib --name {dataAccessProjectName}", FolderPath);
            RunCommand($"dotnet new classlib --name {businessProjectName}", FolderPath);

            // Add class libraries to the solution
            RunCommand($"dotnet sln MySolution.sln add {dataAccessProjectName}/{dataAccessProjectName}.csproj", FolderPath);
            RunCommand($"dotnet sln MySolution.sln add {businessProjectName}/{businessProjectName}.csproj", FolderPath);





            // List of tables
            List<clsTables> TablesList = clsTables.GetAllTables();
            foreach (clsTables TB in TablesList)
            {
                this.Table = TB;
                GenerateBusinessLayerFiles();
                GenerateDataAccessLayerFiles();
            }
        }


        public void RunCommand(string command, string workingDirectory)
        {
            // Set up the process
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory // Set the working directory here
            };

            using (var process = Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"Output:\n{output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error:\n{error}");
                }
            }
        }


    }


}
