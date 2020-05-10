using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{
    public AITreeAsset aiTreeAsset;

    AIMemory myMemory, parentMemory;

    void Start()
    {
        myMemory = gameObject.AddComponent<AIMemory>();
        myMemory.Set(BasicMemory.LastKnownPlayerPosition, 3f, "Child Cell");

        parentMemory = gameObject.transform.parent.gameObject.AddComponent<AIMemory>();

        StartCoroutine(MemoryRoutine());

        AIExecutor.Exec(gameObject, aiTreeAsset, new Dictionary<string, object>() { { "Foo", "This is foo" } });
    }

    private IEnumerator MemoryRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        parentMemory.Set(BasicMemory.LastKnownPlayerPosition, 1f, "Parent Cell");
    }
}
