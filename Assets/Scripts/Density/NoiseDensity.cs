﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Code adapted from Sebastian Lague's Marching Cubes https://www.youtube.com/watch?v=M3iI2l0ltbE

public class NoiseDensity : DensityGenerator {

    [Header ("Noise")]
    public int seed;
    public int numOctaves = 4;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float noiseScale = 1;
    public float noiseWeight = 1;
    public bool closeEdges;
    public float floorOffset = 1;
    public float weightMultiplier = 1;

    public float hardFloorHeight;
    public float hardFloorWeight;

    [Header("Spawn Area (Forced Empty Zone)")]
    public Vector3 spawnCenter = Vector3.zero;
    public float spawnRadius = 5f;
    public float spawnFalloff = 0.1f;

    public bool raiseEdges;
    public float edgeRaiseWidth = 15f;
    public float edgeRaiseHeight = 10f;

    public Vector4 shaderParams;

    public override ComputeBuffer Generate (ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing) {
        buffersToRelease = new List<ComputeBuffer> ();

        // Noise parameters
        var prng = new System.Random (seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        }

        var offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        offsetsBuffer.SetData (offsets);
        buffersToRelease.Add (offsetsBuffer);

        
        densityShader.SetVector ("centre", new Vector4 (centre.x, centre.y, centre.z));
        densityShader.SetInt ("octaves", Mathf.Max (1, numOctaves));
        densityShader.SetFloat ("lacunarity", lacunarity);
        densityShader.SetFloat ("persistence", persistence);
        densityShader.SetFloat ("noiseScale", noiseScale);
        densityShader.SetFloat ("noiseWeight", noiseWeight);
        densityShader.SetBool ("closeEdges", closeEdges);
        densityShader.SetBuffer (0, "offsets", offsetsBuffer);
        densityShader.SetFloat ("floorOffset", floorOffset);
        densityShader.SetFloat ("weightMultiplier", weightMultiplier);
        densityShader.SetFloat ("hardFloor", hardFloorHeight);
        densityShader.SetFloat ("hardFloorWeight", hardFloorWeight);

        densityShader.SetVector("spawnCenter", new Vector4(spawnCenter.x, spawnCenter.y, spawnCenter.z, spawnRadius));
        densityShader.SetFloat("spawnFalloff", spawnFalloff);

        densityShader.SetBool("raiseEdges", raiseEdges);
        densityShader.SetFloat("edgeRaiseWidth", edgeRaiseWidth);
        densityShader.SetFloat("edgeRaiseHeight", edgeRaiseHeight);


        densityShader.SetVector ("params", shaderParams);

        return base.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}