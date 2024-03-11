using UnityEngine;


public class TerrainSector : MonoBehaviour
{
    [SerializeField] Renderer visualComponent;
    [SerializeField] float maxHeight;
    [SerializeField] float minHeight;
    [SerializeField] Terrain mountain;
    [SerializeField] Terrain snowcapMountain;
    [SerializeField] Terrain desertMountain;
    [SerializeField] Terrain grass;
    [SerializeField] Terrain ocean;
    [SerializeField] Terrain jungle;
    [SerializeField] Terrain desert;
    [SerializeField] Terrain tundra;


    public void SetTerrain(TerrainValues value, bool bw)
    {
        visualComponent.material.color = new Color(value.t, value.a, value.m);
    }

    public void SetTerrain(TerrainValues value)
    {
        float height = value.a * maxHeight + minHeight;
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, height/2, transform.position.z);
        if(value.m > ocean.moisture)
        {
            ocean.ApplyTerrain(this);
        }
        else if ((value.a > snowcapMountain.altitude && value.t < snowcapMountain.temperature) || (value.a > mountain.altitude && value.t < tundra.temperature))
        {
            snowcapMountain.ApplyTerrain(this);
        }
        else if ((value.a > desertMountain.altitude && value.t > desertMountain.temperature) || (value.a > mountain.altitude && value.t > desert.temperature))
        {
            desertMountain.ApplyTerrain(this);
        }
        else if (value.a > mountain.altitude)
        {
            mountain.ApplyTerrain(this);
        }
        else if (value.t > jungle.temperature && value.m > jungle.moisture)
        {
            jungle.ApplyTerrain(this);
        }
        else if (value.t > desert.temperature && value.m < desert.moisture)
        {
            desert.ApplyTerrain(this);
        }
        else if (value.t < tundra.temperature)
        {
            tundra.ApplyTerrain(this);
        }
        else
        {
            grass.ApplyTerrain(this);
        }
    }

    public Renderer GetRenderer()
    {
        return visualComponent;
    }
}
