using Microsoft.AspNetCore.Builder;

namespace CamToVr;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", async context =>
        {
            context.Response.StatusCode = 200;

            context.Response.ContentType = "video/webm";
            //context.Response.ContentType = "video/x-matroska";
            //context.Response.ContentType = "application/vnd.apple.mpegurl";
            //context.Response.ContentType = "video/x-msvideo"; // AVI
            
            await context.Response.StartAsync();

            var captureTools = new CaptureTools();
            var stream = captureTools.Run();

            //while (true)
            {
                await stream.CopyToAsync(context.Response.BodyWriter.AsStream());
            }

            //var captureTools = new CaptureTools();
            //return Microsoft.AspNetCore.Http.Results.Stream(captureTools.Run(), "video/webm");
            //return Task.FromResult(Microsoft.AspNetCore.Http.Results.Stream(captureTools.Run(), "video/webm"));
        });

        app.Run();
    }
}