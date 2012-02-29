//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="Andrew Beaton">
//     Copyright (c) Andrew Beaton. All rights reserved. 
// </copyright>
//-----------------------------------------------------------------------
namespace WindowsProductKeyFinder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
     
    /// <summary>
    /// Main form class.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Initializes a new instance of the Form1 class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }
          
        private void Form1_Load(object sender, EventArgs e)
        {
            OperatingSystemTextBox.Text = OSInfo.Description();

            WindowsEditions edition = OSInfo.WindowsEdition();

            string productKeyText = string.Empty;

            if (edition == WindowsEditions.NotSupported) 
            {
                productKeyText = "Not found. Windows version is not supported.";
            } 
            else 
            {
                productKeyText = KeyFinder.GetProductKey(edition);
            }

            ProductKeyTextBox.Text = productKeyText;
        }

        private void CopyKeyToClipboard()
        {
            if (ProductKeyTextBox.Text != string.Empty)
            { 
                Clipboard.SetDataObject(ProductKeyTextBox.Text); 
            }
        } 

        private void ExitApplication()
        {
            Application.Exit();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CopyKeyToClipboard();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExitApplication();
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            this.CopyKeyToClipboard();
        }

        private void CopyButton_MouseHover(object sender, EventArgs e)
        {
            this.StatusLabel.Text = "Copy product key to clipboard.";
        }

        private void CopyButton_MouseLeave(object sender, EventArgs e)
        {
            this.StatusLabel.Text = string.Empty;
        }  

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.ExitApplication();
        }

        private void ExitButton_MouseHover(object sender, EventArgs e)
        {
            this.StatusLabel.Text = "Close the application.";
        }

        private void ExitButton_MouseLeave(object sender, EventArgs e)
        {
            this.StatusLabel.Text = string.Empty;
        }
    }
}
