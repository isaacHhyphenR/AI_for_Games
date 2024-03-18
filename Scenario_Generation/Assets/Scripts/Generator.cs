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
    int[] triangles;
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
        vertices = new Vector3[(int)dimensions.x * (int)dimensions.y];
        uv = new Vector2[(int)dimensions.x * (int)dimensions.y];
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
                vertices[currentIndex] = new Vector3(x, heightNoise, y);
                uv[currentIndex] = new Vector2(x/dimensions.x,y/dimensions.y);
                if(x < dimensions.x - 1 && y < dimensions.x - 1)
                {
                    triangles[triangleIndex++] = currentIndex;
                    triangles[triangleIndex++] = currentIndex + 1;
                    triangles[triangleIndex++] = currentIndex + 1 + (int)dimensions.x;
                    triangles[triangleIndex++] = currentIndex;
                    triangles[triangleIndex++] = currentIndex + 1 + (int)dimensions.x;
                    triangles[triangleIndex++] = currentIndex + (int)dimensions.x;
                }
                currentIndex++;
            }
        }


        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
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
}
