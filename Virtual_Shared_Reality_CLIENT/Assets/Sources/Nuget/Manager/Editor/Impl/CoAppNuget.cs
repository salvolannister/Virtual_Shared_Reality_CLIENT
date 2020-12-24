using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace ARDI.Nuget.Impl
{
    /// <summary>
    /// CoAppNuget is a special Nuget version with some added features like native package creation and consumption. </summary>
    public class CoAppNuget : ICaching
    {
        private NugetConfig m_config;

        /// <summary>
        /// Creates a new <see cref="CoAppNuget"/> using configuration <paramref name="config"/>.</summary>
        /// <param name="config">The nuget configuration to use.</param>
        public CoAppNuget(NugetConfig config)
        {
            m_config = config;
        }

        /// <summary>
        /// Checks if CoApp is installed.</summary>
        /// <returns>True if CoApp was found in this machine, false otherwise.</returns>
        public bool IsInstalled()
        {
            return _SearchCoApp(m_config.CoAppVersion) != null;
        }

        /// <summary>
        /// Install CoApp in this machine.</summary>
        /// <returns>True on success.</returns>
        public bool Install()
        {
            string standardNuget = _DownloadStandardNuget();

            return _InstallCoApp(standardNuget, m_config);
        }

        /// <summary>
        /// Installs all the dependencies listed in <see cref="NugetConfig.Dependencies"/> used during construction.</summary>
        public void InstallDependencies()
        {
            if ((IsInstalled() == false) && (Install() == false))
            {
                Debug.LogError("Could not find nor install CoApp.");
                return;
            }

            string nugetExe = _SearchCoApp(m_config.CoAppVersion);

            _DownloadPackages(m_config, nugetExe);

            _InstallPackages(m_config.Dependencies, nugetExe);
        }

        private void _InstallPackages(List<NugetDependency> p_deps, string p_nugetExe)
        {
            List<NugetDependency> allDeps = new List<NugetDependency>(p_deps);

            foreach (NugetDependency dep in p_deps)
            {
                List<NugetDependency> transitiveDeps = _GetTransitiveDependencies(dep, p_nugetExe);

                _AddUniqueDeps(allDeps, transitiveDeps);
            }

            foreach (NugetDependency dep in allDeps)
            {
                _InstallPackage(dep);
            }

            AssetDatabase.Refresh();
        }

        private List<NugetDependency> _GetTransitiveDependencies(NugetDependency p_dep, string p_nugetExe)
        {
            List<NugetDependency> dependencies = new List<NugetDependency>();

            System.Diagnostics.Process proc = _SpawnProcess(p_nugetExe, _GetListDependenciesParms(p_dep));

            if (proc == null)
                return dependencies;

            while (proc.HasExited == false)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (proc.ExitCode != 0)
            {
                Debug.LogError(proc.StandardOutput.ReadToEnd());
            }
            else
            {
                string line = proc.StandardOutput.ReadLine(); // The first line is the package itself
                string[] parts = line.Split(' ');

                p_dep.Version = parts[1]; // Update my version, since I need the exact string (eg. 0.1 --> 0.1.0)

                while (proc.StandardOutput.Peek() > 0)
                {
                    line = proc.StandardOutput.ReadLine();
                    parts = line.Split(' ');

                    if (parts.Length != 3) // Name, version, total number of transitive dependencies (including grandsons)
                        continue;

                    dependencies.Add(new NugetDependency(parts[0], parts[1], p_dep.Framework));
                }
            }

            return dependencies;
        }

        private void _InstallPackage(NugetDependency p_dep)
        {
            string unpackedPath = _GetPackageUnpackPath(p_dep);

            string projectPackagePath = m_config.GetProjectPluginPath();

            _FindLeafAndCopyDLLs(unpackedPath + "\\lib", projectPackagePath, "net35", "net20");

            _FindLeafAndCopyDLLs(unpackedPath + "\\build\\native\\bin", projectPackagePath, m_config.MsvcVersion.ToString(), m_config.FallbackMsvcVersion.ToString(), "dynamic", "x64", "cdecl", "Release");
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

        private void _DownloadPackages(NugetConfig p_config, string p_nugetExe)
        {
            float progress = 0.0f;

            List<NugetDependency> depsToOverlay = new List<NugetDependency>(p_config.Dependencies);

            foreach (NugetDependency dep in p_config.Dependencies)
            {
                EditorUtility.DisplayProgressBar("Downloading dependencies", "Downloading " + dep.Name + "...", progress);

                if (_DownloadPackage(p_config, dep, p_nugetExe) == false)
                {
                    Debug.LogError("Could not install package " + dep.Name);
                    break;
                }

                _AddUniqueDeps(depsToOverlay, _GetTransitiveDependencies(dep, p_nugetExe));
                progress += 0.5f / p_config.Dependencies.Count;
            }

            foreach (NugetDependency depToOverlay in depsToOverlay)
            {
                EditorUtility.DisplayProgressBar("Downloading dependencies", "Downloading Overlays for " + depToOverlay.Name + "...", progress);

                _DownloadPackageOverlay(p_config, depToOverlay, p_nugetExe);

                progress += 0.5f / depsToOverlay.Count;
            }

            EditorUtility.ClearProgressBar();
        }

        private void _AddUniqueDeps(List<NugetDependency> p_depList, List<NugetDependency> p_toAdd)
        {
            foreach (NugetDependency dep in p_toAdd)
            {
                NugetDependency existing = p_depList.Find(x => x.Name == dep.Name);

                if (existing == null)
                {
                    p_depList.Add(dep);
                    continue;
                }

                if (existing.Version != dep.Version)
                {
                    throw new UnityException("Dependency '" + dep.Name + "' added twice with different versions: " + existing.Version + ", " + dep.Version);
                }
            }
        }

        private bool _DownloadPackageOverlay(NugetConfig p_config, NugetDependency p_dep, string p_nugetExe)
        {
            var process = _SpawnProcess(p_nugetExe, _GetPackageOverlayInstallParms(p_config, p_dep, p_config.MsvcVersion));

            if (ProcessHelper.WaitForExit(process) == true)
                return true;

            var fallbackprocess = _SpawnProcess(p_nugetExe, _GetPackageOverlayInstallParms(p_config, p_dep, p_config.FallbackMsvcVersion));

            if (ProcessHelper.WaitForExit(fallbackprocess) == true)
                return true;

            Debug.LogWarning("No overlay for package " + p_dep.Name + ".");

            return false;
        }

        private bool _DownloadPackage(NugetConfig p_config, NugetDependency p_dep, string p_nugetExe)
        {
            var process = _SpawnProcess(p_nugetExe, _GetPackageInstallParms(p_config, p_dep));

            return ProcessHelper.WaitForExit(process);
        }

        private bool _InstallCoApp(string p_nugetExe, NugetConfig p_config)
        {
            System.Diagnostics.Process nuget = _SpawnProcess(p_nugetExe, _GetInstallCoAppParms(p_config));

            float expectedLines = 2;
            string lastLine = "";
            int linesRead = 0;
            while (nuget.HasExited == false)
            {
                while (nuget.StandardOutput.Peek() > 0)
                {
                    lastLine = nuget.StandardOutput.ReadLine();
                    if (lastLine != null)
                    {
                        linesRead++;
                    }
                }

                EditorUtility.DisplayProgressBar("Downloading CoApp " + p_config.CoAppVersion + "...", "", linesRead / expectedLines);

                System.Threading.Thread.Sleep(100);
            }

            EditorUtility.ClearProgressBar();

            if (nuget.ExitCode != 0)
            {
                Debug.LogError("Couldn't install CoApp: " + lastLine);
                return false;
            }

            return true;
        }

        private System.Diagnostics.Process _SpawnProcess(string executable, string parms)
        {
            return ProcessHelper.SpawnProcess(executable, _GetUnpackPath(), parms);
        }

        private string _SearchCoApp(string p_version)
        {
            string path = _GetNugetInstallPath() + "\\CoAppCMake." + p_version + "\\nuget.exe";

            if (File.Exists(path) == true)
            {
                return path;
            }

            return null;
        }

        private string _DownloadStandardNuget()
        {
            return UnityHelper.DownloadFile(
                "http://www.nuget.org/nuget.exe", 
                Application.temporaryCachePath + "\\nuget.exe"
                );
        }

        private string _GetListDependenciesParms(NugetDependency p_dep)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("dependencies ")
                .Append(p_dep.Name);

            _AppendVersion(sb, p_dep.Version);

            sb.Append(" -LocalRepo \"")
                .Append(_GetUnpackPath())
                .Append("\"");

            return sb.ToString();
        }

        private string _GetPackageOverlayInstallParms(NugetConfig p_config, NugetDependency p_dep, VisualStudioVersions version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("overlay ")
                .Append(" -OverlayPackageDirectory ")
                .Append(_GetPackageUnpackPath(p_dep));

            sb.Append(" -Pivots").Append(" Release cdecl x64 " + version.ToString() + " dynamic");

            _AppendVersion(sb, p_dep.Version);

            sb.Append(_GetSourcesParm(p_config));

            return sb.ToString();
        }

        private string _GetPackageInstallParms(NugetConfig p_config, NugetDependency p_dep)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("install ")
                .Append(p_dep.Name);

            _AppendVersion(sb, p_dep.Version);

            sb.Append(_GetSourcesParm(p_config))
                .Append(" -OutputDirectory ")
                .Append(_GetUnpackPath());

            return sb.ToString();
        }

        private string _GetInstallCoAppParms(NugetConfig p_config)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("install CoAppCMake")
                .Append(_GetSourcesParm(p_config))
                .Append(" -NoCache")
                .Append(" -OutputDirectory ")
                .Append(_GetNugetInstallPath());

            _AppendVersion(sb, p_config.CoAppVersion);

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
                sb.Append(" -Source http://nuget.org/api/odata");

            return sb.ToString();
        }

        private string _GetPackageUnpackPath(NugetDependency p_dep)
        {
            return FileHelper.NormalizePath(System.Environment.GetEnvironmentVariable("TEMP") + "\\" + p_dep.Name + "." + p_dep.Version + "\\");
        }

        private string _GetUnpackPath()
        {
            return FileHelper.NormalizePath(System.Environment.GetEnvironmentVariable("TEMP"));
        }

        private string _GetNugetInstallPath()
        {
            return FileHelper.NormalizePath(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Nuget");
        }

        private string _GetNugetCachePath()
        {
            return FileHelper.NormalizePath(_GetUnpackPath() + "\\..\\Nuget\\Cache");
        }

        /// <summary>
        /// Clears the CoApp cache.</summary>
        public void ClearCache()
        {
            FileHelper.ClearPath(_GetNugetCachePath());

            Debug.Log("NuGet Cache cleared");
        }
    }
}