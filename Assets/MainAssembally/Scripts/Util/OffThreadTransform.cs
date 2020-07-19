using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OffThreadTransform : MonoBehaviour
{
    private static Thread mainThread;

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 localScale;

    public Vector3 Position
    {
        get
        {
            UpdateStats();
            return position;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            UpdateStats();
            return rotation;
        }
    }

    public Vector3 LocalScale
    {
        get
        {
            UpdateStats();
            return localScale;
        }
    }


    private void Awake()
    {
        if(mainThread == null)
        {
            mainThread = Thread.CurrentThread;
        }
    }

    private void Update()
    {
        UpdateStats();
    }

    private void LateUpdate()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (Thread.CurrentThread.Equals(mainThread))
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.localScale = transform.localScale;
        }
    }

    public static implicit operator OffThreadTransform(Transform transform)
    {
        return transform.EnsureComponent<OffThreadTransform>();
    }
}
