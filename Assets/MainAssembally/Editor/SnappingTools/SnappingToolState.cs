using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SnappingToolState
{
    GameObject selectedGameObject;
    SnappingTool snappingTool;

    SnappingSetOwner selectedHandle;
    SnappingSet[] sets;
    Camera lastCamera;

    MeshFilter selectedFilter;
    int selectedIndex;

    Snapper options = null;

    bool isDragging = false;

    (MeshFilter filter, int index) hoveredControl;

    public SnappingToolState(GameObject selectedGameObject, SnappingTool snappingTool)
    {
        this.selectedGameObject = selectedGameObject;
        this.snappingTool = snappingTool;

        selectedHandle = selectedGameObject?.GetComponent<SnappingSetOwner>();
        sets = selectedHandle?.GetComponentsInChildren<SnappingSet>();
        if (sets == null) sets = new SnappingSet[] { };
    }

    public void DrawSceneGUI()
    {
        if (Camera.current != null)
            lastCamera = Camera.current;

        if (selectedHandle != null)
        {
            if (isDragging)
                DrawDragMode();
            else
                DrawIdleMode();
        }
    }

    public void DrawIdleMode()
    {
        Dictionary<int, (MeshFilter filter, int vertex)> controlMap
            = new Dictionary<int, (MeshFilter filter, int vertexIndes)>();

        DrawSnapPoints(new SnappingSetOwner[] { selectedHandle }, true, false, controlMap);
        DrawSelectedSnapPoint();
        ProcessIdleModeEvent(controlMap);

        hoveredControl = GetCurerntControl(controlMap);
    }

    private void DrawSelectedSnapPoint()
    {
        if (selectedFilter != null)
        {
            var (point, normal) = Snapper.GetSnapPoint(selectedFilter, selectedIndex);
            using (new Handles.DrawingScope(Color.green))
            {
                DrawVertexControl(point, normal);
            }
            Quaternion newRotation = DrawRotationControl(point, normal, selectedGameObject.transform.rotation);

            if(newRotation != selectedGameObject.transform.rotation)
            {
                Undo.RecordObject(selectedGameObject.transform, $"Rotate {selectedGameObject.name}");
                Quaternion diff = newRotation * Quaternion.Inverse(selectedGameObject.transform.rotation);
                Vector3 localPosition = selectedGameObject.transform.position - point;
                localPosition = diff * localPosition;
                selectedGameObject.transform.rotation = diff * selectedGameObject.transform.rotation;
                selectedGameObject.transform.position = point + localPosition;
            }
        }
    }

    private void ProcessIdleModeEvent(Dictionary<int, (MeshFilter filter, int vertex)> controlMap)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            int currentIndex = HandleUtility.nearestControl;
            if (controlMap.ContainsKey(currentIndex))
            {
                SelectVertexControl(controlMap);
            }
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
        {
            Undo.RecordObject(selectedGameObject.transform, $"Snap {selectedGameObject.name}");
            options?.Snap(selectedHandle.gameObject, snappingTool.settings.snapMode);
            options?.Itterate();
        }
    }

    private void SelectVertexControl(Dictionary<int, (MeshFilter filter, int vertex)> controlMap)
    {
        (selectedFilter, selectedIndex) = GetCurerntControl(controlMap);
        if (selectedFilter != null)
        {
            options = new Snapper(selectedHandle, selectedFilter, selectedIndex);
            isDragging = true;
        }

    }

    public void DrawDragMode()
    {
        var (currentSnapPoint, currentSnapNormal)
               = Snapper.GetSnapPoint(selectedFilter, selectedIndex);

        // Draw selected vertex control
        DrawVertexControl(currentSnapPoint, currentSnapNormal);

        DrawDragLineToCursor(currentSnapPoint);

        var controlMap = new Dictionary<int, (MeshFilter controlFilter, int controlVertex)>();

        DrawSnapPoints(Object.FindObjectsOfType<SnappingSetOwner>().Where(x => x != selectedHandle), false, true, controlMap);
        ProcessDragModeEvents(controlMap);

        hoveredControl = GetCurerntControl(controlMap);
    }

    private void ProcessDragModeEvents(Dictionary<int, (MeshFilter controlFilter, int controlVertex)> controlMap)
    {
        if (Event.current.type == EventType.MouseUp)
        {
            isDragging = false;
            var (targetFilter, targetIndex) = GetCurerntControl(controlMap);
            SnapObjectToTarget(targetFilter, targetIndex);
        }
    }

    private void SnapObjectToTarget(MeshFilter targetFilter, int targetIndex)
    {
        if (selectedFilter && targetFilter)
        {
            Undo.RecordObject(selectedGameObject.transform, $"Snap {selectedGameObject.name}");
            Snapper.Snap(selectedGameObject, snappingTool.settings.snapMode, selectedFilter, selectedIndex, targetFilter, targetIndex);
        }
    }

    private void DrawDragLineToCursor(Vector3 currentSnapPoint)
    {
        if (lastCamera != null)
        {
            Vector2 currentMousePos = Event.current.mousePosition;

            currentMousePos.y = lastCamera.pixelHeight - currentMousePos.y;

            Ray ray = lastCamera.ScreenPointToRay(currentMousePos);
            Vector3 target = LinePlaneIntersection(ray.origin, ray.direction, currentSnapPoint, lastCamera.transform.forward);
            Handles.DrawLine(currentSnapPoint, target);
        }
    }

    private void DrawVertexControl(Vector3 vertexPoint, Vector3 vertexNormal, int? id = null)
    {
        if (id == null)
        {
            Handles.DrawWireDisc(vertexPoint, vertexNormal, 0.1f);
        }
        else
        {
            Handles.CircleHandleCap(id.Value, vertexPoint, Quaternion.LookRotation(vertexNormal), 0.1f, Event.current.type);
        }
        Handles.DrawLine(vertexPoint, vertexPoint + vertexNormal * 0.1f);
    }

    private Quaternion DrawRotationControl(Vector3 vertexPoint, Vector3 vertexNormal, Quaternion rotation)
    {
        return Handles.Disc(rotation, vertexPoint, vertexNormal, 1f, false, 15f);
    }

    private Vector3 LinePlaneIntersection(Vector3 linePoint, Vector3 lineNormal, Vector3 planePoint, Vector3 planeNormal)
    {
        float alpha = (Vector3.Dot(planePoint, planeNormal) - Vector3.Dot(planeNormal, linePoint)) / Vector3.Dot(planeNormal, lineNormal);
        return alpha * lineNormal + linePoint;
    }

    private void DrawSnapPoints(IEnumerable<SnappingSetOwner> handles, bool drawDiskStyle, bool drawSphereStyle, Dictionary<int, (MeshFilter controlFilter, int controlVertex)> controlMap = null)
    {
        var controlList
            = new List<(
                MeshFilter filter,
                int index,
                Vector3 snapPoint,
                Vector3 snapNormal)>();

        foreach (var handle in handles)
        {
            foreach (var snappingSet in handle.GetComponentsInChildren<SnappingSet>())
            {
                foreach (var filter in snappingSet.GetComponentsInChildren<MeshFilter>())
                {
                    for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                    {
                        var (snapPoint, snapNormal) = Snapper.GetSnapPoint(filter, i);
                        controlList.Add((filter, i, snapPoint, snapNormal));
                    }
                }
            }
        }

        controlList
            = controlList
            .OrderBy(x => -Vector3.Distance(lastCamera.transform.position, x.snapPoint))
            .ToList();

        foreach (var (filter, i, snapPoint, snapNormal) in controlList)
        {
            if (drawDiskStyle)
            {
                if (controlMap != null)
                {
                    int controlId = GUIUtility.GetControlID(FocusType.Passive);
                    controlMap[controlId] = (filter, i);
                    Color drawingColor = (filter, i) == hoveredControl ? Color.red : Color.white;
                    using (new Handles.DrawingScope(drawingColor))
                    {
                        DrawVertexControl(snapPoint + snapNormal * 0.25f, snapNormal, controlId);
                    }
                }
                else
                {
                    Color drawingColor = (filter, i) == hoveredControl ? Color.red : Color.white;
                    using (new Handles.DrawingScope(drawingColor))
                    {
                        DrawVertexControl(snapPoint + snapNormal * 0.25f, snapNormal);
                    }
                }
            }
            if (drawSphereStyle)
            {
                if (controlMap != null)
                {
                    int controlId = GUIUtility.GetControlID(FocusType.Passive);
                    controlMap[controlId] = (filter, i);
                    Color drawingColor = (filter, i) == hoveredControl ? Color.red : Color.white;
                    using (new Handles.DrawingScope(drawingColor))
                    {
                        Handles.SphereHandleCap(controlId, snapPoint, Quaternion.identity, 0.25f, Event.current.type);
                    }
                }
                else
                {
                    Color drawingColor = (filter, i) == hoveredControl ? Color.red : Color.white;
                    using (new Handles.DrawingScope(drawingColor))
                    {
                        Handles.SphereHandleCap(-1, snapPoint, Quaternion.identity, 0.25f, Event.current.type);
                    }
                }
            }
        }
    }

    public (MeshFilter filter, int index) GetCurerntControl(Dictionary<int, (MeshFilter filter, int index)> controlMap)
    {
        if (controlMap.ContainsKey(HandleUtility.nearestControl))
        {
            return controlMap[HandleUtility.nearestControl];
        }
        else
        {
            return (null, -1);
        }
    }
}
