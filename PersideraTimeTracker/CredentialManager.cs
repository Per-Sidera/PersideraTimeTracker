using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PersideraTimeTracker
{
    /// <summary>
    /// Stores and retrieves the Mercury API token in the Windows Credential
    /// Manager (Generic credential, encrypted per-user by Windows/DPAPI).
    /// Uses P/Invoke into advapi32.dll (CredWrite / CredRead / CredDelete).
    /// </summary>
    public static class CredentialManager
    {
        /// <summary>Target name of the credential in the Windows vault.</summary>
        private const string CredentialKey = "PersideraTimeTracker_MercuryToken";

        private const int CRED_TYPE_GENERIC = 1;
        private const int CRED_PERSIST_LOCAL_MACHINE = 2;
        private const int ERROR_NOT_FOUND = 1168;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            public string TargetName;
            public string? Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            public string? TargetAlias;
            public string? UserName;
        }

        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref CREDENTIAL credential, [In] uint flags);

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredDelete(string target, int type, int flags);

        [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern void CredFree([In] IntPtr cred);

        /// <summary>
        /// Stores (or overwrites) the Mercury API token in the Windows vault.
        /// </summary>
        public static void SaveToken(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            byte[] blob = Encoding.Unicode.GetBytes(token);
            IntPtr blobPtr = Marshal.AllocHGlobal(blob.Length);
            try
            {
                Marshal.Copy(blob, 0, blobPtr, blob.Length);

                var cred = new CREDENTIAL
                {
                    Flags = 0,
                    Type = CRED_TYPE_GENERIC,
                    TargetName = CredentialKey,
                    Comment = "Persidera Time Tracker — Mercury API token",
                    CredentialBlobSize = blob.Length,
                    CredentialBlob = blobPtr,
                    Persist = CRED_PERSIST_LOCAL_MACHINE,
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    UserName = Environment.UserName
                };

                if (!CredWrite(ref cred, 0))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException(
                        $"Failed to store Mercury token in Windows Credential Manager (error {error}).");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(blobPtr);
            }
        }

        /// <summary>
        /// Retrieves the Mercury API token, or null if none is stored.
        /// </summary>
        public static string? GetToken()
        {
            if (!CredRead(CredentialKey, CRED_TYPE_GENERIC, 0, out IntPtr credPtr))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_NOT_FOUND)
                {
                    return null;
                }

                throw new InvalidOperationException(
                    $"Failed to read Mercury token from Windows Credential Manager (error {error}).");
            }

            try
            {
                CREDENTIAL cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                if (cred.CredentialBlobSize == 0 || cred.CredentialBlob == IntPtr.Zero)
                {
                    return null;
                }

                byte[] blob = new byte[cred.CredentialBlobSize];
                Marshal.Copy(cred.CredentialBlob, blob, 0, cred.CredentialBlobSize);
                return Encoding.Unicode.GetString(blob);
            }
            finally
            {
                CredFree(credPtr);
            }
        }

        /// <summary>
        /// Removes the stored Mercury token, if present. Returns true if a
        /// credential was deleted.
        /// </summary>
        public static bool DeleteToken()
        {
            if (CredDelete(CredentialKey, CRED_TYPE_GENERIC, 0))
            {
                return true;
            }

            int error = Marshal.GetLastWin32Error();
            if (error == ERROR_NOT_FOUND)
            {
                return false;
            }

            throw new InvalidOperationException(
                $"Failed to delete Mercury token from Windows Credential Manager (error {error}).");
        }

        /// <summary>True when a Mercury token is stored in the vault.</summary>
        public static bool HasToken()
        {
            return !string.IsNullOrEmpty(GetToken());
        }
    }
}
