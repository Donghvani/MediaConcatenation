using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmergenceGuardian.FFmpeg;

namespace MVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = ConfigurationManager.AppSettings.Get("directoryPath");
            string extension = "*.mp4";
            string[] videoFiles = GetFilesInDirectory(path, extension);

            Dictionary<string, List<string>> sortedFilesDictionary = GetDictionary(videoFiles);
            foreach (KeyValuePair<string, List<string>> keyValuePair in sortedFilesDictionary)
            {
                ProcessStartOptions processStartOptions =
                    new ProcessStartOptions(FFmpegDisplayMode.None, "test", ProcessPriorityClass.High);

                MediaMuxer.Concatenate(keyValuePair.Value, "complete\\" +
                                                           new DirectoryInfo(
                                                                   keyValuePair.Key.Remove(
                                                                       keyValuePair.Key.Length - 10))
                                                               .Name + ".mp4",
                    processStartOptions);
                Console.WriteLine(keyValuePair);
            }
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
