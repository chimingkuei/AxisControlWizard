using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Basler.Pylon;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace AxisControlWizard
{
    enum ImageFormat
    {
        BGR8, Mono8, BayerRG8
    }

    class Basler
    {
        public Camera camera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch stopWatch = new Stopwatch();
        public PictureBox Display = new PictureBox();

        public int threshold { get; set; }
        public Mat image { get; set; }
        public ImageFormat ImageFormatType { get; set; }

        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                if (grabResult.IsValid)
                {
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();
                        Mat mat = null;
                        switch (ImageFormatType)
                        {
                            case ImageFormat.BGR8:
                                {
                                    mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC3);
                                    converter.OutputPixelFormat = PixelType.BGR8packed;
                                    break;
                                }
                            case ImageFormat.Mono8:
                                {
                                    mat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC1);
                                    converter.OutputPixelFormat = PixelType.Mono8;
                                    break;
                                }
                        }
                        IntPtr ptrMat = mat.Data;
                        converter.Convert(ptrMat, mat.Step() * mat.Rows, grabResult);
                        image = mat;
                        Bitmap bitmapOld = Display.Image as Bitmap;
                        Display.Image = mat.ToBitmap();
                        if (bitmapOld != null)
                        {
                            bitmapOld.Dispose();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
            finally
            {
                e.DisposeGrabResultIfClone();
            }
        }

        private void ShowException(Exception exception)
        {
            System.Windows.MessageBox.Show("Exception caught:\n" + exception.Message, "Error");
        }

        public void Init(out bool state)
        {
            try
            {
                camera = new Camera();
                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                camera.CameraOpened += Configuration.AcquireContinuous;
                state = true;
            }
            catch (Exception exception)
            {
                ShowException(exception);
                state = false;
            }
        }

        public void OpenCamera()
        {
            Console.WriteLine("Using camera {0}.", camera.CameraInfo[CameraInfoKey.ModelName]);
            camera.Open();
            Console.WriteLine("Camera Width {0}.", camera.Parameters[PLCamera.Width]);
            Console.WriteLine("Camera Height {0}.", camera.Parameters[PLCamera.Height]);
        }

        public void ContinueAcquisition()
        {
            try
            {
                // Start the grabbing of images until grabbing is stopped.
                Configuration.AcquireContinuous(camera, null);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        public void StopAcquisition()
        {
            // Stop the grabbing.
            try
            {
                camera.StreamGrabber.Stop();
                Display.Image = null;
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

    }






}
