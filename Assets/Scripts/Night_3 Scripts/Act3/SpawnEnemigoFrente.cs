using UnityEngine;

public class SpawnEnemigoFrente : MonoBehaviour
{
    public GameObject enemigoPrefab;

    public float distanciaFrente = 10f;
    public float alturaSpawn = 0f;

    public void SpawnEnfrente()
    {
        Transform cam = Camera.main.transform;

        Vector3 posicionSpawn =
            cam.position +
            cam.forward * distanciaFrente;

        posicionSpawn.y += alturaSpawn;

        Instantiate(
            enemigoPrefab,
            posicionSpawn,
            Quaternion.identity
        );

        Debug.Log("Enemigo spawneado enfrente del jugador");
    }
}
