using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using HandGestureRecognition.SkinDetector;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HandGestureRecognition
{
    public partial class Form1 : Form
    {

        IColorSkinDetector skinDetector;

        Image<Bgr, Byte> currentFrame;
        Image<Bgr, Byte> currentFrameCopy;

        Capture grabber;
        AdaptiveSkinDetector detector;

        int frameWidth;
        int frameHeight;

        Hsv hsv_min;
        Hsv hsv_max;
        Ycc YCrCb_min;
        Ycc YCrCb_max;

        Seq<Point> hull;
        Seq<Point> filteredHull;
        Seq<MCvConvexityDefect> defects;
        MCvConvexityDefect[] defectArray;
        MemStorage storage = new MemStorage();

        Rectangle handRect;
        MCvBox2D box;
        Ellipse ellip;
        static int vol = 0;

        public Form1()  // Main Form Constructor
        {
            InitializeComponent();  // Initialize the Components
            grabber = new Emgu.CV.Capture(0);   // Object to Capture the Frame
            grabber.QueryFrame();
            frameWidth = grabber.Width;
            frameHeight = grabber.Height;
            detector = new AdaptiveSkinDetector(1, AdaptiveSkinDetector.MorphingMethod.NONE); // Skin Detector Object
            hsv_min = new Hsv(0, 45, 0); // HSV Color Space .. 
            hsv_max = new Hsv(20, 255, 255);
            YCrCb_min = new Ycc(0, 131, 80); // YCrCb color Space 
            YCrCb_max = new Ycc(255, 185, 135);
            box = new MCvBox2D();
            ellip = new Ellipse();

            // Application.Idle += new EventHandler(FrameGrabber);                        // Start Frame Grabbing
        }


        //Method for grabbing Frames captured from the camera
        void FrameGrabber(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame(); // Capture current frame
            if (currentFrame != null) // Process only if we have a frame Grabbed
            {
                currentFrameCopy = currentFrame.Copy();

                skinDetector = new YCrCbSkinDetector(); //Skin Color detector Object Initialization

                Image<Gray, Byte> skin = skinDetector.DetectSkin(currentFrameCopy, YCrCb_min, YCrCb_max); // Skin region after Erosion & dilate

                ExtractContourAndHull(skin);   // Contour Extraction
                DrawAndComputeFingersNum();
                imageBoxSkin.Image = skin;
                imageBoxFrameGrabber.Image = currentFrame;
                int pauseTime = 1;
                System.Threading.Thread.Sleep(pauseTime);
            }
        }

        private void ExtractContourAndHull(Image<Gray, byte> skin) // Main Method for Region Detection
        {

            {
                //Find the Boundary pixels region
                Contour<Point> contours = skin.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
                Contour<Point> biggestContour = null;

                Double Result1 = 0;
                Double Result2 = 0;
                while (contours != null) //Find Biggest region frm image
                {
                    Result1 = contours.Area;
                    if (Result1 > Result2)
                    {
                        Result2 = Result1;
                        biggestContour = contours;
                    }
                    contours = contours.HNext;
                }
                //If  Biggest Contour Found then proceed
                if (biggestContour != null)
                {
                    //Draw Contour Region
                    //currentFrame.Draw(biggestContour, new Bgr(Color.DarkViolet), 2);
                    Contour<Point> currentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                    // currentFrame.Draw(currentContour, new Bgr(Color.LimeGreen), 2);
                    biggestContour = currentContour;

                    hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                    box = biggestContour.GetMinAreaRect();
                    PointF[] points = box.GetVertices();
                    handRect = box.MinAreaRect();
                    currentFrame.Draw(handRect, new Bgr(200, 0, 0), 1);
                    
                    Point[] ps = new Point[points.Length]; //Points
                    for (int i = 0; i < points.Length; i++)
                        ps[i] = new Point((int)points[i].X, (int)points[i].Y);

                    //currentFrame.DrawPolyline(hull.ToArray(), true, new Bgr(200, 125, 75), 2);
                    currentFrame.Draw(new CircleF(new PointF(box.center.X, box.center.Y), 3), new Bgr(200, 125, 75), 2);

                    PointF center;
                    float radius;

                    CvInvoke.cvMinEnclosingCircle(biggestContour.Ptr, out  center, out  radius);
                    currentFrame.Draw(new CircleF(center, radius), new Bgr(Color.Gold), 2);
                    currentFrame.Draw(new CircleF(new Point((int)center.X, (int)center.Y), 2), new Bgr(Color.Gold), 2);

                    filteredHull = new Seq<Point>(storage);
                    for (int i = 0; i < hull.Total; i++)
                    {
                        if (Math.Sqrt(Math.Pow(hull[i].X - hull[i + 1].X, 2) + Math.Pow(hull[i].Y - hull[i + 1].Y, 2)) > box.size.Width / 10)
                        {
                            filteredHull.Push(hull[i]);
                        }
                    }
                    defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

                    defectArray = defects.ToArray();

                }
            }
        }

        private void DrawAndComputeFingersNum()
        {
            int fingerNum = 0;

            #region hull drawing
            //for (int i = 0; i < filteredHull.Total; i++)
            //{
            //    PointF hullPoint = new PointF((float)filteredHull[i].X,
            //                                  (float)filteredHull[i].Y);
            //    CircleF hullCircle = new CircleF(hullPoint, 4);
            //    currentFrame.Draw(hullCircle, new Bgr(Color.Aquamarine), 2);
            //}
            #endregion

            #region defects drawing
            if (defects != null)
            {
                for (int i = 0; i < defects.Total; i++)
                {
                    PointF startPoint = new PointF((float)defectArray[i].StartPoint.X,
                                                    (float)defectArray[i].StartPoint.Y);

                    PointF depthPoint = new PointF((float)defectArray[i].DepthPoint.X,
                                                    (float)defectArray[i].DepthPoint.Y);

                    PointF endPoint = new PointF((float)defectArray[i].EndPoint.X,
                                                    (float)defectArray[i].EndPoint.Y);

                    LineSegment2D startDepthLine = new LineSegment2D(defectArray[i].StartPoint, defectArray[i].DepthPoint);

                    LineSegment2D depthEndLine = new LineSegment2D(defectArray[i].DepthPoint, defectArray[i].EndPoint);

                    CircleF startCircle = new CircleF(startPoint, 5f);

                    CircleF depthCircle = new CircleF(depthPoint, 5f);

                    CircleF endCircle = new CircleF(endPoint, 5f);


                    if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 5))
                    {
                        fingerNum++;
                        currentFrame.Draw(startDepthLine, new Bgr(Color.Green), 2);
                        currentFrame.Draw(depthEndLine, new Bgr(Color.Magenta), 2);
                    }


                    currentFrame.Draw(startCircle, new Bgr(Color.Red), 2);
                    currentFrame.Draw(depthCircle, new Bgr(Color.Yellow), 5);
                    currentFrame.Draw(endCircle, new Bgr(Color.DarkBlue), 4);
                }
            }
            #endregion

            MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_DUPLEX, 5d, 5d);
            currentFrame.Draw(fingerNum.ToString(), ref font, new Point(50, 150), new Bgr(Color.White));
            bool falg = false;
            int ijj = 0;
            if (fingerNum == 1)
            {
                if (falg == false)
                {
                    //  axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Videos\\Wildlife.wmv";
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                    falg = true;
                }
            }
            else if (fingerNum == 2)
            {
                //axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Videos\\Wildlife.wmv";

                axWindowsMediaPlayer1.Ctlcontrols.play();
                falg = true;
            }
            else if (fingerNum == 3)
            {
                //  axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Videos\\Blaze_test5.wmv";
               // axWindowsMediaPlayer1.Ctlcontrols.stop();
                if (vol <= 100)
                {
                    vol = vol + 10;
                }
                
                //falg = true;
            }
            else if (fingerNum == 4)
            {
                if (vol >= 0)
                {
                    vol = vol - 10;
                }
                else
                {
                    vol = 0;
                }
                axWindowsMediaPlayer1.settings.volume = vol;
            }
            if (fingerNum == 5)
            {
                 axWindowsMediaPlayer1.Ctlcontrols.next();
               // timer1.Stop();
                //  grabber.Dispose();
                falg = true;
                //this.Close();
                //Main mn = new Main();
                //mn.Show();

                return;
                // axWindowsMediaPlayer1.Ctlcontrols.next();
                // axWindowsMediaPlayer1.Ctlcontrols.previous();
            }

        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            FrameGrabber(null, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            bool falg = false;
           
        }

        private void imageBoxFrameGrabber_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            // Get an interface to the first playlist from the library. 
            WMPLib.IWMPPlaylist firstPlaylist = axWindowsMediaPlayer1.playlistCollection.getAll().Item(0);
            firstPlaylist.appendItem(axWindowsMediaPlayer1.newMedia("C:\\Users\\Public\\Videos\\Sample Videos\\Wildlife.wmv"));
            firstPlaylist.appendItem(axWindowsMediaPlayer1.newMedia("C:\\Users\\Public\\Videos\\Sample Videos\\Kalimba.mp3"));
            
            // Make the retrieved playlist the current playlist.
            axWindowsMediaPlayer1.currentPlaylist = firstPlaylist;

         }

  
        private void button2_Click_1(object sender, EventArgs e)
        {
            timer1.Stop();
            grabber.Dispose();
            //falg = true;
            this.Close();
        }



    }
}