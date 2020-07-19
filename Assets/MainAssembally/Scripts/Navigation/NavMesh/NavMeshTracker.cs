using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshTracker : MonoBehaviour
{
    new private Collider collider;

    private Vector3 positionLastFrame;
    private Quaternion rotationLastFrame;
    private Vector3 scaleLastFrame;

    private List<NavCellController> cellController = new List<NavCellController>();

    NavigationMesh mesh;

    public static NavMeshTracker Create(GameObject owner, NavigationMesh mesh)
    {
        var output = owner.AddComponent<NavMeshTracker>();
        output.mesh = mesh;
        return output;
    }

    public void Start()
    {
        collider = gameObject.GetComponent<Collider>();
        Invalidate();
    }

    public void Update()
    {
        if (positionLastFrame != transform.localPosition
            || rotationLastFrame != transform.localRotation
            || scaleLastFrame != transform.localScale)
        {
            Invalidate();
        }
    }

    private void Invalidate()
    {
        positionLastFrame = transform.localPosition;
        rotationLastFrame = transform.localRotation;
        scaleLastFrame = transform.localScale;

        if (collider)
        {
            foreach (var controller in cellController)
            {
                controller.RemoveCollider(collider);
                controller.Invalidate();
            }
            cellController = new List<NavCellController>();
            foreach (var controller in NavCellController.GetCellsInBounds(collider, mesh))
            {
                cellController.Add(controller);
                controller.AddCollider(collider);
                controller.Invalidate();
            }
        }
        else
        {
            foreach(Transform child in transform)
            {
                child.GetComponent<NavMeshTracker>()?.Invalidate();
            }
        }
    }
}
