/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.IO;
using Petals;

namespace Petals
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			Options.finalBGMLocation = Options.streamingAssetsFolder+"BGM/";
			Options.finalImagesLocation = Options.streamingAssetsFolder+"CG/";
			Options.finalScriptsLocation = Options.streamingAssetsFolder+"Scripts/";
			Options.finalSELocation = Options.streamingAssetsFolder+"SE/";
			Options.finalVoiceLocation = Options.streamingAssetsFolder+"voice/";
			/*
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
			
			// ArcUnpacker extracts depending on filename
			Options.extractedImagesLocation = "./MGD jpn~/";
			Options.extractedScriptsLocation = "./MSE~/";
			
			GraphicsConverter.convertGraphics(Options.extractedImagesLocation,"");
			
			Console.ReadLine();
			
			//ArcUnpacker.unpackToDirectory("./MGD jpn");
			//ArcUnpacker.unpackToDirectory("./MSE");
			return;
			
			//GraphicsConverter myGraphicsConverter = new GraphicsConverter("./MGD");
			ScirptConverter myScriptConverter = new ScirptConverter("./MSE");
			
			//myScriptConverter.ConvertScript("./ExtractedScripts/S001.MSD","./S001.txt");
			
			PresetFileMaker _myPresetFileMaker = new PresetFileMaker();
			
			string[] _scriptFileList = Directory.GetFiles(Options.extractedScriptsLocation);
			int i;
			for (i=0;i<_scriptFileList.Length;i++){
				if (Path.GetExtension(_scriptFileList[i])==".MSD"){
					Console.Out.WriteLine(Path.GetFileNameWithoutExtension(_scriptFileList[i]));
					_myPresetFileMaker.addScript(_scriptFileList[i], myScriptConverter.ConvertScript(_scriptFileList[i],"./"+Path.GetFileNameWithoutExtension(_scriptFileList[i])+".txt"));
				}
			}
			
			// Because this is a generic converter for multiple games, I can't have them all having the same preset filename.
			Console.WriteLine("Give this game's savefile a unique name without special characters:");
			string _userPresetFilename = Console.ReadLine().MakeFilenameFriendly();
			_myPresetFileMaker.writePresetFile(Options.streamingAssetsFolder+_userPresetFilename);
			StreamWriter _myStreamWriter = new StreamWriter(new FileStream(Options.streamingAssetsFolder+"includedPreset.txt",FileMode.Create));
			_myStreamWriter.WriteLine(_userPresetFilename);
			_myStreamWriter.Dispose();
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}