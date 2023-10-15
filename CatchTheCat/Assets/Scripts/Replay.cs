using UnityEngine;
using UnityEngine.SceneManagement;

public class Replay : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
