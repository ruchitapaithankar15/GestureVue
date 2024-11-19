using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Diagnostics;
using System.Media;

namespace HandGestureRecognition
{
    public partial class Webcam : Form
    {

        #region Variables
        private Capture capWebCam;
        private HaarCascade haar, haar_eye;
        private HaarCascade _faces;
        private HaarCascade _eyes;
        int xMax;
        int yMax;
        Stopwatch stpWatch, stpWatch2;
        #endregion
        int eye_counter = 0;
        int speed_counter = 0;

        public Webcam()
        {
            InitializeComponent();
            _faces = new HaarCascade("haarcascade_frontalface_alt_tree.xml");

        }



        private void Webcam_Load(object sender, EventArgs e)
        {
            capWebCam = new Capture(0);
            haar = new HaarCascade("haarcascade_frontalface_alt_tree.xml");
            haar_eye = new HaarCascade("parojosG.xml");
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool falg = false;
            using (Image<Bgr, Byte> nextFrame = capWebCam.QueryFrame())
            {
                if (nextFrame != null)
                {
                    Image<Gray, Byte> grayframe = nextFrame.Convert<Gray, Byte>();

                    var faces = grayframe.DetectHaarCascade(haar, 1.4, 4, HAAR_DETECTION_TYPE.FIND_BIGGEST_OBJECT, new Size(nextFrame.Width / 8, nextFrame.Height / 8))[0];
                    if (faces == null || faces.Length <= 0)
                    {
                        if (falg == false)
                        {
                            axWindowsMediaPlayer1.Ctlcontrols.pause();
                            falg = true;
                        }
                        //media
                    }
                    else
                    {
                        //axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Videos\\Wildlife.wmv";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                        falg = true;
                        //media


                    }
                    foreach (var face in faces)
                    {
                        try
                        {
                            nextFrame.Draw(face.rect, new Bgr(555, double.MaxValue, 250), 1);
                            grayframe.ROI = face.rect;
                            var eyes = grayframe.DetectHaarCascade(haar_eye, 1.15, 4, HAAR_DETECTION_TYPE.FIND_BIGGEST_OBJECT, new Size(20, 20))[0];
                            //   MCvAvgComp[][] leftEyesDetected = grayFrame.DetectHaarCascade(_eyes, 1.15, 0, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                            foreach (var eye in eyes)
                            {

                                Rectangle rect = new Rectangle(eye.rect.X + face.rect.X, eye.rect.Y + face.rect.Y, eye.rect.Width, eye.rect.Height);
                                nextFrame.Draw(rect, new Bgr(Color.AliceBlue), 1);
                            }
                        }

                        catch (Exception ee)
                        { }
                    }
                    webcamBox.Image = nextFrame;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Video Files (*.avi)|*.avi|All Files (*.*)|*.*";
            // openFileDialog1.ShowDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string SourcePath = openFileDialog1.FileName;
                string DestinationPath = Application.StartupPath + "\\Videos\\";
                // axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Videos\\Wildlife.wmv";
                axWindowsMediaPlayer1.URL = SourcePath;
            }
        }




   
    

        private void button2_Click_2(object sender, EventArgs e)
        {
             timer1.Stop();
             capWebCam.Dispose();
            //falg = true;
            this.Close();
        }

    }

}
