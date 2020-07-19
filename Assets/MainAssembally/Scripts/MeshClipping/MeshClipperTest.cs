using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

public class MeshClipperTest : MonoBehaviour
{
    public GameObject clippingPlaneSourceObject;
    public GameObject targetObjectToBeClipped;

    private Dictionary<ClippingPlaneSource, ClippingPlaneSet> sourcesToClip = new Dictionary<ClippingPlaneSource, ClippingPlaneSet>();

    private Dictionary<ClippingPlaneSource, Task<Mesh>> generationTasks = new Dictionary<ClippingPlaneSource, Task<Mesh>>();
    private Dictionary<ClippingPlaneSource, GameObject> targetObjects = new Dictionary<ClippingPlaneSource, GameObject>();


    private void Start()
    {
        if (clippingPlaneSourceObject != null)
        {
            var sources = clippingPlaneSourceObject.GetComponentsInChildren<ClippingPlaneSource>();
            foreach (var source in sources)
            {
                MeshFilter meshFilter = source?.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter?.mesh;
                if (mesh != null)
                {
                    sourcesToClip.Add(source, new ClippingPlaneSet(source.transform.localToWorldMatrix, mesh));
                }
            }
        }

        foreach (var source in sourcesToClip)
        {
            foreach (var filter in targetObjectToBeClipped.GetComponentsInChildren<MeshFilter>())
            {
                MeshClipper.Clip(filter.mesh, source.Value, filter.transform.worldToLocalMatrix)
                    .ThenOnMainThread(x => UpdateMesh(x, source.Key, filter.transform, filter.mesh));
            }
        }
    }

    private void Update()
    {

    }

    private void UpdateMesh(Mesh mesh, ClippingPlaneSource clippingPlaneSource, Transform sourceTransform, Mesh sourceMesh)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if(!targetObjects.TryGetValue(clippingPlaneSource, out GameObject newGameObject))
        {
            newGameObject = new GameObject();
            newGameObject.AddComponent<MeshFilter>();
            newGameObject.AddComponent<MeshRenderer>();
            targetObjects.Add(clippingPlaneSource, newGameObject);
        }
        var newFilter = newGameObject.GetComponent<MeshFilter>();
        var renderer = newGameObject.GetComponent<MeshRenderer>();
        newGameObject.transform.position = sourceTransform.position;
        newGameObject.transform.rotation = sourceTransform.rotation;
        newFilter.mesh = mesh;

        MeshClipper.Clip(sourceMesh, sourcesToClip[clippingPlaneSource], sourceTransform.worldToLocalMatrix, mesh)
            .ThenOnMainThread((x) => UpdateMesh(x, clippingPlaneSource, sourceTransform, sourceMesh));

        stopwatch.Stop();
        Debug.Log($"Update Time: {stopwatch.ElapsedMilliseconds}");
    }
}
