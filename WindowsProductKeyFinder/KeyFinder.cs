//-----------------------------------------------------------------------
// <copyright file="KeyFinder.cs" company="Andrew Beaton">
//     Copyright (c) Andrew Beaton. All rights reserved. 
// </copyright>
//-----------------------------------------------------------------------
namespace WindowsProductKeyFinder
{
    using System;
    using System.Collections;
    using Microsoft.Win32;

    /// <summary>
    /// Retrieves and decodes the Windows product key from the system registry.
    /// </summary>
    public static class KeyFinder
    {
        private static int success = 0;
        private static UIntPtr hiveKeyLocalMachine = (UIntPtr)0x80000002;
        private static int keyQueryValue = 0x0001;
        private static int keyWow6464Key = 0x0100;
        private static int keyWow6432Key = 0x0200;
        private static string regKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private static UIntPtr regKeyHandle;

        /// <summary>
        /// Gets the decoded Windows product key based on the Windows edition.
        /// </summary>
        /// <param name="windowsEdition">The Windows edition currently being run.</param>
        /// <returns>A string containing the Windows product key.</returns>
        public static string GetProductKey(WindowsEditions windowsEdition)
        {
            byte[] digitalProductId = null;
            string results = string.Empty;

            try
            {
                digitalProductId = KeyFinder.GetRegistryDigitalProductId(windowsEdition);
            }
            catch (Exception ex)
            {
                results = ex.Message;
            }

            if (digitalProductId != null)
            {
                results = KeyFinder.DecodeProductKey(digitalProductId);
            }
            else
            {
                results = "Product key could not be retrieved.";
            }

            return results;
        }

        [System.Runtime.InteropServices.DllImport("advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        private static extern int RegQueryValueEx(UIntPtr hiveKey, string valueName, int reserved, out uint type, byte[] valueData, ref int bufferSize);

        [System.Runtime.InteropServices.DllImport("advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint = "RegOpenKeyExW", SetLastError = true)]
        private static extern int RegOpenKeyEx(UIntPtr hiveKey, string subKey, uint options, int sam, out UIntPtr result); 

        private static byte[] StringToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            return encoding.GetBytes(str);
        } 

        /// <summary>
        /// Gets the Windows product key from the registry in encoded form.
        /// </summary>
        /// <param name="windowsEdition">The Windows edition of the system being used.</param>
        /// <returns>A byte array containing the value of the encrypted Windows product key.</returns>
        private static byte[] GetRegistryDigitalProductId(WindowsEditions windowsEdition)
        {
            byte[] digitalProductId = null;

            string digitalProductIdKeyName = GetDigitalProductIdKeyNameForWindowsEdition(windowsEdition);

            RegistryKey registry = Registry.LocalMachine.OpenSubKey(regKeyPath, false);
            {
                digitalProductId = registry.GetValue(digitalProductIdKeyName) as byte[];
            }

            registry.Close();

            if (digitalProductId == null)
            {
                uint type;
                int bufferSize = 2048;
                digitalProductId = new byte[2048];

                if (RegOpenKeyEx(hiveKeyLocalMachine, regKeyPath, 0, keyQueryValue | keyWow6464Key, out regKeyHandle) == success)
                {
                    RegQueryValueEx(regKeyHandle, digitalProductIdKeyName, 0, out type, digitalProductId, ref bufferSize);
                }

                if (RegOpenKeyEx(hiveKeyLocalMachine, regKeyPath, 0, keyQueryValue | keyWow6432Key, out regKeyHandle) == success)
                {
                    RegQueryValueEx(regKeyHandle, digitalProductIdKeyName, 0, out type, digitalProductId, ref bufferSize);
                } 
            }

            return digitalProductId;
        }

        /// <summary>
        /// Decodes the encrypted Windows product key.
        /// </summary>
        /// <param name="digitalProductId">A byte array containing the encrypted Windows product key.</param>
        /// <returns>The unencrypted Windows product key as a string.</returns>
        private static string DecodeProductKey(byte[] digitalProductId)
        {
            // Offset of first byte of encoded product key in 
            // 'DigitalProductIdxxx" REG_BINARY value. Offset = 34H.
            const int keyStartIndex = 52;

            // Offset of last byte of encoded product key in 
            // 'DigitalProductIdxxx" REG_BINARY value. Offset = 43H.
            const int keyEndIndex = keyStartIndex + 15;

            // Possible alpha-numeric characters in product key.
            char[] digits = new char[]
            {
                'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'P', 'Q', 'R', 
                'T', 'V', 'W', 'X', 'Y', '2', '3', '4', '6', '7', '8', '9',
            };

            // Length of decoded product key
            const int decodeLength = 29;

            // Length of decoded product key in byte-form.
            // Each byte represents 2 chars.
            const int decodeStringLength = 15;

            // Array of containing the decoded product key.
            char[] decodedChars = new char[decodeLength];

            // Extract byte 52 to 67 inclusive.
            ArrayList hexPid = new ArrayList();

            for (int i = keyStartIndex; i <= keyEndIndex; i++)
            {
                hexPid.Add(digitalProductId[i]);
            }

            for (int i = decodeLength - 1; i >= 0; i--)
            {
                // Every sixth char is a separator.
                if ((i + 1) % 6 == 0)
                {
                    decodedChars[i] = '-';
                }
                else
                {
                    // Do the actual decoding.
                    int digitMapIndex = 0;

                    for (int j = decodeStringLength - 1; j >= 0; j--)
                    {
                        int byteValue = (digitMapIndex << 8) | (byte)hexPid[j];

                        hexPid[j] = (byte)(byteValue / 24);

                        digitMapIndex = byteValue % 24;

                        decodedChars[i] = digits[digitMapIndex];
                    }
                }
            }

            return new string(decodedChars);
        }

        /// <summary>
        /// Gets the correct Product ID Key Name in the registry for the specified Windows edition.
        /// </summary>
        /// <param name="edition">The Windows edition of the system currently in use.</param>
        /// <returns>A string containing the correct Digital Product ID Key name.</returns>
        private static string GetDigitalProductIdKeyNameForWindowsEdition(WindowsEditions edition)
        {
            switch (edition)
            {
                case WindowsEditions.Windows7Professional:
                    return "DigitalProductId4";
                default:
                    return "DigitalProductId";
            }
        }   
    }
}
