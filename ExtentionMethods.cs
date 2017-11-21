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
			bw.Write(System.Text.Encoding.ASCII.GetBytes(_stringToWrite));
		}
	}
}
