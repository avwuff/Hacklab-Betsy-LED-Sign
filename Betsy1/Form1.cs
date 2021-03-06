﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Timers;
using System.Net.NetworkInformation;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Diagnostics;

namespace Betsy1
{
    delegate void DrawToSign();

    public partial class Form1 : Form
    {
        delegate void SetPicCallback(Bitmap b);
        delegate void SetDebugLabel(string t);
        IMediaSeeking seeker;


        Byte[,,] pixLast;

        // Objects to handle sign comms
        public HacklabBetsy Betsy;
        TMP2NET TPM;
        BMIX bMix;

        SignLayer[] pLayers; // The layers of my sign.

        Bitmap HLLogo;
        Graphics HLTime;
        Font HLFont;
        FontFamily HLFamily;
        Brush HLBrush;
        Brush HLClearBrush;
        StringFormat HLFormat;
        Bitmap LayerPaint;


        struct pixFrame // One frame of image data.
        {
            public Byte[,,] pixData;
        }

        pixFrame[] pixBuffer; // The entire buffer
        int bufPos = -1; // The last filled position in the buffer.
        int bufSize;
        bool SplitScanMode = true;

        Byte bSet = 0;
        

        long tStart = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        System.Timers.Timer aTimer;


        cbMethods sampBuf;
        int SplitLinesCount = 1;

        PictureBox[] pBoxes;
        Thread RenderThread;

        public Form1()
        {
            InitializeComponent();


            Betsy = new HacklabBetsy("Any");
            //Betsy = new HacklabBetsy("fe80::a019:94b2:65ab:3113%16");

            // Define the layers.
            // Layer list:
            // 0 - Local plasma effect with Hacklab Logo
            // 1 - Local video or camera
            // 2 - TPM2.NET data
            // 3-6 - BMIX layers


            pLayers = new SignLayer[7];
            pBoxes = new PictureBox[pLayers.Length];

            for (int i = 0; i < pLayers.Length; i++)
            {
                // Create the layer.
                pLayers[i] = new SignLayer(Betsy.LWIDTH, Betsy.LHEIGHT);

                // Create a picturebox for each layer.
                pBoxes[i] = new PictureBox();
                pBoxes[i].Name = "pBoxLayer" + i;
                pBoxes[i].Location = new Point(250 + i * 180, 100);
                pBoxes[i].Size = new Size(Betsy.LWIDTH, Betsy.LHEIGHT);
                pBoxes[i].Visible = true;
                pBoxes[i].BackColor = Color.Black;
                this.Controls.Add(pBoxes[i]);
            }

            // Create a bitmap object for our layer painting.
            LayerPaint = new Bitmap(Betsy.LWIDTH, Betsy.LHEIGHT, PixelFormat.Format24bppRgb);


            TPM = new TMP2NET(Betsy.LWIDTH, Betsy.LHEIGHT, pLayers[2].pData);
            TPM.FrameReceived += TPM_FrameReceived;

            pLayers[3].alpha = true; // BMIX uses alpha mode.
            bMix = new BMIX(Betsy.LWIDTH, Betsy.LHEIGHT);
            bMix.FrameReceived += BMix_FrameReceived;

            // Set up the four endpoints that bMix supports
            // 2331 - Top
            // 2330 - Chroma Key
            // 2324 - Foreground
            // 2329 - Background
            bMix.addLayer(pLayers[3], 2329);
            bMix.addLayer(pLayers[4], 2324);
            bMix.addLayer(pLayers[5], 2330);
            pLayers[5].alpha = true;

            bMix.addLayer(pLayers[6], 2331);

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1;


            pixLast = new Byte[Betsy.LWIDTH, Betsy.LHEIGHT, 3];


            // Set up the split scan buffer
            bufSize = 108;
            pixBuffer = new pixFrame[bufSize];
            for (int i = 0; i < bufSize; i++)
            {
                pixBuffer[i].pixData = new Byte[Betsy.LWIDTH, Betsy.LHEIGHT, 3];
            }


            // Code that renders the default stuff
            HLLogo = new Bitmap(Directory.GetCurrentDirectory() + @"\HLLogo.png");
            HLTime = Graphics.FromImage(HLLogo);
            HLTime.CompositingQuality = CompositingQuality.HighQuality;
            HLTime.InterpolationMode = InterpolationMode.Bicubic;
            HLTime.PixelOffsetMode = PixelOffsetMode.HighQuality;
            HLTime.SmoothingMode = SmoothingMode.HighQuality;

            HLTime.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            HLFamily = new FontFamily("Tahoma");
            HLFont = new Font(HLFamily, 12, FontStyle.Bold );
            HLBrush = new SolidBrush(Color.White);
            HLClearBrush = new SolidBrush(Color.FromArgb(0, 0, 0,0));
            aTimer.Start();

            //RenderThread = new Thread(new ThreadStart(RenderRun));
            //RenderThread.Start();


        }

