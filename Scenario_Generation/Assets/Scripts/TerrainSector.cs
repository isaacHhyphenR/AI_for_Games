using UnityEngine;

public class TerrainSector : MonoBehaviour
{
    [SerializeField] Renderer visualComponent;
    [SerializeField] float maxHeight;
    [SerializeField] float minHeight;
    [Tooltip("Any pixel with less lightness will be water")]
    [SerializeField] float waterValue;
    [Tooltip("Any pixel with more lightness will be mountain")]
    [SerializeField] float mountainValue;
    [Tooltip("Any pixel with more lightness will be snowcap")]
    [SerializeField] float snowValue;
    [SerializeField] Material water;
    [SerializeField] Material snow;
    [SerializeField] Material mountain;
    [SerializeField] Material grass;


    public void SetTerrain(Color value)
    {
        float height = value.r * maxHeight + minHeight;
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, height/2, transform.position.z);
        if(value.r < waterValue)
        {
            visualComponent.material = water;
            gameObject.name = gameObject.name + "water";
        }
        else if (value.r > snowValue)
        {
            visualComponent.material = snow;
            gameObject.name = gameObject.name + "snow";
        }
        else if (value.r > mountainValue)
        {
            visualComponent.material = mountain;
            gameObject.name = gameObject.name + "mountain";
        }
        else
        {
            visualComponent.material = grass;
            gameObject.name = gameObject.name + "grass";
        }
    }
}
