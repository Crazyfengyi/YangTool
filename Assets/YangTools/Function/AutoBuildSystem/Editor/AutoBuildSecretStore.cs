#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 使用 Windows DPAPI 保存本机构建凭据
    /// </summary>
    internal static class AutoBuildSecretStore
    {
        private const int UiForbidden = 0x1;

        [Serializable]
        private sealed class SecretDocument
        {
            public List<SecretEntry> entries = new List<SecretEntry>();
        }

        [Serializable]
        private sealed class SecretEntry
        {
            public string key;
            public string value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DataBlob
        {
            public int size;
            public IntPtr data;
        }

        /// <summary>
        /// 读取已加密保存的凭据
        /// </summary>
        public static string Get(string key)
        {
            SecretDocument document = LoadDocument();
            SecretEntry entry = document.entries.Find(item => item.key == key);
            return entry?.value ?? string.Empty;
        }

        /// <summary>
        /// 保存或移除一个凭据
        /// </summary>
        public static void Set(string key, string value)
        {
            SecretDocument document = LoadDocument();
            document.entries.RemoveAll(item => item.key == key);
            if (!string.IsNullOrEmpty(value))
            {
                document.entries.Add(new SecretEntry { key = key, value = value });
            }

            WriteEncrypted(AutoBuildLocalPaths.GetSecretFilePath(), Encoding.UTF8.GetBytes(JsonUtility.ToJson(document)));
        }

        /// <summary>
        /// 将敏感的临时构建快照加密写入本机目录
        /// </summary>
        public static void WriteEncrypted(string path, byte[] plainBytes)
        {
            byte[] encryptedBytes = Protect(plainBytes);
            string temporaryPath = path + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(temporaryPath, encryptedBytes);
            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, null);
            }
            else
            {
                File.Move(temporaryPath, path);
            }
        }

        /// <summary>
        /// 解密本机 DPAPI 文件
        /// </summary>
        public static byte[] ReadEncrypted(string path)
        {
            return Unprotect(File.ReadAllBytes(path));
        }

        /// <summary>
        /// 删除临时加密文件
        /// </summary>
        public static void DeleteEncryptedFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static SecretDocument LoadDocument()
        {
            string path = AutoBuildLocalPaths.GetSecretFilePath();
            if (!File.Exists(path)) return new SecretDocument();
            SecretDocument document = JsonUtility.FromJson<SecretDocument>(Encoding.UTF8.GetString(ReadEncrypted(path)));
            if (document == null) return new SecretDocument();
            document.entries ??= new List<SecretEntry>();
            return document;
        }

        private static byte[] Protect(byte[] plainBytes)
        {
#if UNITY_EDITOR_WIN
            return TransformData(plainBytes, true);
#else
            throw new PlatformNotSupportedException("构建凭据 DPAPI 存储仅支持 Windows Editor");
#endif
        }

        private static byte[] Unprotect(byte[] encryptedBytes)
        {
#if UNITY_EDITOR_WIN
            return TransformData(encryptedBytes, false);
#else
            throw new PlatformNotSupportedException("构建凭据 DPAPI 存储仅支持 Windows Editor");
#endif
        }

#if UNITY_EDITOR_WIN
        [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(ref DataBlob input, string description, IntPtr entropy,
            IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

        [DllImport("Crypt32.dll", SetLastError = true)]
        private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, IntPtr entropy,
            IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);

        private static byte[] TransformData(byte[] bytes, bool encrypt)
        {
            IntPtr inputMemory = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, inputMemory, bytes.Length);
            DataBlob input = new DataBlob { size = bytes.Length, data = inputMemory };
            DataBlob output;
            bool success = encrypt
                ? CryptProtectData(ref input, "YangTools AutoBuild", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, UiForbidden, out output)
                : CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, UiForbidden, out output);

            try
            {
                if (!success) throw new InvalidOperationException($"DPAPI 操作失败 错误码 {Marshal.GetLastWin32Error()}");
                byte[] result = new byte[output.size];
                Marshal.Copy(output.data, result, 0, output.size);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(inputMemory);
                if (output.data != IntPtr.Zero) LocalFree(output.data);
            }
        }
#endif
    }
}
#endif
