using System;
using System.Text.RegularExpressions;
using Jv.Plugins;
using System.Linq;
using Update;
using System.Diagnostics;
using System.IO;
using Jv.Threading;

namespace Launcher
{
	public class Program
	{
		static readonly Manager PluginManager = new Manager();

		static void BackgroundUpdateCheck()
		{
			Parallel.Start(() =>
			{
				try
				{
					var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
					if (MainUpdateRequired(updater))
					{
						Settings.WriteValue("Update", "CheckUpdates", "True");
						return;
					}
					EnableOnlyPlugins(updater);
					if (updater.GetUpdateFileList().Count > 0)
						Settings.WriteValue("Update", "CheckUpdates", "Plugins");
				}
				catch (Exception ex)
				{
					PluginManager.MessageToPlugin<PLog>(new Exception("BackgroundUpdateCheck Failed", ex));
				}
			});
		}

		public static void Main(string[] args)
		{
			try
			{
				if (!args.ToList().Contains("--disable-updates"))
				{
					bool msgOk = false;
					if (Settings.ReadValue("Update", "CheckUpdates") == "True" || Settings.ReadValue("Update", "CheckUpdates") == string.Empty)
					{
						msgOk = true;
						Console.Write("Checking updates...");
						var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
						Settings.WriteValue("Update", "CheckUpdates", "Plugins");

						if (MainUpdateRequired(updater))
						{
							Console.WriteLine("Update required!");
							updater = new Updater(Settings.ReadValue("Update", "UpdaterUrl"));
							foreach (var file in updater.GetUpdateFileList())
								updater.DownloadFile(file);

							Process.Start("Update.exe", "--wait-process " + Process.GetCurrentProcess().ProcessName + " --auto-execute \"" + AppDomain.CurrentDomain.FriendlyName + "\"");
							return;
						}
					}
					if (Settings.ReadValue("Update", "CheckUpdates") == "Plugins")
					{
						if(!msgOk)
							Console.Write("Checking updates...");
						var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
						Settings.WriteValue("Update", "CheckUpdates", "False");
						EnableOnlyPlugins(updater);
						var pluginFiles = updater.GetUpdateFileList();
						if (pluginFiles.Count > 0)
						{
							Console.Write("Downloading plugins..");
							foreach (var plugin in pluginFiles)
							{
								Console.Write(".");
								updater.DownloadFile(plugin);
							}
						}
						Console.WriteLine("OK");
					}
					else
					{
						BackgroundUpdateCheck();
					}
				}
				else
				{
					BackgroundUpdateCheck();
				}

				LoadPlugins();

				PluginManager.GetPlugin<PProgram>().Run();
			}
			catch (FileNotFoundException ex)
			{
				Settings.WriteValue("Update", "CheckUpdates", "True");
				Console.WriteLine("O arquivo \"{0}\" não foi encontrado.", ex.FileName);
				PluginManager.MessageToPlugin<PLog>(ex);
			}
			catch (Exception ex)
			{
				Settings.WriteValue("Update", "CheckUpdates", "True");
				Exception inner = ex.InnerException;
				Console.WriteLine("Error: {0}", ex.Message);
				while (inner != null)
				{
					Console.WriteLine(inner.Message);
					inner = inner.InnerException;
				}
				PluginManager.MessageToPlugin<PLog>(ex);
			}
		}

		private static void EnableOnlyPlugins(Updater updater)
		{
			foreach (var module in updater.Modules)
			{
				if (module.Name.StartsWith("Plugin."))
					module.Enabled = Settings.ReadValue("Plugin", module.Name.Substring(7)).ToLower() == "enabled";
				else
					module.Enabled = false;
			}
		}

		static bool MainUpdateRequired(Updater updater)
		{
			foreach (var module in updater.Modules)
			{
				if (module.Name.StartsWith("Plugin."))
				{
					module.Enabled = false;
					if (module.Enabled && Settings.ReadValue("Plugin", module.Name.Substring(7)) == string.Empty)
						Settings.WriteValue("Plugin", module.Name.Substring(7), "enabled");
				}
			}

			return updater.GetUpdateFileList().Count > 0;
		}

		private static void LoadPlugins()
		{
			foreach (string fileName in Directory.GetFiles(".", "Plugin.*.dll")
										.Select(dll => dll.Replace('\\', '/').Replace("./", "")))
			{
				try
				{
					string pluginName = Regex.Replace(fileName, @"^Plugin\.([^\.]*).dll$", "$1");
					string pluginStatus = Settings.ReadValue("Plugin", pluginName);

					if (string.IsNullOrEmpty(pluginStatus) || pluginStatus.ToLower() == "enabled")
					{
						PluginManager.LoadPlugin<Plugin>(fileName);
						Settings.WriteValue("Plugin", pluginName, "enabled");
					}
					else
						Settings.WriteValue("Plugin", pluginName, "disabled");
				}
				catch (Jv.Plugins.Exceptions.CouldNotInstantiate) { throw; }
				catch (Jv.Plugins.Exceptions.CouldNotLoad) { }
			}
		}
	}
}
