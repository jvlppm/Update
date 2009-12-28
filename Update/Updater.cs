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
	class FileInfo
	{
		public string Hash { get; set; }
		public long Size { get; set; }
		public string Path { get; set; }
		public string Zip { get; set; }
		public bool CheckUpdates { get; set; }
	}

	public class Updater
	{
		static readonly WebClient Client = new WebClient();
		static readonly HashAlgorithm Hasher = new MD5CryptoServiceProvider();

		readonly XDocument _settings;
		readonly Dictionary<string, FileInfo> _serverFileInfos;

		string BaseUrl { get; set; }
		string AppInfoPath { get; set; }

		public Updater()
		{
			_settings = XDocument.Load("update.xml");

			BaseUrl = _settings.Root.Attribute("BaseUrl").Value;
			AppInfoPath = _settings.Root.Attribute("AppInfo").Value;

			_serverFileInfos = GetServerFileInfos(BaseUrl, AppInfoPath);
		}

		static Dictionary<string, FileInfo> GetServerFileInfos(string address, string appInfoPath)
		{
			Dictionary<string, FileInfo> fileInfos = new Dictionary<string, FileInfo>();
			
			address = address.TrimEnd('/');
			try
			{
				WebClient client = new WebClient();
				Stream strm = client.OpenRead(string.Format("{0}/{1}", address, appInfoPath));
				StreamReader sr = new StreamReader(strm);

				do
				{
					string[] line = sr.ReadLine().Split(' ');
					fileInfos.Add(line[1], new FileInfo{ Hash = line[0], Size = long.Parse(line[2])});
				}
				while (!sr.EndOfStream);
			}
			catch (Exception ex)
			{
				throw new Exception("Online application info could not be read.", ex);
			}

			return fileInfos;
		}

		public void DownloadFile(string file)
		{
			string currentDirectory = string.Empty;
			string[] directories = file.Split('/');
			for (int i = 0; i < directories.Length - 1; i++ )
			{
				currentDirectory += directories[i] + "/";
				if (!Directory.Exists(currentDirectory))
					Directory.CreateDirectory(currentDirectory);
			}

			Client.DownloadFile(BaseUrl + "/" + file, file);
		}

		public List<string> OutdatedFiles()
		{
			Dictionary<string, List<FileInfo>> files = new Dictionary<string, List<FileInfo>>();

			foreach (FileInfo file in GetFileList())
			{
				if (!_serverFileInfos.ContainsKey(file.Path))
					throw new Exception("File not available in server info: \"" + file.Path + "\"");

				if(File.Exists(file.Path) && (!file.CheckUpdates || ComputeHash(file.Path) == _serverFileInfos[file.Path].Hash))
					continue;

				if(!files.ContainsKey(file.Zip))
					files.Add(file.Zip, new List<FileInfo>());

				files[file.Zip].Add(file);
			}

			return UpdateFileList(files);
		}

		List<string> UpdateFileList(Dictionary<string, List<FileInfo>> modules)
		{
			List<string> updateFiles = new List<string>();

			foreach(var module in modules)
			{
				bool addZip = false;
				if(!string.IsNullOrEmpty(module.Key))
				{
					long totalSize = 0;
					foreach (var file in module.Value)
						totalSize += _serverFileInfos[file.Path].Size;

					if (totalSize > _serverFileInfos[module.Key].Size)
						addZip = true;
				}
				if (addZip)
					updateFiles.Add(module.Key);
				else
				{
					foreach(var file in module.Value)
						updateFiles.Add(file.Path);
				}
			}

			return updateFiles;
		}

		static string ComputeHash(string path)
		{
			StringBuilder localHash = new StringBuilder();
			using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
			{
				foreach (Byte hashByte in Hasher.ComputeHash(f))
					localHash.Append(string.Format("{0:x2}", hashByte));
			}
			return localHash.ToString();
		}

		List<FileInfo> GetFileList()
		{
			List<FileInfo> files = new List<FileInfo>();
			var globalDependencies = from dependency in _settings.Descendants("Dependency").Attributes("Module")
									 where dependency.Parent.Parent == _settings.Root
									 select dependency.Value;

			foreach (string globalDependency in globalDependencies)
				GetModuleFiles(globalDependency, files);

			return files;
		}

		void GetModuleFiles(string moduleName, ICollection<FileInfo> files)
		{
			XElement module = _settings.Descendants("Module").Where( m => m.Attribute("Name").Value == moduleName).First();
			var moduleFiles = module.Descendants("File").Select(file => new FileInfo{
         		Path = file.Attribute("Path").Value,
				CheckUpdates = file.Attribute("CheckUpdates") != null? bool.Parse(file.Attribute("CheckUpdates").Value): true,
				Zip = module.Attribute("Zip") == null ? "" : module.Attribute("Zip").Value
         	});

			if (moduleFiles.Count() == 0 && module.Attribute("Zip") != null)
				files.Add(new FileInfo { Path = module.Attribute("Zip").Value, Zip = ""});
			else
			{
				foreach (var file in moduleFiles)
				{
					if (file.Path == string.Empty || files.Contains(file)) continue;
					files.Add(file);
				}
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
