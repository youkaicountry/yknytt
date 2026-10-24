using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        // Sets key=value in one section of INI text, keeping everything else as it was
        public static string setIniValue(string ini, string section, string key, string value)
        {
            var header = Regex.Match(ini, $@"^\[{Regex.Escape(section)}\][^\n]*\n?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (!header.Success) { return $"[{section}]\r\n{key}={value}\r\n{ini}"; }

            int start = header.Index + header.Length;
            var next = Regex.Match(ini.Substring(start), @"^\[", RegexOptions.Multiline);
            int end = next.Success ? start + next.Index : ini.Length;
            string body = ini.Substring(start, end - start);

            var line = Regex.Match(body, $@"^{Regex.Escape(key)}[ \t]*=[^\r\n]*", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            body = line.Success ? body.Remove(line.Index, line.Length).Insert(line.Index, $"{key}={value}") : $"{key}={value}\r\n{body}";
            return ini.Substring(0, start) + body + ini.Substring(end);
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
