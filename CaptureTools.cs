using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using FFMpegCore.Extensions.System.Drawing.Common;
using Emgu.CV.CvEnum;

namespace CamToVr;

class CaptureTools
{
    private VideoCapture _captureDevice;
    private readonly BlockingCollection<IVideoFrame> _frames = new();

    public Stream Run()
    {
        var camIndex = SelectCameraIndex();
        
        _captureDevice = new VideoCapture(camIndex, VideoCapture.API.DShow)
        {
            FlipVertical = false,
        };
        _captureDevice.Set(CapProp.Fps, 10);
        _captureDevice.ImageGrabbed += CaptureDeviceImageGrabbed;
        
        _captureDevice.Start();
        
        var pipeServer = new AnonymousPipeServerStream();
        var pipeClient = new AnonymousPipeClientStream(pipeServer.GetClientHandleAsString());

        Task.Run(() =>
        {
            FFMpegArguments

                .FromPipeInput(new RawVideoPipeSource(_frames.GetConsumingEnumerable()))
                .OutputToPipe(new StreamPipeSink(pipeServer), options => options
                                      .WithVideoCodec("vp9").ForceFormat("webm").WithFramerate(10)
                                      //.WithVideoCodec(VideoCodec.LibX264).WithConstantRateFactor(21).ForceFormat("matroska")
                                      //.WithVideoCodec(VideoCodec.LibX264).ForceFormat("hls")
                                      //.WithVideoCodec(VideoCodec.LibX264).ForceFormat("avi")
                             )
                .ProcessSynchronously();
        });

        /*Task.Run(() =>
        {
            _captureDevice.Stop();
            _captureDevice.Dispose();
            rawOutputStream.Close();
        });*/

        return pipeClient;
    }

    private int SelectCameraIndex()
    {
        var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        if (cameras.Length == 1) return 0;
        foreach (var (camera, index) in WithIndex(cameras))
        {
            Console.WriteLine($"{index}:{camera.Name}");
        }
        Console.WriteLine("Select a camera from the list above:");
        var camIndex = Convert.ToInt32(Console.ReadLine());
        return camIndex;
    }
    
    private void CaptureDeviceImageGrabbed(object sender, System.EventArgs e)
    {
        var frame = new Mat();
        _captureDevice.Retrieve(frame);
        var buffer = new VectorOfByte();
        var input = frame.ToImage<Bgr, byte>();
        CvInvoke.Imencode(".bmp", input, buffer);

        var image = (Bitmap)Image.FromStream(new MemoryStream(buffer.ToArray()));

        //image.RotateFlip(RotateFlipType.Rotate180FlipNone);

        _frames.Add(new BitmapVideoFrameWrapper(image));

        //image.Dispose();
        input.Dispose();
        buffer.Dispose();
    }

    private IEnumerable<(T item, int index)> WithIndex<T>(IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}