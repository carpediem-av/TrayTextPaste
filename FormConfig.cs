/**********************************************************************/
/* Copyright (c) 2023 Carpe Diem Software Developing by Alex Versetty */
/* http://carpediem.0fees.us                                          */
/**********************************************************************/

using System;
using System.Windows.Forms;

namespace TrayTextPaste
{
    public partial class FormConfig : Form
    {
        public FormConfig()
        {
            InitializeComponent();
            Icon = Properties.Resources.AppIcon;

            foreach (RadioButton rb in groupBox1.Controls)
            {
                if (rb.Text == ConfigManager.Current.CopySwitchPasteHotkey)
                {
                    rb.Checked = true;
                }
                else
                {
                    rb.Checked = false;
                }
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            foreach (RadioButton rb in groupBox1.Controls)
            {
                if (rb.Checked)
                {
                    ConfigManager.Current.CopySwitchPasteHotkey = rb.Text;
                    break;
                }
            }

            try
            {
                ConfigManager.save();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка записи файла конфигурации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rbHotkey_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}
