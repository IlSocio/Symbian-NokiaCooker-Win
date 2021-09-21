using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace FuzzyByte.Forms
{
    public partial class TextAreaAdv : UserControl
    {
        public TextAreaAdv()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            richTextBox1.Text = string.Empty;
            // Code useful for performance profiling
            /*StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 50000; i++)
                sb.AppendLine(i + "TEST 123 123 123 TEST 123 123 123 TEST");
            richTextBox1.Text = sb.ToString();/**/

        }

        #region proxy properties
        public Color BackColorText
        {
            get
            {
                return richTextBox1.BackColor;
            }
            set
            {                
                richTextBox1.BackColor = value;
            }
        }

        public new Cursor Cursor
        {
            get
            {
                return richTextBox1.Cursor;
            }
            set
            {
                richTextBox1.Cursor = value;
            }
        }
        #endregion


        public void ScrollToEnd()
        {
            richTextBox1.SelectionStart = int.MaxValue;
            richTextBox1.ScrollToCaret();
        }


        public bool HasData()
        {
            return (richTextBox1.Text.Length > 0);
        }


        public void AddMessage(string message)
        {
            /* Moved to the Form for consistence with others GUI controls.
            if (this.richTextBox1.InvokeRequired)
            {
                MethodInvoker addMessageInvoker = delegate { AddMessage(message); };
                this.Invoke(addMessageInvoker);
             //   richTextBox1.Invoke(new DelAddMessage(AddMessage), new object[] { message });
                return;
            }*/
            richTextBox1.SelectionStart = int.MaxValue;
            richTextBox1.SelectedText = message + Environment.NewLine;
        }


        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text == string.Empty)
                return;
            string s = richTextBox1.Text;
            s = s.Replace("\n", Environment.NewLine);
            Clipboard.SetDataObject(s);
        }
    }
}
