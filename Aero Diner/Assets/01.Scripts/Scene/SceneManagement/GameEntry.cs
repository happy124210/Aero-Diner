using UnityEngine;

public class GameEntry : MonoBehaviour
{
    [SerializeField] private GameObject uiRootStaticPrefab;

    private static bool hasSpawned = false;

    private void Awake()
    {
        if (!hasSpawned)
        {
            var uiRoot = Instantiate(uiRootStaticPrefab);
            DontDestroyOnLoad(uiRoot);
            hasSpawned = true;
        }
    }
}

