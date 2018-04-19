using System;
using System.Windows.Forms;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace ArbitrationSimulator
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Application.Run(new ArbitrationForm());
			}

			catch (Exception)
			{
				Application.Exit();
			}

		}
	}
}
