/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.Drawing;
using System.IO;
using Petals;

// TODO - Alt script file doesn't work - this is due to encryption
namespace Petals{
	class Program{
		static void DirToDir(string srcDirNoEndSlash, string destDirNoEndSlash, bool isMove, bool _capsFilename){
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(srcDirNoEndSlash, "*",  SearchOption.AllDirectories)){
				Directory.CreateDirectory(dirPath.Replace(srcDirNoEndSlash, destDirNoEndSlash));
			}
			
			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(srcDirNoEndSlash, "*.*",  SearchOption.AllDirectories)){
				string _destFilename = newPath.Replace(srcDirNoEndSlash, destDirNoEndSlash);
				if (_capsFilename){
					_destFilename = Path.Combine(Path.GetDirectoryName(_destFilename),Path.GetFileName(_destFilename).ToUpper());
				}
				if (isMove){
					File.Move(newPath,_destFilename);
				}else{
					File.Copy(newPath,_destFilename);
				}
			}
		}
		static void MoveDirToDir(string srcDirNoEndSlash, string destDirNoEndSlash){
			Console.Out.WriteLine("Move "+srcDirNoEndSlash+" to "+destDirNoEndSlash);
			DirToDir(srcDirNoEndSlash,destDirNoEndSlash,true,true);
		}
		static void CopyDirToDir(string srcDirNoEndSlash, string destDirNoEndSlash){
			Console.Out.WriteLine("Copy "+srcDirNoEndSlash+" to "+destDirNoEndSlash);
			DirToDir(srcDirNoEndSlash,destDirNoEndSlash,false,false);
		}
		static void checkForGameFile(string _filename, ref bool _isFileMissing){
			if (!File.Exists(_filename)){
				printGameFileMissing(_filename);
				_isFileMissing=true;
			}
		}
		static void printGameFileMissing(string _filename){
			Console.Out.WriteLine(_filename+" is missing. This is a file you must get from the game.");
		}
		static bool requiredFilesMissing(){
			bool _isFileMissing=false;
			if (!File.Exists("./MSD")){ // Scripts 
				Console.Out.WriteLine("./MSD missing, falling back on ./MSE");
				checkForGameFile("./MSE", ref _isFileMissing);
			}
			if (!File.Exists("./MGD jpn")){
				Console.Out.WriteLine("\"./MGD jpn\" is missing, falling back on ./MGD");
				checkForGameFile("./MGD", ref _isFileMissing);
			}
			checkForGameFile("./BGM", ref _isFileMissing);
			checkForGameFile("./VOICE", ref _isFileMissing);
			checkForGameFile("./SE", ref _isFileMissing);
			if (!Directory.Exists("./Stuff")){
				Console.Out.WriteLine("./Stuff/ not found. This folder is supposed to be included with the download. If not, create a Github issue.");
				_isFileMissing=true;
			}
			if (_isFileMissing){
				pressAnyKeyToContinue();
			}
			return _isFileMissing;
		}
		// Does not work with subdirectories
		static void lazyAddLegarchive(LegArchive _passedArchive, string _dir){
			int i;
			String[] _addThese = Directory.GetFiles(_dir,"*",SearchOption.AllDirectories);
			for (i=0;i<_addThese.Length;++i){
				_passedArchive.addFile(_addThese[i],Path.GetFileName(_addThese[i]));
			}
		}
		static void pressAnyKeyToContinue(){
			Console.WriteLine("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		static String pickBonusScript(){
			if (!Directory.Exists("./manualScripts")){
				Console.WriteLine("Warning, ./manualScripts does not exist");
				return null;
			}
			Console.WriteLine("(0) none");
			String[] _potential = Directory.GetFiles("./manualScripts");
			int i;
			for (i=0;i<_potential.Length;++i){
				Console.WriteLine("("+(i+1)+") "+_potential[i]);
			}
			Console.WriteLine("Chose script:");
			i = Int32.Parse(Console.ReadLine());
			if (i<=0 || i>_potential.Length){
				return null;
			}else{
				return _potential[i-1];
			}
		}
		static void runBonusScript(String _path){
			StreamReader file = new StreamReader(_path);
			Bitmap _curImg=null;
			String line;
			while((line=file.ReadLine())!=null){
				if (line.Length==0 || line[0]=='#'){
					continue;
				}
				String[] args = line.Split(' ');
				if (args[0]=="mkdir"){
					Directory.CreateDirectory(Path.Combine(Options.streamingAssetsFolder,args[1]));
				}else if (args[0]=="subGraphic"){
					GraphicsConverter.saveSingleSubgraphic(_curImg,Path.Combine(Options.streamingAssetsFolder,args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]),Int32.Parse(args[4]),Int32.Parse(args[5]));
				}else if (args[0]=="loadImg"){
					if (_curImg!=null){
						_curImg.Dispose();
					}
					string _fullPath = Path.Combine(Options.extractedImagesLocation,args[1]);
					Console.WriteLine(_fullPath);
					_curImg = new Bitmap(_fullPath);
				}else{
					throw new Exception("invalid command "+args[0]);
				}
			}
			if (_curImg!=null){
				_curImg.Dispose();
			}
			file.Close();
		}
		public static int Main(string[] args){
			Console.WriteLine("Hello World!");
			if (requiredFilesMissing()){
				return 1;
			}
			Console.Write("converting for Vita? (y/n): ");
			if (Console.ReadLine().Trim()=="y"){
				Console.WriteLine("Vita converting mode enabled");
				Options.doResizeGraphics=true;
				Options.useSoundArchive=true;
				Options.screenWidth=960;
				Options.screenHeight=544;
			}else{
				Console.WriteLine("generic platform mode");
				Options.doResizeGraphics=false;
				Options.useSoundArchive=false;
			}
			String _bonusScript = pickBonusScript();
			// ArcUnpacker extracts depending on filename
			Options.extractedImagesLocation = "./MGD jpn~/";
			Options.extractedScriptsLocation = "./MSE~/";
			Options.extractedBGMLocation = "./BGM~/";
			Options.extractedVoiceLocation = "./VOICE~/";
			Options.extractedSELocation = "./SE~/";
			//
			Options.finalImagesLocation = Options.streamingAssetsFolder+"CG/";
			Options.finalScriptsLocation = Options.streamingAssetsFolder+"Scripts/";
			Options.finalSoundArchiveLocation = Options.streamingAssetsFolder+"SEArchive.legArchive";
			if (!Options.useSoundArchive){
				Options.finalBGMLocation = Options.streamingAssetsFolder+"BGM/";
				Options.finalSELocation = Options.streamingAssetsFolder+"SE/";
				Options.finalVoiceLocation = Options.streamingAssetsFolder+"voice/";
			}
			Console.Out.WriteLine("Making StreamingAssets directories...");
			Directory.CreateDirectory(Options.streamingAssetsFolder);
			Directory.CreateDirectory(Options.finalImagesLocation);
			Directory.CreateDirectory(Options.finalScriptsLocation);
			if (!Options.useSoundArchive){
				Directory.CreateDirectory(Options.finalBGMLocation);
				Directory.CreateDirectory(Options.finalSELocation);
				Directory.CreateDirectory(Options.finalVoiceLocation);
			}
			// extract assts
			if (!(args.Length>=1 && args[0]=="--noextract")){
				// Extract scripts with fallback
				Console.Out.Write("Extracting scripts, ");
				if (File.Exists("./MSE")){
					Console.Out.WriteLine("./MSE");
					ArcUnpacker.unpackToDirectory("./MSE");
				}else{
					Options.extractedScriptsLocation = "./MSD~/";
					Console.Out.WriteLine("./MSD");
					ArcUnpacker.unpackToDirectory("./MSD");
				}

				// Extract english graphics with fallback
				if (File.Exists("./MGD jpn")){
					Console.Out.WriteLine("Extracting graphics, ./MGD jpn");
					ArcUnpacker.unpackToDirectory("./MGD jpn");
				}else if (File.Exists("./MGD")){
					Options.extractedImagesLocation = "./MGD~/";
					Console.Out.WriteLine("Extracting graphics, ./MGD");
					ArcUnpacker.unpackToDirectory("./MGD");
				}
				// Extract other stuff
				Console.Out.WriteLine("Extracting voices, ./VOICE");
				ArcUnpacker.unpackToDirectory("./VOICE");
				Console.Out.WriteLine("Extracting BGM, ./BGM");
				ArcUnpacker.unpackToDirectory("./BGM");
				Console.Out.WriteLine("Extracting SE, ./SE");
				ArcUnpacker.unpackToDirectory("./SE");

				// do a little bit of fixing of the mgd extraction.
				// does not resize any graphics or anything horrible like that
				GraphicsConverter.convertMGD(Options.extractedImagesLocation);
			}			
			if (_bonusScript!=null){
				Console.WriteLine("running "+_bonusScript);
				runBonusScript(_bonusScript);
			}
			Console.Out.WriteLine("Converting scripts...");
			//PresetFileMaker _myPresetFileMaker = new PresetFileMaker();
			string[] _scriptFileList = Directory.GetFiles(Options.extractedScriptsLocation);
			if (_scriptFileList.Length==0){
				Console.Out.WriteLine("no scripts found in "+Options.extractedScriptsLocation);
				return 1;
			}
			int i;
			bool _alreadyFoundMainScript=false;
			Array.Sort(_scriptFileList);
			for (i=0;i<_scriptFileList.Length;i++){
				if (Path.GetExtension(_scriptFileList[i]).ToLower()==".msd"){
					Console.Out.WriteLine(Path.GetFileNameWithoutExtension(_scriptFileList[i]));
					bool _isValid=ScriptConverter.ConvertScript(_scriptFileList[i],(_alreadyFoundMainScript ? Options.finalScriptsLocation+Path.GetFileNameWithoutExtension(_scriptFileList[i])+".scr" : Options.finalScriptsLocation+"main.scr"),(i!=_scriptFileList.Length-1 ?Path.GetFileNameWithoutExtension(_scriptFileList[i+1])+".scr" : null));
					if (_isValid){
						_alreadyFoundMainScript=true;
					}
				}
			}
			// if enabled, resize the graphics and save the resized version to the appropriate location
			if (Options.doResizeGraphics){
				Console.Out.WriteLine("Resizing graphics...");
				GraphicsConverter.resizeGraphics(Options.extractedImagesLocation,Options.finalImagesLocation,Options.screenWidth,Options.screenHeight);
			}
			if (Options.useSoundArchive){
				// HACK - This will not support subdirectories because I'm lazy and they aren't needed. This note is only here for me if I come back years later trying to port the second game, or something, which could have subdirectories.
				Console.Out.WriteLine("Creating sound archive...");
				LegArchive _soundArchive = new LegArchive(Options.finalSoundArchiveLocation);
				lazyAddLegarchive(_soundArchive,Options.extractedVoiceLocation);
				lazyAddLegarchive(_soundArchive,Options.extractedBGMLocation);
				lazyAddLegarchive(_soundArchive,Options.extractedSELocation);
				_soundArchive.finish();
			}else{
				Console.Out.WriteLine("Moving from extraction directories to StreamingAssets directories...");
				MoveDirToDir(Options.extractedBGMLocation,Options.finalBGMLocation);
				MoveDirToDir(Options.extractedSELocation,Options.finalSELocation);
				MoveDirToDir(Options.extractedVoiceLocation,Options.finalVoiceLocation);
				// if we didn't resize the graphics, we need to move the original extracted ones
				if (!Options.doResizeGraphics){
					MoveDirToDir(Options.extractedImagesLocation,Options.finalImagesLocation);
				}
			}
			
			Console.Out.WriteLine("Deleting extraction directories...");
			Directory.Delete(Options.extractedBGMLocation,true);
			Directory.Delete(Options.extractedImagesLocation,true);
			Directory.Delete(Options.extractedScriptsLocation,true);
			Directory.Delete(Options.extractedSELocation,true);
			Directory.Delete(Options.extractedVoiceLocation,true);

			Console.Out.WriteLine("Moving included assets to StreamingAssets...");
			CopyDirToDir("./Stuff/",Options.streamingAssetsFolder);
			
			Console.Out.WriteLine("Creating isvnds file.");
			File.Create(Options.streamingAssetsFolder+"isvnds").Dispose();
			
			// Because this is a generic converter for multiple games, I can't have them all having the same preset filename or folder name.
			// The user chooses the name of the folder and preset file
			string _userPresetFilename=null;
			while (String.IsNullOrEmpty(_userPresetFilename)){
				Console.WriteLine("=====\n=====\nGive this game a unique name without special characters\n=====\n=====");
				_userPresetFilename = Console.ReadLine().MakeFilenameFriendly();
			}
			/*
			Console.Out.WriteLine("Making preset...");
			//_myPresetFileMaker.writePresetFile(Options.streamingAssetsFolder+_userPresetFilename);
			StreamWriter _myStreamWriter = new StreamWriter(new FileStream(Options.streamingAssetsFolder+"includedPreset.txt",FileMode.Create));
			_myStreamWriter.WriteLine(_userPresetFilename);
			_myStreamWriter.Dispose();
			*/
			
			Console.Out.WriteLine("Renaming StreamingAssets directory...");
			bool _couldRename=true;
			do{
				try{
					Directory.Move(Options.streamingAssetsFolder,"./"+_userPresetFilename);
				}catch(Exception e){
					Console.Out.WriteLine(e.ToString()+"\nFailed to rename directory, retrying in 3 seconds.");
					System.Threading.Thread.Sleep(3000);
					_couldRename=false;
				}
			}while(_couldRename==false);
			
			Console.Out.WriteLine("Done.");
			return 0;
		}
	}
}
