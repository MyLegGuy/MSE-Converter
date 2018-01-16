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
			
			
			// ArcUnpacker extracts depending on filename
			Options.extractedImagesLocation = "./MGD jpn~/";
			Options.extractedScriptsLocation = "./MSE~/";
			Options.extractedBGMLocation = "./BGM~/";
			Options.extractedVoiceLocation = "./VOICE~/";
			Options.extractedSELocation = "./SE~/";
			
			Options.finalBGMLocation = Options.streamingAssetsFolder+"BGM/";
			Options.finalImagesLocation = Options.streamingAssetsFolder+"CG/";
			Options.finalScriptsLocation = Options.streamingAssetsFolder+"Scripts/";
			Options.finalSELocation = Options.streamingAssetsFolder+"SE/";
			Options.finalVoiceLocation = Options.streamingAssetsFolder+"voice/";
			
			Directory.CreateDirectory(Options.streamingAssetsFolder);
			Directory.CreateDirectory(Options.finalBGMLocation);
			Directory.CreateDirectory(Options.finalImagesLocation);
			Directory.CreateDirectory(Options.finalScriptsLocation);
			Directory.CreateDirectory(Options.finalSELocation);
			Directory.CreateDirectory(Options.finalVoiceLocation);
			
			//Directory.CreateDirectory(Options.extractedBGMLocation);
			//Directory.CreateDirectory(Options.extractedImagesLocation);
			//Directory.CreateDirectory(Options.extractedScriptsLocation);
			//Directory.CreateDirectory(Options.extractedSELocation);
			//Directory.CreateDirectory(Options.extractedVoiceLocation);
			
			
			/*
			
			// English graphics
			Console.Out.WriteLine("Extracting graphics, ./MGD jpn");
			ArcUnpacker.unpackToDirectory("./MGD jpn");
			// Scripts
			Console.Out.WriteLine("Extracting scripts, ./MSE");
			ArcUnpacker.unpackToDirectory("./MSE");
			//
			Console.Out.WriteLine("Extracting voices, ./VOICE");
			ArcUnpacker.unpackToDirectory("./VOICE");
			//
			Console.Out.WriteLine("Extracting BGM, ./BGM");
			ArcUnpacker.unpackToDirectory("./BGM");
			//
			Console.Out.WriteLine("Extracting SE, ./SE");
			ArcUnpacker.unpackToDirectory("./SE");
			
			*/
			
			GraphicsConverter.convertGraphics(Options.extractedImagesLocation,Options.finalImagesLocation,960,544);
			
			//GraphicsConverter myGraphicsConverter = new GraphicsConverter("./MGD");
			
			//myScriptConverter.ConvertScript("./ExtractedScripts/S001.MSD","./S001.txt");
			
			PresetFileMaker _myPresetFileMaker = new PresetFileMaker();
			
			string[] _scriptFileList = Directory.GetFiles(Options.extractedScriptsLocation);
			int i;
			// TODO - Does this work for alphabetical?
			Array.Sort(_scriptFileList);
			
			
			for (i=0;i<_scriptFileList.Length;i++){
				if (Path.GetExtension(_scriptFileList[i])==".MSD"){
					Console.Out.Write(Path.GetFileNameWithoutExtension(_scriptFileList[i]));
					string _nextPresetFilenameScriptConverter=ScriptConverter.ConvertScript(_scriptFileList[i],Options.finalScriptsLocation+Path.GetFileNameWithoutExtension(_scriptFileList[i])+".txt");
					if (_nextPresetFilenameScriptConverter!=null){
						_myPresetFileMaker.addScript(_scriptFileList[i], _nextPresetFilenameScriptConverter);
					}
				}
			}
			
			// Because this is a generic converter for multiple games, I can't have them all having the same preset filename or folder name.
			// The user chooses the name of the folder and preset file
			string _userPresetFilename;
			while (String.IsNullOrEmpty(_userPresetFilename)){
				Console.WriteLine("=====\n=====\nGive this game a unique name without special characters\n=====\n=====");
				_userPresetFilename = Console.ReadLine().MakeFilenameFriendly();
			}
			_myPresetFileMaker.writePresetFile(Options.streamingAssetsFolder+_userPresetFilename);
			StreamWriter _myStreamWriter = new StreamWriter(new FileStream(Options.streamingAssetsFolder+"includedPreset.txt",FileMode.Create));
			_myStreamWriter.WriteLine(_userPresetFilename);
			_myStreamWriter.Dispose();
			Directory.Move(Options.streamingAssetsFolder,"./"+_userPresetFilename);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}