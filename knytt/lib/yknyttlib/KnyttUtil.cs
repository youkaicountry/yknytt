using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace YKnyttLib
{
    public static class KnyttUtil
    {
        public static int BGRToRGBA(int kc)
        {
            if (kc == 0) { return 0; }
            return ((0xFF0000 & kc) >> 8) + ((0xFF00 & kc) << 8) + ((0xFF & kc) << 24) + 0xFF;
        }

        public static int BGRToRGB(int kc)
        {
            return ((0xFF0000 & kc) >> 16) + (0xFF00 & kc) + ((0xFF & kc) << 16);
        }

        // Returns a value in a BGR encoded int. Returns default value if parse fails
        public static int parseBGRString(string val, int def_col)
        {
            return parseIniInt(val) ?? def_col;
        }

        public static int? parseIniInt(string value)
        {
            if (value == null) { return null; }
            value = new string(value.TakeWhile(c => char.IsDigit(c) || c == '-' || c == '+' || c == '.').ToArray());
            return double.TryParse(value,
                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, 
                out var i) ? (int)System.Math.Round(i) : null as int?;
        }

        public static string CompressString(string input)
        {
            var bytes = Encoding.Unicode.GetBytes(input);
            using (var instream = new MemoryStream(bytes))
            {
                using (var outstream = new MemoryStream())
                {
                    // TODO: Try Brotli / Zlib, when able to upgrade .net, or try Smaz
                    using (var cstream = new DeflateStream(outstream, CompressionLevel.Optimal))
                    {
                        instream.CopyTo(cstream);
                    } // Close GZipStream here so the compression can be completed.

                    var result = outstream.ToArray();
                    // TODO: Use higher base
                    return Convert.ToBase64String(result);
                }
            }
        }

        public static string DecompressString(string input)
        {
            var bytes = Convert.FromBase64String(input);
            using (var instream = new MemoryStream(bytes))
            {
                using (var outstream = new MemoryStream())
                {
                    using (var cstream = new DeflateStream(instream, CompressionMode.Decompress))
                    {
                        cstream.CopyTo(outstream);
                    } // Close GZipStream here so the decompression can be completed.

                    return Encoding.Unicode.GetString(outstream.ToArray());
                }
            }
        }
    }
}
