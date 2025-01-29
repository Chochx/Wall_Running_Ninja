using UnityEngine;

public class SpawnPointContainer : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointData
    {
        public Transform spawnPoint;
        [Range(0f, 1f)] public float weight = 1f;
    }

    public SpawnPointData[] spawnPoints;
}
