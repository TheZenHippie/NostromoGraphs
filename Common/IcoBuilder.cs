using System;
using System.IO;
using SkiaSharp;

namespace NostromoGraphs.Common
{
    public static class IcoBuilder
    {
        public static void GenerateIcon(string pngPath, string icoPath)
        {
            if (!File.Exists(pngPath)) return;

            using var srcBitmap = SKBitmap.Decode(pngPath);
            if (srcBitmap == null) return;

            int[] sizes = { 256, 128, 64, 48, 32, 24, 16 };
            byte[][] imagesData = new byte[sizes.Length][];

            for (int i = 0; i < sizes.Length; i++)
            {
                int sz = sizes[i];
                var info = new SKImageInfo(sz, sz, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                using var resized = new SKBitmap(info);
                srcBitmap.ScalePixels(resized, SKFilterQuality.High);

                if (sz == 256)
                {
                    // 256x256 encoded as PNG
                    using var image = SKImage.FromBitmap(resized);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    imagesData[i] = data.ToArray();
                }
                else
                {
                    // <=128x128 encoded as standard Windows DIB (BITMAPINFOHEADER + BGRA pixels + 1-bit AND mask)
                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);

                    int maskRowBytes = ((sz + 31) / 32) * 4;
                    int maskSize = maskRowBytes * sz;
                    int pixelDataSize = sz * sz * 4;

                    // BITMAPINFOHEADER (40 bytes)
                    bw.Write((uint)40);                         // biSize
                    bw.Write((int)sz);                          // biWidth
                    bw.Write((int)(sz * 2));                    // biHeight (doubled for ICO)
                    bw.Write((ushort)1);                        // biPlanes
                    bw.Write((ushort)32);                       // biBitCount
                    bw.Write((uint)0);                          // biCompression (BI_RGB)
                    bw.Write((uint)(pixelDataSize + maskSize)); // biSizeImage
                    bw.Write((int)0);                           // biXPelsPerMeter
                    bw.Write((int)0);                           // biYPelsPerMeter
                    bw.Write((uint)0);                          // biClrUsed
                    bw.Write((uint)0);                          // biClrImportant

                    // Raw pixels in bottom-to-top order (BGRA)
                    IntPtr pixelsPtr = resized.GetPixels();
                    byte[] pixelBytes = new byte[pixelDataSize];
                    System.Runtime.InteropServices.Marshal.Copy(pixelsPtr, pixelBytes, 0, pixelDataSize);

                    int stride = sz * 4;
                    for (int y = sz - 1; y >= 0; y--)
                    {
                        bw.Write(pixelBytes, y * stride, stride);
                    }

                    // 1-bit AND mask (all 0 for 32-bit transparent alpha)
                    byte[] mask = new byte[maskSize];
                    bw.Write(mask);

                    imagesData[i] = ms.ToArray();
                }
            }

            // Write full ICO file
            using (var fs = new FileStream(icoPath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write((ushort)0);              // idReserved
                bw.Write((ushort)1);              // idType (1 = icon)
                bw.Write((ushort)sizes.Length);   // idCount

                uint offset = (uint)(6 + sizes.Length * 16);

                for (int i = 0; i < sizes.Length; i++)
                {
                    int sz = sizes[i];
                    bw.Write((byte)(sz >= 256 ? 0 : sz)); // bWidth
                    bw.Write((byte)(sz >= 256 ? 0 : sz)); // bHeight
                    bw.Write((byte)0);                     // bColorCount
                    bw.Write((byte)0);                     // bReserved
                    bw.Write((ushort)1);                   // wPlanes
                    bw.Write((ushort)32);                  // wBitCount
                    bw.Write((uint)imagesData[i].Length);  // dwBytesInRes
                    bw.Write(offset);                      // dwImageOffset

                    offset += (uint)imagesData[i].Length;
                }

                for (int i = 0; i < sizes.Length; i++)
                {
                    bw.Write(imagesData[i]);
                }
            }
        }
    }
}

