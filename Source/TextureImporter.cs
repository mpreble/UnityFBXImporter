using UnityEngine;
using System.Collections;
using System.IO;

public class TextureImporter : MonoBehaviour {
	
	WWW text;
	Material m;
	
	// Use this for initialization
	public void ImportAllJPG() {
		
		string[] FileNames = Directory.GetFiles(Application.dataPath + "/Textures", "*.jpg");
		
		if(FileNames == null)
		{
			Debug.Log("Didn't find any .jpg files");
			return;
		} // if
		foreach(string s in FileNames)
		{
			FileInfo sourceFile = new FileInfo(s); //used to get name
			
			text = new WWW("file://" + s);
			while(!text.isDone)
			{
			}
			//yield return text;
			BuildDefinitions.textures.Add(text.texture);
			m = new Material(Shader.Find("CustomShaders/BasicShader")); // one mat per texture
			m.mainTexture = text.texture;
			
			string nameExt = sourceFile.Name;
			string nameShort = nameExt.Substring(0,nameExt.Length-4); //name without extension
			
			m.name = nameShort;
			BuildDefinitions.mats.Add(m);
		} // foreach
		
	}
}
