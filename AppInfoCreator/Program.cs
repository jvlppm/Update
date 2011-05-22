using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AppInfoCreator
{
	static class Program
	{
		static readonly HashAlgorithm Hasher = new MD5CryptoServiceProvider();

		static string ComputeHash(string path)
		{
			StringBuilder localHash = new StringBuilder();
			using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
			{
				foreach (byte hashByte in Hasher.ComputeHash(f))
					localHash.Append(string.Format("{0:x2}", hashByte));
			}
			return localHash.ToString();
		}

		static void Main()
		{
			StreamWriter output = new StreamWriter("Files.txt");

			DirectoryInfo dir = new DirectoryInfo(".");

			List<FileInfo> files = new List<FileInfo>();
			files.AddRange(dir.GetFiles("*.dll"));
			files.AddRange(dir.GetFiles("*.zip"));
			files.AddRange(dir.GetFiles("*.exe"));

			foreach(var file in files)
			{
				FileInfo info = new FileInfo(file.FullName);
				output.WriteLine("{0} {1} {2}", ComputeHash(file.FullName) , file.Name, info.Length);
			}

			output.Close();
		}
	}
}
