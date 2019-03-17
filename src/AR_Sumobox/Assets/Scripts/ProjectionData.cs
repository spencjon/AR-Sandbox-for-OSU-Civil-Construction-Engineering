﻿using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public class ProjectionData : MonoBehaviour
{
    private GameObject Projection_Data_GO;
    public string offset;
    public string originalBounds;
    public string projectedBounds;

    // Gets the bounds of the current network as a pair of points in 2-Space
    public void SetProjectionData(XmlDocument xml)
    {
        XmlNode location = xml.DocumentElement.SelectSingleNode("location");
        offset = location.Attributes.GetNamedItem("netOffset").Value;
        originalBounds = location.Attributes.GetNamedItem("origBoundary").Value;
        projectedBounds = location.Attributes.GetNamedItem("convBoundary").Value;
    }

    // Sumo shape sting to List of floats point order is
    // x1, y1, x2, y2, ....
    public List<float> ShapeStringToFloatList(string shape)
    {
        List<float> points = new List<float>();
        char[] find = new char[2];
        find[0] = ',';
        find[1] = ' ';
        string[] cuts = shape.Split(find);
        List<string> cutList = cuts.ToList();
        foreach (string cut in cutList)
        {
            points.Add(float.Parse(cut, CultureInfo.InvariantCulture.NumberFormat));
        }
        return points;
    }

    // Adds a Terrain_Plane prefab to the scene 100 units (meters) larger than the network.
    public void BuildTerrain()
    {
        List<float> bp = ShapeStringToFloatList(projectedBounds);
        float x = bp[2] - bp[0];
        float y = bp[3] - bp[1];
        //float z = 1.0f;
        GameObject chunk = new GameObject();
        chunk.name = "Terrain_Plane";
        chunk.AddComponent<MeshRenderer>();
        Material m = Resources.Load("Materials/Grass_Material", typeof(Material)) as Material;
        chunk.GetComponent<MeshRenderer>().sharedMaterial = m;

        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[4] {
            new Vector3(0.0f,0.0f,y),
            new Vector3(x,0.0f,y),
            new Vector3(x,0.0f,0.0f),
            new Vector3(0.0f,0.0f,0.0f)
        };
        mesh.vertices = verts;
        int[] tris = new int[6] { 0, 1, 3, 1, 2, 3 };
        mesh.triangles = tris;

        mesh.uv = new Vector2[4] { new Vector2(0.0f, 0.0f), new Vector2(0.0f, y), new Vector2(x, y), new Vector2(x, 0.0f) };
        mesh.RecalculateNormals();
        chunk.AddComponent<MeshFilter>().mesh = mesh;
        chunk.isStatic = true;
        chunk.transform.parent = Projection_Data_GO.transform;
        //GameObject.Find("Main_Camera").transform.SetPositionAndRotation(new Vector3(x / 2.0f, y / 2.0f, 1000.0f), Quaternion.Euler(new Vector3(0.0f,180.0f,0.0f)));
    }

    // Start is called before the first frame update
    void Start()
    {
        Projection_Data_GO = GameObject.Find("Projection_Data");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
