using hlsDownload.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace hlsDownload
{
    public partial class Form1 : Form
    {
        public bool checkClear = false;
        string newText = "";
        public Form1()
        {
            InitializeComponent();
            AddClipboardFormatListener(this.Handle);
        }

        public void ExecuteCmd(string link, int output, bool check)
        {
            string saveFolder = tbFolder.Text;
            string name = tbName.Text;
            string command = "";
            if (name == "")
            {
                command = $"/c ffmpeg -v warning -stats -i {link} -c copy -bsf:a aac_adtstoasc {saveFolder}/{output}.mp4";
            }
            else
            {
                command = $"/c ffmpeg -v warning -stats -i {link} -c copy -bsf:a aac_adtstoasc {saveFolder}/\"{name} - {output}\".mp4";
            }
            var process = Process.Start("CMD.exe", command);
            if (check)
                process.WaitForExit();
            return;
        }

        private void selectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select folder to download file";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                tbFolder.Text = fbd.SelectedPath;
            }
        }

        private void btDownload_Click(object sender, EventArgs e)
        {
            int i = int.Parse(numericUpDown1.Value.ToString());
            if (tbFolder == null)
            {
                MessageBox.Show("Pls choose download folder");
            }
            var lines = richTextBox1.Text.Split('\n').ToList();
            foreach (var line in lines)
            {
                if (checkBox3.Checked)
                    ExecuteCmd(line, i, true);
                else
                    ExecuteCmd(line, i, false);
                i++;
            }
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            checkClear = true;
            richTextBox1.Clear();
            richTextBox1.Focus();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (checkClear == true)
            {
                checkClear = false;
                return;
            }
            else
                richTextBox1.AppendText("\n");
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text += Clipboard.GetText();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkClear = true;
            richTextBox1.Clear();
            richTextBox1.Focus();
        }

        //---add data from clipboard to rtb---
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                if (checkBox1.Checked == true)
                {
                    if (newText == Clipboard.GetText()) //sometime clipboard update link 2 time so this will remove duplicate url
                    {
                        return;
                    }
                    else
                    {
                        newText = Clipboard.GetText();
                        if (IsValidUrl(newText))
                        {
                            richTextBox1.AppendText(newText);
                            return;
                        }
                    }
                }
            }
        }

        private bool IsValidUrl(string txt) //check vaild url
        {
            Uri uriResult;
            return Uri.TryCreate(txt, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveClipboardFormatListener(this.Handle);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                this.TopMost = true;
            }
            else this.TopMost = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
