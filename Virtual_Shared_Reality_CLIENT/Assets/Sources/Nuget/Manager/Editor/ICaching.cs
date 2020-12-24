using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARDI.Nuget
{
    /// <summary>
    /// Represents on of the many possible caches involved in nuget operation.
    /// </summary>
    public interface ICaching
    {
        /// <summary>
        /// Clears this cache.</summary>
        void ClearCache();
    }
}