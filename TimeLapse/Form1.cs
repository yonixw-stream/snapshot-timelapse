using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLapse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                txtSavePath.Text = dlgSave.FileName;
            }
        }

        string dateSafeString(DateTime d)
        {
            return d.Year + "-" + d.Month + "-" + d.Day;
        }

        string timeSafeString(DateTime d)
        {
            return d.Hour + "-" + d.Minute + "-" + d.Second;
        }

        bool takePictures = false;
        int saveCounter = 0;
        TimeSpan snapInterval = TimeSpan.FromSeconds(0);
        DateTime lasTimeSaved = DateTime.Now;

        private void tmrSave_Tick(object sender, EventArgs e)
        {
            
            if (takePictures)
            {
                DateTime dateOfnewSave = DateTime.Now;
                pbInterval.Value = 
                    Math.Min(
                    (int)(
                        (dateOfnewSave - lasTimeSaved).TotalSeconds * 100 
                        / (int)numInterval.Value
                    ),100);

                if (dateOfnewSave > lasTimeSaved + snapInterval) {
                    saveCounter++;
                    string file = txtSavePath.Text
                        .Replace("%i", saveCounter.ToString())
                        .Replace("%t", timeSafeString(dateOfnewSave))
                        .Replace("%d", dateSafeString(dateOfnewSave))
                        ;

                    CaptureScreen(Screen.AllScreens[(int)numScreen.Value], file);
                    lasTimeSaved = dateOfnewSave;
                }
            }
        }


        private void CaptureScreen(Screen window, string file)
        {
            // SO? 10016769

            try
            {
                Rectangle s_rect = window.Bounds;
                using (Bitmap bmp = new Bitmap(s_rect.Width, s_rect.Height))
                {
                    using (Graphics gScreen = Graphics.FromImage(bmp))
                        gScreen.CopyFromScreen(s_rect.Location, Point.Empty, s_rect.Size);
                    bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex) {
                takePictures = false;
                MessageBox.Show("Error:\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            saveCounter = 0;
            pbInterval.Value = 0;
            snapInterval = TimeSpan.FromSeconds((int)numInterval.Value);
            takePictures = true;
            startClick.send();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            takePictures = false;
            stopClick.send();
        }

        GoogleAnalytics.gaScreenView myScreen;
        GoogleAnalytics.gaEvent startClick;
        GoogleAnalytics.gaEvent stopClick;
        private void Form1_Load(object sender, EventArgs e)
        {
            myScreen = Program.ga.preDefineScreenView("MainForm", "snaptshot-timelapse", "1.0.0");
            startClick = Program.ga.preDefineEvent("MainProcess", "Start", PositiveValue : (int)numInterval.Value);
            stopClick = Program.ga.preDefineEvent("MainProcess", "Stop", PositiveValue: (int)numInterval.Value);

            gaKeepAlive.Interval = (int)TimeSpan.FromMinutes(14).TotalMilliseconds;
            gaKeepAlive.Enabled = true;
        }

        private void gaKeepAlive_Tick(object sender, EventArgs e)
        {
            myScreen.send();
        }
    }
}