        private void RenderRun()
        {
            while (true)
            {
                mixLayers();
                Thread.Sleep(10);
            }
        }

        private void BMix_FrameReceived(object sender, EventArgs e)
        {
            mixLayers();
        }

        private void TPM_FrameReceived(object sender, EventArgs e)
        {
            pLayers[2].lastPacket = DateTime.Now;
            mixLayers();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("Starting");
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;

            bool hasData = false;

            // If no packets have been received from layers 1 through 3, draw the Hacklab layer.
            for (int i = 1; i < pLayers.Length; i++)
            {
                if (DateTime.Now.Subtract(pLayers[i].lastPacket).TotalSeconds <= 5) hasData = true;
            }

           



            if (!hasData)
            {
                // No layer data has been received from anyone else.  So generate Layer 0.
                plasmaEffect();
                mixLayers();
            }
            pLayers[0].Hidden = hasData; // If any of the other layers contain data, hide the plasma layer.


            aTimer.Enabled = true;
        }

        void SetLabel(string t)
        {
            if (label4.InvokeRequired)
            {
                try
                {
                    SetDebugLabel d = new SetDebugLabel(SetLabel);
                    this.Invoke(d, new object[] { t });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                label4.Text = t;
            }

        }

        void mixLayers()
        {
            Array.Clear(Betsy.pixData, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);

            // Mix the layers together into the output array.
            for (int i = 0; i < pLayers.Length; i++)
            {
                bool isAlpha = pLayers[i].alpha;

                // Only render this layer if packets from it have been received recently.
                if (DateTime.Now.Subtract(pLayers[i].lastPacket).TotalSeconds > 5) continue;
                if (pLayers[i].Hidden) continue;

                if (isAlpha)
                {
                    for (int y = 0; y < Betsy.LHEIGHT; y++)
                    {
                        for (int x = 0; x < Betsy.LWIDTH; x++)
                        {
                            // Only mix if the layer is not zero.
                            if (pLayers[i].pData[x, y, 0] > 0 || pLayers[i].pData[x, y, 1] > 0 || pLayers[i].pData[x, y, 2] > 0)
                            {
                                Betsy.pixData[x, y, 0] = pLayers[i].pData[x, y, 0];
                                Betsy.pixData[x, y, 1] = pLayers[i].pData[x, y, 1];
                                Betsy.pixData[x, y, 2] = pLayers[i].pData[x, y, 2];
                            }
                        }
                    }
                }
                else
                {
                    // We can just use a fast buffer copy.
                    Buffer.BlockCopy(pLayers[i].pData, 0, Betsy.pixData, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);
                }

                //showLayerOnPBox(i);

            }

            draw();
        }

        public void showLayerOnPBox(int layerID)
        {

            try
            {
                BitmapData bData = LayerPaint.LockBits(new Rectangle(0, 0, Betsy.LWIDTH, Betsy.LHEIGHT), ImageLockMode.ReadWrite, LayerPaint.PixelFormat);

                /*the size of the image in bytes */
                int size = bData.Stride * bData.Height;
                byte[] data = new byte[size];

                /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
                //System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

                //Buffer.BlockCopy(pLayers[layerID].pData, 0, data, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);
                
                int i;
                for (int y = 0; y < bData.Height; ++y)
                {
                    for (int x = 0; x < bData.Width; ++x)
                    {
                        i = (y * bData.Stride) + (x * 3);
                        //data is a pointer to the first byte of the 3-byte color data
                        if (i >= 0 && i < size)
                        {
                            data[i + 2] = pLayers[layerID].pData[x, y, 0];
                            data[i + 1] = pLayers[layerID].pData[x, y, 1];
                            data[i + 0] = pLayers[layerID].pData[x, y, 2];
                        }
                    }
                }
                

                System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, size);

                LayerPaint.UnlockBits(bData);
                pBoxes[layerID].Image = LayerPaint;
            } catch (Exception ex)
            {
                // Do nothing
            }
        }


