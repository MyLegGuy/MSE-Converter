/*
 * User: good
 * Date: 1/15/2018
 * Time: 12:48 PM
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace Petals{
	/// Class that lets you easily make preset files for Higurashi-Vita.
	public class PresetFileMaker{
		List<string> presetScriptFiles;
		List<string> presetScriptNames;
		public PresetFileMaker(){
			presetScriptFiles = new List<string>(0);
			presetScriptNames = new List<string>(0);
		}
		public void addScript(string _passedScriptFilename, string _passedScriptFriendlyName){
			presetScriptFiles.Add(Path.GetFileNameWithoutExtension(_passedScriptFilename));
			presetScriptNames.Add(_passedScriptFriendlyName);
		}
		
		void writeStringListForPreset(StreamWriter _myStreamWriter ,List<string> _passedList){
			// Number of script files in the game
			_myStreamWriter.WriteLine(_passedList.Count.ToString("000"));
			// Script filenames
			for (int i=0;i<presetScriptFiles.Count;++i){
				_myStreamWriter.WriteLine(_passedList[i]);
			}
		}
		
		public void writePresetFile(string _filename){
			StreamWriter _myStreamWriter = new StreamWriter(new FileStream(_filename,FileMode.Create));
			writeStringListForPreset(_myStreamWriter,presetScriptFiles);
			// We can skip TIP filename list and TIP unlock list because we call OptionsSetTips in the game specific Lua. We go right to the script friendly names.
			_myStreamWriter.WriteLine("chapternames");
			writeStringListForPreset(_myStreamWriter,presetScriptNames);
			_myStreamWriter.Dispose();
		}
	}
}
