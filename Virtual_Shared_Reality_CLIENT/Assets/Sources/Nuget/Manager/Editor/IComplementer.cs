using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARDI.Nuget
{
    /// <summary>
    /// Complements an installed nuget package with additional assets.</summary>
    /// <remarks>
    /// Complements can be additional downloaded files, platform specific libraries, etc.</remarks>
    public interface IComplementer
    {
        /// <summary>
        /// Downloads and unpacks dependency <paramref name="dep"/> into <paramref name="unpackPath"/>.</summary>
        /// <remarks>A subfolder of <paramref name="unpackPath"/> will be created to house the unpacked files. This subfolder name
        /// contains both the package name and it's version, to avoid any conflicts.</remarks>
        /// <param name="dep">The nuget dependency to download and unpack.</param>
        /// <param name="unpackPath">The path where the package will be unpacked.</param>
        void DownloadComplement(NugetDependency dep, string unpackPath);

        /// <summary>
        /// Installs the given <paramref name="dependency"/>, which is expected to be unpacked at <paramref name="unpackPath"/>,
        /// into <paramref name="installPath"/>.
        /// </summary>
        /// <param name="dependency">The nuget dependency to install.</param>
        /// <param name="unpackPath">The path where the package was unpacked. See <see cref="DownloadComplement(NugetDependency, string)"/>.</param>
        /// <param name="installPath">The path of the target project where the nuget files will be copied to.</param>
        void InstallComplement(NugetDependency dependency, string unpackPath, string installPath);
    }
}