        public void draw()
        {

            if (chkSplitScan.Checked)
            {
                // Copy into the buffer.
                bufPos++;
                if (bufPos >= bufSize) bufPos = 0;
                Buffer.BlockCopy(Betsy.pixData, 0, pixBuffer[bufPos].pixData, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);

                // How much has the frame changed since last time?
                if (chkIntelli.Checked)
                {

                    int thresh = 100;
                    int chCount = 0;

                    // Compare the current buffer to the lasto ne.
                    for (int y = 0; y < Betsy.LHEIGHT; y++)
                    {
                        for (int x = 0; x < Betsy.LWIDTH; x++)
                        {
                            // Has this pixel changed more than our threshhold?
                            if (Math.Abs( 
                                   (pixLast[x, y, 0] + pixLast[x, y, 1] + pixLast[x, y, 2]) -
                                   (Betsy.pixData[x, y, 0] + Betsy.pixData[x, y, 1] + Betsy.pixData[x, y, 2])) > thresh)
                            {
                                chCount++;
                            }
                        }
                    }

                    // Console.WriteLine("Ch count " + chCount);
                    if (chCount > 3000)
                    {
                        // Copy this frame to EVERY buffer
                        for (int i = 0; i < bufSize; i++)
                        {
                            Buffer.BlockCopy(Betsy.pixData, 0, pixBuffer[i].pixData, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);
                        }
                    }

                    // Copy the screen into the buffer for compariting.
                    Buffer.BlockCopy(Betsy.pixData, 0, pixLast, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);
                }


                SplitScanPaint();
            }


            SetLabel("Draw " + DateTime.Now.ToString());
            Betsy.DrawAll();


        }

        private void SplitScanPaint()
        {
            // This function paints into the pixData array from the Split Scan buffer.
            // To do this, we paint row 1 from bufPos, row 2 from bufPos - 1, etc
            for (var y = 0; y < Betsy.LHEIGHT; y++)
            {

                var copyFrom = bufPos - (y / SplitLinesCount);
                if (copyFrom < 0) copyFrom += bufSize; // Loop it back around.


                for (var x = 0; x < Betsy.LWIDTH; x++) // TODO: See if we can replace this with a BlockCopy
                {
                    Betsy.pixData[x, y, 0] = pixBuffer[copyFrom].pixData[x, y, 0];
                    Betsy.pixData[x, y, 1] = pixBuffer[copyFrom].pixData[x, y, 1];
                    Betsy.pixData[x, y, 2] = pixBuffer[copyFrom].pixData[x, y, 2];
                }
            } 
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //sendToTile(0, e.Location.X, e.Location.Y);
        }

        

