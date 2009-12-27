using System;

namespace Update
{
	class Program
	{
		static void Main()
		{
			try
			{
				Updater updater = new Updater();
				updater.UpdateFiles();	
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
				Console.ReadKey();
			}
		}
	}
}
