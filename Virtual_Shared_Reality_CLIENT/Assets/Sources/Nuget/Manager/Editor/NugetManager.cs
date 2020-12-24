using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using ARDI.Nuget.Impl;

using UnityEngine;
using UnityEditor;

namespace ARDI.Nuget
{
    /// <summary>
    /// Implements an EditorWindow to manage nuget dependencies.</summary>
    /// <remarks>
    /// This class creates a <see cref="NugetConfig"/> and passes it along to
    /// both <see cref="ARDI.Nuget.Impl.StandardNuget">StandardNuget</see> and <see cref="ARDI.Nuget.Impl.CoAppNuget">CoAppNuget</see>, which do the
    /// heavy lifting.</remarks>
    public class NugetWindow : EditorWindow
    {
        private static string K_NUGET_DEPS_PATH = "/Plugins/Nuget/nugetConfig.json";

        private static NugetConfig s_nugetConf;

        private CoAppNuget _coapp;
        private StandardNuget _nuget;
        private List<IComplementer> _complementers;
        private List<ICaching> _cachers;

        private bool m_showExceptions = false;
        private bool m_showConfig = false;

        /// <summary>
        /// Shows the Nuget Manager screen. Not to be called directly.</summary>
        [MenuItem("Tecgraf/Nuget Manager")]
        public static void ShowWindow()
        {
            NugetWindow window = EditorWindow.GetWindow<NugetWindow>();

            window.titleContent = new GUIContent("Nuget Manager");
            window.minSize = new Vector2(200, 100);
        }

        private List<ICaching> Cachers
        {
            get
            {
                if (_cachers == null)
                    _cachers = new List<ICaching>() { Nuget, CoApp };

                return _cachers;
            }
        }

        //private List<IComplementer> Complementers
        //{
        //    get
        //    {
        //        if (_complementers == null)
        //            _complementers = new List<IComplementer>() { CoApp };

        //        return _complementers;
        //    }
        //}

        private StandardNuget Nuget
        {
            get
            {
                if (_nuget == null)
                    _nuget = new StandardNuget(_GetConfig());

                return _nuget;
            }
        }

        private CoAppNuget CoApp
        {
            get
            {
                if (_coapp == null)
                {
                    _coapp = new CoAppNuget(_GetConfig()); // Careful: Shared state
                }

                return _coapp;
            }
        }

        void OnGUI()
        {
            bool save = _ShowDependencies(_GetConfig());

            m_showExceptions = EditorGUILayout.Foldout(m_showExceptions, "Exceptions", true);
            if (m_showExceptions == true)
            {
                save |= _ShowExceptions(_GetConfig());
            }

            m_showConfig = EditorGUILayout.Foldout(m_showConfig, "Configuration", true);
            if (m_showConfig == true)
            {
                save |= _ShowConfig(_GetConfig());
            }

            if (GUILayout.Button("Add New Dependency", GUILayout.ExpandWidth(true)) == true)
            {
                _GetConfig().AddDependency(new NugetDependency());
            }

            if (GUILayout.Button("Install All", GUILayout.ExpandWidth(true)) == true)
            {
                FileHelper.ClearPath(_GetConfig().GetProjectPluginPath());

                _InstallDependencies(_GetConfig().Dependencies);
            }

            if (GUILayout.Button("Clear NuGet Cache", GUILayout.ExpandWidth(true)))
            {
                foreach (ICaching caching in Cachers)
                    caching.ClearCache();
            }

            if (save == true)
                _SaveNugetConfig(K_NUGET_DEPS_PATH, _GetConfig());
        }

