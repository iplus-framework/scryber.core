using System;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Scryber.Drawing;
using Scryber.Imaging.Formatted;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;

namespace Scryber.Imaging
{
    public class ImageFactoryTiff : ImageFactoryBase, IPDFImageDataFactory
    {
        private static readonly Regex TiffMatch = new Regex("\\.(tif|tiff)?\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private const string TiffName = "Tiff Image factory";
        private const bool TiffShouldCache = true;
        
        public ImageFactoryTiff()
            :this(TiffMatch, TiffName, TiffShouldCache)
        {

        }

        protected ImageFactoryTiff(Regex match, string name, bool shouldCache)
            : base(match, MimeType.TiffImage,  name, shouldCache)
        {
        }


        protected override ImageData DoLoadRawImageData(IDocument document, IComponent owner, byte[] rawData, MimeType type)
        {
            var span = new ReadOnlySpan<byte>(rawData);
            var img = Image.Load(span);
            
            ImageData data = null;
            var meta = img.Metadata.GetFormatMetadata(TiffFormat.Instance);

            if (meta != null)
            {
                var name = document.GetIncrementID(ObjectTypes.ImageData) + ".tiff";
                const ColorSpace colorSpace = ColorSpace.RGB;
                int bitDepth = img.PixelType.BitsPerPixel;
                bool hasAlpha = HasAlphaChannel(img);
                
                data = GetImageDataForImage(ImageFormat.Tiff, img, name, bitDepth, hasAlpha, colorSpace);
            }



            return data;
        }


        protected override ImageData DoDecodeImageData(Stream stream, IDocument document, IComponent owner, string path)
        {
            var img = Image.Load(stream);

            ImageData data = null;
            var meta = img.Metadata.GetFormatMetadata(TiffFormat.Instance);

            if (null != meta)
            {
                ColorSpace colorSpace =  ColorSpace.RGB; 
                int bitDepth = img.PixelType.BitsPerPixel;
                bool hasAlpha = HasAlphaChannel(img);
                
                data = GetImageDataForImage(ImageFormat.Tiff, img, path, bitDepth, hasAlpha, colorSpace);
            }



            return data;
        }

        private static bool HasAlphaChannel(Image image)
        {
            var metadata = image.Frames.RootFrame.Metadata.GetFormatMetadata(TiffFormat.Instance);
            if (metadata == null)
                return false;

            int colorChannels;
            switch (metadata.PhotometricInterpretation)
            {
                case TiffPhotometricInterpretation.Rgb:
                case TiffPhotometricInterpretation.YCbCr:
                case TiffPhotometricInterpretation.CieLab:
                case TiffPhotometricInterpretation.IccLab:
                case TiffPhotometricInterpretation.ItuLab:
                    colorChannels = 3;
                    break;
                case TiffPhotometricInterpretation.Separated:
                    colorChannels = 4;
                    break;
                default:
                    colorChannels = 1;
                    break;
            }

            return metadata.BitsPerSample is TiffBitsPerSample bitsPerSample
                   && bitsPerSample.Channels > colorChannels;
        }

    }
}
