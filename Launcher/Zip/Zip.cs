using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Update
{
	public static class Zip
	{
		public static void UnZipFile(string zipPath, bool deleteZip)
		{
			UnZipFiles(zipPath, ".", null, deleteZip);
		}

		static void UnZipFiles(string zipPath, string outputFolder, string password, bool deleteZipFile)
		{
			ZipInputStream zip = new ZipInputStream(File.OpenRead(zipPath));
			if (!string.IsNullOrEmpty(password))
				zip.Password = password;
			ZipEntry theEntry;
			while ((theEntry = zip.GetNextEntry()) != null)
			{
				string fileName = Path.GetFileName(theEntry.Name);

				// create directory 
				if (outputFolder != "")
					Directory.CreateDirectory(outputFolder);
				
				if (fileName != String.Empty)
				{
					if (theEntry.Name.IndexOf(".ini") < 0)
					{
						string fullPath = outputFolder + "\\" + theEntry.Name;
						fullPath = fullPath.Replace("\\ ", "\\");
						string fullDirPath = Path.GetDirectoryName(fullPath);
						if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
						FileStream streamWriter = File.Create(fullPath);
						int size = 2048;
						byte[] data = new byte[size];
						while (true)
						{
							size = zip.Read(data, 0, data.Length);
							if (size <= 0)
								break;
							streamWriter.Write(data, 0, size);
						}
						streamWriter.Close();
					}
				}
			}
			zip.Close();
			if (deleteZipFile)
				File.Delete(zipPath);
		}
	}
}