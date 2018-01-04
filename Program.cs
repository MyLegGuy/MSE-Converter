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
			/*
			Options.extractedBGMLocation = Options.streamingAssetsFolder+"BGM/";
			Options.extractedImagesLocation = Options.streamingAssetsFolder+"CG/";
			Options.extractedScriptsLocation = Options.streamingAssetsFolder+"Scripts/";
			Options.extractedSELocation = Options.streamingAssetsFolder+"SE/";
			Options.extractedVoiceLocation = Options.streamingAssetsFolder+"voice/";
			
			Directory.CreateDirectory(Options.streamingAssetsFolder);
			Directory.CreateDirectory(Options.extractedBGMLocation);
			Directory.CreateDirectory(Options.extractedImagesLocation);
			Directory.CreateDirectory(Options.extractedScriptsLocation);
			Directory.CreateDirectory(Options.extractedSELocation);
			Directory.CreateDirectory(Options.extractedVoiceLocation);
			
			ArcUnpacker.unpackToDirectory("./MGD jpn");
			//D:\Programming\C#\PetalsVitaConverter\bin\Debug\arc_unpacker.exe -o=./happy -d=nsystem/fjsys ./MSE
			return;
			*/
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