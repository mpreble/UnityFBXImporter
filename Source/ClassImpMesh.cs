using UnityEngine;
using System.Collections;

public class ClassImpMesh{
	
	int id;
	string name;
	int pId;
	Vector3 origin;
	
	public ClassImpMesh(int i)
	{
		id = i;
		origin = new Vector3(0f,0f,0f);
		pId = 0;
		name = "";
	} // ClassImpMesh
	
	public void SetID(int i)
	{
		id = i;
	} // SetID()
	
	public void SetName(string n)
	{
		name = n;
	} // SetName()
	
	public void SetPID(int i)
	{
		pId = i;
	} // SetPID()
	
	public void SetOrig(Vector3 v)
	{
		origin = v;
	} // SetOrig()
	
	public int GetID()
	{
		return id;
	} // GetID()
	
	public int GetPID()
	{
		return pId;
	} // GetPID()
	
	public string GetName()
	{
		return name;
	} // GetName()
	
	public Vector3 GetOrig()
	{
		return origin;
	} // GetOrig()
}
