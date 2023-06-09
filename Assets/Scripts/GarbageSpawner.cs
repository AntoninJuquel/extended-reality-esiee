using System.Collections.Generic;
using UnityEngine;

public class GarbageSpawner : MonoBehaviour
{
    [SerializeField] private Garbage[] garbages;
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        int i = 0;

        while (i < numCollisionEvents)
        {
            Vector3 pos = collisionEvents[i].intersection;
            var choice = Random.Range(0, garbages.Length);
            var garbage = garbages[choice];

            for (int j = 0; j < garbage.spawnPerCollision; j++)
            {
                if (garbage.currentlySpawned < garbage.maxSpawn)
                {
                    Instantiate(garbage.prefab, pos, Random.rotation);
                    garbages[choice].currentlySpawned++;
                }
            }
            i++;
        }
    }
}

[System.Serializable]
struct Garbage
{
    public GameObject prefab;
    public int spawnPerCollision;
    public int maxSpawn;
    public int currentlySpawned;
}