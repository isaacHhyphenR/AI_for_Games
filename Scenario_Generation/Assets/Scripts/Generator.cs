using UnityEngine;

public class Generator : MonoBehaviour
{
    [Tooltip("If True, will generate using the texture. If False, will generate using random noise.")]
    [SerializeField] bool useTexture;
    [Header("Mesh")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] float sectorSize;
    [Header("Terrain Info")]
    [SerializeField] float maxHeight;
    [SerializeField] Terrain mountain;
    [SerializeField] Terrain snowcapMountain;
    [SerializeField] Terrain desertMountain;
    [SerializeField] Terrain grass;
    [SerializeField] Terrain ocean;
    [SerializeField] Terrain jungle;
    [SerializeField] Terrain desert;
    [SerializeField] Terrain tundra;
    [Header("Texture Based Generation")]
    [Tooltip("Texture used to generate the Temperature, Altitude, and Moisture of the map.")]
    [SerializeField] Texture2D texture;
    [Header("Noise Based Generation")]
    [SerializeField] Vector2 meshDimensions;
    [SerializeField] float frequency;
    [SerializeField] int octaves;


    private void Start()
    {
        Perlin.Init();
        Generate();
    }


    void Generate()
    {
        //Determines size based on method
        Vector2 dimensions;
        if(useTexture)
        {
            dimensions = new Vector2(texture.width, texture.height);
        }
        else
        {
            dimensions = meshDimensions;
        }
        //Arrays we'll pass into the mesh
        int numVertices = (int)dimensions.x * (int)dimensions.y;
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        Color[] colors = new Color[numVertices];
        int[] triangles = new int[(int)((dimensions.x - 1) * (dimensions.y - 1) * 6)];
        //Variables we'll use to keep track of where we are on the mesh
        Vector2 offset = new Vector2(sectorSize * dimensions.x / 2, -sectorSize * dimensions.y / 2);
        int currentIndex = 0;
        int triangleIndex = 0;
        //Cycles through the vertices
        for (int x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                //Determines what terrain this vertex uses
                TerrainValues values = new TerrainValues();
                if(useTexture)
                {
                    Color pixel = texture.GetPixel(x, (int)dimensions.y - y);
                    values = new TerrainValues(pixel.r, pixel.g, pixel.b);
                }
                else
                {
                    float heightNoise = Mathf.Abs(Perlin.noise((x / dimensions.x) * frequency, (y / dimensions.y - 0.5f) * frequency, octaves));
                    float moistureNoise = Mathf.Abs(Perlin.noise((x / dimensions.x) * frequency, (y / dimensions.y - 0.5f) * frequency, octaves, 15));
                    values.t = y / dimensions.y;
                    values.a = heightNoise;
                    values.m = moistureNoise;
                }
                //Translate the terrain data into useable mesh data
                vertices[currentIndex] = new Vector3(x * sectorSize - offset.x, values.a * maxHeight, -y * sectorSize - offset.y);
                uv[currentIndex] = new Vector2(x/dimensions.x,y/dimensions.y);
                colors[currentIndex] = GetTerrainColor(values);
                if (x < dimensions.x - 1 && y < dimensions.y - 1)
                {
                    triangles[triangleIndex++] = currentIndex;
                    triangles[triangleIndex++] = currentIndex + 1 + (int)dimensions.x;
                    triangles[triangleIndex++] = currentIndex + 1;
                    triangles[triangleIndex++] = currentIndex;
                    triangles[triangleIndex++] = currentIndex + (int)dimensions.x;
                    triangles[triangleIndex++] = currentIndex + 1 + (int)dimensions.x;
                }
                currentIndex++;
            }
        }
        //Use what we have generates
        GenerateMesh(vertices, uv, triangles, colors);
    }


    void GenerateMesh(Vector3[] vertices, Vector2[] uv, int[] triangles, Color[] colors)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;
        mesh.SetVertices(vertices);
        mesh.SetUVs(2, uv);
        mesh.SetTriangles(triangles,0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
    }


    Color GetTerrainColor(TerrainValues value)
    {
        if (value.m > ocean.moisture)
        {
            return ocean.GetColor();
        }
        else if ((value.a > snowcapMountain.altitude && value.t < snowcapMountain.temperature) || (value.a > mountain.altitude && value.t < tundra.temperature))
        {
            return snowcapMountain.GetColor();
        }
        else if ((value.a > desertMountain.altitude && value.t > desertMountain.temperature) || (value.a > mountain.altitude && value.t > desert.temperature))
        {
            return desertMountain.GetColor();
        }
        else if (value.a > mountain.altitude)
        {
            return mountain.GetColor();
        }
        else if (value.t > jungle.temperature && value.m > jungle.moisture)
        {
            return jungle.GetColor();
        }
        else if (value.t > desert.temperature && value.m < desert.moisture)
        {
            return desert.GetColor();
        }
        else if (value.t < tundra.temperature)
        {
            return tundra.GetColor();
        }
        else
        {
            return grass.GetColor();
        }
    }
}
