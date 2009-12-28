using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Update
{
	public static class Zip
	{
		public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
		{
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
			if (!string.IsNullOrEmpty(password))
				s.Password = password;
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{
				string directoryName = outputFolder;
				string fileName = Path.GetFileName(theEntry.Name);

				// create directory 
				if (directoryName != "")
					Directory.CreateDirectory(directoryName);
				
				if (fileName != String.Empty)
				{
					if (theEntry.Name.IndexOf(".ini") < 0)
					{
						string fullPath = directoryName + "\\" + theEntry.Name;
						fullPath = fullPath.Replace("\\ ", "\\");
						string fullDirPath = Path.GetDirectoryName(fullPath);
						if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
						FileStream streamWriter = File.Create(fullPath);
						int size = 2048;
						byte[] data = new byte[size];
						while (true)
						{
							size = s.Read(data, 0, data.Length);
							if (size <= 0)
								break;
							streamWriter.Write(data, 0, size);
						}
						streamWriter.Close();
					}
				}
			}
			s.Close();
			if (deleteZipFile)
				File.Delete(zipPathAndFile);
		}
	}
}