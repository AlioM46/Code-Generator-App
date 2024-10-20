using Business_Layer_Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;





namespace Presentation_Layer
{
    public partial class Form1 : Form
    {


        public string Database {  get; set; }
        public string Username {  get; set; }
        public string Password { get; set; }


        public Form1()
        {
            InitializeComponent();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Are you sure you want to generate business and data access layers?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            string FolderPath = "";

            using (var FolderDialog = new FolderBrowserDialog())
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Get the selected folder path
                    FolderPath = FolderDialog.SelectedPath;
                    // Do something with the folder path
                }
            }
            if (FolderPath == "")
            {
                MessageBox.Show("Please choose a folder to save the generated files in",
                    "Choose Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }




            Database = tbDatabaseName.Text.Trim();
            Username= tbUsername.Text.Trim();
            Password= tbPassword.Text.Trim();
            // check if DB exists.

            if (!clsCheckDatabase.DoesDataBaseExist(Database)) {

                MessageBox.Show("Database Does Not Exist.", "",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // check if Credentials is Valid.
            if (!clsCheckDatabase.IsValidCredentials(Database, "sa","sa123456"))
            {

                MessageBox.Show("InValid username and Password.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            clsCodeGenerator Gen = new clsCodeGenerator();
            Gen.Generate(FolderPath);



        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
