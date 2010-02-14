using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Update
{
	static class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string waitProcess = null;
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] == "--wait-process" && i + 1 < args.Length)
					{
						waitProcess = args[i + 1];
						break;
					}
				}

				if (!string.IsNullOrEmpty(waitProcess))
				{
					var processList = Process.GetProcessesByName(waitProcess);
					if (processList.Length == 1)
					{
						Console.WriteLine("Waiting for {0} to end...", waitProcess);
						processList[0].WaitForExit();
					}
				}

				Console.Write("Loading => ");
				Updater updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));

				double lastUpdate = 0;
				int maxBarSize = Console.WindowWidth - Console.CursorLeft;
				int updatedFiles = 0;

				#region Enable Optional Modules
				foreach (var module in updater.Modules)
				{
					string customStatus = Settings.ReadValue(module.Name, "Enabled");
					if (!string.IsNullOrEmpty(customStatus))
						module.Enabled = bool.Parse(customStatus);
				}
				#endregion

				List<string> updateFiles = updater.GetUpdateFileList();
				foreach (string file in updateFiles)
				{
					updater.DownloadFile(file);

					#region Update ProgressBar
					double atualPercentage = ++updatedFiles / ((double)updateFiles.Count);
					for (int i = 1; i < (atualPercentage - lastUpdate) * maxBarSize; i++)
						Console.Write("#");
					lastUpdate = atualPercentage;
					#endregion
				}

				Console.Write(new string('#', Console.WindowWidth - Console.CursorLeft));

				string autoExecute = null;
				for(int i = 0; i < args.Length; i++)
				{
					if (args[i] == "--auto-execute" && i + 1 < args.Length)
					{
						autoExecute = args[i + 1];
						break;
					}
				}
				if (!string.IsNullOrEmpty(autoExecute))
				{
					string[] cmd = autoExecute.Split(' ');
					Process.Start(cmd[0], string.Join(" ", cmd, 1, cmd.Length - 1));
				}
				else
				{
					Console.WriteLine("Updated");
					Console.ReadKey();
				}

			}
			catch(Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
				Console.ReadKey();
			}
		}
	}
}
