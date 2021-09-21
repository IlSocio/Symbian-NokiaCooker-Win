using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Firmware;
using System.Runtime.InteropServices;
using FuzzyByte.Utils;


namespace FuzzyByte.NokiaCooker
{
    public partial class UnlockRofsDialog : Form
    {
        [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd,
                      String textSubAppName, String textSubIdList);

        protected override void OnHandleCreated(EventArgs e)
        {
            SetWindowTheme(this.Handle, "", "");
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 161;
            const int WM_SYSCOMMAND = 274;
            const int HTCAPTION = 2;
            const int SC_MOVE = 61456;

            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam.ToInt32() == SC_MOVE))
            {
                return;
            }

            if ((m.Msg == WM_NCLBUTTONDOWN) && (m.WParam.ToInt32() == HTCAPTION))
            {
                return;
            }

            base.WndProc(ref m);
        } 


        private bool _changed = false;
        private List<byte> iValues;

        protected UnlockRofsDialog()
        {
            InitializeComponent();
        }

        public UnlockRofsDialog(List<byte> values)
        {
            InitializeComponent();
            iValues = values;

            textBox1.Enabled = (values.Count > 0);
            textBox2.Enabled = (values.Count > 1);
            textBox3.Enabled = (values.Count > 2);
            if (textBox1.Enabled)
            {
                textBox1.Text = BytesUtils.ToHex(values[0]);
            }
            if (textBox2.Enabled)
            {
                textBox2.Text = BytesUtils.ToHex(values[1]);
            }
            if (textBox3.Enabled)
            {
                textBox3.Text = BytesUtils.ToHex(values[2]);
            }

            textBox1.GotFocus += new EventHandler(textBox1_GotFocus);
            textBox2.GotFocus += new EventHandler(textBox1_GotFocus);
            textBox3.GotFocus += new EventHandler(textBox1_GotFocus);

            textBox1.LostFocus += new EventHandler(textBox1_LostFocus);
            textBox2.LostFocus += new EventHandler(textBox1_LostFocus);
            textBox3.LostFocus += new EventHandler(textBox1_LostFocus);
        }


        public bool Changed
        {
            get
            {
                return _changed;
            }
            private set
            {
                _changed = value;
            }
        }

        private byte Rofs1Val
        {
            get
            {
                byte res = 0;
                byte.TryParse(textBox1.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out res);
                return res;
            }
        }

        private byte Rofs2Val
        {
            get
            {
                byte res = 0;
                byte.TryParse(textBox2.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out res);
                return res;
            }
        }

        private byte Rofs3Val
        {
            get
            {
                byte res = 0;
                byte.TryParse(textBox3.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out res);
                return res;
            }
        }

        private bool IsHex(char c)
        {
            //            bool isHex;
            //            isHex = int.TryParse(s, System.Globalization.NumberStyles.AllowHexSpecifier, null, out i);

            //            if (!(Regex.IsMatch(e.KeyChar.ToString(), "^[0-9a-fA-F]+$")))
            //                e.Handled = true;
            return (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9');
        }

        void textBox1_GotFocus(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }

        void textBox1_LostFocus(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            byte res = 0;
            byte.TryParse(tb.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out res);
            tb.Text = BytesUtils.ToHex(res);
        }        

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            bool isHexChar = IsHex(e.KeyChar);
            if (isHexChar && tb.SelectedText.Length == 0 && tb.Text.Length == 2)
            {
                e.Handled = true;
                return;
            }
            if (e.KeyChar == 8)
            {
                return;
            }
            e.Handled = !isHexChar;
        }

        private void UnlockRofsDialog_Load(object sender, EventArgs e)
        {
        }

        private void UnlockRofsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textBox1.Enabled && Rofs1Val != iValues[0])
            {
                iValues[0] = Rofs1Val;
                Changed = true;
            }
            if (textBox2.Enabled && Rofs2Val != iValues[1])
            {
                iValues[1] = Rofs2Val;
                Changed = true;
            }
            if (textBox3.Enabled && Rofs3Val != iValues[2])
            {
                iValues[2] = Rofs3Val;
                Changed = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

    }
}
