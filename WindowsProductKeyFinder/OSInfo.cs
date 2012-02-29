//-----------------------------------------------------------------------
// <copyright file="OSInfo.cs" company="Andrew Beaton">
//     Copyright (c) Andrew Beaton. All rights reserved. 
// </copyright>
//-----------------------------------------------------------------------
namespace WindowsProductKeyFinder
{
    using System;
    using System.Management;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents information about the current operating system. 
    /// </summary>
    public class OSInfo
    {
        /// <summary>
        /// Initializes static members of the OSInfo class.
        /// Gets Operating System Name, Service Pack, and Architecture using WMI with the legacy methods as a fallback
        /// Credit: http://andrewensley.com/2009/10/c-detect-windows-os-version-%E2%80%93-part-2-wmi/.
        /// </summary>
        /// <returns>String containing the name of the operating system followed by its service pack (if any) and architecture</returns>
        static OSInfo()
        {
            ManagementObjectSearcher objMOS = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem");

            // Variables to hold our return value
            string os = string.Empty;
            string servicePack = string.Empty;
            int architecture = 0;

            try
            {
                foreach (ManagementObject objManagement in objMOS.Get())
                {
                    // Get OS version from WMI - This also gives us the edition
                    object caption = objManagement.GetPropertyValue("Caption");

                    if (caption != null)
                    {
                        // Remove all non-alphanumeric characters so that only letters, numbers, and spaces are left.
                        string osC = Regex.Replace(caption.ToString(), "[^A-Za-z0-9 ]", string.Empty);

                        // string osC = osCaption.ToString();
                        // If the OS starts with "Microsoft," remove it.  We know that already
                        if (osC.StartsWith("Microsoft"))
                        {
                            osC = osC.Substring(9);
                        }

                        // If the OS now starts with "Windows," again... useless.  Remove it.
                        if (osC.Trim().StartsWith("Windows"))
                        {
                            osC = osC.Trim().Substring(7);
                        }

                        // Remove any remaining beginning or ending spaces.
                        os = osC.Trim();

                        // Only proceed if we actually have an OS version - service pack is useless without the OS version.
                        if (!string.IsNullOrEmpty(os))
                        {
                            object sp = null;

                            try
                            {
                                // Get OS service pack from WMI
                                sp = objManagement.GetPropertyValue("ServicePackMajorVersion");
                                if (sp != null && sp.ToString() != "0")
                                {
                                    servicePack = "Service Pack " + sp.ToString();
                                }
                                else
                                {
                                    // Service Pack not found.  Try built-in Environment class.
                                    servicePack = "Service Pack " + GetOSServicePackLegacy();
                                }
                            }
                            catch (Exception)
                            {
                                // There was a problem getting the service pack from WMI.  Try built-in Environment class.
                                servicePack = GetOSServicePackLegacy();
                            }
                        }

                        object osA = null;

                        try
                        {
                            // Get OS architecture from WMI
                            osA = objManagement.GetPropertyValue("OSArchitecture");
                            if (osA != null)
                            {
                                string arch = osA.ToString();

                                // If "64" is anywhere in there, it's a 64-bit architectore.
                                architecture = arch.Contains("64") ? 64 : 32;
                            }
                        }
                        catch (Exception)
                        {
                            // There was a problem getting the operating system architecture. Try built-in Environment class.
                            architecture = GetOSArchitectureLegacy();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Failed to get data from WMI so fall back to the legacy method.
            }

            // If WMI couldn't tell us the OS, use our legacy method.
            // We won't get the exact OS edition, but something is better than nothing.
            if (os == string.Empty)
            {
                os = GetOSLegacy();
                servicePack = GetOSServicePackLegacy();
                architecture = GetOSArchitectureLegacy();
            }

            OS = "Microsoft Windows";

            string[] parts = Regex.Split(os, " ");
            Release = parts[0];

            if (servicePack != null || servicePack != string.Empty)
            {
                ServicePack = servicePack;
            }

            Edition = os.Substring(Release.Length).Trim().Replace(ServicePack, string.Empty).TrimEnd();
            X86Version = architecture.ToString();
        }

        /// <summary>
        /// Gets a string that contains the operating systems name.
        /// </summary>
        public static string OS { get; private set; }

        /// <summary>
        /// Gets a string that contains the operating systems release.
        /// </summary>
        public static string Release { get; private set; }

        /// <summary>
        /// Gets a string that contains the operating systems edition.
        /// </summary>
        public static string Edition { get; private set; }

        /// <summary>
        /// Gets a string that contains the operating systems service pack.
        /// </summary>
        public static string ServicePack { get; private set; }

        /// <summary>
        /// Gets a string that contains the operating systems x86 version.
        /// </summary>
        public static string X86Version { get; private set; }
     
        /// <summary>
        /// Gets a string that contains the operating systems description.
        /// </summary>
        /// <returns>A string containing the operating systems description.</returns>
        public static string Description()
        {
            StringBuilder description = new StringBuilder();

            description.Append(OS + " ");

            if (!(string.IsNullOrEmpty(Release)))
            {
                description.Append(Release + " "); 
            }

            if (!(string.IsNullOrEmpty(Edition))) 
            {
                description.Append(Edition + " "); 
            }

            if (!(string.IsNullOrEmpty(ServicePack))) 
            {
                description.Append(ServicePack + " "); 
            }

            if (!(string.IsNullOrEmpty(X86Version)))
            {
                description.Append(X86Version + " bit");
            }

            return description.ToString();
        }

        /// <summary>
        /// Returns the <see cref="WindowsEdition"/> of the opeating system.
        /// </summary>
        /// <returns>The WindowsEdition of the operating system.</returns>
        public static WindowsEditions WindowsEdition()
        {
            string windowsEditionString = "Windows" + Release + Edition;

            try
            {  
                return (WindowsEditions)Enum.Parse(typeof(WindowsEditions), windowsEditionString.Replace(" ", string.Empty), true);    
            }
            catch (ArgumentException)
            {
                return WindowsEditions.NotSupported;
            }
        }

        /// <summary>
        /// Gets Operating System Name using .Net's Environment class.
        /// </summary>
        /// <returns>String containing the name of the operating system followed by its service pack (if any).</returns>
        private static string GetOSLegacy()
        {
            // Get Operating system information.
            OperatingSystem os = Environment.OSVersion;

            // Get version information about the os.
            Version vs = os.Version;

            // Variable to hold our return value
            string operatingSystem = string.Empty;

            if (os.Platform == PlatformID.Win32Windows)
            {
                // This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                        {
                            operatingSystem = "98SE";
                        }
                        else
                        {
                            operatingSystem = "98";
                        }

                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                        {
                            operatingSystem = "2000";
                        }
                        else
                        {
                            operatingSystem = "XP";
                        }

                        break;
                    case 6:
                        if (vs.Minor == 0)
                        {
                            operatingSystem = "Vista";
                        }
                        else
                        {
                            operatingSystem = "7";
                        }

                        break;
                    default:
                        break;
                }
            } 

            // Return the information we've gathered.
            return operatingSystem;
        }

        /// <summary>
        /// Gets the installed Operating System Service Pack using .Net's Environment class.
        /// </summary>
        /// <returns>
        /// String containing the operating system's installed service pack (if any).
        /// </returns>
        private static string GetOSServicePackLegacy()
        {
            // Get service pack from Environment Class
            string sp = Environment.OSVersion.ServicePack;

            if (sp != null && sp.ToString() != string.Empty && sp.ToString() != " ")
            {
                // If there's a service pack, return it
                return sp.ToString();
            }

            // No service pack.  Return an empty string
            return string.Empty;
        }

        /// <summary> 
        /// Gets Operating System Architecture.  This does not tell you if the program in running in
        /// 32- or 64-bit mode or if the CPU is 64-bit capable.  It tells you whether the actual Operating
        /// System is 32- or 64-bit.
        /// </summary>
        /// <returns>Int containing 32 or 64 representing the number of bits in the OS Architecture.</returns>
        private static int GetOSArchitectureLegacy()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return (string.IsNullOrEmpty(pa) || string.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64;
        }
    }
}
