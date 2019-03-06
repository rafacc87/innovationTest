using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SearchData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            // Clear DataSource
            dgvData.DataSource = null;

            // Open file dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Comma Separated Value (*.csv)|*.csv";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // schema.ini
                    generateSchema(openFileDialog.FileName);

                    // Read CSV
                    DataTable dt = readCSV(openFileDialog.FileName);

                    // Only with rows and 3 columns(min.)
                    if (dt.Rows.Count > 0 && dt.Columns.Count > 2)
                    {
                        // Rename columns
                        try
                        {
                            dt.Columns[0].ColumnName = "Name";
                            dt.Columns[1].ColumnName = "City";
                            dt.Columns[2].ColumnName = "Phone";
                        }
                        catch (Exception)
                        {

                        }

                        // Add the DataSource
                        dgvData.DataSource = dt;
                        // Enable Step 2
                        gbStep2.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("No rows found to list or did not contain 3 columns");
                        gbStep2.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Generate schema.ini
        /// </summary>
        /// <param name="filePath">File path of CSV</param>
        private void generateSchema(String filePath)
        {
            String path = Path.Combine(Path.GetDirectoryName(filePath), "schema.ini");

            // Create text of schema.ini
            using (StreamWriter sw = File.CreateText(path))
            {
                // CSV file
                sw.WriteLine("["+Path.GetFileName(filePath)+"]");
                // Delimited
                sw.WriteLine("Format=Delimited("+this.txtDelimiter.Text+")");
                // Header?
                sw.WriteLine("ColNameHeader=" + (this.cbHeader.Checked ? "True" : "False"));
            }
        }

        /// <summary>
        /// Read CSV from file
        /// </summary>
        /// <param name="filePath">Path of specified file</param>
        /// <returns>DataTable of CSV</returns>
        private DataTable readCSV(String filePath)
        {
            // Vars
            string pathOnly = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            // Header?
            string header = this.cbHeader.Checked ? "Yes" : "No";
            // Query
            string sql = @"SELECT * FROM [" + fileName + "]";

            // Connection
            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties='text;HDR=" + header + ";'"))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                // Generate DataTable
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // Search by Name and City
                (dgvData.DataSource as DataTable).DefaultView.RowFilter = String.IsNullOrEmpty(txtSearch.Text) ?
                    "" :
                    String.Format("Name LIKE '%{0}%' OR City LIKE '%{0}%'", txtSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter go to search
            if (e.KeyChar == 13) btnSearch_Click(null, null);
        }
    }
}
