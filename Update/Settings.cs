using System.Runtime.InteropServices;
using System.Text;

namespace Update
{
	public static class Settings
	{
		readonly static string Path = string.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), "Settings.ini");

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		public static void WriteValue(string section, string key, string value)
		{
			WritePrivateProfileString(section, key, value, Path);
		}

		public static string ReadValue(string section, string key)
		{
			StringBuilder temp = new StringBuilder(255);
			GetPrivateProfileString(section, key, "", temp, 255, Path);
			return temp.ToString();
		}
	}
}
