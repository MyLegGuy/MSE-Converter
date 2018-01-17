/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 6:38 PM
 */
using System;
using System.IO;
namespace Petals
{
	/// <summary>
	/// Description of ExtentionMethods.
	/// </summary>
	public static class ExtentionMethods
	{
		public static void GoodWriteString(this BinaryWriter bw, string _stringToWrite){
			bw.Write(System.Text.Encoding.UTF8.GetBytes(_stringToWrite));
		}
		public static string MakeFilenameFriendly(this string filename){
		    return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
		}
		public static String GetPathWithoutExtention(this string path){
			return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
    	}
	}
}
