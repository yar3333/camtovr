using System.IO;

namespace CamToVr;

class RawVideoPipeDataWriter : Stream
{
    private readonly FileStream fout = File.OpenWrite("c:\\test2.mp4");

    public override void Flush() {}

    public override int Read(byte[] buffer, int offset, int count) { return 0; }

    public override long Seek(long offset, SeekOrigin origin) { return offset; }

    public override void SetLength(long value) {}

    public override void Write(byte[] buffer, int offset, int count)
    {
        fout.Write(buffer, offset, count);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length { get; }
    public override long Position { get; set; }

    public override void Close()
    {
        fout.Close();
        base.Close();
    }
}