using System.Runtime.InteropServices;
using LibAvifSharp.NativeTypes;
using SkiaSharp;
using System.Threading;

namespace LibAvifSharp;

public static class AvifEncoder
{
    static SemaphoreSlim svtAv1Lock = new SemaphoreSlim(1, 1);

    public static EncodedImage Encode(SKImage image, Action<EncoderSetttings>? settings = null)
    {
        using var bitmap = SKBitmap.FromImage(image);
        return Encode(bitmap, settings);
    }

    public static EncodedImage Encode(SKBitmap bm, Action<EncoderSetttings>? settings = null)
    {
        bool svtLockAcquired = false;
        var encoderSettings = new EncoderSetttings();
        if(settings != null)
        {
            settings(encoderSettings);
        }

        NativeInterop.EnsureNativeLibraryLoaderRegistered();
        var pixels = bm.GetPixels();

        var image = NativeInterop.avifImageCreate((uint)bm.Width, (uint)bm.Height, (uint)(bm.ColorType == SKColorType.Bgra8888 ? 8 : 16), encoderSettings.PixelFormat);
    

        var rgb = new AvifRGBImage();

        NativeInterop.avifRGBImageSetDefaults(in rgb, image);

        rgb.Pixels = pixels;
        rgb.RowBytes = (uint)bm.RowBytes;
        rgb.Format = AvifRGBFormat.AVIF_RGB_FORMAT_BGRA;

        var result = NativeInterop.avifImageRGBToYUV(image, in rgb);
        if(result != AvifResult.AVIF_RESULT_OK)
        {
            throw new Exception($"Failed to convert RGB to YUV: {result}");
        }

        if (encoderSettings.CodecChoice == AvifCodecChoice.AVIF_CODEC_CHOICE_SVT)
        {
            svtAv1Lock.Wait();
            svtLockAcquired = true;
        }

        AvifRWData output = default; // Declare here
        try
        {
            var encoderPtr = NativeInterop.avifEncoderCreate();

            var encoder = Marshal.PtrToStructure<NativeTypes.AvifEncoder>(encoderPtr);

        encoder.Quality = encoderSettings.Quality;
        encoder.QualityAlpha = encoderSettings.QualityAlpha;
        encoder.CodecChoice = encoderSettings.CodecChoice;
        encoder.Speed = encoderSettings.Speed;

        Marshal.StructureToPtr(encoder, encoderPtr, false);

        AvifResult addImageResult = NativeInterop.avifEncoderAddImage(encoderPtr, image, 1, AvifAddImageFlag.AVIF_ADD_IMAGE_FLAG_SINGLE);
        if (addImageResult != AvifResult.AVIF_RESULT_OK)
        {
            throw new Exception($"Failed to add image: {addImageResult}");
        }

        output = new AvifRWData(); // Initialize here
        NativeInterop.avifEncoderFinish(encoderPtr, ref output);
            NativeInterop.avifEncoderDestroy(encoderPtr);
            NativeInterop.avifImageDestroy(image);
        }
        finally
        {
            if (svtLockAcquired)
            {
                svtAv1Lock.Release();
            }
        }
        
        // dont free RGB pixels as they are owned by the bitmap
        // NativeInterop.avifRGBImageFreePixels(ref rgb);

        return new EncodedImage(output);
    }
}