using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Scryber.PDF;
using Scryber.PDF.Native;

namespace Scryber.Imaging.Formatted
{
    /// <summary>
    /// ImageData for the 24 bit RGB image
    /// </summary>
    public class PDFImageSharpRGB24Data : PDFImageSharpData<Rgb24>
    {


        public PDFImageSharpRGB24Data(Image img, string source)
            : base(img, source)
        {
        }


        protected override long DoRenderImageData(IStreamFilter[] filters, ContextBase context, PDFWriter writer)
        {
            long total = 0;

            //New v2 ImageSharp accessor pattern
            this.PixelImage.ProcessPixelRows(accessor =>
            {
                int width = this.PixelWidth;

                byte[] buffer = new byte[width * 3];

                Span<byte> bufferSpan = new Span<byte>(buffer);
                for (int r = 0; r < this.PixelHeight; r++)
                {
                    var span = accessor.GetRowSpan(r);
                    for (int p = 0; p < width; p++)
                    {
                        int offset = p * 3;
                        bufferSpan[offset] = span[p].R;
                        bufferSpan[offset + 1] = span[p].G;
                        bufferSpan[offset + 2] = span[p].B;
                    }

                    writer.WriteRaw(buffer, 0, buffer.Length);
                    total += buffer.Length;
                }

            });

            //int width = this.PixelWidth;
            
            //byte[] buffer = new byte[width * 3];

            //Span<byte> bufferSpan = new Span<byte>(buffer);

            //for (int r = 0; r < this.PixelHeight; r++)
            //{
            //    ReadOnlySpan<Rgb24> span = this.PixelImage.GetPixelRowSpan(r);
            //    ops.ToRgb24Bytes(config, span, buffer, width);
            //    writer.WriteRaw(buffer, 0, buffer.Length);
            //    total += buffer.Length;
            //}

            return total;
        }

        protected override long DoRenderAlphaData(IStreamFilter[] filters, ContextBase context, PDFWriter writer)
        {
            throw new InvalidOperationException("The Bgr24 format does not have an alpha component");
        }

        public override void ResetFilterCache()
        {

        }
    }
}
