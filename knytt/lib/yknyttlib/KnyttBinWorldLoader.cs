using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

/**
 * This class was adapted from KnyttSharp, by Codeusa
 * https://github.com/Codeusa/KnyttSharp
 * MIT License
 * Copyright (c) 2019 Andrew Sampson
 **/

namespace YKnyttLib
{
    public class KnyttBinWorldLoader
    {
        private readonly Dictionary<string, byte[]> _files;
        private readonly Dictionary<string, string> _paths;

        /// <summary>
        /// The name of the level found inside the .knytt.bin 
        /// </summary>
        public string RootDirectory { get; set; }

        public KnyttBinWorldLoader(byte[] data) : this(new MemoryStream(data)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="knyttBinPath"></param>
        public KnyttBinWorldLoader(Stream stream)
        {
            RootDirectory = string.Empty;
            _files = new Dictionary<string, byte[]>();
            _paths = new Dictionary<string, string>();
            open(stream);
        }

        /// <summary>
        /// Retrieve a file directly from .knytt.bin without saving to disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] GetFile(string fileName)
        {
            return !_files.ContainsKey(fileName.ToLower()) ? null : _files[fileName.ToLower()];
        }

        /// <summary>
        /// Add or replace a file in memory. Call Pack() to get the new .knytt.bin.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public void SetFile(string fileName, byte[] data)
        {
            _paths[fileName.ToLower()] = fileName;
            _files[fileName.ToLower()] = data;
        }

        /// <summary>
        /// Pack all files back into .knytt.bin data.
        /// </summary>
        /// <returns></returns>
        public byte[] Pack()
        {
            var encoding = Encoding.GetEncoding(1252);
            using (var stream = new MemoryStream())
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(encoding.GetBytes($"NF{RootDirectory}\0"));
                binaryWriter.Write((uint)_files.Count);
                foreach (var file in _files)
                {
                    binaryWriter.Write(encoding.GetBytes($"NF{_paths[file.Key].Replace('/', '\\')}\0"));
                    binaryWriter.Write((uint)file.Value.Length);
                    binaryWriter.Write(file.Value);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Retrieve the size of a file in .knytt.bin without saving to disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int GetFileSize(string fileName)
        {
            return !_files.ContainsKey(fileName.ToLower()) ? -1 : _files[fileName.ToLower()].Length;
        }

        /// <summary>
        /// Retrieve a list of all files in the .knytt.bin 
        /// </summary>
        /// <param name="regexFilter"></param>
        /// <returns></returns>
        public List<string> GetFileNames(string regexFilter = "")
        {
            if (string.IsNullOrWhiteSpace(regexFilter)) return _files.Keys.ToList();
            var matchedFiles = new List<string>();
            foreach (var key in _files.Keys)
            {
                if (Regex.IsMatch(key, regexFilter))
                {
                    matchedFiles.Add(key);
                }
            }
            return matchedFiles;
        }

        /// <summary>
        /// Proceed with opening and parsing a .knytt.bin file.
        /// </summary>
        /// <returns></returns>
        private void open(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream))
            {
                var header = binaryReader.ReadChars(2);
                if (!header.SequenceEqual(new[] { 'N', 'F' }))
                {
                    throw new InvalidOperationException("Not a valid compressed World file (.knytt.bin) -- missing NF header.");
                }

                var encoding = Encoding.GetEncoding(1252);
                //get the level name.
                char levelChr;
                while ((levelChr = encoding.GetChars(new byte[]{binaryReader.ReadByte()})[0]) != 0)
                {
                    RootDirectory += levelChr;
                }
                //this doesn't match the final files I find, so that is confusing.
                var fileCount = binaryReader.ReadUInt32();

                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    //skip ['N', 'F']
                    binaryReader.BaseStream.Seek(2, SeekOrigin.Current);

                    var filePath = string.Empty;
                    char fileChr;
                    //build a file name
                    while ((fileChr = encoding.GetChars(new byte[]{binaryReader.ReadByte()})[0]) != 0)
                    {
                        filePath += (fileChr == '\\') ? '/' : fileChr;
                    }
                    var sizeData = binaryReader.ReadBytes(4);

                    //what in the fuck
                    var fileSize = sizeData[0] + sizeData[1] * 256 + sizeData[2] * 65536 + sizeData[3] * 16777216;

                    //read and store the file.
                    SetFile(filePath, binaryReader.ReadBytes(fileSize));
                }
            }
        }
    }
}
