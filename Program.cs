/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.IO;

namespace Petals
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			GraphicsConverter myGraphicsConverter = new GraphicsConverter("./MGD");
			ScirptConverter myScriptConverter = new ScirptConverter("./MSE");
			
			//myScriptConverter.ConvertScript("./ExtractedScripts/S001.MSD","./S001.txt");
			
			string[] _scriptFileList = Directory.GetFiles(Options.extractedScriptsLocation);
			int i;
			for (i=0;i<_scriptFileList.Length;i++){
				if (Path.GetExtension(_scriptFileList[i])==".MSD"){
					Console.Out.WriteLine(Path.GetFileNameWithoutExtension(_scriptFileList[i]));
					myScriptConverter.ConvertScript(_scriptFileList[i],"./"+Path.GetFileNameWithoutExtension(_scriptFileList[i])+".txt");
				}
			}
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}