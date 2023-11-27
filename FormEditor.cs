/**********************************************************************/
/* Copyright (c) 2023 Carpe Diem Software Developing by Alex Versetty */
/* http://carpediem.0fees.us                                          */
/**********************************************************************/

using CDSD.Forms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace TrayTextPaste
{
    public partial class FormEditor : Form
    {
        const int maxStrCount = 5000;

        public FormEditor()
        {
            InitializeComponent();
            Icon = Properties.Resources.AppIcon;
            textBox1.Text = ConfigManager.Current.Strings;
        }

        private void save_Click(object sender, EventArgs e)
        {
            string[] sep = { "\r\n" };
            var array = textBox1.Text.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            
            if (array.Count() > maxStrCount)
            {
                MessageBox.Show($"Слишком много строк (>{maxStrCount})");
            }
            else
            {
                ConfigManager.Current.Strings = textBox1.Text;
                
                try
                {
                    ConfigManager.save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка записи файла конфигурации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Close();
            }
        }

        private void insertTab_Click(object sender, EventArgs e)
        {
            textBox1.Paste("{tab}");
        }

        private void changeTextIndent_Click(object sender, EventArgs e)
        {
            var text = textBox1.SelectedText;
            if (string.IsNullOrEmpty(text)) return;

            string[] sep = { "\r\n" };
            var strArray = text.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (HaveStartSpaces(text))
            {
                for (int i = 0; i < strArray.Count(); i++) strArray[i] = strArray[i].TrimStart(new char[] { ' ' });
            }
            else
            {
                for (int i = 0; i < strArray.Count(); i++) strArray[i] = "    " + strArray[i];
            }

            text = string.Join("\r\n", strArray);
            textBox1.Paste(text);
        }

        private bool HaveStartSpaces(string str)
        {
            var strTrimmed = str.TrimStart(new char[] { ' ' });
            return str.Length - strTrimmed.Length > 1;
        }

        private void about_Click(object sender, EventArgs e)
        {
            var f = new FAbout();
            f.ShowDialog();
        }
    }
}
