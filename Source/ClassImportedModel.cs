using UnityEngine;
using System.Collections;

public class ClassImportedModel {
	
	public int id;
	public string name;
	Vector3[] verts;
	Vector3[] norms;
	int[] polyVerts;
	int[] uvIndex;
	Vector2[] uvs;
	
	
	public void SetVerts(Vector3[] vertsIn)
	{
		verts = vertsIn;
	} // SetVerts()
	
	public void SetPolyVerts(int[] polyVertsIn)
	{
		polyVerts = polyVertsIn;
	} // SetPolyVerts()
	
	public void SetNorms(Vector3[] normsIn)
	{
		norms = normsIn;
	} // SetNorms()
	
	public void SetUVs(Vector2[] uvsIn)
	{
		uvs = uvsIn;
	} // SetUVs()
	
	public void SetUVIndex(int[] uvIn)
	{
		uvIndex = uvIn;
	} // SetUVIndex()
	
	public Vector3[] GetVerts()
	{
		return verts;
	} // GetVerts()
	
	public int[] GetPolyVerts()
	{
		return polyVerts;
	} // GetPolyVerts()
	
	public Vector3[] GetNorms()
	{
		return norms;
	} // GetNorms()
	
	public Vector2[] GetUVs()
	{
		return uvs;
	} // GetUVs()
	
	public int[] GetUVIndex()
	{
		return uvIndex;
	} // GetUVIndex()
}
