using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.IO;
using Ogxd;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class BlenderPostprocessor : AssetPostprocessor
{
    private event Action HeirarcyFunction;

    private Dictionary<GameObject, Dictionary<string, object>> properties = new Dictionary<GameObject, Dictionary<string, object>>();

    bool
        RunBruteForcePositionTest = false,
        RunBruteForceRotationTest = false,
        RunBruteForceScaleTest = false;

    private const string BLENDER_ROTATION_SIGN_DIFF = "-+-";
    private static readonly Vector3 ROTATION_BEFORE_ROTATION = new Vector3(90, 0, 0);
    private static readonly Vector3 ROTATION_AFTER_ROTATION = new Vector3(0, 180, 0);

    private const string BLENDER_POSITION_SIGN_DIFF = "---";
    private const string BLENDER_POSITION_AXIS_SWAP = "xzy";
    private static readonly Vector3 POSITION_BEFORE_ROTATION = new Vector3(0, 0, 0);
    private static readonly Vector3 POSITION_AFTER_ROTATION = new Vector3(90, 0, 0);

    private const string BLENDER_SCALE_SIGN_DIFF = "--+";
    private const string BLENDER_SIGN_AXIS_SWAP = "xyz";
    private static readonly Vector3 SCALE_BEFORE_ROTATION = new Vector3(0, 0, 0);
    private static readonly Vector3 SCALE_AFTER_ROTATION = new Vector3(0, 0, 180f);

    private void OnPostprocessGameObjectWithUserProperties(GameObject gameObject,
        string[] propNames,
        object[] values)
    {
        var gameObjectMap = new Dictionary<string, object>();
        properties[gameObject] = gameObjectMap;

        foreach (var (name, value) in propNames.Zip(values))
        {

            Debug.Log($"{name} = {value}");
            gameObjectMap[name] = value;
            foreach (var type in TypeCache.GetTypesWithAttribute<BlenderComponentAttribute>())
            {
                var attr = type.GetCustomAttribute<BlenderComponentAttribute>();
                if (name == attr.propertyName)
                {
                    HeirarcyFunction += attr.ProcessGameObject(type, gameObject, name, value);
                }
            }
        }
    }

    private void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        importer.preserveHierarchy = true;
        importer.isReadable = true;
    }

  

    private void OnPostprocessModel(GameObject gameObject)
    {
        GetConfigurationForModel(gameObject);

        if (RunBruteForcePositionTest || RunBruteForceRotationTest || RunBruteForceScaleTest)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in gameObject.transform)
            {
                children.Add(child);
            }

            if (RunBruteForcePositionTest)
            {
                Debug.Log(ReverseEngineerUnityPositionTransformationViaBruteForce(children.ToArray()));
            }
            if (RunBruteForceRotationTest)
            {
                Debug.Log(ReverseEngineerUnityRotationTransformationViaBruteForce(children.ToArray()));
            }
            if (RunBruteForceScaleTest)
            {
                Debug.Log(ReverseEngineerUnityScaleTransformationViaBruteForce(children.ToArray()));
            }
        }

        var map = GetTransformMap(gameObject.transform);
        ApplyMapToTransforms(gameObject.transform, map);

        ProcessTransform(gameObject.transform);

        HeirarcyFunction?.Invoke();
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach(var assetPath in importedAssets)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (asset != null)
            {
                var texture = Thumbnail.Render(asset, 1024);

                Texture2D texture2D = new Texture2D(texture.width, texture.height);

                RenderTexture.active = texture;
                texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                RenderTexture.active = null;

                byte[] bytes = texture2D.EncodeToPNG();
                File.WriteAllBytes(assetPath.Replace(".blend", ".png"), bytes);
                AssetDatabase.ImportAsset(assetPath.Replace(".blend", ".png"));
            }
        }
    }

    private object GetPropertyForObject(GameObject gameObject, string propertyName)
    {
        if (properties.TryGetValue(gameObject, out var gameObjectMap))
        {
            if (gameObjectMap.TryGetValue(propertyName, out var o))
            {
                return o;
            }
        }
        return null;
    }

    private T GetPropertyForObject<T>(GameObject gameObject, string propertyName)
    {
        var output = GetPropertyForObject(gameObject, propertyName);
        if (output is T t)
        {
            return t;
        }
        else
        {
            return default;
        }
    }

    private void GetConfigurationForModel(GameObject gameObject)
    {
        // Process This
        if (GetPropertyForObject<string>(gameObject, "runBruteForcePositionTest") == "true")
        {
            RunBruteForcePositionTest = true;
        }
        if (GetPropertyForObject<string>(gameObject, "runBruteForceRotationTest") == "true")
        {
            RunBruteForceRotationTest = true;
        }
        if(GetPropertyForObject<string>(gameObject, "runBruteForceScaleTest") == "true")
        {
            RunBruteForceScaleTest = true;
        }

        // Process Children
        foreach (Transform child in gameObject.transform)
        {
            GetConfigurationForModel(child.gameObject);
        }
    }

    private static Vector3 ParseVector(string input)
    {
        string numberRegex = @"[0-9|\.|-]*";
        string vectorRegex = $"\\(({numberRegex}), ({numberRegex}), ({numberRegex})\\)";

        var match = Regex.Match(input, vectorRegex);

        float x = float.Parse(match.Groups[1].Value);
        float y = float.Parse(match.Groups[2].Value);
        float z = float.Parse(match.Groups[3].Value);

        return new Vector3(x, y, z);
    }

    private static string ReverseEngineerUnityRotationTransformationViaBruteForce(Transform[] children)
    {
        List<string> output = new List<string>();
        foreach (var signOption in GetSignOptions())
        {
            foreach (var axisOption in GetAxisPermutations())
            {
                foreach (var beforeRot in GetRotationOptions())
                {
                    foreach (var afterRot in GetRotationOptions())
                    {
                        var correct = EvaluateTransformationRotation(signOption, axisOption, beforeRot, afterRot, children);
                        string thisResult = $"{signOption} {axisOption} {beforeRot} {afterRot}";
                        if (correct)
                            output.Add(thisResult);
                    }
                }
            }
        }
        if (output.Count == 0)
        {
            return "Rotation Results Failed";
        }
        else
        {
            return "Rotation Results:\n" + string.Join("\n", output);
        }
    }

    private static string ReverseEngineerUnityPositionTransformationViaBruteForce(Transform[] children)
    {
        List<string> output = new List<string>();
        foreach (var signOption in GetSignOptions())
        {
            foreach (var axisOption in GetAxisPermutations())
            {
                foreach (var beforeRot in GetRotationOptions())
                {
                    foreach (var afterRot in GetRotationOptions())
                    {
                        var correct = EvaluateTransformationPosition(signOption, axisOption, beforeRot, afterRot, children);
                        string thisResult = $"{signOption} {axisOption} {beforeRot} {afterRot}";
                        if (correct)
                        {
                            output.Add(thisResult);
                            return "Position Result: " + thisResult;
                        }
                    }
                }
            }
        }
        if (output.Count == 0)
        {
            return "Position Results Failed";
        }
        else
        {
            return "Position Result:\n" + string.Join("\n", output);
        }
    }

    private string ReverseEngineerUnityScaleTransformationViaBruteForce(Transform[] children)
    {
        foreach(var signOption in GetSignOptions())
        {
            foreach(var axisOption in GetAxisPermutations())
            {
                foreach(var beforeRot in GetRotationOptions())
                {
                    foreach(var afterRot in GetRotationOptions())
                    {
                        if (TestScaleTransformation(signOption, axisOption, beforeRot, afterRot, children))
                        {
                            return $"Scale Results: {signOption} {axisOption} {beforeRot} {afterRot}";
                        }
                    }
                }
            }
        }
        return "Scale Results Failed";
    }

    private bool TestScaleTransformation(string signOption, string axisOption, Vector3 beforeRot, Vector3 afterRot, Transform[] children)
    {
        foreach(var child in children)
        {
            string scaleS = GetPropertyForObject<string>(child.gameObject, "scale");
            if (scaleS != null)
            {
                Vector3 localScale = ParseVector(scaleS);
                var expected = RemapVectorAxis(localScale, axisOption);
                var actual = TransformScale(signOption, axisOption, beforeRot, afterRot, child.localScale);
                if (Vector3.Distance(expected, actual) > 0.1f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static Vector3 TransformScale(string signOption, string axisOption, Vector3 beforeRot, Vector3 afterRot, Vector3 scale)
    {
        scale = Quaternion.Euler(beforeRot) * scale;
        scale = ChangeAxisSigns(scale, signOption);
        scale = RemapVectorAxis(scale, axisOption);
        scale = Quaternion.Euler(afterRot) * scale;
        return scale;
    }

    private static void ApplyMapToTransforms(Transform transform, Dictionary<Transform, (Vector3 position, Quaternion rotation, Vector3 scale)> map)
    {
        if (map.ContainsKey(transform))
        {
            var newTransform = map[transform];
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
            transform.localScale = newTransform.scale;
        }

        foreach (Transform child in transform)
        {
            ApplyMapToTransforms(child, map);
        }
    }

    private static Dictionary<Transform, (Vector3 position, Quaternion rotation, Vector3 scale)> GetTransformMap(Transform transform)
    {
        Dictionary<Transform, (Vector3 position, Quaternion rotation, Vector3 scale)> output 
            = new Dictionary<Transform, (Vector3 position, Quaternion rotation, Vector3 scale)>();

        foreach(Transform child in transform)
        {
            AddTransformToMap(child, output);
        }

        return output;
    }

    private static void AddTransformToMap(Transform transform, Dictionary<Transform, (Vector3 position, Quaternion rotation, Vector3 scale)> output)
    {
        Vector3 position = TransformPosition(transform.position, BLENDER_POSITION_SIGN_DIFF, BLENDER_POSITION_AXIS_SWAP, POSITION_BEFORE_ROTATION, POSITION_AFTER_ROTATION);
        Quaternion rotation = TransformRotation(transform.rotation, "xyz", BLENDER_ROTATION_SIGN_DIFF, ROTATION_BEFORE_ROTATION, ROTATION_AFTER_ROTATION);
        Vector3 scale = TransformScale(BLENDER_SCALE_SIGN_DIFF, BLENDER_SIGN_AXIS_SWAP, SCALE_BEFORE_ROTATION, SCALE_AFTER_ROTATION, transform.localScale);

        output[transform] = (position, rotation, scale);

        foreach(Transform child in transform)
        {
            AddTransformToMap(child, output);
        }
    }

    private static bool EvaluateTransformationPosition(string signOption, string axisSwapOption, Vector3 beforeRot, Vector3 afterRot, Transform[] children)
    {
        return false;
        //foreach(var child in children)
        //{
        //    var (pos, euler, forward, up) = GetVectorsFromName(child.name);
        //    Vector3 expected = RemapVectorAxis(pos, "xzy");
        //    Vector3 actual = TransformPosition(child.position, signOption, axisSwapOption, beforeRot, afterRot);
        //    if(Vector3.Distance(expected, actual) > 0.1f)
        //    {
        //        return false;
        //    }
        //}
        //return true;
    }

    private static bool EvaluateTransformationRotation(string signOption, string axisSwapOption, Vector3 beforeRot, Vector3 afterRot, Transform[] children)
    {
        return false;
        //foreach (var child in children)
        //{
        //    var (pos, euler, forward, up) = GetVectorsFromName(child.name);
        //    Quaternion expected = Quaternion.LookRotation(RemapVectorAxis(forward, "xzy"), RemapVectorAxis(up, "xzy"));
        //    Quaternion actual = TransformRotation(child.transform.rotation, axisSwapOption, signOption, beforeRot, afterRot);
        //    float angle = Quaternion.Angle(expected, actual);
        //    if(angle > 10f)
        //    {
        //        return false;
        //    }
        //}
        //return true;
    }

    private static IEnumerable<string> GetAxisPermutations()
    {
        yield return "xzy";
        yield return "xyz";

        yield return "yxz";
        yield return "yzx";

        yield return "zxy";
        yield return "zyx";
    }

    private static IEnumerable<string> GetSignOptions()
    {
        yield return "---";
        yield return "--+";
        yield return "-+-";
        yield return "-++";

        yield return "+--";
        yield return "+-+";
        yield return "++-";
        yield return "+++";
    }

    private static IEnumerable<Vector3> GetRotationOptions()
    {
        float[] angles = new[] { 0f, 90f, 180f, 270f };
        foreach (var x in angles)
            foreach (var y in angles)
                foreach (var z in angles)
                {
                    yield return new Vector3(x, y, z);
                }
    }

    private static Quaternion TransformRotation(Quaternion rot, string axisSwapOption, string signOption, Vector3 beforeRot, Vector3 afterRot)
    {
        rot *= Quaternion.Euler(beforeRot);

        Vector3 forward = rot * Vector3.forward;
        Vector3 up = rot * Vector3.up;

        forward = RemapVectorAxis(forward, axisSwapOption);
        up = RemapVectorAxis(up, axisSwapOption);

        forward = ChangeAxisSigns(forward, signOption);
        up = ChangeAxisSigns(up, signOption);

        rot = Quaternion.LookRotation(forward, up);
        rot *= Quaternion.Euler(afterRot);
        return rot;
    }

    private static Vector3 TransformPosition(Vector3 source, string signOption, string axisSwapOption, Vector3 beforeRot, Vector3 afterRot)
    {
        source = Quaternion.Euler(beforeRot) * source;
        source = RemapVectorAxis(source, axisSwapOption);
        source = ChangeAxisSigns(source, signOption);
        source = Quaternion.Euler(afterRot) * source;

        return source;
    }

    private static Vector3 RemapVectorAxis(Vector3 vector, string map)
    {
        Vector3 output = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            char axis = map[i];
            switch (axis)
            {
                case 'x':
                    output[i] = vector.x;
                    break;
                case 'y':
                    output[i] = vector.y;
                    break;
                case 'z':
                    output[i] = vector.z;
                    break;
            }
        }
        return output;
    }

    private static Vector3 ChangeAxisSigns(Vector3 euler, string signs)
    {
        for (int i = 0; i < 3; i++)
        {
            char sign = signs[i];
            switch (sign)
            {
                case '-':
                    euler[i] = -euler[i];
                    break;
                case '+':
                    euler[i] = euler[i];
                    break;
            }
        }
        return euler;
    }

    private void ProcessTransform(Transform transform)
    {
        //Func<Vector3, Vector3> trasnformPosition = (x) => new Vector3(-x.x, x.y, -x.z);
        Func<Vector3, Vector3> transformVertex = (x) => new Vector3(-x.x, x.z, x.y);
        //Func<Vector3, Vector3> transformVertex = (x) => x;
        //Func<Quaternion, Quaternion> transformRotation = (x) => Quaternion.Euler(90f, 0f, 0f) * x;
        Func<Vector4, Vector4> transformTangent = (x) => new Vector4(-x.x, x.z, x.y, -x.w);

        foreach (Transform child in transform)
        {
            ProcessTransform(child);
        }

        MeshFilter filter = transform.GetComponent<MeshFilter>();

        if (filter)
        {
            Mesh mesh = filter.sharedMesh;
            mesh.vertices = mesh.vertices.Select(transformVertex).ToArray();
            mesh.normals = mesh.normals.Select(transformVertex).ToArray();
            int[] triangles = mesh.triangles;
            mesh.tangents = mesh.tangents.Select(transformTangent).ToArray();

            mesh.RecalculateBounds();
        }
    }

    private void Swap<T>(ref T a, ref T b)
    {
        T temp = b;
        a = b;
        b = temp;
    }

    private string PrintHieharcy(GameObject gameObject, string indentationType = "|--", string currentIndentation = "")
    {
        string output = currentIndentation + gameObject.name;
        foreach (Transform child in gameObject.transform)
        {
            output += "\n" + PrintHieharcy(child.gameObject, indentationType, currentIndentation + indentationType);
        }
        return output;
    }
}
