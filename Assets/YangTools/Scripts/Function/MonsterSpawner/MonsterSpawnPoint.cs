using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform spawnRoot;

    public Vector3 GetPosition()
    {
        return spawnRoot != null ? spawnRoot.position : transform.position;
    }
}
