using System.Runtime.InteropServices;
using System.Security;

namespace WebPLib;

[SuppressUnmanagedCodeSecurity]
internal sealed class UnsafeNativeMethods
{
    #region Imports
    
    [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
    private static extern IntPtr WebPDecodeBGRAInto_x86([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer, int outputBufferSize, int outputStride);
    
    [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
    private static extern IntPtr WebPDecodeBGRAInto_x64([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer, int outputBufferSize, int outputStride);
    
    [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
    private static extern IntPtr WebPDecodeBGRInto_x86([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer, int outputBufferSize, int outputStride);
    
    [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
    private static extern IntPtr WebPDecodeBGRInto_x64([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer, int outputBufferSize, int outputStride);
    
    [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
    private static extern Vp8StatusCode WebPGetFeaturesInternal_x86([In] IntPtr rawWebP, UIntPtr dataSize, ref WebPBitstreamFeatures features, int webpDecoderAbiVersion);
    
    [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
    private static extern Vp8StatusCode WebPGetFeaturesInternal_x64([In] IntPtr rawWebP, UIntPtr dataSize, ref WebPBitstreamFeatures features, int webpDecoderAbiVersion);
    
    #endregion

    private const int WebpDecoderAbiVersion = 0x0208;

    /// <summary>
    /// Decode WEBP image pointed to by *data and returns BGRA samples into a pre-allocated buffer.
    /// </summary>
    /// <param name="data">WebP data.</param>
    /// <param name="dataSize">Size of the data.</param>
    /// <param name="buffer">Pointer to the decoded WebP image.</param>
    /// <param name="bufferSize">Size of the allocated buffer.</param>
    /// <param name="stride">Specifies the distance between scan lines.</param>
    /// <exception cref="Exception">Thrown if invalid platform.</exception>
    internal static void WebPDecodeBgraInto(IntPtr data, int dataSize, IntPtr buffer, int bufferSize, int stride)
    {
        switch (IntPtr.Size)
        {
            case 4:
                WebPDecodeBGRAInto_x86(data, (UIntPtr)dataSize, buffer, bufferSize, stride);
                break;
            
            case 8:
                WebPDecodeBGRAInto_x64(data, (UIntPtr)dataSize, buffer, bufferSize, stride);
                break;
            
            default:
                throw new Exception("Invalid platform. Can not find proper function.");
        }
    }

    /// <summary>
    /// Decode WEBP image pointed to by *data and returns BGR samples into a pre-allocated buffer.
    /// </summary>
    /// <param name="data">WebP data.</param>
    /// <param name="dataSize">Size of the data.</param>
    /// <param name="buffer">Pointer to the decoded WebP image.</param>
    /// <param name="bufferSize">Size of the allocated buffer.</param>
    /// <param name="stride">Specifies the distance between scan lines.</param>
    /// <exception cref="Exception">Thrown if invalid platform.</exception>
    internal static void WebPDecodeBgrInto(IntPtr data, int dataSize, IntPtr buffer, int bufferSize, int stride)
    {
        switch (IntPtr.Size)
        {
            case 4:
                WebPDecodeBGRInto_x86(data, (UIntPtr)dataSize, buffer, bufferSize, stride);
                break;
            
            case 8:
                WebPDecodeBGRInto_x64(data, (UIntPtr)dataSize, buffer, bufferSize, stride);
                break;
            
            default:
                throw new Exception("Invalid platform. Can not find proper function.");
        }
    }

    /// <summary>
    /// Get info of WepP image.
    /// </summary>
    /// <param name="pinnedPtr">WebP bytes.</param>
    /// <param name="dataSize">Size of the data.</param>
    /// <param name="features">WebP features.</param>
    /// <returns><see cref="Vp8StatusCode"/></returns>
    /// <exception cref="Exception">Thrown if invalid platform.</exception>
    internal static Vp8StatusCode WebPGetFeatures(IntPtr pinnedPtr, int dataSize, ref WebPBitstreamFeatures features)
    {
        return IntPtr.Size switch
        {
            4 => WebPGetFeaturesInternal_x86(pinnedPtr, (UIntPtr)dataSize, ref features, WebpDecoderAbiVersion),
            8 => WebPGetFeaturesInternal_x64(pinnedPtr, (UIntPtr)dataSize, ref features, WebpDecoderAbiVersion),
            _ => throw new Exception("Invalid platform. Can not find proper function.")
        };
    }
}