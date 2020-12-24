using System;
using System.IO;

using UnityEditor;

namespace ARDI.Nuget.Impl
{
    /// <summary>
    /// Helper functions to manipulate file and folders.</summary>
    public static class FileHelper
    {
        /// <summary>
        /// Normalizes directory separator chars and retuns <paramref name="path"/>'s full path.</summary>
        /// <remarks>
        /// <seealso cref=""><a href="https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getfullpath?view=netframework-4.7.2">Path.GetFullPath</a>.</seealso></remarks>
        /// <param name="path">File or folder path to normalize.</param>
        /// <returns>Normalized full path of <paramref name="path"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        public static string NormalizePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            string temp = path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            return Path.GetFullPath(temp);
        }

        /// <summary>
        /// Copies all files from <paramref name="sourcePath"/> that match <paramref name="globPattern"/> into <paramref name="destinationPath"/>.</summary>
        /// <remarks>Each file in <paramref name="sourcePath"/> is processed individually. It's deleted from <paramref name="destinationPath"/> if it already exists there.
        /// <seealso cref="">For more information about the glob pattern, see <a href="https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=netframework-4.7.2#System_IO_Directory_GetFiles_System_String_System_String_">Directory.GetFiles</a>.</seealso></remarks>
        /// <param name="sourcePath">Source folder of the copy operation.</param>
        /// <param name="destinationPath">Destination folder of the copy operation.</param>
        /// <param name="globPattern">Glob pattern to select which files to copy.</param>
        public static void Copy(string sourcePath, string destinationPath, string globPattern)
        {
            foreach (string file in Directory.GetFiles(sourcePath, globPattern))
            {
                FileInfo fileInfo = new FileInfo(file);

                string dest = NormalizePath(destinationPath + "\\" + fileInfo.Name);

                if (File.Exists(dest) == true)
                {
                    UnityEngine.Debug.Log("Deleting " + dest);

                    File.Delete(dest);
                }

                UnityEngine.Debug.Log("Copying " + dest);

                FileUtil.CopyFileOrDirectory(
                    file,
                    dest
                    );
            }
        }

        /// <summary>
        /// Returns the path obtained by starting from <paramref name="root"/> and descending into <paramref name="allowedPaths"/>.</summary>
        /// <remarks>
        /// Starting from <paramref name="root"/>, this method recursivelly descends into the first path in <paramref name="allowedPaths"/> found in the current path.</remarks>
        /// <param name="root">Root path to start the descent.</param>
        /// <param name="allowedPaths">Paths that we are allowed to descend into.</param>
        /// <returns>The normalized path of <paramref name="root"/>'s subdir obtained by recursivelly descending into <paramref name="allowedPaths"/>, if present.</returns>
        public static string FindLeafPath(string root, params string[] allowedPaths)
        {
            root = NormalizePath(root);

            foreach (string allowedSubdir in allowedPaths)
            {
                string dir = root + Path.DirectorySeparatorChar + allowedSubdir;

                if (Directory.Exists(dir) == true)
                {
                    return FindLeafPath(dir, allowedPaths);
                }
            }

            return root;
        }

        /// <summary>
        /// Removes any file or folder present in <paramref name="path"/>.</summary>
        /// <param name="path">Path to clear.</param>
        public static void ClearPath(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (DirectoryNotFoundException ex)
            {
                UnityEngine.Debug.LogWarning(ex);
            }

            Directory.CreateDirectory(path);
        }
    }
}