        void plasmaEffect()
        {

            int w = Betsy.LWIDTH;
            int h = Betsy.LHEIGHT;

            // Plasma
            double yv;
            double xv;

            long end_ts = 90;

            double o1, o2, o3;

            double timer = ((double)((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - tStart)) / 1000;

            o1 = (double)(4 - (end_ts - timer)) / 4;
            o2 = (double)(end_ts - timer) / 4;
            o3 = (double)timer;

            // Buffer we use while painting.
            Byte[,,] pInt = new Byte[Betsy.LWIDTH, Betsy.LHEIGHT, 3];

            
            //pDraw.loadPixels();
            for (int y = 0; y < h; y++)
            {

                yv = (double)y / (double)h - 0.5;
                for (int x = 0; x < w; x++)
                {
                    
                    xv = (float)x / (float)w - 0.5;
                    // b.SetPixel(x, y, Color.FromArgb(calc_v(xv, yv, o1), calc_v(xv, yv, o2), calc_v(xv, yv, o3)));
                    pInt[x, y, 0] = (Byte)(calc_v(xv, yv, o1 / 3) * 0.5);
                    pInt[x, y, 1] = (Byte)(calc_v(xv, yv, o2 / 3) * 0.5);
                    pInt[x, y, 2] = (Byte)(calc_v(xv, yv, o3 / 3) * 0.5);

                }
            }

            // Print the current time onto the logo in the top-right corner.
            HLTime.CompositingMode = CompositingMode.SourceCopy;
            HLTime.FillRectangle(HLClearBrush, 80, 0, 90, 20);
            HLTime.CompositingMode = CompositingMode.SourceOver;

            Pen tPen = new Pen(Color.Black, (float)1.51);
            GraphicsPath tPath = new GraphicsPath();
            tPath.AddString(DateTime.Now.ToString("HH:mm:ss"), HLFamily, (int)FontStyle.Bold, 16, new Point(80, 0), StringFormat.GenericDefault);
            HLTime.FillPath(HLBrush, tPath);
            HLTime.DrawPath(tPen, tPath);
            //HLTime.
            //HLTime.DrawString(DateTime.Now.ToString("hh:mm:ss"), HLFont, HLBrush, 80,0);

            tPath.Dispose();
            tPen.Dispose();
            // Copy the Hacklab logo onto the plasma effect



            BitmapData bData = HLLogo.LockBits(new Rectangle(0, 0, 162, 108), ImageLockMode.ReadOnly, HLLogo.PixelFormat);

            /*the size of the image in bytes */
            int size = bData.Stride * bData.Height;
            byte[] data = new byte[size];

            /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            int i;
            for (int y = 0; y < bData.Height; ++y)
            {

                for (int x = 0; x < bData.Width; ++x)
                {
                    i = (y * bData.Stride) + (x * 4);

                    float alphaScale = (float)data[i + 3] / 255;

                    //data is a pointer to the first byte of the 3-byte color data
                    if (i >= 0 && i < size)
                    {
                        if (data[i+3] > 0 && !(x >= 80 && y <= 20)) // Keep it from messing with the clock
                        {
                            yv = (double)y / (double)h - 0.5;
                            xv = (float)x / (float)w - 0.5;
                            byte bb = (byte)(calc_v(xv, yv, o3 /4 ) * 0.7);
                            data[i] = (byte)Math.Max(data[i] - bb, 0);
                            data[i+1] = (byte)Math.Max(data[i+1] - bb, 0);
                            data[i+2] = (byte)Math.Max(data[i+2] - bb, 0);
                        }


                        pInt[x, y, 0] = (byte)(((float)pInt[x, y, 0] * (1 - alphaScale)) + ((float)data[i + 2] * alphaScale));  // + data[i + 2]; // red
                        pInt[x, y, 1] = (byte)(((float)pInt[x, y, 1] * (1 - alphaScale)) + ((float)data[i + 1] * alphaScale)); // green
                        pInt[x, y, 2] = (byte)(((float)pInt[x, y, 2] * (1 - alphaScale)) + ((float)data[i] * alphaScale)); // blue

                        /*data[i + 2] = pLayers[0].pData[x, y, 0];
                        data[i + 1] = pLayers[0].pData[x, y, 1];
                        data[i + 0] = pLayers[0].pData[x, y, 2];
                        data[i + 3] = 255;
                        */
                    }
                }
            }

            //System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, size);

            HLLogo.UnlockBits(bData);
            pLayers[0].lastPacket = DateTime.Now; // Indicate that data is available.

            // Copy into the final buffer
            Buffer.BlockCopy(pInt, 0, pLayers[0].pData, 0, Betsy.LWIDTH * Betsy.LHEIGHT * 3);

            //pictureBox1.Image = HLLogo;
        }


        // For plasma effect
        int calc_v(double xv, double yv, double offset)
        {
            double o_div_3 = offset / 3;
            double cy = yv + 0.5 * Math.Cos(o_div_3);
            double xv_sin_half = xv * Math.Sin(offset / 2);
            double yv_cos_od3 = yv *  Math.Cos(o_div_3);
            double cx = xv + 0.5 * Math.Sin(offset / 5);
            double v2 = Math.Sin(10 * (xv_sin_half + yv_cos_od3) + offset);
            double magic = Hypot(cx, cy) * 10;
            double v1 = Math.Sin(xv * 10 + offset);
            double v3 = Math.Sin(magic + offset);
            double v = (v1 + v2 + v3) * Math.PI / 2;
            return (int)(127.5 * Math.Sin(v) + 127.5);
        }
        double Hypot(double x, double y) { return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); }

