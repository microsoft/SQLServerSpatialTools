// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public sealed class SqlClrSigningKey : Task
{
    [Required]
    public string KeyFile { get; set; }

    public bool CreateIfMissing { get; set; }

    [Output]
    public string PublicKey { get; set; }

    public override bool Execute()
    {
        try
        {
            if (!File.Exists(KeyFile))
            {
                if (!CreateIfMissing)
                {
                    Log.LogError("Signing key '{0}' does not exist. Production builds require an existing private .snk key; they never generate one. Development key creation requires a non-design-time build.", KeyFile);
                    return false;
                }

                CreateDevelopmentKey();
            }

            var parameters = ReadPrivateKey();
            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(parameters);
                var probe = new byte[] { 1, 2, 3 };
                var signature = rsa.SignData(probe, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                if (!rsa.VerifyData(probe, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    throw new CryptographicException("The private signing key does not match its public key.");
            }

            PublicKey = GetPublicKey(parameters);
            return true;
        }
        catch (Exception exception) when (exception is IOException ||
                                          exception is InvalidDataException ||
                                          exception is UnauthorizedAccessException ||
                                          exception is CryptographicException ||
                                          exception is ArgumentException ||
                                          exception is NotSupportedException ||
                                          exception is TargetInvocationException)
        {
            Log.LogError("Cannot use signing key '{0}': {1}", KeyFile, exception.Message);
            return false;
        }
    }

    private void CreateDevelopmentKey()
    {
        var directory = Path.GetDirectoryName(KeyFile);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            RestrictUnixPermissions(directory, true);
        }

        var temporaryFile = Path.Combine(directory, "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                var parameters = rsa.ExportParameters(true);
                using (var stream = new FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    RestrictUnixPermissions(temporaryFile, false);
                    using (var writer = new BinaryWriter(stream))
                    {
                        WriteBlobHeader(writer, 7, 0x32415352, parameters);
                        WriteInteger(writer, parameters.Modulus);
                        WriteInteger(writer, parameters.P);
                        WriteInteger(writer, parameters.Q);
                        WriteInteger(writer, parameters.DP);
                        WriteInteger(writer, parameters.DQ);
                        WriteInteger(writer, parameters.InverseQ);
                        WriteInteger(writer, parameters.D);
                    }
                }
            }

            // Publishing without overwrite makes concurrent solution builds share the winning key.
            try
            {
                File.Move(temporaryFile, KeyFile);
                Log.LogMessage(MessageImportance.High, "Created local development signing key '{0}'. Do not trust this key on production SQL Servers.", KeyFile);
            }
            catch (IOException) when (File.Exists(KeyFile))
            {
                Log.LogMessage(MessageImportance.Low, "Using signing key created by another build: '{0}'.", KeyFile);
            }
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }

    private RSAParameters ReadPrivateKey()
    {
        using (var stream = File.OpenRead(KeyFile))
        using (var reader = new BinaryReader(stream))
        {
            if (reader.ReadByte() != 7 || reader.ReadByte() != 2 ||
                reader.ReadUInt16() != 0 || reader.ReadUInt32() != 0x2400 ||
                reader.ReadUInt32() != 0x32415352)
                throw new InvalidDataException("Expected a private strong-name .snk key. PFX, public-only, and delay-signing keys are not supported.");

            var bitLength = reader.ReadUInt32();
            if (bitLength < 1024 || bitLength > 16384 || bitLength % 16 != 0)
                throw new InvalidDataException("Invalid RSA key size.");

            var length = (int)bitLength / 8;
            if (stream.Length != 20 + length * 9 / 2)
                throw new InvalidDataException("Invalid private strong-name key length.");

            var exponent = reader.ReadUInt32();
            var exponentBytes = BitConverter.GetBytes(exponent);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(exponentBytes);
            var exponentLength = exponentBytes.Length;
            while (exponentLength > 1 && exponentBytes[exponentLength - 1] == 0)
                exponentLength--;
            Array.Resize(ref exponentBytes, exponentLength);
            Array.Reverse(exponentBytes);

            return new RSAParameters
            {
                Exponent = exponentBytes,
                Modulus = ReadInteger(reader, length),
                P = ReadInteger(reader, length / 2),
                Q = ReadInteger(reader, length / 2),
                DP = ReadInteger(reader, length / 2),
                DQ = ReadInteger(reader, length / 2),
                InverseQ = ReadInteger(reader, length / 2),
                D = ReadInteger(reader, length)
            };
        }
    }

    private static string GetPublicKey(RSAParameters parameters)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(0x2400u);
            writer.Write(0x8004u);
            writer.Write((uint)(20 + parameters.Modulus.Length));
            WriteBlobHeader(writer, 6, 0x31415352, parameters);
            WriteInteger(writer, parameters.Modulus);
            return BitConverter.ToString(stream.ToArray()).Replace("-", "").ToLowerInvariant();
        }
    }

    private static void WriteBlobHeader(BinaryWriter writer, byte type, uint magic, RSAParameters parameters)
    {
        writer.Write(type);
        writer.Write((byte)2);
        writer.Write((ushort)0);
        writer.Write(0x2400u);
        writer.Write(magic);
        writer.Write((uint)(parameters.Modulus.Length * 8));
        uint exponent = 0;
        foreach (var value in parameters.Exponent)
            exponent = checked(exponent * 256 + value);
        writer.Write(exponent);
    }

    private static byte[] ReadInteger(BinaryReader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException("The signing key is truncated.");
        Array.Reverse(bytes);
        return bytes;
    }

    private static void WriteInteger(BinaryWriter writer, byte[] bytes)
    {
        var littleEndian = (byte[])bytes.Clone();
        Array.Reverse(littleEndian);
        writer.Write(littleEndian);
    }

    private static void RestrictUnixPermissions(string path, bool directory)
    {
        if (Path.DirectorySeparatorChar == '\\')
            return;

        // The task also runs on Framework MSBuild, which lacks the Unix permissions API.
        foreach (var method in typeof(File).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var parameters = method.GetParameters();
            if (method.Name == "SetUnixFileMode" && parameters.Length == 2 && parameters[0].ParameterType == typeof(string))
            {
                method.Invoke(null, new[] { (object)path, Enum.ToObject(parameters[1].ParameterType, directory ? 448 : 384) });
                return;
            }
        }

        throw new PlatformNotSupportedException("Development key generation on Unix requires a .NET 6 or later MSBuild host.");
    }
}
