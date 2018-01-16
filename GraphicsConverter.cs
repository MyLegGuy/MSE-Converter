/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 5:30 PM
 */
using System;
using System.IO;
using System.Drawing;

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
		
		static Bitmap goodResizeImage(Bitmap _sourceImage, Size _newSize){
			/*Bitmap _resultBitmap = new Bitmap(_newSize.Width,_newSize.Height);
			using (Graphics goodGraphics = Graphics.FromImage(_resultBitmap)){
				goodGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				goodGraphics.DrawImage(_sourceImage,0,0,_newSize.Width,_newSize.Height);
			}
			return _resultBitmap;*/
			return new Bitmap(_sourceImage,_newSize.Width,_newSize.Height);
			
		}
		
		// Dividing by a smaller number gives a bigger result than dividing by a bigger a number
		// Whichever one needs to stretch less is the one we stretch to
		static bool FitToWidth(int _imgWidth, int _imgHeight, int _screenWidth, int _screenHeight){
			double _tempWidthResult = (double)_imgWidth/_screenWidth;
			double _tempHeightResult = (double)_imgHeight/_screenHeight;
			if (_tempWidthResult>_tempHeightResult){
				return true;
			}
			return false;
		}
		
		static double ImageToScreenRatio(int _imgSize, int _screenSize){
			return _imgSize/(double)_screenSize;
		}
		
		static int SizeScaledOutput(int _original, double _scaleFactor){
			return (int)Math.Floor(_original/(double)_scaleFactor);
		}
		
		public static void convertGraphics(string _passedExtractionDirectory, string _passedFinalDirectory, int screenWidth, int screenHeight){
			// ArcUnpacker extracts some images as MGD instead of PNG. Let's fix that first.
			string[] _imageFileList = Directory.GetFiles(_passedExtractionDirectory);
			int i;
			for (i=0;i<_imageFileList.Length;i++){
				if (Path.GetExtension(_imageFileList[i])==".MGD"){
					fixMGDPNG(_imageFileList[i]);
				}
			}
			// Update the listing because we removed some .MGD and added some .PNG
			_imageFileList = Directory.GetFiles(_passedExtractionDirectory);
			
			// If images are going to fit to the screen's width, or the screen's height
			bool doFitToWidth = FitToWidth(800,600,screenWidth,screenHeight);
			for (i=0;i<_imageFileList.Length;i++){
				Bitmap currentFile = new Bitmap(_imageFileList[i]);
				Console.Out.WriteLine("({0}x{1}) Image: {2}",currentFile.Width,currentFile.Height,_imageFileList[i]);
				double _tempRatio;
				if (doFitToWidth==true){
					_tempRatio = ImageToScreenRatio(currentFile.Width,screenWidth);
				}else{
					_tempRatio = ImageToScreenRatio(currentFile.Height,screenHeight);
				}
					
				Bitmap newScaledBitmap =  goodResizeImage(currentFile, new Size(SizeScaledOutput(currentFile.Width,_tempRatio), SizeScaledOutput(currentFile.Height,_tempRatio)));
					
				newScaledBitmap.Save(_passedFinalDirectory+Path.GetFileName(_imageFileList[i]));
				newScaledBitmap.Dispose();
				
				currentFile.Dispose();
			}
			
			
		}
	}
}