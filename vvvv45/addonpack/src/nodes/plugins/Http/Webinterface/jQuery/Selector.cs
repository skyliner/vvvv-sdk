using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Selector : IScriptGenerator
	{
		public static Selector AllSelector = new RawStringSelector("*");
		public static Selector DocumentSelector = new NameObjectSelector("document");
		public static Selector ThisSelector = new NameObjectSelector("this");


		#region IScriptGenerator Members

		public abstract string PScript(int indentSteps, bool breakInternalLines);

		#endregion
	}
}
