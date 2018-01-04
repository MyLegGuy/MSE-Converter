/*
 * Created by SharpDevelop.
 * User: good
 * Date: 12/31/2017
 * Time: 11:58 PM
 */
using System;
using System.Diagnostics;

namespace Petals
{
	/// <summary>
	/// Using arc_unpacker executable located at Options.arcUnpackerLocation to extract stuff.
	/// </summary>
	public static class ArcUnpacker{
		public static void unpackToDirectory(string _filename){
			unpackToDirectory(_filename,null);
		}
		public static void unpackToDirectory(string _filename, string _extractDirectory){
			//D:\Programming\C#\PetalsVitaConverter\bin\Debug\arc_unpacker.exe -o=./happy -d=nsystem/fjsys ./MSE
			ProcessStartInfo startInfo = new ProcessStartInfo();        
			startInfo.FileName = System.IO.Directory.GetCurrentDirectory()+"/arc_unpacker.exe";
			if (_extractDirectory!=null){
				startInfo.Arguments = "-o="+_extractDirectory+" -d=nsystem/fjsys \""+_filename+"\"";
			}else{
				startInfo.Arguments = "-d=nsystem/fjsys \""+_filename+"\"";
			}
			Process.Start(startInfo);
		}
	}
}
