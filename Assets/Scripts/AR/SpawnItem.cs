using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class TreasureARSpawner : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    private readonly List<ARRaycastHit> hits = new();
    private Coroutine pendingSpawn;
    [SerializeField] private Transform arCamera;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject keyPrefab;

    private GameObject spawnedItem;
    private void Start()
    {
        SpawnChest(Color.red);
    }
    public void SpawnChest(Color color)
    {
        SpawnItem(chestPrefab, color);
    }

    public void SpawnKey(Color color)
    {
        SpawnItem(keyPrefab, color);
    }

    private void SpawnItem(GameObject prefab, Color color)
    {
        if (raycastManager == null || prefab == null)
        {
            Debug.LogError("Priraď AR Raycast Manager a prefab predmetu.", this);
            return;
        }

        if (pendingSpawn != null)
            StopCoroutine(pendingSpawn);

        pendingSpawn = StartCoroutine(SpawnOnPlane(prefab, color));
    }

    private IEnumerator SpawnOnPlane(GameObject prefab, Color color)
    {
        while (true)
        {
            yield return null;

            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
                continue;

            ARRaycastHit hit = hits[0];

            // Ak mieriš príliš blízko, počkaj na vzdialenejší bod.
            if (hit.distance < 1.5f)
                continue;

            if (spawnedItem != null)
                Destroy(spawnedItem);

            spawnedItem = Instantiate(prefab, hit.pose.position, Quaternion.identity);

            foreach (Renderer itemRenderer in spawnedItem.GetComponentsInChildren<Renderer>())
                itemRenderer.material.color = color;

            pendingSpawn = null;
            yield break;
        }
    }
}