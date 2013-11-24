using UnityEngine;
using System.Collections;
using System;

/*
 * As the name suggests, this class holds multiple
 * utility functions. For now, this is mainly for
 * file parsing, and especially for fbx parsing
 * 
 * MAP 7/19/13
 */

public class Utilities{
	
	/*
	 * Reads from start until reaching a character
	 * This is very generic in that it takes in any array of
	 * characters to read until (so in one instance, read until whitespace,
	 * in another, read until a comma or a semicolon, etc)
	 * 
	 * Start is meant to change as it's streaming through the file array
	 * That is, the entire txt file is read into an array. Start in this function
	 * is where you are in the file. So as you keep reading till, you need to keep
	 * changing where you are for when you leave the function.
	 */ 
	public static string ReadTill(string file, char[] until, ref int start)
	{
		int startHold;
		startHold = start;
		
		for(; start<file.Length; start++)
		{
			if(file[start]=='\0')
			{
				string ret = file.Substring(startHold, start-startHold+1);
				return ret;
			} // if
				
			for(int i = 0; i<until.Length; i++)
			{
				if(file[start] == until[i])
				{
					string ret = file.Substring(startHold,start-startHold);
					return ret;
				} // if
			} // for
		} // for
		
		//not the best way to handle EOF
		if(start == file.Length)
		{
			string ret = file.Substring(startHold, start-startHold);
			return ret;
		} // if
		
		return new string(new char[]{'f','a','i','l',' ','t','i','l','l'}); // required to compile
	} // ReadTill()
	
	
	public static string ReadSkip(string file, char[] skip, ref int start)
	{
		int startHold;
		startHold = start;
		
		for(; start<file.Length; start++)
		{
			if(file[start]=='\0')
			{
				string ret = file.Substring(startHold, start-startHold+1);
				return ret;
			} // if
				
			for(int i = 0; i<skip.Length; i++)
			{
				if(file[start] == skip[i])
				{
					break;
				} // if
				else if(file[start] != skip[i] && i<skip.Length-1)
				{
					continue;
				} // else if
				string ret = file.Substring(startHold, start-startHold);
				return ret;
				
			} // for
		} // for
		return new string(new char[]{'f','a','i','l',' ','s','k','i','p'}); //required to compile
	} // ReadSkip()

}
