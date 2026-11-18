// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public sealed class SqlClrScripts : Task
{
    [Required]
    public string AssemblyPath { get; set; }

    [Required]
    public string OutputDirectory { get; set; }

    public ITaskItem[] SourceFiles { get; set; }

    public bool GenerateTrustScript { get; set; }

    public string ServerAssemblyPath { get; set; }

    public string ExpectedPublicKey { get; set; }

    public string SigningMode { get; set; }

    [Output]
    public ITaskItem[] WrittenFiles { get; set; }

    public override bool Execute()
    {
        try
        {
            Directory.CreateDirectory(OutputDirectory);
            if (OutputDirectory.IndexOf('"') >= 0)
                throw new ArgumentException("SQLCMD script paths cannot contain double quotes.");

            var files = new System.Collections.Generic.List<ITaskItem>();
            if (GenerateTrustScript)
            {
                var publicKey = AssemblyName.GetAssemblyName(AssemblyPath).GetPublicKey();
                if (publicKey == null || publicKey.Length == 0)
                    throw new InvalidDataException("The output assembly is unsigned. Rebuild with SQL CLR signing enabled.");
                var actualPublicKey = BitConverter.ToString(publicKey).Replace("-", "").ToLowerInvariant();
                if (string.IsNullOrEmpty(ExpectedPublicKey) || actualPublicKey != ExpectedPublicKey)
                    throw new InvalidDataException("The output assembly does not match the selected signing key. Rebuild before generating its trust script.");

                var path = string.IsNullOrEmpty(ServerAssemblyPath) ? AssemblyPath : ServerAssemblyPath;
                if (path.IndexOfAny(new[] { '\r', '\n' }) >= 0)
                    throw new ArgumentException("The server assembly path cannot contain line breaks.");

                var text = "-- Generated SQL CLR trust setup. Run once as a SQL Server administrator.\n" +
                           "-- The DLL path must be accessible to the SQL Server service account.\n" +
                           "-- This trusts the signing key, not just this DLL. Keep the private key protected.\n" +
                           "-- Reuse existing trust for later builds with the same key; use new key/login names when rotating keys.\n" +
                           (string.Equals(SigningMode, "Development", StringComparison.OrdinalIgnoreCase) ? "-- DEVELOPMENT KEY: use only on an isolated local test SQL Server, never in production.\n" : "") +
                           "USE master;\nGO\n\n" +
                           "CREATE ASYMMETRIC KEY [SQLSpatialToolsSigningKey]\n" +
                           "FROM EXECUTABLE FILE = N'" + path.Replace("'", "''") + "';\n\n" +
                           "CREATE LOGIN [SQLSpatialToolsSigningLogin]\n" +
                           "FROM ASYMMETRIC KEY [SQLSpatialToolsSigningKey];\n\n" +
                           "GRANT UNSAFE ASSEMBLY TO [SQLSpatialToolsSigningLogin];\nGO\n";
                WriteIfChanged(Path.Combine(OutputDirectory, "TrustAssembly.sql"), text, files);
            }
            else
            {
                foreach (var source in SourceFiles)
                {
                    var sourcePath = source.GetMetadata("FullPath");
                    var text = File.ReadAllText(sourcePath)
                        .Replace("DLLPath", AssemblyPath.Replace("'", "''"))
                        .Replace("ScriptDirPath", OutputDirectory);
                    WriteIfChanged(Path.Combine(OutputDirectory, Path.GetFileName(sourcePath)), text, files);
                }
            }

            WrittenFiles = files.ToArray();
            return true;
        }
        catch (Exception exception) when (exception is IOException ||
                                          exception is InvalidDataException ||
                                          exception is UnauthorizedAccessException ||
                                          exception is ArgumentException ||
                                          exception is BadImageFormatException)
        {
            Log.LogError("Cannot prepare SQL CLR scripts: {0}", exception.Message);
            return false;
        }
    }

    private static void WriteIfChanged(string path, string text, System.Collections.Generic.List<ITaskItem> files)
    {
        if (!File.Exists(path) || File.ReadAllText(path) != text)
            File.WriteAllText(path, text, new UTF8Encoding(false));
        files.Add(new TaskItem(path));
    }
}
