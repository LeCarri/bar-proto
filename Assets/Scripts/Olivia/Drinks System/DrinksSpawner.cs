using System.Collections;
using UnityEngine;

public class DrinksSpawner : MonoBehaviour, IInteractable
{
    private Transform spawnpoint;

    [SerializeField]
    [InspectorName("Spawneable")]
    private GameObject spawneable;

    [SerializeField]
    [InspectorName("Spawn cooldown")]
    private float spawnCooldown = 1;

    private float currCooldown = 0;

    private bool canSpawn = true;
    private void Start()
    {
        spawnpoint = gameObject.GetComponentInChildren<Transform>();
    }

    private void SpawnDrink() 
    {
        canSpawn = false;
        GameObject newSpawneable = Instantiate(spawneable, spawnpoint.position, Quaternion.identity);
        newSpawneable.transform.parent = null;
        StartCoroutine(StartCooldown());
    }
    public bool CanInteract()
    {
        return true;
    }

    public string GetDescription()
    {
        return "guh";
    }

    public void Interact()
    {
        SpawnDrink();
    }

    private IEnumerator StartCooldown() 
    {
        while (!canSpawn) 
        {
            if (currCooldown < spawnCooldown)
                currCooldown += Time.deltaTime;

            yield return null;
        }

        canSpawn = true;
        yield break;
    }
}
