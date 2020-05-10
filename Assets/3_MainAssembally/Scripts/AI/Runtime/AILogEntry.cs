using System.Collections.Generic;
using UnityEngine;

public class AILogEntry
{
    public readonly int FrameNumber;
    public readonly long GUID;
    public string methodName;
    private List<(string argName, object argValue)> arguments = new List<(string argName, object argValue)>();

    public AILogEntry(long GUID)
    {
        this.GUID = GUID;
        FrameNumber = Time.frameCount;
    }

    public IEnumerable<(string argName, object argValue)> Arguments => arguments;

    public void LogArgument(string argName, object argValue)
    {
        arguments.Add((argName, argValue));
    }
}