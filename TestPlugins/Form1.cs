using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TestPlugins
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();

            if (args.Length < 4)
            {
                // Launched directly by user
                return;
            }
            // Launched through Nokia Cooker
            lblFile.Text = args[0];
            lblPath.Text = args[1];
            lblFilename.Text = args[2];
            lblType.Text = args[3];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


    }
}
