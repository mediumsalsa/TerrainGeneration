using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 100;
    public int depth = 100;
    public float scale = 20f;
    public float heightScale = 10f; // Scales the height of the terrain
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        float[,] heightMap = GenerateHeightMap();
        CreateMeshData(heightMap);
        BuildMesh();
    }

    float[,] GenerateHeightMap()
    {
        float[,] heightMap = new float[width, depth];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                heightMap[x, z] = GeneratePerlinNoise(x, z);
            }
        }
        return heightMap;
    }

    float GeneratePerlinNoise(int x, int z)
    {
        float height = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / scale) * frequency;
            float sampleZ = (z / scale) * frequency;

            height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return height;
    }

    void CreateMeshData(float[,] heightMap)
    {
        vertices = new Vector3[width * depth];
        triangles = new int[(width - 1) * (depth - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Apply height scale
                vertices[vertexIndex] = new Vector3(x, heightMap[x, z] * heightScale, z);

                if (x < width - 1 && z < depth - 1)
                {
                    // Create two triangles for each square in the grid
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + width;
                    triangles[triangleIndex + 2] = vertexIndex + width + 1;

                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + width + 1;
                    triangles[triangleIndex + 5] = vertexIndex + 1;

                    triangleIndex += 6;
                }
                vertexIndex++;
            }
        }
    }

    void BuildMesh()
    {
        // Initialize the mesh
        mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };

        // Assign UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                uvs[z * width + x] = new Vector2((float)x / (width - 1), (float)z / (depth - 1));
            }
        }

        mesh.uv = uvs; // Assign UVs
        mesh.RecalculateNormals(); // Ensure lighting works correctly
        GetComponent<MeshFilter>().mesh = mesh; // Assign the mesh to the MeshFilter
    }
}
