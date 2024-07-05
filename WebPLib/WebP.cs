using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WebPLib;

public class WebP : IWebP
{
    #region IWebP implementations

    /// <summary>
    /// <inheritdoc cref="IWebP.Load"/>
    /// </summary>
    public Bitmap Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var bitmap = this.Decode(bytes);

        return bitmap;
    }

    #endregion

    #region Helper functions

    /// <summary>
    /// Decode a WebP image.
    /// </summary>
    /// <param name="bytes">Data to uncompress.</param>
    /// <returns>Bitmap of the WebP data.</returns>
    private Bitmap Decode(byte[] bytes)
    {
        var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        GetImageInfo(
            bytes, 
            out var width, 
            out var height, 
            out var hasAlpha);

        var bitmap = new Bitmap(
            width, 
            height, 
            hasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

        var data = bitmap.LockBits(
            new(0, 0, width, height),
            ImageLockMode.WriteOnly,
            bitmap.PixelFormat);

        var size = data.Stride * height;
        var dataPtr = pinned.AddrOfPinnedObject();

        if (bitmap.PixelFormat is PixelFormat.Format24bppRgb)
        {
            UnsafeNativeMethods.WebPDecodeBgrInto(
                dataPtr,
                bytes.Length,
                data.Scan0,
                size,
                data.Stride);
        }
        else
        {
            UnsafeNativeMethods.WebPDecodeBgraInto(
                dataPtr,
                bytes.Length,
                data.Scan0,
                size,
                data.Stride);
        }

        bitmap.UnlockBits(data);

        if (pinned.IsAllocated)
        {
            pinned.Free();
        }

        return bitmap;
    }

    /// <summary>
    /// Get info of WEBP data.
    /// </summary>
    /// <param name="bytes">WebP data.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="hasAlpha">Whether image has alpha channel.</param>
    /// <exception cref="Exception">Thrown if loaded library gives invalid output.</exception>
    private void GetImageInfo(
        byte[] bytes, 
        out int width, 
        out int height, 
        out bool hasAlpha)
    {
        var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        var pinnedPtr = pinned.AddrOfPinnedObject();
        var features = new WebPBitstreamFeatures();
        var result = UnsafeNativeMethods.WebPGetFeatures(pinnedPtr, bytes.Length, ref features);

        if (result is not Vp8StatusCode.Vp8StatusOk)
        {
            throw new Exception("Unable to load WebP image.");
        }

        width = features.Width;
        height = features.Height;
        hasAlpha = features.Has_alpha is 1;

        if (pinned.IsAllocated)
        {
            pinned.Free();
        }
    }

#endregion
}