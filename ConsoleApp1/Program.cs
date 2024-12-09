using System.Management;



var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SecuritySetting");
foreach (ManagementObject obj in searcher.Get())
{
	Console.WriteLine($"Name: {obj["Caption"]}");
}

