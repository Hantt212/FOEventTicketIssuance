using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using NLog;
using POSPatronImage.Support;
using TableTransactionRolling.Support;

namespace POSPatronImage
{
    public partial class FOEventTicketIssuance : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool oem1 = false;
        private bool oemQuestion = false;
        private bool shiftKey = false;
        private readonly List<string> keys = new List<string>();
        private readonly Function function = new Function();
        private DataTable transTable;
        private DataTable printedTransTable;
        int ID = 0;
        ReportViewer reportViewer;
        public FOEventTicketIssuance()
        {
            InitializeComponent();

            //this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            //this.tabControl1.DrawItem += new DrawItemEventHandler(this.tabControl1_DrawItem);

            //this.tabPagePrintedHistory.Enabled = true;

            this.btnPrinterList.Visible = ConfigurationManager.AppSettings["PrinterIsShown"] == "1";
            panelProgramInfo.Visible = true;
            // Log form initialization
            Logger.Info("TableTransactions form initialized.");
        }



        public List<string> GetLocalPrinterNames()
        {
            List<string> printerNames = new List<string>();

            // Get the list of installed printers
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printerNames.Add(printer);
            }

            return printerNames;
        }




        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FOEventTicketIssuance_Load(object sender, EventArgs e)
        {
            string reportPath = @"Reports\FOEventTicket.rdlc";
            // DataTable dt = function.AddFOEventTicket();
            reportViewer = new ReportViewer();
            DataTable dt = function.LoadPreFOEventTicket();
            ID = Convert.ToInt32(dt.Rows[0]["ID"]);
            function.LoadRDLCToPanel(reportPath, pnlTicket, dt, reportViewer);
        }

        private void button_Print_Click(object sender, EventArgs e)
        {
            try
            {
                string printerName = new PrinterSettings().PrinterName;
                byte[] reportBytes = reportViewer.LocalReport.Render("IMAGE", function.DeviceInfo());

                Logger.Info("Report rendered successfully.");

                // Perform silent EMF printing
                function.PrintEMF(reportBytes, printerName);
                function.UpdateFOEventTicket(ID);
                FOEventTicketIssuance_Load(sender, e);
            }
            catch (Exception ex)
            {
                // Log the exception and display a user-friendly message
                Logger.Error(ex, "An error occurred during the reprint process.");
                MessageBox.Show("An error occurred. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void pnlTicket_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
