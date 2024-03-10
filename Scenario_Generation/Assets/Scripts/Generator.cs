using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] Texture2D baseMap;
    [SerializeField] GameObject terrainPrefab;
    float terrainSize;
    private void Start()
    {
        terrainSize = terrainPrefab.transform.localScale.x;
        Vector2 offset = new Vector2(terrainSize * baseMap.width / 2, - terrainSize * baseMap.height / 2);
        for (int x = 0; x < baseMap.width; x++)
        {
            for (int y = 0; y < baseMap.height; y++)
            {
                Vector3 newPos = new Vector3(x * terrainSize - offset.x, 0, y * - terrainSize - offset.y);
                TerrainSector newSquare = Instantiate(terrainPrefab, newPos, Quaternion.identity).GetComponent<TerrainSector>();
                newSquare.gameObject.name = x + "," + y + ": ";
                newSquare.SetTerrain(baseMap.GetPixel(x, baseMap.height - y));
                newSquare.transform.SetParent(transform, false);
            }
        }
    }
}
