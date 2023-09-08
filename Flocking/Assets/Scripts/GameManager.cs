using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] DebugPanel debugPanel;
    [SerializeField] GameObject boidPrefab;
    Boid boidScript;
    public static GameManager instance;
    Vector3 screenBounds;
    bool debugMode = true;
    int maxBoids;
    GameObject[] boidArray;

    private void Awake()
    {
        instance = this;
        boidScript = boidPrefab.GetComponent<Boid>();
        screenBounds = boidScript.GetBounds();
    }

    private void Update()
    {
        if(debugMode)
        {
            maxBoids = (int)debugPanel.maxBoidSlider.GetValue();
            boidArray = GameObject.FindGameObjectsWithTag("Boid");
            if (boidArray.Length < maxBoids)
            {
                SpawnBoid();
            }
            else if (boidArray.Length > maxBoids)
            {
                DespawnBoid();
            }
            //sets variables
            foreach(GameObject boid in boidArray)
            {
                boidScript = boid.GetComponent<Boid>();
                boidScript.SetSpeed(debugPanel.speedSlider.GetValue());
                boidScript.SetCohesionRadius(debugPanel.cohesionRadiusSlider.GetValue());
                boidScript.SetSeparationRadius(debugPanel.separationRadiusSlider.GetValue());
                boidScript.SetAlignmentRadius(debugPanel.alignmentRadiusSlider.GetValue());
                boidScript.SetCohesionForce(debugPanel.cohesionForceSlider.GetValue());
                boidScript.SetSeparationForce(debugPanel.separationForceSlider.GetValue());
                boidScript.SetAlignmentForce(debugPanel.alignmentForceSlider.GetValue());
            }
        }
    }

    void SpawnBoid()
    {
        float xSpawn = Random.Range(-screenBounds.z, screenBounds.z);
        float ySpawn = 0.5f;
        float zSpawn = Random.Range(-screenBounds.x, screenBounds.x);
        Vector3 spawnPos = new Vector3(xSpawn, ySpawn, zSpawn);
        float yRot = Random.Range(-1f, 1f);
        Quaternion spawnRot = new Quaternion(0f,yRot,0f,1f);
        Instantiate(boidPrefab, spawnPos, spawnRot);
    }
    void DespawnBoid()
    {
        int victim = Random.Range(0, boidArray.Length);
        Destroy(boidArray[victim]);
    }
}
