﻿/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:36 PM
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Petals;
namespace Petals{
	/// <summary>
	/// Description of ScirptConverter.
	/// </summary>
	public class ScriptConverter{
		enum cmdType{
			UNKNOWN,
			IMAGEUNKNOWN,
			BG,
			BUST,
			BGM,
			SE,
			VOICE,
			TEXT,
			CHOICE,
		};
		static bool IsAsciiCharacter(byte _value){
			if (_value>=0x20 && _value<=0x7F){
				return true;
			}
			return false;
		}
		static bool IsSpecialCharacter(byte _value){
			// Centered dot character thing
			if (_value==0x81){
				return true;
			}
			return false;
		}
		static bool IsCertianFileType(string _fileDirectory,string _filename, string _fileExtention){
			return File.Exists(Path.Combine(_fileDirectory,_filename+_fileExtention));
		}
		static byte[] ReadNullTerminatedString(BinaryReader br){
			List<byte> _tempReadBytes = new List<byte>();
			byte _lastReadByte;
			while(true){
				_lastReadByte = br.ReadByte();
				if (_lastReadByte==0x00){
					if (_tempReadBytes.Count==0){
						return null;
					}
					break;
				}else if (_lastReadByte==0x81){ // newlines are 0x81 0x40 0x81 0x40
					long _positionCache = br.BaseStream.Position;
					
					// Check if 
					byte _lastReadNewlineByte = br.ReadByte();
					if (_lastReadNewlineByte==0x40){
						_lastReadNewlineByte = br.ReadByte();
						if (_lastReadNewlineByte==0x81){
							_lastReadNewlineByte = br.ReadByte();
							if (_lastReadNewlineByte==0x40){
								int i;
								// Remove useless spaces
								for (i=_tempReadBytes.Count-1;i>=0;i--){
									if (_tempReadBytes[i]==0x20){
										_tempReadBytes[i]=0;
									}else{
										break;
									}
								}
								// Just replace it with a space. My engine will take care of newlines.
								_tempReadBytes.Add(0x20);
								continue;
							}
						}
					}
					
					// We messed up by checking the next few characters. Fix that with our cache.
					br.BaseStream.Position = _positionCache;
					
					// Here, we check for special characters
					byte _lastReadSpecialCheckByte = br.ReadByte();
					
					if (_lastReadSpecialCheckByte==0x75){ // Left hook bracket
						_tempReadBytes.Add((byte)((int)' '));
						_tempReadBytes.Add((byte)((int)'"'));
					}else if (_lastReadSpecialCheckByte==0x76){ // Right hook bracket
						_tempReadBytes.Add((byte)((int)'"'));
						_tempReadBytes.Add((byte)((int)' '));
					}else if (_lastReadSpecialCheckByte == 0x99){ // Star character
						_tempReadBytes.Add(0xE2); // Special character byte
						
						_tempReadBytes.Add(152);
						_tempReadBytes.Add(134);
					}else if (_lastReadSpecialCheckByte == 0xF4){ // Music note character
						_tempReadBytes.Add(0xE2); // Special character byte
						
						_tempReadBytes.Add(153);
						_tempReadBytes.Add(170);
					}else{
						// This is how a lone 0x81 would appear
						_tempReadBytes.Add((byte)'*');
					}
					continue;
				}else if (!IsAsciiCharacter(_lastReadByte)){
					if (IsSpecialCharacter(_lastReadByte)){
						_tempReadBytes.Add(_lastReadByte);
						continue;
					}
					// avoid consuming potential command
					if (_lastReadByte==0x03){
						br.BaseStream.Position-=1;
					}
					// We read a string, but it wasn't null terminated. Must not be an ASCII string.
					return null;	
				}
				_tempReadBytes.Add(_lastReadByte);
			}
			_tempReadBytes.RemoveAll(x => x == 0); // Remove all 0 bytes. These are created by the newline parser
			return _tempReadBytes.ToArray();
		}
		static bool ContainsIlligalCharacters(string filename){
			char[] _tempInvalidCharacters = Path.GetInvalidFileNameChars();
			int i;
			for (i=0;i<_tempInvalidCharacters.Length;i++){
				if (filename.Contains(_tempInvalidCharacters[i].ToString())){
					return true;
				}
			}
			return false;
		}
		static string MakeStringArgument(string _passedString){
			_passedString = _passedString.Replace("'","\\'");
			_passedString = _passedString.Replace("\"","\\\"");
			return "\""+_passedString+"\"";
		}
		
