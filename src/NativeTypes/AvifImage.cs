using System.Runtime.InteropServices;
using LibAvifSharp;

namespace LibAvifSharp.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct AvifImage
{
    // Image information
    public uint Width;
    public uint Height;
    public uint Depth; // all planes must share this depth; if depth>8, all planes are uint16_t internally
    public AvifPixelFormat YuvFormat;

    public AvifRange YuvRange;

    public AvifChromaSamplePosition YuvChromaSamplePosition;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.AVIF_PLANE_COUNT_YUV)]
    public IntPtr[] YuvPlanes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.AVIF_PLANE_COUNT_YUV)]
    public uint[] YuvRowBytes;
    
    public AvifBool ImageOwnsYUVPlanes;

    public IntPtr AlphaPlane;

    public uint AlphaRowBytes;
    public AvifBool ImageOwnsAlphaPlane;
    public AvifBool AlphaPremultiplied;


    // ICC Profile
    AvifRWData icc;

    // CICP information:
    // These are stored in the AV1 payload and used to signal YUV conversion. Additionally, if an
    // ICC profile is not specified, these will be stored in the AVIF container's `colr` box with
    // a type of `nclx`. If your system supports ICC profiles, be sure to check for the existence
    // of one (avifImage.icc) before relying on the values listed here!
    AvifColorPrimaries colorPrimaries;
    AvifTransferCharacteristics transferCharacteristics;
    AvifMatrixCoefficients matrixCoefficients;

    // CLLI information:
    // Content Light Level Information. Used to represent maximum and average light level of an
    // image. Useful for tone mapping HDR images, especially when using transfer characteristics
    // SMPTE2084 (PQ). The default value of (0, 0) means the content light level information is
    // unknown or unavailable, and will cause libavif to avoid writing a clli box for it.
    // AvifContentLightLevelInformationBox Clli;
    uint Clli; // placeholder for 2 short struct

    // Transformations - These metadata values are encoded/decoded when transformFlags are set
    // appropriately, but do not impact/adjust the actual pixel buffers used (images won't be
    // pre-cropped or mirrored upon decode). Basic explanations from the standards are offered in
    // comments above, but for detailed explanations, please refer to the HEIF standard (ISO/IEC
    // 23008-12:2017) and the BMFF standard (ISO/IEC 14496-12:2022).
    //
    // To encode any of these boxes, set the values in the associated box, then enable the flag in
    // transformFlags. On decode, only honor the values in boxes with the associated transform flag set.
    // These also apply to gainMap->image, if any.
    AvifTransformFlags transformFlags;
    AvifPixelAspectRatioBox pasp;
    /*
    AvifCleanApertureBox clap;
    AvifImageRotation irot;
    AvifImageMirror imir;

    // Metadata - set with avifImageSetMetadata*() before write, check .size>0 for existence after read
    AvifRWData exif; // exif_payload chunk from the ExifDataBlock specified in ISO/IEC 23008-12:2022 Section A.2.1.
                     // The value of the 4-byte exif_tiff_header_offset field, which is not part of this avifRWData
                     // byte sequence, can be retrieved by calling avifGetExifTiffHeaderOffset(avifImage.exif).
    AvifRWData xmp;

    // Version 1.0.0 ends here.

    // Other properties attached to this image item (primary or gainmap).
    // At decoding: Forwarded here as opaque byte sequences by the avifDecoder.
    // At encoding: Set using avifImageAddOpaqueProperty() or avifImageAddUUIDProperty() and written by the
    //              avifEncoder as non-essential properties in the order that they are added to the image.
    avifImageItemProperty * Properties; // NULL only if numProperties is 0.
    nuint NumProperties;

    // Gain map image and metadata. NULL if no gain map is present.
    // Owned by the avifImage and gets freed when calling avifImageDestroy().
    // gainMap->image->transformFlags is always AVIF_TRANSFORM_NONE.
    AvifGainMap * GainMap;

    // Version 1.2.0 ends here. Add any new members after this line.
    */
}