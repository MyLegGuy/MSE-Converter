/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:36 PM
 */
using System;
using System.IO;
using System.Collections.Generic;
using Petals;
namespace Petals{
	/// <summary>
	/// Description of ScirptConverter.
	/// </summary>
	public class ScirptConverter{
		
		bool IsAsciiCharacter(byte _value){
			if (_value>=0x20 && _value<=0x7F){
				return true;
			}
			return false;
		}
		bool IsSpecialCharacter(byte _value){
			// Centered dot character thing
			if (_value==0x81){
				return true;
			}
			return false;
		}
		bool IsCertianFileType(string _fileDirectory,string _filename, string _fileExtention){
			if (File.Exists(Path.Combine(_fileDirectory,_filename+_fileExtention))){
				return true;
			}
			return false;
		}
		// TODO 81 40 81 40 is newline
		// TODO 81 by itself is a dot
		byte[] ReadNullTerminatedString(BinaryReader br){
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
					_tempReadBytes.Add(0x81);
					continue;
				}else if (!IsAsciiCharacter(_lastReadByte)){	
					if (IsSpecialCharacter(_lastReadByte)){
						_tempReadBytes.Add(_lastReadByte);
						continue;
					}
					return null;	
				}
				_tempReadBytes.Add(_lastReadByte);
			}
			_tempReadBytes.RemoveAll(x => x == 0);
			return _tempReadBytes.ToArray();
		}
		bool ContainsIlligalCharacters(string filename){
			char[] _tempInvalidCharacters = Path.GetInvalidFileNameChars();
			int i;
			for (i=0;i<_tempInvalidCharacters.Length;i++){
				if (filename.Contains(_tempInvalidCharacters[i].ToString())){
					return true;
				}
			}
			return false;
		}
		string MakeStringArgument(string _passedString){
			_passedString = _passedString.Replace("'","\\'");
			_passedString = _passedString.Replace("\"","\\\"");
			return "\""+_passedString+"\"";
		}
		void WriteDialougeCommand(BinaryWriter bw, string _text, string _name){
			bw.GoodWriteString(String.Format("ShowDialogue({0},{1});\n",MakeStringArgument(_text),MakeStringArgument(_name)));
		}
		void WriteVoiceCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("PlayVoice({0});\n",MakeStringArgument(_filename)));
		}
		void WriteSECommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("PlaySE({0});\n",MakeStringArgument(_filename)));
		}
		void WriteShowBustCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("ShowBust({0});\n",MakeStringArgument(_filename)));
		}
		void WriteShowBackgroundCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("ShowBackground({0});\n",MakeStringArgument(_filename)));
		}
		void WritePlayBGMCommand(BinaryWriter bw, string _filename){
			bw.GoodWriteString(String.Format("PlayBGM({0});\n",MakeStringArgument(_filename)));
		}
		
		// TODO - Special byte research
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
		void WriteCommand(BinaryWriter bw,string _readString, byte _specialByte){
			if (!ContainsIlligalCharacters(_readString)){
				if (IsCertianFileType(Options.extractedVoiceLocation,_readString,".ogg")){
					WriteVoiceCommand(bw,_readString);
					return;
				}else if (IsCertianFileType(Options.extractedBGMLocation,_readString,".ogg")){
					WritePlayBGMCommand(bw,_readString);
					return;
				}else if (IsCertianFileType(Options.extractedImagesLocation,_readString,".png")){
					if (_specialByte==0x66){
						// Definetly a CG or background
						WriteShowBackgroundCommand(bw,_readString);
					}else if (_specialByte==0x67){
						// Definetly a bust
						WriteShowBustCommand(bw,_readString);
					}else{
						// Unknown
						Console.Out.WriteLine("Unknown image type.");
						WriteShowBackgroundCommand(bw,_readString);
					}
					return;
				}else if (IsCertianFileType(Options.extractedSELocation,_readString,".ogg")){
					WriteSECommand(bw,_readString);
					return;
				}
			}
			WriteDialougeCommand(bw,_readString,"");
		}
		
		public void ConvertScript(string _passedFilename, string _passedOutputFilename){
			FileStream mainFileStream = new FileStream(_passedOutputFilename,FileMode.Create);
			BinaryWriter bw = new BinaryWriter(mainFileStream);
			BinaryReader br = new BinaryReader(new FileStream(_passedFilename,FileMode.Open));
			bw.GoodWriteString(("function main()\n"));
			
			while (br.BaseStream.Position != br.BaseStream.Length){
				byte _lastReadByte;
				_lastReadByte = br.ReadByte();
				if (_lastReadByte==0x03){
					byte[] _readString = ReadNullTerminatedString(br);
					if (_readString!=null){
						Console.Out.WriteLine(System.Text.Encoding.Default.GetString(_readString));
						byte _readSpecialByte;
						try{
							_readSpecialByte=br.ReadByte();
						}catch(Exception){
							_readSpecialByte=0;
						}
						WriteCommand(bw,System.Text.Encoding.ASCII.GetString(_readString),_readSpecialByte);
					}
				}
				
			}
			
			bw.GoodWriteString(("end"));
			bw.Dispose();
			mainFileStream.Dispose();
			br.Dispose();
		}
		public ScirptConverter(string _passedArchivedScriptFile){
		}
	}
}
