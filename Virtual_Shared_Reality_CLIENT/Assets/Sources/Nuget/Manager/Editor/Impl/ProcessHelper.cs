using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Helper functions to create and manipulate new processes.</summary>
/// <remarks>
/// \todo
///TODO: Move this class to a namespace.
/// </remarks>
public class ProcessHelper
{
    /// <summary>
    /// Spawns a new process.</summary>
    /// <param name="executable">Full path for the executable.</param>
    /// <param name="workingDir">New process' working directory.</param>
    /// <param name="parms">New process' command line arguments.</param>
    /// <param name="redirect">True to redirect the new process' 
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?view=netframework-4.7.2">Standard Output</a> and 
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standarderror?view=netframework-4.7.2">Standard Error</a> to streams. </param>
    /// <returns></returns>
    public static Process SpawnProcess(string executable, string workingDir, string parms, bool redirect = true)
    {
        Process process = new Process();

        process.StartInfo.WorkingDirectory = workingDir;
        process.StartInfo.FileName = executable;
        process.StartInfo.Arguments = parms;
        process.StartInfo.RedirectStandardOutput = redirect;
        process.StartInfo.RedirectStandardError = redirect;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        UnityEngine.Debug.Log(executable);
        UnityEngine.Debug.Log(parms);

        if (process.Start() == false)
        {
            UnityEngine.Debug.LogError("Could not start " + executable);
            return null;
        }

        return process;
    }

    private static string _TryRead(StreamReader stream)
    {
        try
        {
            return stream.ReadToEnd();
        }
        catch
        {
            return "";
        }
    }

    private static string _TryReadLine(StreamReader stream)
    {
        try
        {
            return stream.ReadLine();
        }
        catch
        {
            return null;
        }
    }

    private static bool _TryCanRead(StreamReader stream)
    {
        try
        {
            return stream.Peek() > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void PrintStandardStreams(Process process)
    {
        string message = _TryRead(process.StandardOutput); 
        if (string.IsNullOrWhiteSpace(message) == false)
            UnityEngine.Debug.LogWarning(message);

        message = _TryRead(process.StandardError);
        if (string.IsNullOrWhiteSpace(message) == false)
            UnityEngine.Debug.LogError(message);
    }

    /// <summary>
    /// Enumerates all the lines written by the <paramref name="process"/> into it's standard out until the process finishes.</summary>
    /// <param name="process">The process to read the standard out from.</param>
    /// <returns>All the lines written by the process into it's standard out.</returns>
    public static IEnumerable<string> ReadStandardOut(Process process)
    {
        do
        {
            string ret = _TryReadLine(process.StandardOutput);

            if (ret != null)
                yield return ret;

        } while ((process.HasExited == false) || (_TryCanRead(process.StandardOutput) == true));
    }

    /// <summary>
    /// Waits for <paramref name="process"/> to finish.</summary>
    /// <param name="process">The process to wait for.</param>
    /// <returns>True if its ExitCode equals zero. False otherwise.</returns>
    public static bool WaitForExit(Process process)
    {
        process.WaitForExit();

        if (process.ExitCode == 0)
            return true;

        PrintStandardStreams(process);

        return false;
    }
}
