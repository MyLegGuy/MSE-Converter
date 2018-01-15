﻿/*
 * User: knoob
 * Date: 11/20/2017
 * Time: 6:49 PM
 */
using System;

namespace Petals
{
	/// <summary>
	/// Description of Options.
	/// </summary>
	public static class Options
	{
		public const string streamingAssetsFolder = "./othergame/";
		public const string arcUnpackerLocation = "./arc_unpacker.exe";
		
		// Look in Program.cs for their values. They are what I would expect and relative to streamingAssetsFolder
		// They also end in a slash
		// These are the locations of the files after they've been extracted, processed, and moved.
		public static string finalSELocation;
		public static string finalVoiceLocation;
		public static string finalImagesLocation;
		public static string finalBGMLocation;
		public static string finalScriptsLocation;
		
		public static string extractedSELocation;
		public static string extractedVoiceLocation;
		public static string extractedImagesLocation;
		public static string extractedBGMLocation;
		public static string extractedScriptsLocation;
	}
}
