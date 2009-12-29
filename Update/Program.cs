using System;
using System.Collections.Generic;

namespace Update
{
	static class Program
	{
		static void Main()
		{
			try
			{
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
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
				Console.ReadKey();
			}
		}
	}
}
