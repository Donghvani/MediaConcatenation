using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using EmergenceGuardian.FFmpeg;

namespace MVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            //MergeTsFiles();

            string terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            string videoFilePath = "a.mp4";
            string audioFilePath = "b.mp4";
            string outPutFile = "d.mp4";
            if (
                File.Exists(terminal)
                &&
                File.Exists(videoFilePath)
                &&
                File.Exists(audioFilePath)
                )
            {
                MergeVideoAndAudio(terminal, videoFilePath, audioFilePath, outPutFile);
            }            
        }

        public static void MergeVideoAndAudio(string terminal, string videoFilePath, string audioFilePath, string outPutFile)
        {
            string args = $"ffmpeg -i {videoFilePath} -i {audioFilePath} -shortest {outPutFile}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = terminal,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = args
            };
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        public static void MergeTsFiles()
        {
            string path = ConfigurationManager.AppSettings.Get("directoryPath");
            //string extension = "*.mp4";
            string extension = "*.ts";
            string[] videoFiles = GetFilesInDirectory(path, extension);

            Dictionary<string, List<string>> sortedFilesDictionary = GetDictionary(videoFiles);
            foreach (KeyValuePair<string, List<string>> keyValuePair in sortedFilesDictionary)
            {
                ProcessStartOptions processStartOptions =
                    new ProcessStartOptions(FFmpegDisplayMode.None, "test", ProcessPriorityClass.High);

                var fileName = new DirectoryInfo(keyValuePair.Key).Name + ".mp4";
                var saveDir = keyValuePair.Key.Replace(path, @"C:\TMP");
                var savePath = saveDir + "\\" + fileName;
                if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new Exception("invalid filename");
                }

                if (!File.Exists(savePath))
                {
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }

                    var completionStatus = MediaMuxer.Concatenate(keyValuePair.Value, savePath, processStartOptions);
                    Console.WriteLine($"{completionStatus}: {savePath}");
                }
            }
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        /// <summary>
        /// Get array of files within the directory with selected extension
        /// </summary>
        /// <param name="path">Direcoty path</param>
        /// <param name="extension">File extension (example: "*.mp4")</param>
        /// <returns>Array of files</returns>
        private static string[] GetFilesInDirectory(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        /// <summary>
        /// Get dictionary object where keys will be directory names
        ///  and values will be files within that directory
        /// </summary>
        /// <param name="files">List of files</param>
        /// <returns>Dictionary object</returns>
        private static Dictionary<string, List<string>> GetDictionary(string[] files)
        {
            Dictionary<string, List<string>> sortedFiles = new Dictionary<string, List<string>>();
            foreach (string file in files)
            {
                string folder = Path.GetDirectoryName(file);
                if (sortedFiles.ContainsKey(folder))
                {
                    sortedFiles[folder].Add(file);
                }
                else
                {
                    sortedFiles.Add(folder, new List<string> {file});
                }
            }
            foreach (string key in sortedFiles.Keys)
            {
                sortedFiles[key].Sort((a, b) =>
                {
                    int fileA = Convert.ToInt32(Path.GetFileNameWithoutExtension(a));
                    int fileB = Convert.ToInt32(Path.GetFileNameWithoutExtension(b));
                    return fileA - fileB;
                });
            }
            return sortedFiles;
        }
    }
}
