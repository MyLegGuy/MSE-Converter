/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;

namespace Petals
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			GraphicsConverter myGraphicsConverter = new GraphicsConverter("./MGD");
			ScirptConverter myScriptConverter = new ScirptConverter("./MSE");
			myScriptConverter.ConvertScript("./ExtractedScripts/S001.MSD","./S001.lua");
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}