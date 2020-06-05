using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static Game instance;

    private GameObject[] players;
    public static IEnumerable<GameObject> Players => instance.players;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        instance = new GameObject("Game").AddComponent<Game>();
    }

    public void Awake()
    {
        players = new GameObject[] { new GameObject("Player") };
    }
}
