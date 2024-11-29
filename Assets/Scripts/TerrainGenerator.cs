using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    public int width = 100;
    public int depth = 100;
    public float scale = 20f;
    public float heightScale = 10f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;
    public float textureTiling = 10f;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private float offsetX; // X-axis offset for noise
    private float offsetZ; // Z-axis offset for noise

    void Start()
    {
        RandomizeOffsets();
        GenerateTerrain();
    }

    void Update()
    {
        // Generate a new terrain when the spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeOffsets();
            RandomizeParameters();
            GenerateTerrain();
        }
    }

    void RandomizeOffsets()
    {
        // Assign a completely random seed for offsets
        offsetX = Random.Range(0f, 10000f);
        offsetZ = Random.Range(0f, 10000f);
    }

    void RandomizeParameters()
    {
        // Slightly randomize terrain parameters for variation
        scale = Random.Range(15f, 25f);
        heightScale = Random.Range(8f, 12f);
        persistence = Random.Range(0.4f, 0.6f);
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
        float frequency = 1f;
        float amplitude = 1f;

        // Combine multiple layers of Perlin noise
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + offsetX) / scale * frequency;
            float sampleZ = (z + offsetZ) / scale * frequency;
            height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;

            frequency *= lacunarity;   // Increase frequency
            amplitude *= persistence; // Decrease amplitude
        }

        // Apply height curve for shaping
        height = heightCurve.Evaluate(height);

        // Modify valleys and flattening
        if (height < 0.3f) // Low areas
        {
            height *= 0.5f; // Slightly flatten
        }
        else if (height > 0.4f && height < 0.6f) // Valleys
        {
            height *= 0.8f; // Reduce height slightly
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
        mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                uvs[z * width + x] = new Vector2((float)x / (width - 1) * textureTiling,
                                                 (float)z / (depth - 1) * textureTiling);
            }
        }

        mesh.uv = uvs;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
