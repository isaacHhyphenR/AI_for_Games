using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] Texture2D baseMap;
    [SerializeField] GameObject terrainPrefab;
    [SerializeField] float terrainSize;
    private void Start()
    {
        Vector2 offset = new Vector2(terrainSize * baseMap.width / 2, - terrainSize * baseMap.height / 2);
        for (int x = 0; x < baseMap.width; x++)
        {
            for (int y = 0; y < baseMap.height; y++)
            {
                Vector3 newPos = new Vector3(x * terrainSize - offset.x, 0, y * - terrainSize - offset.y);
                GameObject newSquare = Instantiate(terrainPrefab, newPos, Quaternion.identity);
                newSquare.transform.SetParent(transform, false);
                if (baseMap.GetPixel(x,y).r > 0.5)
                {
                    newSquare.transform.localScale = new Vector3(terrainSize, terrainSize * 5, terrainSize);
                }
            }
        }
    }
}
