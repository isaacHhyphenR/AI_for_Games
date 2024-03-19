using UnityEditor.TerrainTools;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Tooltip("If True, will generate using the texture. If False, will generate using random noise.")]
    [SerializeField] bool useTexture;
    [SerializeField] GameObject sectorPrefab;
    float sectorSize;
    [Header("Mesh")]
    [SerializeField] MeshFilter meshFilter;
    Vector3[] vertices;
    Vector2[] uv;
    Color[] colors;
    int[] triangles;
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
    [Header("")]
    [Header("Texture Generation")]
    [Tooltip("Texture used to generate the Temperature, Altitude, and Moisture of the map.")]
    [SerializeField] Texture2D texture;
    [Header("Noise Generation")]
    [SerializeField] Vector2 dimensions;
    [SerializeField] float frequency;
    [SerializeField] int octaves;
    [SerializeField] bool flatGreyscale;


    private void Start()
    {
        if(useTexture)
        {
            GenerateFromTexture();
        }
        else
        {
            GenerateFromNoise();
        }
    }


    void GenerateFromNoise()
    {
        int numVertices = (int)dimensions.x * (int)dimensions.y;
        vertices = new Vector3[numVertices];
        uv = new Vector2[numVertices];
        colors = new Color[numVertices];
        triangles = new int[(int)((dimensions.x - 1) * (dimensions.x - 1) * 6)];

        Perlin.Init();
        sectorSize = sectorPrefab.transform.localScale.x;
        Vector2 offset = new Vector2(sectorSize * texture.width / 2, -sectorSize * texture.height / 2);
        int currentIndex = 0;
        int triangleIndex = 0;
        for (int x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                float heightNoise = Mathf.Abs(Perlin.noise((x/dimensions.x ) * frequency, (y / dimensions.x - 0.5f) * frequency, octaves));
                float moistureNoise = Mathf.Abs(Perlin.noise((x / dimensions.x) * frequency, (y / dimensions.x - 0.5f) * frequency, octaves, 15));

                //GenerateSector(new TerrainValues(y/dimensions.y, heightNoise, moistureNoise), x, y, offset);
                vertices[currentIndex] = new Vector3(x - offset.x, heightNoise * maxHeight, -y - offset.y);
                uv[currentIndex] = new Vector2(x/dimensions.x,y/dimensions.y);
                colors[currentIndex] = GetTerrainColor(new TerrainValues(y / dimensions.y, heightNoise, moistureNoise));
                if (x < dimensions.x - 1 && y < dimensions.x - 1)
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


        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.SetColors(colors);
    }

    void GenerateFromTexture()
    {
        sectorSize = sectorPrefab.transform.localScale.x;
        Vector2 offset = new Vector2(sectorSize * texture.width / 2, - sectorSize * texture.height / 2);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixel = texture.GetPixel(x, texture.height - y);
                GenerateSector(new TerrainValues(pixel.r, pixel.g, pixel.b), x, y, offset);
            }
        }
    }

    void GenerateSector(TerrainValues data, int x, int y, Vector2 offset)
    {
        Vector3 newPos = new Vector3(x * sectorSize - offset.x, 0, y * -sectorSize - offset.y);
        TerrainSector newSquare = Instantiate(sectorPrefab, newPos, Quaternion.identity).GetComponent<TerrainSector>();
        newSquare.gameObject.name = x + "," + y + ": ";
        newSquare.SetTerrain(data, flatGreyscale);
        newSquare.transform.SetParent(transform, false);
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
