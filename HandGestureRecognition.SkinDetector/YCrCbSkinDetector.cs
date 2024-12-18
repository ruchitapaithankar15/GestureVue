﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;

namespace HandGestureRecognition.SkinDetector
{
    public class YCrCbSkinDetector:IColorSkinDetector
    {
        public override Image<Gray, byte> DetectSkin(Image<Bgr, byte> Img, IColor min, IColor max)
        {
            Image<Ycc, Byte> currentYCrCbFrame = Img.Convert<Ycc, Byte>(); // Convert the image frame to YCrCb 
            Image<Gray, byte> skin = new Image<Gray, byte>(Img.Width, Img.Height); // Gray Scale
            skin = currentYCrCbFrame.InRange((Ycc)min,(Ycc) max); // Black & white Skin Region
            StructuringElementEx rect_12 = new StructuringElementEx(12, 12, 6, 6, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvErode(skin, skin, rect_12, 1); // Apply Erosion
            StructuringElementEx rect_6 = new StructuringElementEx(6, 6, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvDilate(skin, skin, rect_6, 2); //Dilation
            return skin;
        }
        
    }
}
