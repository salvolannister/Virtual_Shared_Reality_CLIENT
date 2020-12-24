using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace ARDI.Nuget.Impl
{
    /// <summary>
    /// The standard Nuget implementation.</summary>
    /// <remarks>
    /// \todo
    ///TODO: Investigate why this class has a completely different interface from <see cref="CoAppNuget"/>.
    /// </remarks>
    public class StandardNuget : ICaching
    {
        private NugetConfig _config;

        /// <summary>
        /// Creates a new <see cref="StandardNuget"/> using configuration <paramref name="config"/>.</summary>
        /// <param name="config">The nuget configuration to use.</param>
        public StandardNuget(NugetConfig config)
        {
            _config = config;
        }

        private string _GetNugetExecutable(string p_version)
        {
            string path = FileHelper.NormalizePath(_GetNugetInstallPath() + "\\Nuget." + p_version + "\\nuget.exe");

            if (File.Exists(path) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                Debug.Log(Path.GetDirectoryName(path));

                _DownloadStandardNuget(p_version, path);

                if (File.Exists(path) == false)
                {
                    Debug.LogError("Could not find nor install CoApp.");
                    return null;
                }
            }

            return path;
        }

        /// <summary>
        /// Downloads and unpacks dependency <paramref name="dep"/> into <paramref name="unpackPath"/>.</summary>
        /// <remarks>A subfolder of <paramref name="unpackPath"/> will be created to house the unpacked files. This subfolder name
        /// contains both the package name and it's version, to avoid any conflicts.</remarks>
        /// <param name="dep">The nuget dependency to download and unpack.</param>
        /// <param name="unpackPath">The path where the package will be unpacked.</param>
        public void DownloadDependency(NugetDependency dep, string unpackPath)
        {
            string nugetExe = _GetNugetExecutable(_config.StandardNugetVersion);

            if (nugetExe == null)
                return;

            _DownloadPackage(_config, dep, nugetExe);
        }

        /// <summary>
        /// Lists all the standard nuget packages that were downloaded and unpacked into <paramref name="path"/>.</summary>
        /// <param name="path">Path where to look for unpacked nuget packages.</param>
        /// <returns>List of unpacked nuget packages in <paramref name="path"/>.</returns>
        public List<NugetDependency> GetDownloadedPackages(string path)
        {
            List<NugetDependency> ret = new List<NugetDependency>();

            System.Diagnostics.Process proc = ProcessHelper.SpawnProcess(_GetNugetExecutable(_config.StandardNugetVersion), _config.GetProjectUnpackPath(), _GetListPackagesParms(path), true);

            foreach (string line in ProcessHelper.ReadStandardOut(proc))
            {
                // Debug.Log(line);

                string[] parts = line.Split(' ');

                if (parts.Length == 2)
                {
                    // Debug.Log("Accepted");
                    ret.Add(new NugetDependency(parts[0], parts[1], ""));
                }
                else
                {
                    Debug.LogError("Could not parse line " + line);
                }
            }

            return ret;
        }

        /// <summary>
        /// Installs <paramref name="dep"/>, which must be already unpacked at <paramref name="unpackPath"/>, into <paramref name="installPath"/>.</summary>
        /// <param name="dep">The nuget package to install.</param>
        /// <param name="unpackPath">The path where the package was <see cref="DownloadDependency">unpacked</see>.</param>
        /// <param name="installPath">Path where the contents of <paramref name="dep"/> will be copied to.</param>
        public void InstallDependency(NugetDependency dep, string unpackPath, string installPath)
        {
            string unpackedPath = _GetPackageUnpackPath(unpackPath, dep);

            _FindLeafAndCopyDLLs(unpackedPath + "\\lib", installPath, dep.Framework);
        }

        private string _GetListPackagesParms(string path)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("list -Prerelease ");

            sb.Append("-Source \"")
                .Append(path.Replace("\\", "\\\\"))
                .Append("\" ");

            return sb.ToString();
        }

        private void _FindLeafAndCopyDLLs(string p_root, string p_destination, params string[] p_allowedSubdirs)
        {
            p_root = FileHelper.NormalizePath(p_root);

            if (Directory.Exists(p_root) == false)
            {
                return;
            }

            p_root = FileHelper.FindLeafPath(p_root, p_allowedSubdirs);

            FileHelper.Copy(p_root, p_destination, "*.dll");
            FileHelper.Copy(p_root, p_destination, "*.tx");
            FileHelper.Copy(p_root, p_destination, "*.cel");
        }

        private bool _DownloadPackage(NugetConfig p_config, NugetDependency p_dep, string p_nugetExe)
        {
            var process = ProcessHelper.SpawnProcess(p_nugetExe, p_config.GetProjectUnpackPath(), _GetPackageDownloadParms(p_config, p_dep));

            return ProcessHelper.WaitForExit(process);
        }

        private string _DownloadStandardNuget(string version, string outPath)
        {
            return UnityHelper.DownloadFile(
                $"https://dist.nuget.org/win-x86-commandline/v{ version }/nuget.exe",
                outPath
                );
        }

        private string _GetPackageDownloadParms(NugetConfig p_config, NugetDependency p_dep)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("install -Prerelease ")
                .Append(p_dep.Name);

            _AppendVersion(sb, p_dep.Version);

            sb.Append(_GetSourcesParm(p_config))
                .Append(" -OutputDirectory \"")
                .Append(p_config.GetProjectUnpackPath().Replace("\\", "\\\\"))
                .Append("\" ");

            sb.Append("-Framework ")
                .Append(p_dep.Framework)
                .Append(" ");

            return sb.ToString();
        }

        private void _AppendVersion(StringBuilder p_sb, string p_version)
        {
            if (string.IsNullOrEmpty(p_version) == false)
            {
                p_sb.Append(" -Version ").Append(p_version);
            }
        }

        private string _GetSourcesParm(NugetConfig p_config)
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(p_config.Source) == false)
                sb.Append(" -Source ").Append(p_config.Source);

            if (p_config.UseNugetOrg == true)
                sb.Append(" -Source http://nuget.org/api/v2/");

            return sb.ToString();
        }

        private string _GetPackageUnpackPath(string unpackPath, NugetDependency p_dep)
        {
            return FileHelper.NormalizePath(unpackPath + "\\" + p_dep.Name + "." + p_dep.Version + "\\");
        }

        private string _GetNugetInstallPath()
        {
            return FileHelper.NormalizePath(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Nuget");
        }

        /// <summary>
        /// Clears the standard nuget cache.</summary>
        public void ClearCache()
        {
            string nugetExecutable = _GetNugetExecutable(_config.StandardNugetVersion);

            if (nugetExecutable != null)
            {
                ProcessHelper.SpawnProcess(nugetExecutable, _config.GetProjectUnpackPath(), _GetClearParms(_config));
            }

            Debug.Log("NuGet Cache cleared");
        }

        private string _GetClearParms(NugetConfig p_config)
        {
            return "locals all -Clear";
        }
    }
}