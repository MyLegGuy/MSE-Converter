/*
 * Created by SharpDevelop.
 * User: good
 * Date: 12/31/2017
 * Time: 11:58 PM
 */
using System;
using System.Diagnostics;
using System.IO;

namespace Petals
{
	/// <summary>
	/// Using arc_unpacker executable located at Options.arcUnpackerLocation to extract stuff.
	/// </summary>
	public static class ArcUnpacker{
		public static string _exeLocation = null;
		public static void unpackToDirectory(string _filename){
			unpackToDirectory(_filename,null);
		}
		public static void unpackToDirectory(string _filename, string _extractDirectory){
			if (_exeLocation==null){
				if (File.Exists(System.IO.Directory.GetCurrentDirectory()+"/arc_unpacker.exe")){
					_exeLocation = System.IO.Directory.GetCurrentDirectory()+"/arc_unpacker.exe";
				}else if (File.Exists(System.IO.Directory.GetCurrentDirectory()+"/arc_unpacker")){
					_exeLocation = System.IO.Directory.GetCurrentDirectory()+"/arc_unpacker";
				}else{
					Console.Out.WriteLine("arc_unpacker not found in "+System.IO.Directory.GetCurrentDirectory());
					Environment.Exit(1);
				}
			}

			//D:\Programming\C#\PetalsVitaConverter\bin\Debug\arc_unpacker.exe -o=./happy -d=nsystem/fjsys ./MSE
			ProcessStartInfo startInfo = new ProcessStartInfo();        
			startInfo.FileName = _exeLocation;
			if (_extractDirectory!=null){
				startInfo.Arguments = "-o="+_extractDirectory+" -d=nsystem/fjsys \""+_filename+"\"";
			}else{
				startInfo.Arguments = "-d=nsystem/fjsys \""+_filename+"\"";
			}
			Process _myProcess = Process.Start(startInfo);
			_myProcess.WaitForExit();
		}
	}
}
