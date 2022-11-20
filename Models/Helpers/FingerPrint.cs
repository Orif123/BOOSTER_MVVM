using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Helpers
{
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;

        /// <summary>
        /// Generates a 16 byte Unique Identification code of a computer
        /// from CPU, BIOS, BASE
        /// Example: 4876-8DB5-EE85-69D3
        /// </summary>
        /// <returns></returns>
        public static string GetComputerID()
        {
            string strToHash = "CPU  >> " + GetCPU() + "\nBIOS >> " + GetBIOS() + "\nBASE >> " + GetMotherboard()
                //+"\nDISK >> "+ GetDisk() + "\nVIDEO >> " + GetVideoController() +"\nMAC >> "+ GetMAC()
                ;
            return GetHash(strToHash);
        }

        /// <summary>
        /// Return PC Hardware Summary (CPU, BIOS, BASE, DISK, VIDEO, MAC)
        /// </summary>
        /// <returns></returns>
        public static string GetSummary()
        {
            //return $"CPU:   {GetCPU()}\nBIOS:  {GetBIOS()}\nBASE:  {GetMotherboard()}\nDISK:  {GetDisk()}\nVIDEO: {GetVideoController()}\nMAC:   {GetMAC()}";
            return "CPU\t" + GetCPU() + "\nBIOS\t" + GetBIOS() + "\nBASE\t" + GetMotherboard();

        }

        /// <summary>
        /// Creating hash code from the string
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        public static string GetHash(string summary)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(summary);
            var dd = sec.ComputeHash(bt);
            return GetHexString(dd);
        }

        /// <summary>
        /// Return CPU ID
        /// </summary>
        /// <returns></returns>
        private static string GetCPU()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as very time consuming
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = Identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = Identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = Identifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    retVal += Identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }

        /// <summary>
        /// Return BIOS Identifier
        /// </summary>
        /// <returns></returns>
        public static string GetBIOS()
        {
            return Identifier("Win32_BIOS", "Manufacturer")
                + Identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                + Identifier("Win32_BIOS", "IdentificationCode")
                + Identifier("Win32_BIOS", "SerialNumber")
                + Identifier("Win32_BIOS", "ReleaseDate")
                + Identifier("Win32_BIOS", "Version");
        }

        /// <summary>
        /// Return main physical hard drive ID
        /// </summary>
        /// <returns></returns>
        public static string GetDisk()
        {
            return Identifier("Win32_DiskDrive", "Model")
                + Identifier("Win32_DiskDrive", "Manufacturer")
                + Identifier("Win32_DiskDrive", "Signature")
                + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        /// <summary>
        /// Return motherboard ID
        /// </summary>
        /// <returns></returns>
        public static string GetMotherboard()
        {
            return Identifier("Win32_BaseBoard", "Model")
                + Identifier("Win32_BaseBoard", "Manufacturer")
                + Identifier("Win32_BaseBoard", "Name")
                + Identifier("Win32_BaseBoard", "SerialNumber");
        }

        /// <summary>
        /// Return primary video controller ID
        /// </summary>
        /// <returns></returns>
        public static string GetVideoController()
        {
            return Identifier("Win32_VideoController", "DriverVersion")
                + Identifier("Win32_VideoController", "Name");
        }

        /// <summary>
        /// Return MAC ID first enabled network card
        /// </summary>
        /// <returns></returns>
        public static string GetMAC()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }

        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length / 2; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length / 2 && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        /// <summary>
        /// Return a hardware identifier
        /// </summary>
        /// <param name="wmiClass"></param>
        /// <param name="wmiProperty"></param>
        /// <param name="wmiMustBeTrue"></param>
        /// <returns></returns>
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            ManagementClass managementClass = new ManagementClass(wmiClass);
            ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();
            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                if (managementObject[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            if (managementObject[wmiProperty] != null)
                            {
                                result = managementObject[wmiProperty].ToString();
                                break;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Return a hardware identifier
        /// </summary>
        /// <param name="wmiClass"></param>
        /// <param name="wmiProperty"></param>
        /// <returns></returns>
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            ManagementClass managementClass = new ManagementClass(wmiClass);
            ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();
            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        if (managementObject[wmiProperty] != null)
                        {
                            result = managementObject[wmiProperty].ToString();
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }
    }
}
