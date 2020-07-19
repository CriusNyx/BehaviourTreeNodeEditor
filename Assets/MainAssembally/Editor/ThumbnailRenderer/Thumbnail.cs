using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class Thumbnail
{
    private static GameObject threePointLight;

    public static RenderTexture Render(GameObject gameObject, int resolution)
    {
        if(threePointLight == null)
        {
            threePointLight = Resources.Load<GameObject>("Prefabs/ThreePointLights");
        }

        //calculate object bounds
        Bounds bounds = new Bounds();
        foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        var renderScene = EditorSceneManager.NewPreviewScene();
        
        GameObject parent = new GameObject("Scene Parent");

        GameObject instance = GameObjectFactory.Instantiate(gameObject, parent: parent.transform);

        GameObject cameraGameObject = GameObjectFactory.Create("Camera", parent: parent.transform);

        var camera = cameraGameObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Nothing;
        camera.forceIntoRenderTexture = true;
        
        cameraGameObject.transform.position = bounds.center + Vector3.right + Vector3.forward + Vector3.up;
        cameraGameObject.transform.position *= Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) * 1.2f;
        cameraGameObject.transform.rotation = Quaternion.Euler(0, 225, 0) * Quaternion.Euler(45f, 0, 0);

        RenderTexture texture = new RenderTexture(resolution, resolution, 16);
        texture.Create();

        camera.targetTexture = texture;
        camera.scene = renderScene;

        GameObject lightGameObject = GameObjectFactory.Instantiate(threePointLight, parent: parent.transform);
        var light = lightGameObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        EditorSceneManager.MoveGameObjectToScene(parent, renderScene);

        camera.Render();
        camera.targetTexture = null;

        GameObject.DestroyImmediate(parent);

        EditorSceneManager.UnloadSceneAsync(renderScene);

        return texture;
    }
}
