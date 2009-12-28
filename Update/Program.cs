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
				double lastUpdate = 0;
				Updater updater = new Updater();
				Console.Write("Loading => ");
				int maxBarSize = Console.WindowWidth - Console.CursorLeft;

				int updatedFiles = 1;
				List<string> updateFiles = updater.OutdatedFiles();
				foreach (string file in updateFiles)
				{
					updater.DownloadFile(file);
					if(file.EndsWith(".zip"))
						Zip.UnZipFiles(file, ".", "", true);

					double atualPercentage = updatedFiles++ / ((double)updateFiles.Count);
					for (int i = 1; i < (atualPercentage - lastUpdate) * maxBarSize; i++)
						Console.Write("#");
					lastUpdate = atualPercentage;
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
