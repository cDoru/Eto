using System;
using Eto;
using Eto.Misc;
using Eto.Test;
using Eto.Forms;
using Eto.Drawing;
using System.Diagnostics;

namespace Eto.Test.Gtk3
{
	class Startup
	{
		//[STAThread]
		static void Main (string [] args)
		{
#if DEBUG
			Debug.Listeners.Add (new ConsoleTraceListener());
#endif
			var generator = Generator.GetGenerator (Generators.Gtk3Assembly);
			
			var app = new TestApplication (generator);
			app.Run (args);
		}
	}
}

