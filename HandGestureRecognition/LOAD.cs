﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HandGestureRecognition
{
    public partial class LOAD : Form
    {
        public LOAD()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 fs = new Form1(); 
            fs.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Webcam wc = new Webcam();
            wc.Show();
        }
    }
}