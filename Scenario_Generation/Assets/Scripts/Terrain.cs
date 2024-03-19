using UnityEngine;

public struct TerrainValues
{
    [Tooltip("Temperature")]
    public float t;
    [Tooltip("Altitude")]
    public float a;
    [Tooltip("Moisture")]
    public float m;
    public TerrainValues(float temperature, float altitude, float moisture)
    {
        t = temperature;
        a = altitude;
        m = moisture;
    }
}
public class Terrain : MonoBehaviour
{
    public float temperature;
    public float moisture;
    public float altitude;
    [SerializeField] Material material;
    [SerializeField] GameObject entities;

    public void ApplyTerrain(TerrainSector sector)
    {
        sector.GetRenderer().material = material;
        if(entities != null)
        {
            GameObject entity = Instantiate(entities,sector.transform);
            entity.transform.position = new Vector3(0,sector.transform.localScale.y,0);
        }
        sector.gameObject.name = sector.gameObject.name + gameObject.name;
    }

    public Color GetColor()
    {
        return material.color;
    }
}
