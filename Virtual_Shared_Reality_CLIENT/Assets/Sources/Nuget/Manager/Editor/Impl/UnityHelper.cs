using System.IO;

using UnityEditor;
using UnityEngine;

namespace ARDI.Nuget.Impl
{
    /// <summary>
    /// Helper functions to download files.</summary>
    /// <remarks>
    /// \todo
    ///TODO: Update this class to use UnityWebRequest
    /// </remarks>
    public static class UnityHelper
    {
        /// <summary>
        /// Downloads the file pointed by <paramref name="url"/> and writes it into <paramref name="path"/>.</summary>
        /// <remarks>
        /// Shows a progress bar in Unity editor.</remarks>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="path">Path where the file will be written.</param>
        /// <returns>Normalized <paramref name="path"/>.</returns>
        [System.Obsolete]
        public static string DownloadFile(string url, string path)
        {
            path = FileHelper.NormalizePath(path);

            WWW www = DownloadFile(url);

            File.WriteAllBytes(path, www.bytes);

            return path;
        }

        /// <summary>
        /// Downloads a file and leaves it in the WWW object.</summary>
        /// Shows a progress bar in Unity editor.</remarks>
        /// <param name="url">The URL of the file to download.</param>
        /// <returns>The WWW object holding the file.</returns>
        [System.Obsolete]
        public static WWW DownloadFile(string url)
        {
            WWW www = new WWW(url);

            while (www.isDone == false)
            {
                EditorUtility.DisplayProgressBar("Downloading...", "", www.progress);

                System.Threading.Thread.Sleep(100);
            }

            EditorUtility.ClearProgressBar();

            return www;
        }
    }
}