        private void button3_Click(object sender, EventArgs e)
        {
            aTimer.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Betsy.resetSign();

        }

        private void trkIntensity_Scroll(object sender, EventArgs e)
        {
            Betsy.gainScale = (double)trkIntensity.Value / 100;
        }

       

        private void button4_Click(object sender, EventArgs e)
        {
            /*
            BackgroundWorker videoPlayer = new BackgroundWorker();
            videoPlayer.DoWork += VideoPlayer_DoWork;

            videoPlayer.RunWorkerAsync();
            */

            VideoPlayer_DoWork(null, null);



        }

        private void VideoPlayer_DoWork(object sender, DoWorkEventArgs e)
        {

            Type comType = Type.GetTypeFromCLSID(new Guid("e436ebb3-524f-11ce-9f53-0020af0ba770"));
            IGraphBuilder graphBuilder = (IGraphBuilder)Activator.CreateInstance(comType);

            IMediaEventEx mediaEvent = (IMediaEventEx)graphBuilder;
            IMediaControl mediaControl = (IMediaControl)graphBuilder;
            IVideoWindow videoWindow = (IVideoWindow)graphBuilder;
            IBasicAudio basicAudio = (IBasicAudio)graphBuilder;
            IBasicVideo basicVideo = (IBasicVideo)graphBuilder;

            comType = Type.GetTypeFromCLSID(new Guid("C1F400A0-3F08-11d3-9F0B-006008039E37"));
            ISampleGrabber sampleGrabber = (ISampleGrabber)Activator.CreateInstance(comType);

            
            AMMediaType mediaType = new AMMediaType();
            mediaType.majorType = MediaType.Video;
            mediaType.subType = MediaSubType.RGB24;
            mediaType.formatType = FormatType.VideoInfo;
            sampleGrabber.SetMediaType(mediaType);

            graphBuilder.AddFilter((IBaseFilter)sampleGrabber, "Render");

            String video = @"W:\Videos\TV Shows\Silicon Valley\1x01 - Minimum Viable Product.mkv";
            video = @"W:\Videos\movies\Teenage Mutant Ninja Turtles II The Secret of the Ooze [1080p] (1991)\Teenage Mutant Ninja Turtles II The Secret of the Ooze [1080p].mkv";
            // video = @"w:\Videos\TV Shows\Impractical Jokers\5x16 - Laundry Day.mkv"
            video = @"W:\Videos\TV Shows\Star Trek\The Next Generation\3x26 - The Best of Both Worlds.mkv";
            //video = @"c:\temp\duffless.mp4";
            //video = @"C:\Users\Avatar-X\Videos\Moaning\2x01 - Art.mkv";
            // int hr = graphBuilder.RenderFile(@"c:\temp\duffless.mp4", null);
            //int hr = graphBuilder.RenderFile, null);
            /*
            ICaptureGraphBuilder2 captureGraphBuilder = new CaptureGraphBuilder2() as ICaptureGraphBuilder2;
            IBaseFilter sourceFilter;

            sourceFilter = FindCaptureDevice();
            graphBuilder.AddFilter(sourceFilter, "Camera");
            captureGraphBuilder.SetFiltergraph(graphBuilder);
            captureGraphBuilder.RenderStream(PinCategory.Preview , MediaType.Video, sourceFilter, null, sampleGrabber as IBaseFilter );
            Marshal.ReleaseComObject(sourceFilter);
            */
            video = @"W:\Videos\TV Shows\Simpsons\04x20 - Whacking Day.mp4";


            int hr = graphBuilder.RenderFile(video, null);




            seeker = (IMediaSeeking)graphBuilder;

            videoWindow.put_AutoShow(OABool.True);
            basicAudio.put_Volume(10000);

            sampleGrabber.SetOneShot(false);
            sampleGrabber.SetBufferSamples(false);

            AMMediaType connectedMediaType = new AMMediaType();
            sampleGrabber.GetConnectedMediaType(connectedMediaType);



            // VideoInfoHeader videoHeader = (VideoInfoHeader)connectedMediaType.;

            sampBuf = new cbMethods();
            //sampBuf.myOwner = this;
            basicVideo.get_VideoWidth(out sampBuf.vw);
            basicVideo.get_VideoHeight(out sampBuf.vh);
            sampBuf.seeker = seeker;
            sampBuf.pLayer = pLayers[1];
            sampBuf.myOwner = this;

            Console.WriteLine("Video size: " + sampBuf.vw + ", " + sampBuf.vh);

            //the same object has implemented the ISampleGrabberCB interface.
            //0 sets the callback to the ISampleGrabberCB::SampleCB() method.
            sampleGrabber.SetCallback(sampBuf, 0);

            /*
            sampBuf.vw = 1920;
            sampBuf.vh = 1080;
            */


            mediaControl.Run();

            /*
            EventCode eventCode;
            mediaEvent.WaitForCompletion(-1, out eventCode);
            */

            // Marshal.ReleaseComObject(sampleGrabber);
            // Marshal.ReleaseComObject(graphBuilder);

        }


