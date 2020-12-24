using System;
using System.Collections.Generic;
using System.ComponentModel;

using ARDI.Nuget.Impl;

using UnityEngine;

namespace ARDI.Nuget
{
    /// <summary>
    /// The Visual Studio version used in this project.</summary>
    /// <remarks>
    /// Used to distinguish which set of native libraries should be installed
    /// since different versions are incompatible among themselves.
    /// For more information, see <a href="https://docs.microsoft.com/en-us/cpp/porting/binary-compat-2015-2017?view=vs-2017">here</a>.</remarks>
    [Flags]
    public enum VisualStudioVersions
    {
        [Description("Visual Studio|2010")]
        v100 = (0 << 0),
        [Description("Visual Studio|2012")]
        v110 = (1 << 0),
        [Description("Visual Studio|2013")]
        v120 = (1 << 1),
        [Description("Visual Studio|2015")]
        v140 = (1 << 2),
        [Description("Visual Studio|2017")]
        v150 = (1 << 3)
    }

    /// <summary>
    /// All configurations needed to install nugets to a Unity project,
    /// plus the list of user selected dependencies.</summary>
    /// <remarks>
    /// \todo
    ///TODO: Consider moving unpackPath to here.
    /// </remarks>
    [System.Serializable]
    public class NugetConfig
    {
        /// <summary>
        /// The list of a project's nuget root dependencies.</summary>
        public List<NugetDependency> Dependencies = new List<NugetDependency>();

        /// <summary>
        /// List of packages that will not be installed, even if they are transitive dependencies of a <see cref="Dependencies">root dependency</see>.</summary>
        public List<string> Exceptions = new List<string>();

        /// <summary>
        /// The <see cref="ARDI.Nuget.Impl.CoAppNuget">CoApp</see> version to install and use.</summary>
        public string CoAppVersion = "1.0";

        /// <summary>
        /// The <see cref="ARDI.Nuget.Impl.StandardNuget">standard nuget</see> version to install and use.</summary>
        public string StandardNugetVersion = "4.7.1";

        /// <summary>
        /// The URL of the (private) nuget feed to use.</summary>
        public string Source = "http://siviepsrv/api/odata";

        /// <summary>
        /// Use nuget.org as a fallback if the package cannot be found in <see cref="Source"/>.</summary>
        public bool UseNugetOrg = true;

        /// <summary>
        /// The Visual Studio version of the native libraries fetched from CoApp.</summary>
        public VisualStudioVersions MsvcVersion = VisualStudioVersions.v120;

        /// <summary>
        /// The fallback Visual Studio version of the native libraries fetched from CoApp. This is used when libraries matching <see cref="MsvcVersion"/> are not found.</summary>
        public VisualStudioVersions FallbackMsvcVersion = VisualStudioVersions.v120;

        /// <summary>
        /// Adds a new <see cref="Dependencies">root dependency</see>.</summary>
        /// <param name="dep">The new root dependency.</param>
        public void AddDependency(NugetDependency dep)
        {
            Dependencies.Add(dep);
        }

        /// <summary>
        /// Removes a <see cref="Dependencies">root dependency</see> by name.</summary>
        /// <param name="depName">Name of the dependency to remove.</param>
        public void RemoveDependency(string depName)
        {
            Dependencies.RemoveAll(x =>
            {
                return x.Name == depName;
            });
        }

        /// <summary>
        /// Adds a new exception to <see cref="NugetConfig.Exceptions">package exceptions</see> list.</summary>
        /// <param name="exception">Name of package to add to exceptions list.</param>
        public void AddException(string exception)
        {
            Exceptions.Add(exception);
        }

        /// <summary>
        /// Removes <see cref="NugetConfig.Exceptions">package exception</see> by name.</summary>
        /// <param name="exception">Name of the package to exclude from the list of package exceptions.</param>
        public void RemoveException(string exception)
        {
            Exceptions.Remove(exception);
        }

        /// <summary>
        /// Removes the i-th <see cref="NugetConfig.Exceptions">package exception</see>.</summary>
        /// <param name="i">The index of the exception to remove.</param>
        public void RemoveException(int i)
        {
            Exceptions.RemoveAt(i);
        }

        /// <summary>
        /// Returns the path where the nuget packages will be installed into.</summary>
        /// <returns>The full, normalized path in the Unity project folder where the nuget packages will be installed.</returns>
        public string GetProjectPluginPath()
        {
            return FileHelper.NormalizePath(Application.dataPath + "\\Plugins\\Nuget\\Packages");
        }

        /// <summary>
        /// Returns the temporary path used to unpack the nuget packages during installation.</summary>
        /// <returns>A full, normalized temporary path.</returns>
        public string GetProjectUnpackPath()
        {
            string temp = Environment.GetEnvironmentVariable("TEMP") + "\\" + Application.companyName + "." + Application.productName + "\\Nuget\\";

            return FileHelper.NormalizePath(temp);
        }
    }

    /// <summary>
    /// Information about a single nuget dependency.</summary>
    /// <remarks>This class is mutable.</remarks>
    [System.Serializable]
    public class NugetDependency
    {
        /// <summary>
        /// Creates a new <see cref="NugetDependency"/> where all fields are empty strings.</summary>
        public NugetDependency()
        {
            Name = "";
            Version = "";
            Framework = "";
        }

        /// <summary>
        /// Creates a new <see cref="NugetDependency"/> with the given data.</summary>
        /// <param name="name">The dependency's package name.</param>
        /// <param name="version">The dependency's desired version.</param>
        /// <param name="framework">The dependency's desired target framework.</param>
        public NugetDependency(string name, string version, string framework)
        {
            Name = name;
            Version = version;
            Framework = framework;
        }

        /// <summary>
        /// This dependency's package name.</summary>
        public string Name;

        /// <summary>
        /// This dependency's desired version.</summary>
        public string Version;

        /// <summary>
        /// This dependency's desired target framework.</summary>
        public string Framework;

        /// <summary>
        /// Converts this <see cref="NugetDependency"/> into a string.
        /// </summary>
        /// <returns><see cref="NugetDependency.Name">Name</see>.<see cref="NugetDependency.Version">Version</see> (<see cref="NugetDependency.Framework">Framework</see>)</returns>
        public override string ToString()
        {
            return Name + "." + Version + " (" + Framework + ")";
        }
    }
}