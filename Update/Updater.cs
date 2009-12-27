using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Update
{
	public class Updater
	{
		static readonly WebClient Client = new WebClient();
		static readonly HashAlgorithm Hasher = new MD5CryptoServiceProvider();

		readonly XDocument _settings;
		readonly Dictionary<string, string> _fileHash;

		public string BaseUrl { get; private set; }
		public string AppInfoPath { get; private set; }

		public Updater()
		{
			_settings = XDocument.Load("update.xml");

			BaseUrl = _settings.Root.Attribute("BaseUrl").Value;
			AppInfoPath = _settings.Root.Attribute("AppInfo").Value;

			_fileHash = GetFileInfos(BaseUrl, AppInfoPath);
		}

		static Dictionary<string, string> GetFileInfos(string address, string appInfoPath)
		{
			Dictionary<string, string> fileInfos = new Dictionary<string, string>();
			
			address = address.TrimEnd('/');
			try
			{
				WebClient client = new WebClient();
				Stream strm = client.OpenRead(string.Format("{0}/{1}", address, appInfoPath));
				StreamReader sr = new StreamReader(strm);

				do
				{
					string[] line = sr.ReadLine().Split(' ');
					fileInfos.Add(line[1], line[0]);
				}
				while (!sr.EndOfStream);
			}
			catch (Exception ex)
			{
				throw new Exception("Online application info could not be read.", ex);
			}

			return fileInfos;
		}

		public void UpdateFiles()
		{
			foreach(string file in OutdatedFiles())
			{
				Client.DownloadFile(BaseUrl + "/" + file, file);
			}
		}

		List<string> OutdatedFiles()
		{
			List<string> files = new List<string>();

			foreach(string file in GetFileList())
			{
				if (!_fileHash.ContainsKey(file))
					throw new Exception("File not available in server info: \"" + file + "\"");

				if (ComputeHash(file) != _fileHash[file])
					files.Add(file);
			}
			return files;
		}

		public static string ComputeHash(string path)
		{
			try
			{
				StringBuilder localHash = new StringBuilder();
				if(!File.Exists(path)) return null;
				using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
				{
					foreach (Byte hashByte in Hasher.ComputeHash(f))
					{
						localHash.Append(string.Format("{0:x2}", hashByte));
					}
				}
				return localHash.ToString();
			}
			catch { return null; }
		}

		public List<string> GetFileList()
		{
			List<string> files = new List<string>();
			var globalDependencies = from dependency in _settings.Descendants("Dependency").Attributes("Module")
									 where dependency.Parent.Parent == _settings.Root
									 select dependency.Value;

			foreach (string globalDependency in globalDependencies)
				GetModuleFiles(globalDependency, files);

			return files;
		}

		void GetModuleFiles(string moduleName, ICollection<string> files)
		{
			var moduleFiles = _settings.Descendants("Module").Where(
									module => module.Attribute("Name").Value == moduleName
								).First().Descendants("File").Select(file => file.Attribute("Path").Value);

			foreach (var file in moduleFiles)
			{
				if (file == string.Empty || files.Contains(file)) continue;
				files.Add(file);
			}

			var dependencies = from dependency in _settings.Descendants("Dependency")
							   where dependency.Parent.Attribute("Name") != null &&
									 dependency.Parent.Attribute("Name").Value == moduleName
							   select dependency.Attribute("Module").Value;

			foreach (string dependency in dependencies)
				GetModuleFiles(dependency, files);
		}
	}
}
