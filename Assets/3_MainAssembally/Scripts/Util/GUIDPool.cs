using System.Collections.Generic;
using UnityEngine;

public class GUIDPool
{
    private HashSet<ulong> usedGUIDs;
    private ShortTwister shortTwister = new ShortTwister((ulong)Mathf.Abs(Random.Range(1, int.MaxValue)));

    public GUIDPool(params ulong[] takenGUIDs) : this((IEnumerable<ulong>)takenGUIDs)
    {

    }

    public GUIDPool(IEnumerable<ulong> takenGUIDs)
    {
        usedGUIDs = new HashSet<ulong>(takenGUIDs);

    }

    public void Add(ulong takenGUID)
    {
        usedGUIDs.Add(takenGUID);
    }

    public ulong GetGUID()
    {
        ulong potentialGUID = (ulong)Mathf.Abs(Random.Range(0, int.MaxValue));
        while (usedGUIDs.Contains(potentialGUID))
        {
            potentialGUID = shortTwister.GetNext(potentialGUID);
        }
        Add(potentialGUID);
        return potentialGUID;
    }

    public bool Contains(ulong value) => usedGUIDs.Contains(value);
}
