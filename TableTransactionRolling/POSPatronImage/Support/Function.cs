using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using System.Runtime.InteropServices;
using Microsoft.ReportingServices.Interfaces;
using System.Net;


namespace POSPatronImage.Support
{
    public class Function
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly SqlConnection cnCon;
        public string DeviceInfo()
        {
            return  @"
                        <DeviceInfo>
                            <OutputFormat>EMF</OutputFormat>
                            <PageWidth>3.15in</PageWidth>         <!-- 80mm paper -->
                            <PageHeight>13in</PageHeight>         <!-- Arbitrary tall height for receipts -->
                            <MarginTop>0.1in</MarginTop>
                            <MarginLeft>0.1in</MarginLeft>
                            <MarginRight>0.1in</MarginRight>
                            <MarginBottom>0.1in</MarginBottom>
                        </DeviceInfo>";
        }
        public Function()
        {
            try
            {
                Logger.Info("Function class initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing Function class.");
                throw;
            }
        }
        private string GetDecryptedConnectionString()
        {
            string secretKey = ConfigurationManager.AppSettings["SecretKey"];
            string encryptedCNCon = ConfigurationManager.ConnectionStrings["CNConnection"].ConnectionString;
            return EncryptionHelper.DecryptString(encryptedCNCon, secretKey);
        }
      
        public void LoadRDLCToPanel(string reportPath, Panel targetPanel, DataTable dataSource, ReportViewer reportViewer)
        {
            // Ensure panel is clear
            targetPanel.Controls.Clear();

            // Create the ReportViewer
           // ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.Dock = DockStyle.Fill;
            reportViewer.ShowToolBar = false;

            // Load RDLC report
            // reportViewer.LocalReport.ReportPath = Path.Combine(Application.StartupPath, reportPath);
            reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, reportPath);

            // Set the data source (use correct dataset name from RDLC file)
            ReportDataSource rds = new ReportDataSource("FOEventTicketDataSet", dataSource);
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);

            // Important: call RefreshReport AFTER setting everything
            reportViewer.RefreshReport();

            // Add to panel
            targetPanel.Controls.Add(reportViewer);
        }

        public DataTable UpdateFOEventTicket(int ID)
        {
            string connectionString = "data source=10.21.3.10;initial catalog=FOPortal;user id=fo.user;password=Password1@;trustservercertificate=True;MultipleActiveResultSets=True";
            DataTable resultTable = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("UpdateFOEventTicket", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", ID);

                // Fill the DataTable with the SELECT result from the SP
                adapter.Fill(resultTable);
            }

            return resultTable;
        }

        public DataTable LoadPreFOEventTicket()
        {
            string connectionString = "data source=10.21.3.10;initial catalog=FOPortal;user id=fo.user;password=Password1@;trustservercertificate=True;MultipleActiveResultSets=True";
            DataTable resultTable = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("AddFOEventTicketNew", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                // Add the parameter correctly with type
                cmd.Parameters.Add(new SqlParameter("@CompName", SqlDbType.NVarChar, 50)
                {
                    Value = System.Net.Dns.GetHostName()
                });
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.Fill(resultTable);
            }

            return resultTable;
        }


        public void PrintEMF(byte[] emfBytes, string printerName)
        {
            try
            {
                using (MemoryStream emfStream = new MemoryStream(emfBytes))
                {
                    // Create a Metafile from the stream (ensure the stream position is reset)
                    emfStream.Position = 0;
                    using (Metafile pageImage = new Metafile(emfStream))
                    {
                        PrintDocument printDoc = new PrintDocument();
                        printDoc.PrinterSettings.PrinterName = printerName;

                        if (!printDoc.PrinterSettings.IsValid)
                        {
                            MessageBox.Show("Invalid printer name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Logger.Error("Invalid printer name.");
                            return;
                        }

                        printDoc.PrintPage += (sender, e) =>
                        {
                            // Print the page image on the printer
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                            e.HasMorePages = false; // Print only one page
                        };

                        // Start printing
                        printDoc.Print();
                    }
                }
            }
            catch (ExternalException ex)
            {
                Logger.Error(ex, "An error occurred while trying to create or use the Metafile for printing.");
                MessageBox.Show("An error occurred while trying to print. Please try again or contact support.",
                                "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "A general error occurred while printing.");
                MessageBox.Show("An error occurred while trying to print. Please try again or contact support.",
                                "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
