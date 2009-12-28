﻿using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AppInfoCreator
{
	class Program
	{
		static readonly HashAlgorithm Hasher = new MD5CryptoServiceProvider();
		public static string ComputeHash(string path)
		{
			try
			{
				StringBuilder localHash = new StringBuilder();
				if (!File.Exists(path)) return null;
				using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
				{
					foreach (byte hashByte in Hasher.ComputeHash(f))
					{
						localHash.Append(string.Format("{0:x2}", hashByte));
					}
				}
				return localHash.ToString();
			}
			catch { return null; }
		}

		static void Main(string[] args)
		{
			StreamWriter output = new StreamWriter("files.txt");

			DirectoryInfo dir = new DirectoryInfo(".");

			List<FileInfo> files = new List<FileInfo>();
			files.AddRange(dir.GetFiles("*.dll"));
			files.AddRange(dir.GetFiles("*.zip"));

			foreach(var file in files)
			{
				output.WriteLine("{0} {1}", ComputeHash(file.FullName) , file.Name);
			}

			output.Close();
		}
	}
}
