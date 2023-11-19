using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SuperUnityBuild.BuildTool
{
    public static class BuildConstantsGenerator
    {
        public const string NONE = "None";

        public const string FileName = "BuildConstants.cs";

        public static string FindFile()
        {
            string[] fileSearchResults = Directory.GetFiles(Application.dataPath, FileName, SearchOption.AllDirectories);
            string filePath = null;
            char[] separatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            for (int i = 0; i < fileSearchResults.Length; i++)
            {
                var thisFilePath = fileSearchResults[i];
                var thisFilePathSplit = thisFilePath.Split(separatorChars);
                if (thisFilePathSplit.Length > 0)
                {
                    if (thisFilePathSplit[thisFilePathSplit.Length - 1].Equals(FileName))
                    {
                        filePath = thisFilePath;
                        break;
                    }
                }
            }

            return filePath;
        }

        public static void Generate(
            DateTime buildTime,
            string filePath = "",
            string currentVersion = "",
            BuildReleaseType currentReleaseType = null,
            BuildPlatform currentBuildPlatform = null,
            BuildScriptingBackend currentScriptingBackend = null,
            BuildArchitecture currentBuildArchitecture = null,
            BuildDistribution currentBuildDistribution = null)
        {
            // Find the BuildConstants file.
            string currentFilePath = FindFile();
            string finalFileLocation;
            if (string.IsNullOrEmpty(currentFilePath))
            {
                finalFileLocation = Path.Combine(filePath, FileName);
            }
            else
            {
                finalFileLocation = currentFilePath;
            }

            // Generate strings
            string versionString = currentVersion;
            string releaseTypeString = currentReleaseType == null ? NONE : SanitizeString(currentReleaseType.typeName);
            string platformString = currentBuildPlatform == null ? NONE : SanitizeString(currentBuildPlatform.platformName);
            string scriptingBackendString = currentScriptingBackend == null ? NONE : SanitizeString(currentScriptingBackend.name);
            string architectureString = currentBuildArchitecture == null ? NONE : SanitizeString(currentBuildArchitecture.name);
            string distributionString = currentBuildDistribution == null ? NONE : SanitizeString(currentBuildDistribution.distributionName);

            if (File.Exists(finalFileLocation))
            {
                // Delete existing version.
                File.Delete(finalFileLocation);
            }
            else
            {
                // Ensure desired path exists if generating for the first time.
                var fileInfo = new FileInfo(finalFileLocation);
                if (!fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                }
            }

            // Create a buffer that we'll use to check for any duplicated names.
            List<string> enumBuffer = new List<string>();

            using (StreamWriter writer = new StreamWriter(finalFileLocation))
            {
                // Start of file
                writer.WriteLine("using System;");
                writer.WriteLine("");
                writer.WriteLine("// This file is auto-generated. Do not modify or move this file.");
                writer.WriteLine();
                writer.WriteLine("namespace SuperUnityBuild.Generated");
                writer.WriteLine("{");

                // Write ReleaseType enum.
                writer.WriteLine("    public enum ReleaseType");
                writer.WriteLine("    {");
                writer.WriteLine("        {0},", NONE);
                enumBuffer.Add(NONE);
                foreach (BuildReleaseType releaseType in BuildSettings.releaseTypeList.releaseTypes)
                {
                    string addedString = SanitizeString(releaseType.typeName);

                    if (!enumBuffer.Contains(addedString))
                    {
                        enumBuffer.Add(addedString);
                        writer.WriteLine("        {0},", addedString);
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Validate ReleaseType string.
                if (!enumBuffer.Contains(releaseTypeString))
                    releaseTypeString = NONE;

                // Write Platform enum.
                enumBuffer.Clear();
                writer.WriteLine("    public enum Platform");
                writer.WriteLine("    {");
                writer.WriteLine("        {0},", NONE);
                enumBuffer.Add(NONE);
                foreach (BuildPlatform platform in BuildSettings.platformList.platforms)
                {
                    string addedString = SanitizeString(platform.platformName);

                    if (platform.enabled && !enumBuffer.Contains(addedString))
                    {
                        enumBuffer.Add(addedString);
                        writer.WriteLine("        {0},", addedString);
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Validate Platform string.
                if (!enumBuffer.Contains(platformString))
                    platformString = NONE;

                // Write Scripting Backend enum.
                enumBuffer.Clear();
                writer.WriteLine("    public enum ScriptingBackend");
                writer.WriteLine("    {");
                writer.WriteLine("        {0},", NONE);
                enumBuffer.Add(NONE);
                foreach (BuildPlatform platform in BuildSettings.platformList.platforms)
                {
                    if (platform.enabled)
                    {
                        foreach (BuildScriptingBackend scriptingBackend in platform.scriptingBackends)
                        {
                            string addedString = SanitizeString(scriptingBackend.name);

                            if (scriptingBackend.enabled && !enumBuffer.Contains(addedString))
                            {
                                enumBuffer.Add(addedString);
                                writer.WriteLine("        {0},", addedString);
                            }
                        }
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Validate Scripting Backend string.
                if (!enumBuffer.Contains(scriptingBackendString))
                    scriptingBackendString = NONE;

                // Write Architecture enum.
                enumBuffer.Clear();
                writer.WriteLine("    public enum Architecture");
                writer.WriteLine("    {");
                writer.WriteLine("        {0},", NONE);
                enumBuffer.Add(NONE);
                foreach (BuildPlatform platform in BuildSettings.platformList.platforms)
                {
                    if (platform.enabled)
                    {
                        foreach (BuildArchitecture arch in platform.architectures)
                        {
                            string addedString = SanitizeString(arch.name);

                            if (arch.enabled && !enumBuffer.Contains(addedString))
                            {
                                enumBuffer.Add(addedString);
                                writer.WriteLine("        {0},", addedString);
                            }
                        }
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Validate Architecture string.
                if (!enumBuffer.Contains(architectureString))
                    architectureString = NONE;

                // Write Distribution enum.
                enumBuffer.Clear();
                writer.WriteLine("    public enum Distribution");
                writer.WriteLine("    {");
                writer.WriteLine("        {0},", NONE);
                enumBuffer.Add(NONE);
                foreach (BuildPlatform platform in BuildSettings.platformList.platforms)
                {
                    if (platform.enabled)
                    {
                        foreach (BuildDistribution dist in platform.distributionList.distributions)
                        {
                            string addedString = SanitizeString(dist.distributionName);

                            if (dist.enabled && !enumBuffer.Contains(addedString))
                            {
                                enumBuffer.Add(addedString);
                                writer.WriteLine("        {0},", addedString);
                            }
                        }
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Validate Distribution string.
                if (!enumBuffer.Contains(distributionString))
                    distributionString = NONE;

                // Start of class.
                writer.WriteLine("    public static class BuildConstants");
                writer.WriteLine("    {");

                // Write current values.
                writer.WriteLine("        public static readonly DateTime buildDate = new DateTime({0});", buildTime.Ticks);
                writer.WriteLine("        public const string version = \"{0}\";", versionString);
                writer.WriteLine("        public const ReleaseType releaseType = ReleaseType.{0};", releaseTypeString);
                writer.WriteLine("        public const Platform platform = Platform.{0};", platformString);
                writer.WriteLine("        public const ScriptingBackend scriptingBackend = ScriptingBackend.{0};", scriptingBackendString);
                writer.WriteLine("        public const Architecture architecture = Architecture.{0};", architectureString);
                writer.WriteLine("        public const Distribution distribution = Distribution.{0};", distributionString);
                writer.WriteLine();

                // Push values to accessible version.
                writer.WriteLine("        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSplashScreen)]");
                writer.WriteLine("        public static void StoreBuildConstants()");
                writer.WriteLine("        {");

                void WritePush(string i_target, string i_value)
                {
                    writer.WriteLine($"            SuperUnityBuild.Runtime.BuildConstants.{i_target} = {i_value};");
                }
                WritePush("BuildDate", "buildDate");
                WritePush("Version", "version");
                WritePush("ReleaseType", "releaseType.ToString()");
                WritePush("Platform", "platform.ToString()");
                WritePush("Architecture", "architecture.ToString()");
                WritePush("Distribution", "distribution.ToString()");

                writer.WriteLine("        }");

                // End of class.
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.WriteLine();
            }

            // Refresh AssetDatabse so that changes take effect.
            AssetDatabase.Refresh();
        }

        private static string SanitizeString(string str)
        {
            str = Regex.Replace(str, "[^a-zA-Z0-9_]", "_", RegexOptions.Compiled);
            if (char.IsDigit(str[0]))
            {
                str = "_" + str;
            }
            return str;
        }
    }
}
