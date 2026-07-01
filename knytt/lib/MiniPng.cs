using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class MiniPng
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    // ---------- Quick size check without full decode ----------

    public static bool TryReadSize(byte[] data, out int width, out int height)
    {
        width = height = 0;
        if (data == null || data.Length < 24) return false;

        for (int i = 0; i < 8; i++)
            if (data[i] != Signature[i]) return false;

        string type = Encoding.ASCII.GetString(data, 12, 4);
        if (type != "IHDR") return false;

        width = ReadInt32BE(data, 16);
        height = ReadInt32BE(data, 20);
        return true;
    }

    // ---------- Full decode to RGBA8 ----------

    public static bool TryDecode(byte[] data, out int width, out int height, out byte[] rgba)
    {
        width = height = 0;
        rgba = null;

        if (data == null || data.Length < 8) return false;
        for (int i = 0; i < 8; i++)
            if (data[i] != Signature[i]) return false;

        int bit_depth = 0, color_type = 0;
        bool interlace = false;
        int idat_total = 0;
        var idat_offsets = new List<int>();
        var idat_lens = new List<int>();

        // palette: 256 RGBA entries
        byte[] palette = null;

        int pos = 8;
        while (pos + 8 <= data.Length)
        {
            int len = ReadInt32BE(data, pos);
            string type = Encoding.ASCII.GetString(data, pos + 4, 4);
            int data_start = pos + 8;

            if (type == "IHDR")
            {
                width = ReadInt32BE(data, data_start);
                height = ReadInt32BE(data, data_start + 4);
                bit_depth = data[data_start + 8];
                color_type = data[data_start + 9];
                interlace = data[data_start + 12] != 0;
            }
            else if (type == "PLTE")
            {
                // len is a multiple of 3: R,G,B per entry
                int count = len / 3;
                palette = new byte[256 * 4];
                for (int i = 0; i < count; i++)
                {
                    palette[i * 4]     = data[data_start + i * 3];
                    palette[i * 4 + 1] = data[data_start + i * 3 + 1];
                    palette[i * 4 + 2] = data[data_start + i * 3 + 2];
                    palette[i * 4 + 3] = 255; // fully opaque by default
                }
            }
            else if (type == "tRNS")
            {
                if (color_type == 3)
                {
                    // for palette: len alpha bytes, one per index
                    if (palette == null) palette = new byte[256 * 4];
                    for (int i = 0; i < len; i++)
                        palette[i * 4 + 3] = data[data_start + i];
                }
                // tRNS for color_type 0/2 (single transparent color) is not supported —
                // it's a rare case and requires separate handling
            }
            else if (type == "IDAT")
            {
                idat_offsets.Add(data_start);
                idat_lens.Add(len);
                idat_total += len;
            }
            else if (type == "IEND")
            {
                break;
            }

            pos = data_start + len + 4;
        }

        if (bit_depth != 8 || interlace) return false;
        if (color_type == 3 && palette == null) return false;

        int channels;
        switch (color_type)
        {
            case 0: channels = 1; break; // gray
            case 2: channels = 3; break; // rgb
            case 3: channels = 1; break; // palette — 1 byte index per pixel
            case 4: channels = 2; break; // gray+alpha
            case 6: channels = 4; break; // rgba
            default: return false;
        }

        byte[] idat = new byte[idat_total];
        int idat_pos = 0;
        for (int i = 0; i < idat_offsets.Count; i++)
        {
            Array.Copy(data, idat_offsets[i], idat, idat_pos, idat_lens[i]);
            idat_pos += idat_lens[i];
        }

        int stride = width * channels;
        int raw_size = height * (stride + 1);

        // extra stride bytes at the start = virtual zero row for Up/Average/Paeth at y=0
        byte[] buf = new byte[stride + raw_size];
        ZlibInflateInto(idat, buf, stride, raw_size);

        rgba = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            int filter_offset = stride + y * (stride + 1);
            byte filter_type = buf[filter_offset];
            int row_offset = filter_offset + 1;
            int prev_offset = row_offset - (stride + 1);

            UnfilterRowInPlace(buf, row_offset, prev_offset, stride, channels, filter_type);
            ConvertRowToRgba(buf, row_offset, width, channels, color_type, palette, rgba, y * width * 4);
        }

        return true;
    }

    // ---------- Unfiltering and conversion (decode helpers) ----------

    private static void UnfilterRowInPlace(byte[] buf, int row_offset, int prev_offset, int stride, int bpp, byte filter_type)
    {
        switch (filter_type)
        {
            case 1: // Sub
                for (int x = bpp; x < stride; x++)
                    buf[row_offset + x] = (byte)(buf[row_offset + x] + buf[row_offset + x - bpp]);
                break;
            case 2: // Up
                for (int x = 0; x < stride; x++)
                    buf[row_offset + x] = (byte)(buf[row_offset + x] + buf[prev_offset + x]);
                break;
            case 3: // Average
                for (int x = 0; x < stride; x++)
                {
                    int a = x >= bpp ? buf[row_offset + x - bpp] : 0;
                    buf[row_offset + x] = (byte)(buf[row_offset + x] + (a + buf[prev_offset + x]) / 2);
                }
                break;
            case 4: // Paeth
                for (int x = 0; x < stride; x++)
                {
                    int a = x >= bpp ? buf[row_offset + x - bpp] : 0;
                    int b = buf[prev_offset + x];
                    int c = x >= bpp ? buf[prev_offset + x - bpp] : 0;
                    buf[row_offset + x] = (byte)(buf[row_offset + x] + PaethPredictor(a, b, c));
                }
                break;
            // case 0 (None) — nothing to do
        }
    }

    private static void ConvertRowToRgba(byte[] buf, int row_offset, int width, int channels, int color_type, byte[] palette, byte[] rgba, int dst_offset)
    {
        switch (color_type)
        {
            case 0: // gray
                for (int x = 0; x < width; x++)
                {
                    byte g = buf[row_offset + x];
                    int d = dst_offset + x * 4;
                    rgba[d] = g; rgba[d + 1] = g; rgba[d + 2] = g; rgba[d + 3] = 255;
                }
                break;
            case 2: // rgb
                for (int x = 0; x < width; x++)
                {
                    int s = row_offset + x * 3;
                    int d = dst_offset + x * 4;
                    rgba[d] = buf[s]; rgba[d + 1] = buf[s + 1]; rgba[d + 2] = buf[s + 2]; rgba[d + 3] = 255;
                }
                break;
            case 3: // palette — look up color from PLTE/tRNS by index
                for (int x = 0; x < width; x++)
                {
                    int p = buf[row_offset + x] * 4;
                    int d = dst_offset + x * 4;
                    rgba[d] = palette[p]; rgba[d + 1] = palette[p + 1]; rgba[d + 2] = palette[p + 2]; rgba[d + 3] = palette[p + 3];
                }
                break;
            case 4: // gray+alpha
                for (int x = 0; x < width; x++)
                {
                    int s = row_offset + x * 2;
                    byte g = buf[s];
                    int d = dst_offset + x * 4;
                    rgba[d] = g; rgba[d + 1] = g; rgba[d + 2] = g; rgba[d + 3] = buf[s + 1];
                }
                break;
            case 6: // rgba — layout already matches, copy the whole row in one call
                Array.Copy(buf, row_offset, rgba, dst_offset, width * 4);
                break;
        }
    }

    private static int PaethPredictor(int a, int b, int c)
    {
        int p = a + b - c;
        int pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        if (pa <= pb && pa <= pc) return a;
        return pb <= pc ? b : c;
    }

    // ---------- zlib ----------

    private static void ZlibInflateInto(byte[] zlib_data, byte[] dest, int dest_offset, int expected_len)
    {
        using (var ms = new MemoryStream(zlib_data, 2, zlib_data.Length - 2 - 4))
        using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
        {
            int total_read = 0;
            while (total_read < expected_len)
            {
                int read = ds.Read(dest, dest_offset + total_read, expected_len - total_read);
                if (read <= 0) break;
                total_read += read;
            }
        }
    }

    // ---------- utilities ----------

    private static int ReadInt32BE(byte[] d, int off) =>
        (d[off] << 24) | (d[off + 1] << 16) | (d[off + 2] << 8) | d[off + 3];
}
