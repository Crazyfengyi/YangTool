using UnityEngine;

public class MonsterAutoDespawn : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 8f;

    private void Start()
    {
        if (lifeSeconds > 0f)
        {
            Destroy(gameObject, lifeSeconds);
        }
    }
}
