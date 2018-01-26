﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class UpdateTerrain : MonoBehaviour {
	public GameObject DepthSourceManager;
	public Texture2D heightmap;
	public float scale = 10;		// Size of the resulting mesh
	public float magnitude = 1;		// Maximum height of the resulting mesh

	private DepthSourceManager dsm;
    private CoordinateMapper mapper;
    private KinectSensor sensor;
	private Mesh mesh;
	private float spacing;		// The distance between vertices in the mesh

    private const int downsampleSize = 4;   // The amount to downsample the raw depth sensor data

    // Create a new mesh by generating a set of vertices and triangles
    void CreateMesh(int width, int height) {
        // Initialize vertex and triangle arrays
        Vector3[] vertices = new Vector3[width * height];
		int[] triangles = new int[(height - 1) * (width - 1) * 6];

		// Populate vertex array
		for (int i = 0; i < height; i++) {
			for (int j = 0; j < width; j++) {
				vertices [j + width * i] = new Vector3 (j * spacing, 0, i * spacing);
			}
		}

		int idx = 0; // index for triangle array

		// Populate triangle array
		for (int i = 0; i < height - 1; i++) {
			for (int j = 0; j < width - 1; j++) {

				triangles [idx++] = j + width * i; 				// '
				triangles [idx++] = j + width * (1 + i);		// :
				triangles [idx++] = 1 + j + width * i;	 		// :'

				triangles [idx++] = j + width * (1 + i);		// .
				triangles [idx++] = 1 + j + width * (1 + i); 	// ..
				triangles [idx++] = 1 + j + width * i; 			// .:
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}

	//update the terrain mesh with height data from Kinect sensor
	void UpdateMesh() {
        if (sensor == null)
            return;

		Color[] pixels;

		pixels = heightmap.GetPixels ();			// Get array of pixels from heightmap

		// Initialize vertex and triangle arrays
		Vector3[] vertices = new Vector3[pixels.Length];

		// Populate vertex array
		for (int i = 0; i < heightmap.height; i++) {
			for (int j = 0; j < heightmap.width; j++) {
				float y = pixels [j + heightmap.width * i].grayscale;		// Get Y value from greyscale value
				vertices [j + heightmap.width * i] = new Vector3 (j * spacing, y * magnitude, i * spacing);
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}

	void Start () {
		spacing = scale / heightmap.height;

		mesh = new Mesh ();		// Initialize mesh

		GetComponent<MeshFilter> ().mesh = mesh;
		if (DepthSourceManager != null) {
			dsm = DepthSourceManager.GetComponent<DepthSourceManager> ();
		}

		//CreateMesh (heightmap.width, heightmap.height);

        sensor = KinectSensor.GetDefault();
        if(sensor != null) {
            var frameDesc = sensor.DepthFrameSource.FrameDescription;
            mapper = sensor.CoordinateMapper;

            // Downsample to lower resolution
            CreateMesh(frameDesc.Width / downsampleSize, frameDesc.Height / downsampleSize);

            if (!sensor.IsOpen) {
                sensor.Open();
            }
        }
	}


	void Update() {
		UpdateMesh ();
	}


    void OnApplicationQuit()
    {
        if (_Mapper != null) {
            _Mapper = null;
        }

        if (sensor != null) {
            if (sensor.IsOpen) {
                sensor.Close();
            }

            sensor = null;
        }
    }
}
