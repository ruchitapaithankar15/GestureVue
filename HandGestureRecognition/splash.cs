using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

namespace HandGestureRecognition
{
    public partial class splash : Form
    {
        public splash()
        {
            InitializeComponent();
        }

        private void Spalash_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 1; i <= 100; i++)
            {
                // Wait 10 milliseconds.
                Thread.Sleep(10);
                // Report progress.
                backgroundWorker1.ReportProgress(i);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            Text = e.ProgressPercentage.ToString(CultureInfo.InvariantCulture);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(50);
            Hide();
            LOAD objLogin = new LOAD();
            objLogin.Show();

        }
    }
}