        public IBaseFilter FindCaptureDevice()
        {
            Console.WriteLine("Start the Sub FindCaptureDevice");
            int hr = 0;
            IEnumMoniker classEnum = null;
            IMoniker[] moniker = new IMoniker[1];
            object source = null;
            ICreateDevEnum devEnum = (ICreateDevEnum)new CreateDevEnum();
            hr = devEnum.CreateClassEnumerator(FilterCategory.VideoInputDevice, out classEnum, 0);
            Console.WriteLine("Create an enumerator for the video capture devices : " + DsError.GetErrorText(hr));
            DsError.ThrowExceptionForHR(hr);
            Marshal.ReleaseComObject(devEnum);
            if (classEnum == null)
            {
                throw new ApplicationException("No video capture device was detected.\\r\\n\\r\\n" + "This sample requires a video capture device, such as a USB WebCam,\\r\\n" + "to be installed and working properly.  The sample will now close.");
            }
            if (classEnum.Next(moniker.Length, moniker, IntPtr.Zero) == 0)
            {
                // Skip to the Nth camera
                //classEnum.Next(moniker.Length, moniker, IntPtr.Zero);
                //classEnum.Next(moniker.Length, moniker, IntPtr.Zero);

                Guid iid = typeof(IBaseFilter).GUID;
                moniker[0].BindToObject(null, null, iid, out source);
            }
            else
            {
                throw new ApplicationException("Unable to access video capture device!");
            }
            Marshal.ReleaseComObject(moniker[0]);
            Marshal.ReleaseComObject(classEnum);
            return (IBaseFilter)source;
        }
        /*
        public void SetPic(Bitmap b)
        {

            if (pictureBox1.InvokeRequired)
            {
                SetPicCallback d = new SetPicCallback(SetPic);
                this.Invoke(d, new object[] { b });
            }
            else
            {
                pictureBox1.Image = b;
                pictureBox1.Refresh();
            }
        }
        */

        public void drawNext()
        {
            DrawToSign d = new DrawToSign(mixLayers);
            this.Invoke(d);
            
        }