        private void _InstallDependencies(List<NugetDependency> dependencies)
        {
            string unpackPath = _GetConfig().GetProjectUnpackPath();
            string installPath = _GetConfig().GetProjectPluginPath();

            foreach (NugetDependency dep in dependencies)
            {
                FileHelper.ClearPath(unpackPath);

                Debug.Log("Downloading " + dep);
                Nuget.DownloadDependency(dep, unpackPath);

                //    foreach (IComplementer complementer in _complementers)
                //    {
                //        complementer.DownloadComplement(dep, unpackPath);

                //        complementer.InstallComplement(dep, unpackPath, installPath);
                //    }
                //}

                foreach (NugetDependency transitiveDep in Nuget.GetDownloadedPackages(unpackPath))
                {
                    transitiveDep.Framework = dep.Framework;

                    if (_ExceptionApplies(_GetConfig().Exceptions, transitiveDep) == false)
                    {
                        Debug.Log("Installing " + transitiveDep);
                        Nuget.InstallDependency(transitiveDep, unpackPath, installPath);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private bool _ExceptionApplies(List<string> exceptions, NugetDependency dep)
        {
            string lowerName = dep.Name.ToLower();

            bool ret = exceptions.Any(x => x.ToLower() == lowerName);

            if (ret == true)
            {
                Debug.LogWarning("Installation of dependency " + dep + " was blocked by exception rule.");
            }

            return ret;
        }

        private bool _ShowExceptions(NugetConfig config)
        {
            int toRemove = -1;
            GUI.changed = false;

            for (int i = 0; i < config.Exceptions.Count; i++)
            {
                GUILayout.BeginHorizontal();

                config.Exceptions[i] = EditorGUILayout.TextField(config.Exceptions[i]);

                if (GUILayout.Button("Remove", GUILayout.Width(190)) == true)
                    toRemove = i;

                GUILayout.EndHorizontal();
            }

            if (toRemove != -1)
            {
                config.RemoveException(toRemove);
            }

            if (GUILayout.Button("Add Exception") == true)
            {
                config.AddException("");
            }

            return GUI.changed;
        }

        private bool _ShowConfig(NugetConfig p_config)
        {
            bool save = false;
            int layoutWidth = 110;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Compiler Version", GUILayout.Width(layoutWidth));
            save |= _EnumField(ref p_config.MsvcVersion);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fallback Compiler Version", GUILayout.Width(layoutWidth));
            save |= _EnumField(ref p_config.FallbackMsvcVersion);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CoApp Version", GUILayout.Width(layoutWidth));
            save |= _TextField(ref p_config.CoAppVersion, 180);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Nuget Version", GUILayout.Width(layoutWidth));
            save |= _TextField(ref p_config.StandardNugetVersion, 180);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source", GUILayout.Width(layoutWidth));
            save |= _TextField(ref p_config.Source, 180);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use Nuget.org", GUILayout.Width(layoutWidth));
            save |= _BoolField(ref p_config.UseNugetOrg);
            GUILayout.EndHorizontal();

            return save;
        }

        private bool _ShowDependencies(NugetConfig p_config)
        {
            bool save = false;
            string toRemove = null;

            float versionWidth = _CalculateFieldWidth(p_config.Dependencies, x => x.Version);
            float frameworkWidth = _CalculateFieldWidth(p_config.Dependencies, x => x.Framework);
            float nameWidth = _CalculateNameWidth(position, versionWidth + frameworkWidth);

            foreach (var dep in p_config.Dependencies)
            {
                GUILayout.BeginHorizontal();

                save |= _TextField(ref dep.Name, nameWidth);

                save |= _TextField(ref dep.Version, versionWidth);

                save |= _TextField(ref dep.Framework, frameworkWidth);

                if (GUILayout.Button("Remove") == true)
                    toRemove = dep.Name;

                GUILayout.Space(30);

                if (GUILayout.Button("Install") == true)
                {
                    _InstallDependencies(new List<NugetDependency>() { dep });
                }

                GUILayout.EndHorizontal();
            }

            if (toRemove != null)
            {
                save = true;
                p_config.RemoveDependency(toRemove);
            }

            return save;
        }

        private float _CalculateNameWidth(Rect windowRect, float versionWidth)
        {
            return windowRect.width - (versionWidth + 210);
        }

        private float _CalculateFieldWidth(List<NugetDependency> dependencies, Func<NugetDependency, string> func)
        {
            float width = 50;

            foreach (NugetDependency dep in dependencies)
            {
                width = Mathf.Max(width, GUI.skin.textField.CalcSize(new GUIContent(func(dep))).x);
            }

            return width;
        }

        private bool _BoolField(ref bool p_field)
        {
            bool temp = GUILayout.Toggle(p_field, "");
            if (temp != p_field)
            {
                p_field = temp;
                return true;
            }

            return false;
        }

        private bool _TextField(ref string p_field, float p_width)
        {
            string temp = EditorGUILayout.TextField(p_field, GUILayout.Width(p_width));
            if (temp != p_field)
            {
                p_field = temp;
                return true;
            }

            return false;
        }

        private bool _EnumField(ref VisualStudioVersions p_field)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            VisualStudioVersions op = (VisualStudioVersions)CustomGUI.EnumPopup<VisualStudioVersions>(position, (int)p_field);
            if (op != p_field)
            {
                p_field = op;
                return true;
            }
            return false;
        }

        private static NugetConfig _GetConfig()
        {
            if (s_nugetConf == null)
            {
                s_nugetConf = _LoadDependenciesFile(K_NUGET_DEPS_PATH);

                if (s_nugetConf == null)
                {
                    s_nugetConf = _CreateEmptyDependenciesFile(K_NUGET_DEPS_PATH);
                }
            }

            return s_nugetConf;
        }

        private static NugetConfig _LoadDependenciesFile(string p_path)
        {
            string json;

            try
            {
                json = File.ReadAllText(_GetAbsolutePath(p_path));
            }
            catch (System.Exception)
            {
                return null;
            }

            NugetConfig ret = new NugetConfig();

            EditorJsonUtility.FromJsonOverwrite(json, ret);

            return ret;
        }

        private static NugetConfig _CreateEmptyDependenciesFile(string p_path)
        {
            NugetConfig nugetDeps = new NugetConfig();

            _SaveNugetConfig(p_path, nugetDeps);

            return nugetDeps;
        }

        private static void _SaveNugetConfig(string p_path, NugetConfig p_nugetDeps)
        {
            string absolutePath = _GetAbsolutePath(p_path);

            using (StreamWriter writer = File.CreateText(absolutePath))
            {
                if (writer != null)
                {
                    writer.Write(EditorJsonUtility.ToJson(p_nugetDeps, true));
                }
                else
                {
                    Debug.Log("Error opening file " + absolutePath + " for writing.");
                }
            }
        }

        private static string _GetAbsolutePath(string p_path)
        {
            return Path.GetFullPath(Application.dataPath + p_path);
        }

        /// <summary>
        /// Static class used to render custom enum-related GUIs.</summary>
        public static class CustomGUI
        {
            // Cache of enum values.
            private static Dictionary<Type, int[]> s_EnumValueCache = new Dictionary<Type, int[]>();
            // Cache of enum descriptions/nicified values.
            private static Dictionary<Type, string[]> s_EnumDescriptionCache = new Dictionary<Type, string[]>();

            /// <summary>
            /// Returns all the possible values of an enum type as ints.</summary>
            /// <typeparam name="T">The enum type.</typeparam>
            /// <returns>All of <typeparamref name="T"/>'s possible values as ints.</returns>
            public static int[] GetEnumValues<T>()
            {
                if (!s_EnumValueCache.ContainsKey(typeof(T)))
                    s_EnumValueCache[typeof(T)] = Enum.GetValues(typeof(T)).Cast<int>().ToArray();
                return s_EnumValueCache[typeof(T)];
            }

            /// <summary>
            /// Returns the description of each value of an enum type.</summary>
            /// <remarks>
            /// To set an enum's value description, use <see cref="System.ComponentModel.DescriptionAttribute"/>.
            /// The descriptions are returned in the same order as <see cref="GetEnumValues{T}"/>.</remarks>
            /// <typeparam name="T">The enum type.</typeparam>
            /// <returns>The description associated with each one of <typeparamref name="T"/>'s values.</returns>
            public static string[] GetEnumDescriptions<T>()
            {
                if (!s_EnumDescriptionCache.ContainsKey(typeof(T)))
                {
                    var list = new List<string>();
                    foreach (var value in Enum.GetValues(typeof(T)))
                    {
                        var attribute = typeof(T).GetMember(value.ToString())[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                        string regName = ObjectNames.NicifyVariableName(value.ToString());
                        list.Add(attribute != null ? attribute.Description.Replace('|', ' ') + " (" + regName.ToLower() + ")" : regName);
                    }
                    s_EnumDescriptionCache[typeof(T)] = list.ToArray();
                }
                return s_EnumDescriptionCache[typeof(T)];
            }

            /// <summary>
            /// Shows a popup where the user can select one of an enum's values.</summary>
            /// <typeparam name="T">The enum type.</typeparam>
            /// <param name="position">The rectangle on screen where the popup will appear.</param>
            /// <param name="value">The current selected enum value.</param>
            /// <returns>The enum value selected by the user.</returns>
            public static int EnumPopup<T>(Rect position, int value)
            {
                var values = GetEnumValues<T>();
                int selectedIndex = EditorGUI.Popup(position, Array.IndexOf(values, value), GetEnumDescriptions<T>());
                return (selectedIndex >= 0 && selectedIndex < values.Length) ? values[selectedIndex] : value;
            }
        }
    }
}
