using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// 2019 Maken it so.
// https://www.youtube.com/c/makenitso

namespace SpleeterGui
{
    public partial class Form1 : Form
    {
        private string stem_count = "2";
        private string mask_extension = "average";
        private string storage = "";
        private int files_remain = 0;
        private List<string> files_to_process = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        public void LoadStuff()
        {
            txt_output_directory.Text = Properties.Settings.Default.output_location;
            storage = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpleeterGUI";
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (files_remain == 0)
            {
                if (txt_output_directory.Text == "")
                {
                    MessageBox.Show("Please select an output directory");
                    return;
                }

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                files_remain = 0;
                foreach (string file in files)
                {
                    files_to_process.Add(file);
                    files_remain++;
                }
                textBox1.AppendText("Starting processing of all songs\r\n");
                progressBar1.Maximum = files_remain + 1;
                progressBar1.Value = 0; 
                progress_txt.Text = "Starting..." + files_remain + " songs remaining";
                next_song();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }
   

        private void next_song()
        {
            if (files_remain > 0)
            {
                string pyPath = storage + @"\python\python.exe";
                progressBar1.Value = progressBar1.Value + 1;
                Console.WriteLine("starting " + files_to_process[0]);
                System.IO.File.WriteAllText(storage + @"\config.json", get_config_string());
                textBox1.AppendText("Processing " + files_to_process[0] + "\r\n");
                progress_txt.Text = "Working..." + files_remain + " songs remaining";
                
                ProcessStartInfo processStartInfo = new ProcessStartInfo(pyPath, @" -W ignore -m spleeter separate -i " + (char)34 + files_to_process[0] + (char)34 + " -o " + (char)34 + txt_output_directory.Text + (char)34 + " -p " + (char)34 + storage + @"\config.json" + (char)34);
                processStartInfo.WorkingDirectory = storage;

                processStartInfo.UseShellExecute = false;
                processStartInfo.ErrorDialog = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;

                files_to_process.Remove(files_to_process[0]);

                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(ProcessExited);
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
                bool processStarted = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                progress_txt.Text = "idle";
                textBox1.AppendText("Finished processing all songs\r\n");
                progressBar1.Value = progressBar1.Maximum;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Invoke((Action)(() =>
            {
                files_remain--;
                next_song();
            }));
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txt_output_directory.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.output_location = txt_output_directory.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                txt_output_directory.Text = "";
            }
        }

        public void runCmd(String command)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = command;
            Process cmd = Process.Start(cmdsi);
            while (!cmd.HasExited)
            {
                Application.DoEvents();
            }
            cmd.Close();
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string get_config_string()
        {
            string readText = File.ReadAllText(storage + @"\" + stem_count + "stems.json");
            if (mask_extension == "average")
            {
                readText = readText.Replace("zeros", "average");
            }
            return readText;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mask_extension = checkBox1.Checked ? "average" : "zeros";
        }

       

        private void spleeterGithubPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/deezer/spleeter");
        }

        private void makenItSoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/user/mitchellcj/videos");
        }

        private void stems2_Click(object sender, EventArgs e)
        {
            stem_count = "2";
        }

        private void stems4_Click(object sender, EventArgs e)
        {
            stem_count = "4";
        }

        private void stems5_Click(object sender, EventArgs e)
        {
            stem_count = "5";
        }
  
        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        private void helpFAQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://makenweb.com/spleeter_help.php");
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadStuff();
        }

        private void parts_btn2_Click(object sender, EventArgs e)
        {
            parts_label.Text = "Vocal + Accompaniment";
            parts_btn2.UseVisualStyleBackColor = false;
            parts_btn4.UseVisualStyleBackColor = true;
            parts_btn5.UseVisualStyleBackColor = true;
            stem_count = "2";
        }

        private void parts_btn4_Click(object sender, EventArgs e)
        {
            parts_label.Text = "Vocal + Bass + Drums + Other";
            parts_btn2.UseVisualStyleBackColor = true;
            parts_btn4.UseVisualStyleBackColor = false;
            parts_btn5.UseVisualStyleBackColor = true;
            stem_count = "4";
        }

        private void parts_btn5_Click(object sender, EventArgs e)
        {
            parts_label.Text = "Vocal + Bass + Drums + Piano + Other";
            parts_btn2.UseVisualStyleBackColor = true;
            parts_btn4.UseVisualStyleBackColor = true;
            parts_btn5.UseVisualStyleBackColor = false;
            stem_count = "5";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (files_remain == 0)
            {
                openFileDialog2.ShowDialog();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            if (files_remain == 0)
            {
                if (txt_output_directory.Text == "")
                {
                    MessageBox.Show("Please select an output directory");
                    return;
                }
                files_remain = 0;
                foreach (String file in openFileDialog2.FileNames)
                {
                    Debug.WriteLine("BBB"+ file);
                    files_to_process.Add(file);
                    files_remain++;
                }
                textBox1.AppendText("Starting processing of all songs\r\n");
                progressBar1.Maximum = files_remain + 1;
                progressBar1.Value = 0;
                progress_txt.Text = "Starting..." + files_remain + " songs remaining";
                next_song();
            }
        }
    }
}
