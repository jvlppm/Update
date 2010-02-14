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

	public class Module
	{
		public Module(string name)
		{
			Name = name;
		}
		public string Name { get; private set; }
		public bool Enabled { get; set; }
	}

	public class Updater
	{
		#region Private Attributes
		static readonly WebClient Client = new WebClient();
		static readonly HashAlgorithm Hasher = new MD5CryptoServiceProvider();

		readonly XDocument _modules;
		readonly Dictionary<string, FileInfo> _serverFiles;
		readonly string _updateUrl;

		public List<Module> Modules { get; private set; }
		#endregion

		#region Public Methods
		public Updater(string updateUrl)
		{
			_updateUrl = updateUrl;

			_modules = XDocument.Load(new StreamReader(Client.OpenRead(_updateUrl + "/Modules.xml")));
			_serverFiles = GetServerFileInfos(_updateUrl + "/Files.txt");

			Modules = GetModules();
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

			try
			{
				if (!File.Exists(file) || ComputeHash(file) != _serverFiles[file].Hash)
					Client.DownloadFile(_updateUrl + "/" + file, file);
			}
			catch(Exception ex)
			{
				throw new Exception(string.Format("File could not be downloaded: \"{0}\"", file), ex);
			}

			if (file.EndsWith(".zip"))
				Zip.UnZipFile(file, true);
		}

		public List<string> GetUpdateFileList()
		{
			Dictionary<string, List<FileInfo>> files = new Dictionary<string, List<FileInfo>>();

			foreach (FileInfo file in GetFileList())
			{
				if (!_serverFiles.ContainsKey(file.Path))
					throw new Exception("File not available in server info: \"" + file.Path + "\"");

				if(File.Exists(file.Path) && (!file.CheckUpdates || ComputeHash(file.Path) == _serverFiles[file.Path].Hash))
					continue;

				if(!files.ContainsKey(file.Zip))
					files.Add(file.Zip, new List<FileInfo>());

				files[file.Zip].Add(file);
			}

			return UpdateFileList(files);
		}
		#endregion

		#region Private Methods
		static Dictionary<string, FileInfo> GetServerFileInfos(string address)
		{
			Dictionary<string, FileInfo> fileInfos = new Dictionary<string, FileInfo>();

			address = address.TrimEnd('/');
			try
			{
				StreamReader sr = new StreamReader(Client.OpenRead(address));

				do
				{
					string[] line = sr.ReadLine().Split(' ');
					fileInfos.Add(line[1], new FileInfo { Hash = line[0], Size = long.Parse(line[2]) });
				}
				while (!sr.EndOfStream);
			}
			catch (Exception ex)
			{
				throw new Exception("Online application info could not be read.", ex);
			}

			return fileInfos;
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
						totalSize += _serverFiles[file.Path].Size;

					if (totalSize > _serverFiles[module.Key].Size)
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

		List<Module> GetModules()
		{
			var modules =	from module in _modules.Descendants("Download")
							select new Module(module.Attribute("Module").Value)
							{
								Enabled = module.Attribute("EnabledByDefault") == null || bool.Parse(module.Attribute("EnabledByDefault").Value)
							};

			return modules.ToList();
		}

		List<FileInfo> GetFileList()
		{
			List<FileInfo> files = new List<FileInfo>();

			foreach (Module module in Modules)
			{
				if (module.Enabled)
					GetModuleFiles(module.Name, files);
			}

			return files;
		}

		void GetModuleFiles(string moduleName, ICollection<FileInfo> files)
		{
			XElement module = _modules.Descendants("Module").Where( m => m.Attribute("Name").Value == moduleName).First();
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

			var dependencies = from dependency in _modules.Descendants("Dependency")
							   where dependency.Parent.Attribute("Name") != null &&
									 dependency.Parent.Attribute("Name").Value == moduleName
							   select dependency.Attribute("Module").Value;

			foreach (string dependency in dependencies)
				GetModuleFiles(dependency, files);
		}
		#endregion
	}
}