		static void WriteDialougeCommand(BinaryWriter bw, string _text, string _name){
			bw.GoodWriteString(String.Format("luastring ShowDialogue({0},{1});\n",MakeStringArgument(_text),MakeStringArgument(_name)));
		}
		static void WriteVoiceCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("luastring PlayVoice({0});\n",MakeStringArgument(_filename)));
		}
		static void WriteSECommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("luastring PlaySE({0});\n",MakeStringArgument(_filename)));
		}
		static void WriteShowBustForecast(BinaryWriter bw, List<Tuple<string, byte>> _passedBustCommandList){
			bw.GoodWriteString("luastring ShowBustForecast({");
			int i;
			for (i=0;i<_passedBustCommandList.Count;i++){
				if (i!=0){
					bw.GoodWriteString(",");
				}
				bw.GoodWriteString("0x"+_passedBustCommandList[i].Item2.ToString("X"));
			}
			bw.GoodWriteString("});\n");
		}
		static void WriteShowBustCommand(BinaryWriter bw, string _filename, byte _passedPositionByte){
			bw.GoodWriteString(String.Format("luastring ShowBust({0},{1});\n",MakeStringArgument(_filename),"0x"+_passedPositionByte.ToString("X")));
		}
		static void WriteShowBackgroundCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("luastring ShowBackground({0});\n",MakeStringArgument(_filename)));
		}
		static void WritePlayBGMCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("luastring PlayBGM({0});\n",MakeStringArgument(_filename)));
		}
		/*	
		d0 - sound?
		Da - sound?
		67 - Bust?
		e4 - filter?
		da - Voice?
		05 - Dialouge?
		01 - DIalouge?
		66 - CG?
		*/
		static cmdType getCommandType(string _readString, byte _specialByte){
			// HACK for length
			if (_readString.Length<Options.minStringLength){
				return cmdType.UNKNOWN;
			}
			// I think this is the special byte for the filters.
			if (_specialByte==0xE4){
				return cmdType.UNKNOWN;
			}
			if (_specialByte==0xEE){ // GS_EN_NS
				return cmdType.UNKNOWN;
			}
			if (_specialByte==0x65){ // GS_HI_NS
				return cmdType.UNKNOWN;
			}
			if (_specialByte==0x03){				
				Console.Out.WriteLine("Found choice command.");
				return cmdType.CHOICE;
			}
			// Guess what type of command this is based on filename
			if (!ContainsIlligalCharacters(_readString)){
				if (IsCertianFileType(Options.extractedImagesLocation,_readString,".png")){
					if (_specialByte==0x66){
						// Definetly a CG or background
						return cmdType.BG;
					}else if (_specialByte==0x67){
						// Definetly a bust
						return cmdType.BUST;
					}else{ // Unknown
						return cmdType.IMAGEUNKNOWN;
					}
				}else{
					// maybe it's a sound command. guess using the different extraction directories
					if (IsCertianFileType(Options.extractedVoiceLocation,_readString,".ogg")){
						Console.WriteLine("found voice");
						return cmdType.VOICE;
					}else if (IsCertianFileType(Options.extractedBGMLocation,_readString,".ogg")){
						return cmdType.BGM;
					}else if (IsCertianFileType(Options.extractedSELocation,_readString,".ogg")){
						return cmdType.SE;
					}
				}
			}
			// Alright, well, there's no file with a name that's the same as this string's. It must be dialogue.
			return cmdType.TEXT;
		}
		
		static void WriteCommand(cmdType _type, BinaryWriter bw, BinaryReader br, string _readString, byte _specialByte, List<Tuple<string, byte>> _passedBustCommandList){
			if (_type==cmdType.TEXT || _type==cmdType.BGM || _type==cmdType.SE || _type==cmdType.VOICE){
				WriteBustCommandList(bw,_passedBustCommandList);
			}
			switch(_type){
				case cmdType.CHOICE:
					// todo
					return;
				case cmdType.UNKNOWN:
					return;
				case cmdType.BG:
					WriteShowBackgroundCommand(bw,_readString);
					break;
				case cmdType.BUST:
					// Read the bust position byte.
					br.BaseStream.Position+=9;
					byte _readPositionByte=br.ReadByte();
					// Is DAMMY, we don't need it.
					if (_readPositionByte==0xEC){
						return;
					}
					// CU01, CU02, CU03 all have position byte 00
					// CB far left
					// C8 middle
					// CE far right
					if (!(_readPositionByte==0xCB || _readPositionByte==0xC8 || _readPositionByte==0xCE || _readPositionByte==0x00)){ 
						Console.Out.WriteLine("===Unknown position byte===");
						Console.Out.WriteLine(_readPositionByte.ToString("X"));
						Console.Out.WriteLine(_readString);
					}
					_passedBustCommandList.Add(new Tuple<string, byte>(_readString,_readPositionByte));
					break;
				case cmdType.IMAGEUNKNOWN:
					br.BaseStream.Position+=9;
					byte _tempReadPositionByte = br.ReadByte();
					Console.Out.WriteLine("===Unknown image type.===");
					Console.Out.WriteLine("Special byte: 0x"+_specialByte.ToString("X"));
					Console.Out.WriteLine("Position byte: 0x"+_tempReadPositionByte.ToString("X"));
					Console.Out.WriteLine("Filename: "+_readString);
					WriteDialougeCommand(bw,"Special byte: 0x" +_specialByte.ToString("X")+"  Position byte: 0x"+_tempReadPositionByte.ToString("X")+"   Filename: "+_readString,"NAME_ERROR");
					WriteShowBackgroundCommand(bw,_readString);
					break;
				case cmdType.BGM:
					WritePlayBGMCommand(bw,_readString);
					break;
				case cmdType.SE:
					WriteSECommand(bw,_readString);
					break;
				case cmdType.VOICE:
					WriteVoiceCommand(bw,_readString);
					break;
				case cmdType.TEXT:
					WriteDialougeCommand(bw,_readString,"");
					break;
			}
		}
		
		static void WriteBustCommandList(BinaryWriter bw, List<Tuple<string, byte>> _passedBustCommandList){
			if (_passedBustCommandList.Count==0){
				return;
			}
			WriteShowBustForecast(bw,_passedBustCommandList);
			int i;
			for (i=0;i<_passedBustCommandList.Count;i++){
				WriteShowBustCommand(bw,_passedBustCommandList[i].Item1,_passedBustCommandList[i].Item2);
			}
			_passedBustCommandList.Clear();
		}
		
		// Returns the name of the script which is the first string found.
		public static string ConvertScript(string _passedFilename, string _passedOutputFilename, string _nextScriptFilename){
			BinaryReader br = new BinaryReader(new FileStream(_passedFilename,FileMode.Open));
			List<Tuple<string, byte>> upcomingBustDisplayCommands = new List<Tuple<string, byte>>();
			bool _isSearchingForChoiceEnd=false;
			bool _isFirstCommand=true; // false if at least one command has been written
			string _foundScriptTitle="NOTITLE";
			
			byte[] _possibleMagicBytes = br.ReadBytes(14);
			if (System.Text.Encoding.ASCII.GetString(_possibleMagicBytes)!="MSCENARIO FILE"){
				Console.WriteLine("Corrupted or encrypted.");
				return null;
			}
			FileStream mainFileStream = new FileStream(_passedOutputFilename,FileMode.Create);
			BinaryWriter bw = new BinaryWriter(mainFileStream);
			//bw.GoodWriteString(("function main()\n"));
			bw.GoodWriteString("ScriptConverter v "+Options.scriptConverterVersion+"\n");
			while (br.BaseStream.Position != br.BaseStream.Length){
				byte _lastReadByte;
				_lastReadByte = br.ReadByte();
				
				/*if (_isSearchingForChoiceEnd==true && _lastReadByte==0xFF){
					// FF FF FF FF FF FF FF FF
					// marks the end of the first choice block.
					int i;
					for (i=0;i<7;i++){ // We already read the first FF, make sure the next 7 are also FF.
						_lastReadByte = br.ReadByte();
						if (_lastReadByte!=0xFF){
							br.BaseStream.Position--;
							break;
						}
					}
					if (i!=7){
						continue;
					}
					Console.Out.WriteLine("Found the end!");
					bw.GoodWriteString("else\n");
					WriteDialougeCommand(bw,"Second choice exclusive!","");
					bw.GoodWriteString("end\n");
					_isSearchingForChoiceEnd=false;
					continue;
				}*/
				
				if (_lastReadByte==0x03){
				newReadCommand:
					if (_isSearchingForChoiceEnd==true){ // 03 00 05 marks the choice end.
						byte _nextByte = br.ReadByte();
						if (_nextByte==0x00){
							_nextByte = br.ReadByte();
							if (_nextByte==0x05){
								// Choice found
								bw.GoodWriteString("text (proceeding to second choice dialog)\n");
								bw.GoodWriteString("fi\n");
								bw.GoodWriteString("//TODO - Second choice if statement\n");
								_isSearchingForChoiceEnd=false;
								continue;
							}else{
								br.BaseStream.Seek(-2,SeekOrigin.Current);
							}
						}else{
							br.BaseStream.Seek(-1,SeekOrigin.Current);
						}
					}
					byte[] _readString = ReadNullTerminatedString(br);
					if (_readString!=null){
						byte _readSpecialByte;
						try{
							_readSpecialByte=br.ReadByte();
						}catch(Exception){
							_readSpecialByte=0;
						}
						cmdType _curType = getCommandType(Encoding.UTF8.GetString(_readString),_readSpecialByte);
						if (_curType==cmdType.UNKNOWN && _readSpecialByte==0x03){
							goto newReadCommand; // avoid consuming a potential command
						}
						// HACK - if the first command is dialogue, treat it as the scene title
						if (_isFirstCommand && _curType==cmdType.TEXT){
							_isFirstCommand=false;
							_foundScriptTitle = System.Text.Encoding.ASCII.GetString(_readString);
							bw.GoodWriteString(String.Format("setvar curSceneName = \"{0}\"\n",_foundScriptTitle.Replace('"','\'')));
							continue;
						}else{
							if (_curType!=cmdType.UNKNOWN){
								_isFirstCommand=false;
							}
						}
						WriteCommand(_curType,bw,br,Encoding.UTF8.GetString(_readString),_readSpecialByte,upcomingBustDisplayCommands);
						if (_curType==cmdType.CHOICE){ // Did I just write a choice command?
							if (System.Text.Encoding.ASCII.GetString(_readString).Length>Options.minStringLength){
								GraphicsConverter.splitChoiceGraphic(Path.Combine(Options.extractedImagesLocation,Encoding.ASCII.GetString(_readString)));
								// WriteCommand started the if statement, I need to be looking out for where to end it.
								_isSearchingForChoiceEnd=true;
								bw.GoodWriteString(String.Format("imagechoice {0}0 {0}1 {0}2 {0}5 {0}6 {0}7\n",Encoding.ASCII.GetString(_readString)));
								bw.GoodWriteString("if selected == 1\n");
							}
						}
						
					}
				}
			}
			
			if (_nextScriptFilename!=null){
				bw.GoodWriteString("jump "+_nextScriptFilename);
			}
			//bw.GoodWriteString(("end"));
			bw.Dispose();
			mainFileStream.Dispose();
			br.Dispose();
			
			Console.Out.WriteLine("OK");
			
			return _foundScriptTitle;
		}
	}
}
