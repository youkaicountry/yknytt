using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// The name of the level found inside the .knytt.bin 
        /// </summary>
        public string RootDirectory { get; private set; }

        public KnyttBinWorldLoader(byte[] data) : this(new MemoryStream(data)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="knyttBinPath"></param>
        public KnyttBinWorldLoader(Stream stream)
        {
            RootDirectory = string.Empty;
            _files = new Dictionary<string, byte[]>();
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

                //get the level name.
                while (binaryReader.PeekChar() != 0)
                {
                    RootDirectory += binaryReader.ReadChar();
                }
                //this doesn't match the final files I find, so that is confusing.
                var fileCount = binaryReader.ReadUInt32();

                // Skip initial N/F
                binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    //skip ['N', 'F']
                    binaryReader.BaseStream.Seek(2, SeekOrigin.Current);

                    var filePath = string.Empty;
                    char fileChr;
                    //build a file name
                    while ((fileChr = binaryReader.ReadChar()) != 0)
                    {
                        filePath += (fileChr == '\\') ? '/' : fileChr;
                    }
                    var sizeData = binaryReader.ReadBytes(4);

                    //what in the fuck
                    var fileSize = sizeData[0] + sizeData[1] * 256 + sizeData[2] * 65536 + sizeData[3] * 16777216;

                    //read and store the file.
                    _files[filePath.ToLower()] = binaryReader.ReadBytes(fileSize);
                }
            }
        }
    }
}