        private void videoTrack_MouseUp(object sender, MouseEventArgs e)
        {
            if (seeker != null)
            {
                // How long is the video?
                long pDur;
                DsLong pPos;
                seeker.GetDuration(out pDur);


                pPos = (long)((double)pDur * ((double)videoTrack.Value / 10000));

                Console.WriteLine("Seeking to " + pPos);
                //seeker.SetPositions(pPos, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame, 0, AMSeekingSeekingFlags.NoPositioning);
                seeker.SetPositions(pPos, AMSeekingSeekingFlags.SeekToKeyFrame | AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning);
                sampBuf.seekTime = pPos;
            }
        }

        private void trackLines_Scroll(object sender, EventArgs e)
        {
            SplitLinesCount = trackLines.Value;
        }

        private void button5_Click(object sender, EventArgs e)
        {

            Type comType = Type.GetTypeFromCLSID(new Guid("e436ebb3-524f-11ce-9f53-0020af0ba770"));
            IGraphBuilder graphBuilder = (IGraphBuilder)Activator.CreateInstance(comType);

            IMediaEventEx mediaEvent = (IMediaEventEx)graphBuilder;
            IMediaControl mediaControl = (IMediaControl)graphBuilder;
            IVideoWindow videoWindow = (IVideoWindow)graphBuilder;
            IBasicAudio basicAudio = (IBasicAudio)graphBuilder;
            IBasicVideo basicVideo = (IBasicVideo)graphBuilder;

            comType = Type.GetTypeFromCLSID(new Guid("C1F400A0-3F08-11d3-9F0B-006008039E37"));
            ISampleGrabber sampleGrabber = (ISampleGrabber)Activator.CreateInstance(comType);


            AMMediaType mediaType = new AMMediaType();
            mediaType.majorType = MediaType.Video;
            mediaType.subType = MediaSubType.RGB24;
            mediaType.formatType = FormatType.VideoInfo;
            sampleGrabber.SetMediaType(mediaType);

            graphBuilder.AddFilter((IBaseFilter)sampleGrabber, "Render");

            ICaptureGraphBuilder2 captureGraphBuilder = new CaptureGraphBuilder2() as ICaptureGraphBuilder2;
            IBaseFilter sourceFilter;

            sourceFilter = FindCaptureDevice();
            graphBuilder.AddFilter(sourceFilter, "Camera");
            captureGraphBuilder.SetFiltergraph(graphBuilder);
            captureGraphBuilder.RenderStream(PinCategory.Preview , MediaType.Video, sourceFilter, null,  sampleGrabber as IBaseFilter );
            Marshal.ReleaseComObject(sourceFilter);

            seeker = (IMediaSeeking)graphBuilder;

            videoWindow.put_AutoShow(OABool.True);
            basicAudio.put_Volume(10000);

            sampleGrabber.SetOneShot(false);
            sampleGrabber.SetBufferSamples(false);

            AMMediaType connectedMediaType = new AMMediaType();
            sampleGrabber.GetConnectedMediaType(connectedMediaType);



            // VideoInfoHeader videoHeader = (VideoInfoHeader)connectedMediaType.;

            sampBuf = new cbMethods();
            basicVideo.get_VideoWidth(out sampBuf.vw);
            basicVideo.get_VideoHeight(out sampBuf.vh);
            sampBuf.seeker = seeker;
            sampBuf.pLayer = pLayers[1];
            sampBuf.myOwner = this;


            Console.WriteLine("Video size: " + sampBuf.vw + ", " + sampBuf.vh);

            //the same object has implemented the ISampleGrabberCB interface.
            //0 sets the callback to the ISampleGrabberCB::SampleCB() method.
            sampleGrabber.SetCallback(sampBuf, 0);

            sampBuf.vw =  1920;
            sampBuf.vh =  1080;


            mediaControl.Run();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //RenderThread.Abort();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Sign is online: " + Betsy.SignOnline);

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Betsy.setGain(100);
        }
    }

    public class cbMethods : ISampleGrabberCB
    {
        public Form1 myOwner;
        public int vw;
        public int vh;
        public IMediaSeeking seeker;
        public long seekTime = 0;
        public SignLayer pLayer;

