/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.IO;
using Petals;

namespace Petals{
	class Program{
		
		static void DirToDir(string srcDirNoEndSlash, string destDirNoEndSlash, bool isMove){
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(srcDirNoEndSlash, "*",  SearchOption.AllDirectories)){
				Directory.CreateDirectory(dirPath.Replace(srcDirNoEndSlash, destDirNoEndSlash));
			}
			
			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(srcDirNoEndSlash, "*.*",  SearchOption.AllDirectories)){
				if (isMove){
					File.Move(newPath, newPath.Replace(srcDirNoEndSlash, destDirNoEndSlash));
				}else{
					File.Copy(newPath, newPath.Replace(srcDirNoEndSlash, destDirNoEndSlash));
				}
			}
		}
		
		static void MoveDirToDir(string srcDirNoEndSlash, string destDirNoEndSlash){
			Console.Out.WriteLine("Move "+srcDirNoEndSlash+" to "+destDirNoEndSlash);
			DirToDir(srcDirNoEndSlash,destDirNoEndSlash,true);
		}
		static void CopyDirToDir(string srcDirNoEndSlash, string destDirNoEndSlash){
			Console.Out.WriteLine("Copy "+srcDirNoEndSlash+" to "+destDirNoEndSlash);
			DirToDir(srcDirNoEndSlash,destDirNoEndSlash,false);
		}
		
		public static void Main(string[] args){
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
			
			Console.Out.WriteLine("Making StreamingAssets directories...");
			Directory.CreateDirectory(Options.streamingAssetsFolder);
			Directory.CreateDirectory(Options.finalBGMLocation);
			Directory.CreateDirectory(Options.finalImagesLocation);
			Directory.CreateDirectory(Options.finalScriptsLocation);
			Directory.CreateDirectory(Options.finalSELocation);
			Directory.CreateDirectory(Options.finalVoiceLocation);
			
			// TODO - Check if the user has ArcUnpacker. If not, warn them and offer to download it if they're on Windows.
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
			
			Console.Out.WriteLine("Converting graphics...");
			GraphicsConverter.convertGraphics(Options.extractedImagesLocation,Options.finalImagesLocation,960,544);
			
			//GraphicsConverter myGraphicsConverter = new GraphicsConverter("./MGD");
			
			//myScriptConverter.ConvertScript("./ExtractedScripts/S001.MSD","./S001.txt");
			
			PresetFileMaker _myPresetFileMaker = new PresetFileMaker();
			
			
			Console.Out.WriteLine("Converting scripts...");
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
			
			Console.Out.WriteLine("Moving from extraction directories to StreamingAssets directories...");
			MoveDirToDir(Options.extractedBGMLocation,Options.finalBGMLocation);
			MoveDirToDir(Options.extractedSELocation,Options.finalSELocation);
			MoveDirToDir(Options.extractedVoiceLocation,Options.finalVoiceLocation);
			
			Console.Out.WriteLine("Deleting extraction directories...");
			Directory.Delete(Options.extractedBGMLocation,true);
			Directory.Delete(Options.extractedImagesLocation,true);
			Directory.Delete(Options.extractedScriptsLocation,true);
			Directory.Delete(Options.extractedSELocation,true);
			Directory.Delete(Options.extractedVoiceLocation,true);
			
			// TODO - Check if the "Stuff" folder exists.
			Console.Out.WriteLine("Moving included assets to StreamingAssets...");
			CopyDirToDir("./Stuff/",Options.streamingAssetsFolder);
			
			// Because this is a generic converter for multiple games, I can't have them all having the same preset filename or folder name.
			// The user chooses the name of the folder and preset file
			string _userPresetFilename=null;
			while (String.IsNullOrEmpty(_userPresetFilename)){
				Console.WriteLine("=====\n=====\nGive this game a unique name without special characters\n=====\n=====");
				_userPresetFilename = Console.ReadLine().MakeFilenameFriendly();
			}
			Console.Out.WriteLine("Making preset...");
			_myPresetFileMaker.writePresetFile(Options.streamingAssetsFolder+_userPresetFilename);
			StreamWriter _myStreamWriter = new StreamWriter(new FileStream(Options.streamingAssetsFolder+"includedPreset.txt",FileMode.Create));
			_myStreamWriter.WriteLine(_userPresetFilename);
			_myStreamWriter.Dispose();
			Console.Out.WriteLine("Renaming StreamingAssets directory...");
			Directory.Move(Options.streamingAssetsFolder,"./"+_userPresetFilename);
			
			Console.Write("Done!\nPress any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}