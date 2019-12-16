/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:36 PM
 */

using System;
using System.IO;
using Petals;
namespace Petals{
	/// <summary>
	/// Description of ScirptConverter.
	/// </summary>
	public class ScriptConverter{
		static bool strictAscii=true;
		static string MakeStringArgument(string _passedString){
			_passedString = _passedString.Replace("'","\\'");
			_passedString = _passedString.Replace("\"","\\\"");
			return "\""+_passedString+"\"";
		}
		static string readCString(BinaryReader r){
			string _ret="";
			char _curChar='a';
			while(true){
				if (!strictAscii){
					try{
						_curChar=r.ReadChar();
					}catch(System.ArgumentException){
						Console.WriteLine("bad char at 0x{0:X}",r.BaseStream.Position);
						r.ReadByte();
					}
				}else{
					_curChar=(char)r.ReadByte();
				}
				if (_curChar=='\0'){
					break;
				}
				_ret+=_curChar;
			}
			return _ret;
		}
		static Object readArg(BinaryReader r, byte _type){
			switch(_type){
				case 0x01:
				case 0x02:
				case 0x05:
					return r.ReadInt32();
				case 0x03:
					return readCString(r);
				case 0x04:
					//Console.WriteLine("needed 0x04 thing..");
					//System.Environment.Exit(1);
					//check every other byte (starting with the following one) for int32 FF and then stop. But then progress two more bytes after that.
				{
					Console.WriteLine("0x04 is at 0x{0:X}",r.BaseStream.Position-1);
					byte _ffStreak=0;
					while(true){
						byte _cur = r.ReadByte();
						if (_cur==0xFF){
							if (++_ffStreak==4){
								r.BaseStream.Seek(2,SeekOrigin.Current);
								break;
							}
						}else{
							_ffStreak=0;
						}
					}
				}
				break;
				default:
					r.ReadByte();
					break;
			}
			return null;
		}
		static bool commandArrOpen=false;
		static void startCommand(StreamWriter w, short _id){
			w.Write("luastring c(0x{0:X}",_id);
		}
		static void endCommand(StreamWriter w){
			if (commandArrOpen){
				w.Write("}");
				commandArrOpen=false;
			}
			w.WriteLine(")");
		}
		static void appendArg(StreamWriter w, Object _addThis){
			if (commandArrOpen){
				w.Write(", ");
			}else{
				w.Write(", {");
				commandArrOpen=true;
			}
			if (_addThis is string){
				w.Write("{0}",MakeStringArgument((string)_addThis));
			}else if (_addThis==null){
				w.Write("nil");
			}else{
				w.Write("0x{0:X}",_addThis);
			}
		}
		public static bool ConvertScript(string _sourceFile, string _destFile, string _nextScriptFilename){
			BinaryReader reader = new BinaryReader(File.OpenRead(_sourceFile));
			byte[] _possibleMagicBytes = reader.ReadBytes(14);
			if (System.Text.Encoding.ASCII.GetString(_possibleMagicBytes)!="MSCENARIO FILE"){
				Console.WriteLine("Corrupted or encrypted.");
				return false;
			}
			StreamWriter w = new StreamWriter(File.OpenWrite(_destFile));
			// find the start of the title command. it's marked with 0xED 0x03
			byte _lastByte=0;
			while(true){
				byte _curByte = reader.ReadByte();
				if (_curByte==0x03 && _lastByte==0xED){
					break;
				}
				_lastByte=_curByte;
			}
			reader.BaseStream.Seek(-2,SeekOrigin.Current); // go back before the command ID
			// read the rest!
			while(true){
				short _id;
				try{
					_id = reader.ReadInt16();
				}catch(EndOfStreamException){
					break;
				}
				startCommand(w,_id);
				short _numArgBytes = reader.ReadInt16();
				long _startPos = reader.BaseStream.Position;
				long _destPos=_startPos+_numArgBytes;			
				while(reader.BaseStream.Position<_destPos){
					Object _lastRead = readArg(reader,reader.ReadByte());
					appendArg(w,_lastRead);
				}
				endCommand(w);
				if (reader.BaseStream.Position!=_destPos){
					Console.Error.WriteLine("read too much. started at 0x{2:X} expected end at 0x{1:X} after reading 0x{3:X} bytes. actually ended at 0x{0:X}",reader.BaseStream.Position,_destPos,_startPos,_numArgBytes);
					System.Environment.Exit(1);
				}
			}
			reader.Dispose();
			if (_nextScriptFilename!=null){
				w.WriteLine("jump "+_nextScriptFilename);
			}
			w.Dispose();
			return true;
		}
	}
}
