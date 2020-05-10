using UnityEngine;

public struct PlayersLastPosition
{
    public readonly Vector3 position;
    public readonly Quaternion rotation;

    public PlayersLastPosition(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}