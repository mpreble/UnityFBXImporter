using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public class FBXImporter : MonoBehaviour{
	
	//File Parsing variables
	// \r is like a new line apparently
	char[] white = new char[]{' ', '\n', '\t', '\r'};
	char[] newLn = new char[]{'\n', '\r'};
	char[] quote = new char[]{'\"'};
	char[] comma = new char[]{','};
	char[] mlArr = new char[]{'\t','}'};
	
	char[] line;
	string fileCompl;
	string currentWord;
	int depth;
	int iterator;
	
	//arrays to link names and meshes
	List<ClassImpMesh> impMeshes;
	static List<ClassImportedModel> impModels;
	
	//Base GameObject. Name of file
	GameObject baseGO;
	
	//finished meshes go into the model
	//definitions array
	BuildDefinitions defs;
	
	//working with file reading
	FileInfo sourceFile;
	StreamReader reader;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	/*this function imports every file that
	ends in .fbx in the Models folder and
	every sub folder, and stores them in game
	as deactivated game objects & children
	
	The file name is used as a base object.
	Each mesh is then its own game object, parented
	to the base. This is especially important for
	things like a turret which needs to turn to face
	an enemy independant of the chassis
	
	
	IMPORTANT: This is draft 1. I haven't looked into importing
	hierarchies, which will be necessary, such as the gun barrel
	parented to the turret, parented to the chassis.
	MAP
	7/3/13
	*/
	public void ImportAllFBX()
	{
		string[] FileNames = Directory.GetFiles(Application.dataPath + "/Models", "*.fbx");
		impModels = new List<ClassImportedModel>();
		
		if(FileNames == null)
		{
			Debug.Log("Didn't find any .fbx files");
			return;
		} // if
		foreach(string s in FileNames)
		{
			ParseFBX(s);
		} // foreach
		
		return;
	} // ImportAllFBX()
	
	/*
	 * Opens the file at path and parses through it
	 * Gathering all of the information from the fbx ASCII file
	 * (vert points, UVs, normals) and storing them in Unity game
	 * objects (one for each mesh)
	 */
	void ParseFBX(string path)
	{
		sourceFile = new FileInfo(path); //Application.dataPath + "/Models/" + 
		if(sourceFile != null && sourceFile.Exists)
			reader = new StreamReader(path);
		
		if(reader == null)
		{
			Debug.Log(sourceFile + " not found");
		} // if
		else
		{
			string nameExt = sourceFile.Name;
			string nameShort = nameExt.Substring(0,nameExt.Length-4); //name without extension
			
			baseGO = new GameObject(nameShort);
			impMeshes = new List<ClassImpMesh>();
			depth = 0;
			iterator = 0;
			
			//read entire file into char array
			//reader.BaseStream.Seek(0,SeekOrigin.Begin);
			reader.BaseStream.Position = 0;
			reader.DiscardBufferedData();
			//reader.BaseStream.Seek(0,SeekOrigin.End);
			fileCompl = reader.ReadToEnd();
			//fileCompl = new char[(int)reader.BaseStream.Position];
			//Debug.Log(reader.BaseStream.Position + " :reader pos;fileCompl size: " + fileCompl.Length);
			//reader.Read(fileCompl, 0, 12367);
			reader.Close();
			/*
			 * Skips until white or end of line. If you see a comment, jump to next line
			 * skip things in quotes (if first char is quote, skip until next quote)
			 */ 
			#region Autodesk Importing
			//Autodesk fbx files (most recently, fbx 7.3.0 MAP 8/4/13
			if(fileCompl.Contains("Autodesk"))
			{
				while(iterator<fileCompl.Length)
				{
					Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
					currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
					
					if(iterator >= fileCompl.Length)
					{
						FinalizeAsset();
						return;
					} // if
			
					
					if(currentWord[0] == ';')
					{
						//comment line
						//goes to the next line and restarts the loop
						Utilities.ReadTill(fileCompl, newLn, ref iterator);
						Debug.Log(@fileCompl[iterator]);
						continue;
					} // if
					
					if(currentWord[0] == '\"')
					{
						//handle quoted information. For now this just include the model name
						//in syntax "Model::NAME"
						continue;
					} // if
					
					if(currentWord[0] == '{')
					{
						depth++;
						continue;
					} // if
					
					if(currentWord[0] == '}')
					{
						depth--;
						continue;
					} // if
					
					switch(currentWord.ToString())
					{
					case "FBXHeaderExtension:":
						SkipSection();
						break;
					case "GlobalSettings:":
						SkipSection();
						break;
					case "Documents:":
						SkipSection();
						break;
					case "References:":
						SkipSection();
						break;
					case "Definitions:":
						SkipSection();
						break;
					case "Takes:":
						SkipSection();
						break;
						
					case "Objects:":
						ObjectBuilding();
						break;
						
					case "Connections:":
						Connections();
						break;
						
					default:
						break;
					} //switch
				} //while(true)
			}
			#endregion
			
			#region Blender Importing
			//Blender fbx files (most recently, fbx 6.1.0 MAP 8/4/13
			else if(fileCompl.Contains("Blender"))
			{
				while(iterator<fileCompl.Length)
				{
					Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
					currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
					
					if(iterator >= fileCompl.Length)
					{
						FinalizeAsset();
						return;
					} // if
			
					
					if(currentWord[0] == ';')
					{
						//comment line
						//goes to the next line and restarts the loop
						Utilities.ReadTill(fileCompl, newLn, ref iterator);
						Debug.Log(@fileCompl[iterator]);
						continue;
					} // if
					
					if(currentWord[0] == '\"')
					{
						//handle quoted information. For now this just include the model name
						//in syntax "Model::NAME"
						continue;
					} // if
					
					if(currentWord[0] == '{')
					{
						depth++;
						continue;
					} // if
					
					if(currentWord[0] == '}')
					{
						depth--;
						continue;
					} // if
					
					switch(currentWord.ToString())
					{
					case "FBXHeaderExtension:":
						SkipSection();
						break;
					case "GlobalSettings:":
						SkipSection();
						break;
					case "Documents:":
						SkipSection();
						break;
					case "References:":
						SkipSection();
						break;
					case "Definitions:":
						SkipSection();
						break;
					case "Takes:":
						SkipSection();
						break;
						
					case "Objects:":
						BlenderObjectBuilding();
						break;
						
					case "Connections:":
						BlenderConnections();
						break;
						
					default:
						break;
					} //switch
				} //while(true)
			} // if
			#endregion
			return;
		}	
	} // ParseFBX
	
	void SkipSection()
	{
		int dStart = depth;
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
					
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth==dStart)
				{
					return;
				} // if
				else if(depth < dStart)
				{
					Debug.Log("Um...we've more than skipped a section (FBXImporter.cs SkipSection())");
				} // else if
				continue;
			} // if
			
		} // while(true)
	} // SkipSection()
	
	#region Autodesk Functions
	
	void ObjectBuilding()
	{
		ClassImportedModel impMesh = new ClassImportedModel(); //this'll be thrown away, but HAS to exist to compile
		int depthSt = depth;
		
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				Debug.Log("hi");
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), "\"Model::\\S+\""))
				{
					//meshNames.Add(currentWord.ToString().Substring(8,currentWord.Length-10));
					//Doing this arbitrarily. The next mesh name should come right after creating an impMesh
					//which means it's for the last member of the impMeshes list
					impMeshes[impMeshes.Count-1].SetName(currentWord.ToString().Substring(8,currentWord.Length-10));
					impMeshes[impMeshes.Count-1].SetOrig(ImpRotPoint());
				} // if
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth==depthSt)
				{
					impModels.Add(impMesh);//add last mesh
					return; // done with Objects: section
				} // if
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "Geometry:":
				if(impMesh.id != 0)
				{
					impModels.Add(impMesh);
				} // if
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				impMesh = new ClassImportedModel();
				impMesh.id = Convert.ToInt32(Utilities.ReadTill(fileCompl, comma, ref iterator).ToString()); //next word should be FBXID, with comma at end
				iterator++;//get off the comma character
				break;
				
			case "Vertices:":
				impMesh.SetVerts(ImpVerts());
				break;
				
			case "PolygonVertexIndex:":
				impMesh.SetPolyVerts(ImpPolyVerts()); //poly-verts needs the size of the vert array because of how Unity does norms/uvs
				break;
				
			case "Model:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				impMeshes.Add(new ClassImpMesh(Convert.ToInt32(Utilities.ReadTill(fileCompl, comma, ref iterator).ToString()))); // next word should be modelID, with comma at end
				iterator++;//get off the comma character
				break;
				
			case "LayerElementNormal:":
				impMesh.SetNorms(ImpNorms());
				break;
				
			case "UV:":
				impMesh.SetUVs(ImpUVPoints());
				break;
				
			case "UVIndex:":
				impMesh.SetUVIndex(ImpUVIndex());
				break;
				
			default:
				break;
			} // switch
		} //while(true)
	} // ObjectBuilding()
	
	Vector3[] ImpVerts()
	{
		float[] impFl;
		string[] impVals;
		string[] multiArr;
		string multiArrSt = "";
		Vector3[] verts;
		int i;
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				depth--;
				continue;
			} // if
			
			if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), @"\*\d+"))
			{
				impFl = new float[Convert.ToInt32(currentWord.ToString().Substring(1))];
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "a:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, mlArr, ref iterator); // this is the set of values, separated by commas
				multiArr = currentWord.ToString().Split(newLn);
				multiArrSt = String.Concat(multiArr);
				
				impVals = multiArrSt.Split(comma);
				impFl = new float[impVals.Length];
				
				if(impVals.Length != impFl.Length)
					Debug.Log("Float array and split string array not equal (FBXImporter ImpVerts())");
				
				for(i=0;i<impVals.Length; i++)
				{
					impFl[i] = Convert.ToSingle(impVals[i]);
				} // for
				
				verts = new Vector3[impFl.Length/3];
				
				for(i=0;i<verts.Length;i++)
				{
					verts[i] = new Vector3(impFl[3*i], impFl[3*i+1], impFl[3*i+2]);
				} // for
				
				return verts;
				//break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpVerts()
	
	int[] ImpPolyVerts()
	{
		int[] impIn;
		string[] impVals;
		int[] polyVerts;
		string[] multiArr;
		string multiArrSt;
		int i;
		
		
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				depth--;
				continue;
			} // if
			
			if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), @"\*\d+"))
			{
				impIn = new int[Convert.ToInt32(currentWord.ToString().Substring(1))];
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "a:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, mlArr, ref iterator); // this is the set of values, separated by commas
				multiArr = currentWord.ToString().Split(newLn);
				multiArrSt = String.Concat(multiArr);
				
				impVals = multiArrSt.Split(comma);
				impIn = new int[impVals.Length];
				
				if(impVals.Length != impIn.Length)
					Debug.Log("Int array and split string array not equal (FBXImporter ImpPolyVerts())");
				
				for(i=0;i<impVals.Length; i++)
				{
					impIn[i] = Convert.ToInt32(impVals[i]);
				} // for
				
				polyVerts = new int[impIn.Length];
				
				for(i=0;i<polyVerts.Length;i++)
				{
					if(impIn[i]<0)
					{
						polyVerts[i] = -1*(impIn[i]+1);
						continue;
					} // if
					polyVerts[i] = impIn[i];
				} // for
				return polyVerts;
				//break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpPolyVerts()
	
	Vector3[] ImpNorms()
	{
		float[] impFl;
		string[] impVals;
		string[] multiArr;
		string multiArrSt = "";
		Vector3[] norms;
		int i;
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				depth--;
				continue;
			} // if
			
			if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), @"\*\d+"))
			{
				impFl = new float[Convert.ToInt32(currentWord.ToString().Substring(1))];
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "a:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, mlArr, ref iterator); // this is the set of values, separated by commas
				multiArr = currentWord.ToString().Split(newLn);
				multiArrSt = String.Concat(multiArr);
				
				impVals = multiArrSt.Split(comma);
				impFl = new float[impVals.Length];
				
				if(impVals.Length != impFl.Length)
					Debug.Log("Float array and split string array not equal (FBXImporter ImpVerts())");
				
				for(i=0;i<impVals.Length; i++)
				{
					impFl[i] = Convert.ToSingle(impVals[i]);
				} // for
				
				norms = new Vector3[impFl.Length/3];
				
				for(i=0;i<norms.Length;i++)
				{
					norms[i] = new Vector3(impFl[3*i], impFl[3*i+1], impFl[3*i+2]);
				} // for
				
				return norms;
				//break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpNorms()
	
	Vector2[] ImpUVPoints()
	{
		float[] impFl;
		string[] impVals;
		string[] multiArr;
		string multiArrSt = "";
		Vector2[] uvs;
		int i;
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				depth--;
				continue;
			} // if
			
			if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), @"\*\d+"))
			{
				impFl = new float[Convert.ToInt32(currentWord.ToString().Substring(1))];
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "a:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, mlArr, ref iterator); // this is the set of values, separated by commas
				multiArr = currentWord.ToString().Split(newLn);
				multiArrSt = String.Concat(multiArr);
				
				impVals = multiArrSt.Split(comma);
				impFl = new float[impVals.Length];
				
				if(impVals.Length != impFl.Length)
					Debug.Log("Float array and split string array not equal (FBXImporter ImpVerts())");
				
				for(i=0;i<impVals.Length; i++)
				{
					impFl[i] = Convert.ToSingle(impVals[i]);
				} // for
				
				uvs = new Vector2[impFl.Length/2];
				
				for(i=0;i<uvs.Length;i++)
				{
					uvs[i] = new Vector3(impFl[2*i], impFl[2*i+1]);
				} // for
				
				return uvs;
				//break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpUVPoints()
	
	int[] ImpUVIndex()
	{
		int[] impIn;
		string[] impVals;
		int[] polyVerts;
		string[] multiArr;
		string multiArrSt;
		int i;
		
		
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				depth--;
				continue;
			} // if
			
			if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), @"\*\d+"))
			{
				impIn = new int[Convert.ToInt32(currentWord.ToString().Substring(1))];
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "a:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, mlArr, ref iterator); // this is the set of values, separated by commas
				multiArr = currentWord.ToString().Split(newLn);
				multiArrSt = String.Concat(multiArr);
				
				impVals = multiArrSt.Split(comma);
				impIn = new int[impVals.Length];
				
				if(impVals.Length != impIn.Length)
					Debug.Log("Int array and split string array not equal (FBXImporter ImpPolyVerts())");
				
				for(i=0;i<impVals.Length; i++)
				{
					impIn[i] = Convert.ToInt32(impVals[i]);
				} // for
				
				return impIn;
				//break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpUVIndex()
	
	
	Vector3 ImpRotPoint()
	{
		int depthStart = depth;
		string[] split;
		int i;
		
		
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth <= depthStart)
					return new Vector3(0,0,0);
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "P:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, newLn, ref iterator); // read to the end of line, separate by commas
				split = currentWord.ToString().Split(comma);
				
				//Here I'm assuming the last 3 strings in the split are
				//the x, y, and z coordinates of the center point
				if(split[0].Equals("\"RotationPivot\""))
				{
					int len = split.Length;
					Vector3 temp = new Vector3(Convert.ToSingle(split[len-3]),
												Convert.ToSingle(split[len-2]),
												Convert.ToSingle(split[len-1]));
					return temp;
				} // if
				break;
				
			default:
				break;
			} // switch
		}//while(true)
	} //ImpRotPoint()
	
	void Connections()
	{
		int depthSt = depth;
		string[] splitLabels;
		int i,j,k;
		bool ct;
		Vector3[] newVerts;

		while(true)
		{
			ct = true;
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth==depthSt)
				{
					return; // done with Objects: section
				} // if
				continue;
			} // if
			
			
			switch(currentWord.ToString())
			{
			case "C:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
				splitLabels = currentWord.ToString().Split(new char[]{','});
				
				//Connecting Mesh info to geometry
				for(i=0;i<impModels.Count && ct;i++)
				{
					if(impModels[i].id == Convert.ToInt32(splitLabels[1]))
					{
						for(j=0;j<impMeshes.Count;j++)
						{
							if(impMeshes[j].GetID() == Convert.ToInt32(splitLabels[2]))
							{
								impModels[i].name = impMeshes[j].GetName();
								
								//Handles origin being at different place by moving every vert
								if(impMeshes[j].GetOrig() != new Vector3(0f,0f,0f))
								{
									newVerts = impModels[i].GetVerts();
									for(k=0;k<newVerts.Length;k++)
									{
										newVerts[k] -= impMeshes[j].GetOrig();
									} // for
									impModels[i].SetVerts(newVerts);
								}
								ct = false;
								break;
							}
						} // for
					} // if
				} // for
				
				//Connecting Meshes to other meshes (parenting)
				//This just finds info. Doesn't actually parent
				ct = true;
				for(i=0;i<impMeshes.Count && ct;i++)
				{
					if(impMeshes[i].GetID() == Convert.ToInt32(splitLabels[1]))
					{
						for(j=0;j<impMeshes.Count;j++)
						{
							if(impMeshes[j].GetID() == Convert.ToInt32(splitLabels[2]))
							{
								impMeshes[i].SetPID(impMeshes[j].GetID());
								ct = false;
								break;
							}
						} // for
					} // if
				} // for
				break;
				
			default:
				break;
			} // switch
		} // while(true)
		
	} // Connections()
	
	#endregion
	
	#region Blender Functions
	void BlenderObjectBuilding()
	{
		ClassImportedModel impMesh = new ClassImportedModel(); //this'll be thrown away, but HAS to exist to compile
		int depthSt = depth;
		
		while(true)
		{
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '\"')
			{
				//handle quoted information. For now this just include the model name
				//in syntax "Model::NAME"
				if(System.Text.RegularExpressions.Regex.IsMatch(currentWord.ToString(), "\"Model::\\S+\""))
				{
					//meshNames.Add(currentWord.ToString().Substring(8,currentWord.Length-10));
					//Doing this arbitrarily. The next mesh name should come right after creating an impMesh
					//which means it's for the last member of the impMeshes list
					impMeshes[impMeshes.Count-1].SetName(currentWord.ToString().Substring(8,currentWord.Length-10));
					impMeshes[impMeshes.Count-1].SetOrig(ImpRotPoint());
				} // if
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth==depthSt)
				{
					impModels.Add(impMesh);//add last mesh
					return; // done with Objects: section
				} // if
				continue;
			} // if
			
			switch(currentWord.ToString())
			{
			case "Geometry:":
				if(impMesh.id != 0)
				{
					impModels.Add(impMesh);
				} // if
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				impMesh = new ClassImportedModel();
				impMesh.id = Convert.ToInt32(Utilities.ReadTill(fileCompl, comma, ref iterator).ToString()); //next word should be FBXID, with comma at end
				iterator++;//get off the comma character
				break;
				
			case "Vertices:":
				impMesh.SetVerts(ImpVerts());
				break;
				
			case "PolygonVertexIndex:":
				impMesh.SetPolyVerts(ImpPolyVerts()); //poly-verts needs the size of the vert array because of how Unity does norms/uvs
				break;
				
			case "Model:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				impMeshes.Add(new ClassImpMesh(Convert.ToInt32(Utilities.ReadTill(fileCompl, comma, ref iterator).ToString()))); // next word should be modelID, with comma at end
				iterator++;//get off the comma character
				break;
				
			case "LayerElementNormal:":
				impMesh.SetNorms(ImpNorms());
				break;
				
			case "UV:":
				impMesh.SetUVs(ImpUVPoints());
				break;
				
			case "UVIndex:":
				impMesh.SetUVIndex(ImpUVIndex());
				break;
				
			default:
				break;
			} // switch
		} //while(true)
	} // BlenderObjectBuilding()
	
	
	void BlenderConnections()
	{
		int depthSt = depth;
		string[] splitLabels;
		int i,j,k;
		bool ct;
		Vector3[] newVerts;

		while(true)
		{
			ct = true;
			Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
			currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
			
			if(currentWord[0] == ';')
			{
				//comment line
				//goes to the next line and restarts the loop
				Utilities.ReadTill(fileCompl, newLn, ref iterator);
				continue;
			} // if
			
			if(currentWord[0] == '{')
			{
				depth++;
				continue;
			} // if
			
			if(currentWord[0] == '}')
			{
				if(--depth==depthSt)
				{
					return; // done with Objects: section
				} // if
				continue;
			} // if
			
			
			switch(currentWord.ToString())
			{
			case "C:":
				Utilities.ReadSkip(fileCompl, white, ref iterator); //puts the iterator on the start of the next word
				currentWord = Utilities.ReadTill(fileCompl, white, ref iterator);
				splitLabels = currentWord.ToString().Split(new char[]{','});
				
				//Connecting Mesh info to geometry
				for(i=0;i<impModels.Count && ct;i++)
				{
					if(impModels[i].id == Convert.ToInt32(splitLabels[1]))
					{
						for(j=0;j<impMeshes.Count;j++)
						{
							if(impMeshes[j].GetID() == Convert.ToInt32(splitLabels[2]))
							{
								impModels[i].name = impMeshes[j].GetName();
								
								//Handles origin being at different place by moving every vert
								if(impMeshes[j].GetOrig() != new Vector3(0f,0f,0f))
								{
									newVerts = impModels[i].GetVerts();
									for(k=0;k<newVerts.Length;k++)
									{
										newVerts[k] -= impMeshes[j].GetOrig();
									} // for
									impModels[i].SetVerts(newVerts);
								}
								ct = false;
								break;
							}
						} // for
					} // if
				} // for
				
				//Connecting Meshes to other meshes (parenting)
				//This just finds info. Doesn't actually parent
				ct = true;
				for(i=0;i<impMeshes.Count && ct;i++)
				{
					if(impMeshes[i].GetID() == Convert.ToInt32(splitLabels[1]))
					{
						for(j=0;j<impMeshes.Count;j++)
						{
							if(impMeshes[j].GetID() == Convert.ToInt32(splitLabels[2]))
							{
								impMeshes[i].SetPID(impMeshes[j].GetID());
								ct = false;
								break;
							}
						} // for
					} // if
				} // for
				break;
				
			default:
				break;
			} // switch
		} // while(true)
		
	} // BlenderConnections()
	#endregion
	
	void FinalizeAsset()
	{
		List<GameObject> needsParent = new List<GameObject>();
		GameObject temp;
		MeshFilter mf;
		int i,j,k, tempid, pId;
		for(i=0;i<impModels.Count;i++)
		{
			temp = new GameObject(impModels[i].name);
			temp.AddComponent<MeshRenderer>();
			mf = temp.AddComponent<MeshFilter>();
			
			UnifyVerts(impModels[i]);
			
			mf.mesh.vertices = impModels[i].GetVerts();
			mf.mesh.triangles = impModels[i].GetPolyVerts();
			mf.mesh.normals = impModels[i].GetNorms();
			mf.mesh.uv = impModels[i].GetUVs();
			
			temp.active = false;
			needsParent.Add(temp);
		} // for
		
		//parent objects as necessary
		//also handles moved origin for convenience
		for(i=0;i<needsParent.Count;i++)
		{
			tempid = FindMeshID(impModels[i].name);
			
			if(impMeshes[tempid].GetPID() == 0)
			{
				needsParent[i].transform.parent = baseGO.transform;
				continue;
			} // if
			//parenting
			for(j=0;j<impMeshes.Count;j++)
			{
				if(impMeshes[j].GetID()==impMeshes[tempid].GetPID())
				{
					for(k=0;k<impModels.Count;k++)
					{
						if(impModels[k].name.Equals(impMeshes[j].GetName()))
						{
							needsParent[i].transform.parent = needsParent[k].transform;
							break;
						} // if
					} // for
					break;
				} // if
			} // for
			
			//origin moving
			for(j=0;j<impMeshes.Count;j++)
			{
				if(needsParent[i].name.Equals(impMeshes[j].GetName()))
				{
					if(impMeshes[j].GetOrig()!= new Vector3(0f,0f,0f))
					{
						needsParent[i].transform.position+=impMeshes[j].GetOrig();
					} // if
				} // if
			} // for
			
		} // for
		
		
		baseGO.active = false;
		BuildDefinitions.AddMesh(baseGO);
		impModels.Clear();
		impMeshes.Clear();
		needsParent.Clear();
		
		
	} // FinalizeAsset()
	
	#region Util Functions
	void UnifyVerts(ClassImportedModel impModel)
	{
		int i,j;
		int[] polyVerts = impModel.GetPolyVerts();
		int[] uvIndex = impModel.GetUVIndex();
		Vector3[] verts = impModel.GetVerts();
		Vector3[] norms = impModel.GetNorms();
		Vector2[] uvs = impModel.GetUVs();
		List<VertexClass> vList = new List<VertexClass>();
		VertexClass vc;
		
		
		for(i=0;i<polyVerts.Length;i++)
		{
			vc = new VertexClass();
			vc.pos = verts[polyVerts[i]];
			vc.uv = uvs[uvIndex[i]];
			
			if(norms.Length == verts.Length)
			{
				vc.norm = norms[polyVerts[i]];
			} // if
			else
			{
				vc.norm = norms[i]; //HERE
			}
			
			if(vList.Count<1)
			{
				polyVerts[i] = i;
				vList.Add(vc);
				continue;
			} // if
			
			for(j = 0; j<vList.Count; j++)
			{
				if(vList[j].pos == vc.pos
					&& vList[j].uv == vc.uv
					&& vList[j].norm == vc.norm)
				{
					polyVerts[i] = j;
					break;
				}
				
				if(j==vList.Count-1)
				{
					polyVerts[i] = i;
					vList.Add(vc);
				} //if
			} // for
		} // for
		
		verts = new Vector3[vList.Count];
		norms = new Vector3[vList.Count];
		uvs = new Vector2[vList.Count];
		
		for(i=0;i<verts.Length;i++)
		{
			verts[i] = vList[i].pos;
			norms[i] = vList[i].norm;
			uvs[i] = vList[i].uv;
		} // for
		
		impModel.SetVerts(verts);
		impModel.SetNorms(norms);
		impModel.SetUVs(uvs);
		impModel.SetPolyVerts(polyVerts);
		return;
	} // UnifyVerts()
	
	int FindMeshID(string name)
	{

		for(int i=0;i<impMeshes.Count;i++)
		{
			if(impMeshes[i].GetName().Equals(name))
				return i;
		} // for
		return -1;
	} // FindMeshID()
	
	
	#endregion
	
}