        Bitmap bmp;
        Graphics graph;
        public cbMethods()
        {
            bmp = new Bitmap(162, 108);
            graph = Graphics.FromImage(bmp);


            graph.InterpolationMode = InterpolationMode.Bilinear;
            graph.CompositingQuality = CompositingQuality.HighSpeed;
            graph.SmoothingMode = SmoothingMode.HighSpeed;
            
            /*
                         graph.InterpolationMode = InterpolationMode.High;
                         graph.CompositingQuality = CompositingQuality.HighQuality;
                         graph.SmoothingMode = SmoothingMode.AntiAlias;
                         */


        }


        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            throw new NotImplementedException();
        }

        public int SampleCB(double sampleTime, IMediaSample pSample)
        {
            int len = pSample.GetActualDataLength();
            IntPtr pbuf;

            long p1, p2, p3, howLate;
            howLate = 0;

            
            pSample.GetTime(out p1, out p2);
            seeker.GetCurrentPosition(out p3);
            p1 += seekTime;

            howLate = p3 - p1;
            
            Console.WriteLine("Got sample");
            //
            if (howLate > 0)
            {
                // Console.WriteLine("P1: " + p1 + " media: " + p3 + "Late " + howLate);
                // Console.WriteLine("Skipping frame that is " + howLate + " late.");
            }

            if (pSample.GetPointer(out pbuf) == 0 && len > 0 && howLate <= 0 && vw > 0)
            {
                //byte[] buf = new byte[len];
                //Marshal.Copy(pbuf, buf, 0, len);

                /*
                for (int i = 0; i < len; i += 2)
                    buf[i] = (byte)(255 - buf[i]);
                Marshal.Copy(buf, 0, pbuf, len);
                */

                // fill in rgbValues, e.g. with a for loop over an input array
                var im = new Bitmap(vw, vh, 3 * vw,
                         PixelFormat.Format24bppRgb,
                         pbuf);

                double s = Math.Max((double)162 / (double)vw, (double)108 / (double)vh);
                int ow = (int)((double)vw * s);
                int oh = (int)((double)vh * s);
                int ox = (162 / 2) - (ow / 2);
                int oy = (108 / 2) - (oh / 2);

                graph.DrawImage(im, new Rectangle(ox, oy, ow, oh), 0, 0, vw, vh, GraphicsUnit.Pixel );

                // myOwner.SetPic(bmp);

                // Calculate where on our sign the video will go.
                // 

                BitmapData bData = bmp.LockBits(new Rectangle(0, 0, 162, 108), ImageLockMode.ReadOnly, bmp.PixelFormat);

                /*the size of the image in bytes */
                int size = bData.Stride * bData.Height;

                /*Allocate buffer for image*/
                byte[] data = new byte[size];

                /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
                System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

                int i;
                for (int y = 0; y < bData.Height; ++y)
                {
                    for (int x = 0; x < bData.Width; ++x)
                    {
                        i = ((107 - y) * bData.Stride) + (x * 4);
                        //data is a pointer to the first byte of the 3-byte color data
                        if (i >= 0 && i < size)
                        {
                            pLayer.pData[x, y, 0] = data[i + 2]; // red
                            pLayer.pData[x, y, 1] = data[i + 1]; // green
                            pLayer.pData[x, y, 2] = data[i]; // blue
                        }
                    }
                }

                bmp.UnlockBits(bData);

                /*
                int i = 0;
                for (int y = 0; y < 108; y++)
                {
                    int ys = vh - (int)((double)y / s);

                    for (int x = 0; x < 162; x++)
                    {
                        // calculate the co-ordinates of this pixel in the source video.
                        int xs = (int)((double)x / s);

                        // index into the video
                        i = (ys * vw * 3) + (xs * 3);
                        if (i > 0 && i < len)
                        {
                            myOwner.pixData[x, y, 0] = buf[i + 2];
                            myOwner.pixData[x, y, 1] = buf[i + 1];
                            myOwner.pixData[x, y, 2] = buf[i + 0];
                        }
                    }
                }
                */


                pLayer.lastPacket = DateTime.Now;
                myOwner.drawNext();


            }


            /* other code */
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

    }


    




}
