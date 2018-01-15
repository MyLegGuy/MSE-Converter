/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.IO;

namespace Petals{
	public static class GraphicsConverter{
		
		// Byte with index 3 gets shifted to index 2
		// Byte at the end is not shifted
		static void shiftArrayLeft(ref byte[] _arrayToShift){
			for (int i=0;i!=_arrayToShift.Length-1;i++){
				_arrayToShift[i] = _arrayToShift[i+1];
			}
		}
		
		// Checks if byte array contains bytes that mean the start of a PNG file
		static bool didFindPNGStart(byte[] _passedPossibleStart){
			if (_passedPossibleStart[0]==0x89 && _passedPossibleStart[1]==0x50 && _passedPossibleStart[2]==0x4E && _passedPossibleStart[3]==0x47){
				return true;
			}
			return false;
		}
		// 4 bytes are actaully after this, this checks for IEND
		static bool didFindPNGEnd(byte[] _passedPossibleStart){
			if (_passedPossibleStart[0]==0x49 && _passedPossibleStart[1]==0x45 && _passedPossibleStart[2]==0x4E && _passedPossibleStart[3]==0x44){
				return true;
			}
			return false;
		}
		
		static void writePNGStart(FileStream _passedFilestream){
			_passedFilestream.WriteByte(0x89);
			_passedFilestream.WriteByte(0x50);
			_passedFilestream.WriteByte(0x4E);
			_passedFilestream.WriteByte(0x47);
		}
		
		// If this .MGD file has one PNG file in it only then this function will extract the PNG and delete the MGD
		static void fixMGDPNG(string _mgdFilename){
			byte[] _byteHistory;
			_byteHistory = new byte[4];
			FileStream _mgdFilestream = new FileStream(_mgdFilename,FileMode.Open);
			FileStream _pngFilestream;
			while (true){
				int _tempReadResult = _mgdFilestream.ReadByte();
				if (_tempReadResult==-1){ // If we reach the end of the file without finding it.
					Console.Out.WriteLine("Could not fix "+_mgdFilename);
					_mgdFilestream.Dispose();
					return;
				}
				shiftArrayLeft(ref _byteHistory);
				_byteHistory[3]=(byte)_tempReadResult;
				if (didFindPNGStart(_byteHistory)){
					break;
				}
			}
			_pngFilestream = new FileStream(Path.ChangeExtension(_mgdFilename,".png"),FileMode.Create);
			writePNGStart(_pngFilestream); // We already read the start of the PNG, so we need to write it to the new file
			while (true){
				int _tempReadResult = _mgdFilestream.ReadByte();
				if (_tempReadResult==-1){ // If we reach the end of the file without finding it.
					Console.Out.WriteLine("Broken MGD file "+_mgdFilename);
					_pngFilestream.Dispose();
					_mgdFilestream.Dispose();
					return;
				}
				shiftArrayLeft(ref _byteHistory);
				_byteHistory[3]=(byte)_tempReadResult;
				_pngFilestream.WriteByte(_byteHistory[3]);
				if (didFindPNGEnd(_byteHistory)){
					break;
				}
			}
			
			// Write the last 4 PNG bytes called crc32
			_pngFilestream.WriteByte(0xAE);
			_pngFilestream.WriteByte(0x42);
			_pngFilestream.WriteByte(0x60);
			_pngFilestream.WriteByte(0x82);
			
			_pngFilestream.Dispose();
			_mgdFilestream.Dispose();
			File.Delete(_mgdFilename);
		}
		
		public static void convertGraphics(string _passedExtractionDirectory, string _passedFinalDirectory){
			// ArcUnpacker extracts some images as MGD instead of PNG. Let's fix that first.
			string[] _imageFileList = Directory.GetFiles(_passedExtractionDirectory);
			int i;
			for (i=0;i<_imageFileList.Length;i++){
				Console.Out.WriteLine(_imageFileList[i]);
				if (Path.GetExtension(_imageFileList[i])==".MGD"){
					fixMGDPNG(_imageFileList[i]);
				}
			}
			// Update the listing because we removed some .MGD and added some .PNG
			_imageFileList = Directory.GetFiles(_passedExtractionDirectory);
		}
	}
}