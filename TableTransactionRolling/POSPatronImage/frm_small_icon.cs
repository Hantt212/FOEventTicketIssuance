﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using POSPatronImage;

namespace TableTransactionRolling
{
    public partial class frm_small_icon : Form
    {
        //For moving form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        //---------------------------------------------

        bool isShowing = false;
        POSPatronImage.FOEventTicketIssuance f1 = new POSPatronImage.FOEventTicketIssuance();
        int x;
        int y;

        public frm_small_icon()
        {
            InitializeComponent();            
        }  

        private void pictureBox1_Click(object sender, EventArgs e)
        {                
        }

        private void frm_small_icon_Load(object sender, EventArgs e)
        {  
            Bitmap bmpFrmBack = new Bitmap(pictureBox1.Image);

            CreateControlRegion(this, bmpFrmBack);
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            this.Left = rightmost.WorkingArea.Right - this.Width;
            this.Top = rightmost.WorkingArea.Bottom/2 - this.Height;
        }
        // Create and apply the given bitmap region on the supplied control
        public static void CreateControlRegion(Control control, Bitmap bitmap)
        {
            // Return if control and bitmap are null
            if (control == null || bitmap == null)
                return;

            // Set our control's size to be the same as the bitmap
            control.Width = bitmap.Width;
            control.Height = bitmap.Height;

            // Check if we are dealing with Form here
            if (control is System.Windows.Forms.Form)
            {
                // Cast to a Form object
                Form form = (Form)control;

                // Set our form's size to be a little larger that the bitmap just 
                // in case the form's border style is not set to none in the first 
                // place
                form.Width += 15;
                form.Height += 35;

                // No border
                form.FormBorderStyle = FormBorderStyle.None;

                // Set bitmap as the background image
                form.BackgroundImage = bitmap;

                // Calculate the graphics path based on the bitmap supplied
                GraphicsPath graphicsPath = CalculateControlGraphicsPath(bitmap);

                // Apply new region
                form.Region = new Region(graphicsPath);
            }

            // Check if we are dealing with Button here
            else if (control is System.Windows.Forms.Button)
            {
                // Cast to a button object
                Button button = (Button)control;

                // Do not show button text
                button.Text = "";

                // Change cursor to hand when over button
                button.Cursor = Cursors.Hand;

                // Set background image of button
                button.BackgroundImage = bitmap;

                // Calculate the graphics path based on the bitmap supplied
                GraphicsPath graphicsPath = CalculateControlGraphicsPath(bitmap);

                // Apply new region
                button.Region = new Region(graphicsPath);
            }
        }

        // Calculate the graphics path that representing the figure in the bitmap 
        // excluding the transparent color which is the top left pixel.
        private static GraphicsPath CalculateControlGraphicsPath(Bitmap bitmap)
        {
            // Create GraphicsPath for our bitmap calculation
            GraphicsPath graphicsPath = new GraphicsPath();

            // Use the top left pixel as our transparent color
            Color colorTransparent = bitmap.GetPixel(0, 0);

            // This is to store the column value where an opaque pixel is first found.
            // This value will determine where we start scanning for trailing 
            // opaque pixels.
            int colOpaquePixel = 0;

            // Go through all rows (Y axis)
            for (int row = 0; row < bitmap.Height; row++)
            {
                // Reset value
                colOpaquePixel = 0;

                // Go through all columns (X axis)
                for (int col = 0; col < bitmap.Width; col++)
                {
                    // If this is an opaque pixel, mark it and search 
                    // for anymore trailing behind
                    if (bitmap.GetPixel(col, row) != colorTransparent)
                    {
                        // Opaque pixel found, mark current position
                        colOpaquePixel = col;

                        // Create another variable to set the current pixel position
                        int colNext = col;

                        // Starting from current found opaque pixel, search for 
                        // anymore opaque pixels trailing behind, until a transparent
                        // pixel is found or minimum width is reached
                        for (colNext = colOpaquePixel; colNext < bitmap.Width; colNext++)
                            if (bitmap.GetPixel(colNext, row) == colorTransparent)
                                break;

                        // Form a rectangle for line of opaque pixels found and 
                        // add it to our graphics path
                        graphicsPath.AddRectangle(new Rectangle(colOpaquePixel,
                                                   row, colNext - colOpaquePixel, 1));

                        // No need to scan the line of opaque pixels just found
                        col = colNext;
                    }
                }
            }

            // Return calculated graphics path
            return graphicsPath;
        }

        private void frm_small_icon_MouseDown(object sender, MouseEventArgs e)
        {
            
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isShowing == false)
            {
                f1.Show();
                isShowing = true;
            }
            else
            {
                f1.Hide();
                isShowing = false;
            }

            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
