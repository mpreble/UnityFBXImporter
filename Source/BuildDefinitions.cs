using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildDefinitions : MonoBehaviour {
	
	public static List<GameObject> modelDefinitions;
	
	public static List<Texture2D> textures;
	public static List<Material> mats;
	
	
	// Use this for initialization
	void Start () {
		
		//in case not initialized
		if(modelDefinitions == null)
		{
			modelDefinitions = new List<GameObject>();
		} // if
		
		if(textures == null)
		{
			textures = new List<Texture2D>();
		} // if
		
		if(mats == null)
		{
			mats = new List<Material>();
		} // if
	}
	
	public static GameObject FindMesh(string name)
	{
		for(int i = 0; i<modelDefinitions.Count; i++)
		{
			if(modelDefinitions[i].name == name)
			{
				return (GameObject)modelDefinitions[i];
			}
		}
		return null;
	}
	
	public static void AddMesh(GameObject go)
	{
		//in case not initialized
		if(modelDefinitions == null)
		{
			modelDefinitions = new List<GameObject>();
		} // if
		
		modelDefinitions.Add(go);
	} // AddMesh()
	
}