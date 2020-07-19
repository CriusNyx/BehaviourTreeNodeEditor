using DynamicBinding;
using GameEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameEngine.AIEditor
{
    public class BehaviourTreeEditor : NodeEditor
    {
        private bool isCached = false;

        [MenuItem("Tools/Behaviour Tree Editor")]
        public static void GetBehaviourTreeEditor()
        {
            GetWindow<BehaviourTreeEditor>();
        }

        public AIExecutionLog log;

        protected override string AutoSaveFileName => "BehaviourTreeEditorAutoSave";

        protected override IEnumerable<(string itemName, string itemTooltip, Action onClick)> GetMenuItems(Vector2 nodeSpawnPosition)
        {
            List<(string, string, Action)> output = new List<(string, string, Action)>();

            output.Add((
                "Root",
                "Creates a new root node",
                () => ConstructNewNode(
                    nodeSpawnPosition,
                    (x, y) => new NodeEditorNode(x, y)
                    {
                        data = new AITreeDefinition()
                    })));

            foreach (var methodInfo in TypeCache.GetMethodsWithAttribute<AIMethodAttribute>())
            {
                var attribute = methodInfo.GetCustomAttribute<AIMethodAttribute>();
                output.Add((
                    methodInfo.DeclaringType + "/" + methodInfo.Name,
                    attribute.tooltip,
                    () => ConstructNewNode(
                        nodeSpawnPosition,
                        (x, y) => new NodeEditorNode(x, y)
                        {
                            data = new MethodBinding(methodInfo)
                        })));
            }
            return output;
        }

        protected override void OnGUIProtected()
        {
            HashSet<NodeEditorNode> visitedNodes = new HashSet<NodeEditorNode>();
            foreach (var node in save.Nodes)
            {
                if (node?.data is AITreeDefinition def)
                {
                    if (def.arguments == null)
                    {
                        def.arguments = new AITreeDefinitionArgument[] { };
                    }
                    ValidateNodesRecursively(
                        node,
                        visitedNodes,
                        def.arguments.Select(x => x.value).ToArray(),
                        TypeCache
                            .GetTypesWithAttribute<BindableEnumAttribute>()
                            .SelectMany(
                                x => Enum.GetValues(x).Cast<Enum>())
                            .ToArray());
                }
            }
        }

        private void ValidateNodesRecursively(NodeEditorNode node, HashSet<NodeEditorNode> visitedNodes, string[] arguments, Enum[] enumArguments)
        {
            // Prevents infinate recursion on bad tree
            if (visitedNodes.Contains(node))
            {
                return;
            }
            visitedNodes.Add(node);


            if (node.data is MethodBinding binding)
            {
                binding.ValidateArgs(arguments, enumArguments);
            }
            foreach (var child in node.GetChildren())
            {
                ValidateNodesRecursively(child, visitedNodes, arguments, enumArguments);
            }
        }

        private void WaitForRegenerateUpdate()
        {
            if (isCached)
            {
                EditorApplication.update -= WaitForRegenerateUpdate;
                RegenerateContextMenu();
            }
        }

        protected override void DrawTopBar()
        {
            GUILayout.BeginHorizontal();
            DrawFileBar();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Debug Print", GUILayout.Width(200)))
            {
                foreach (var node in save.Nodes)
                {
                    if (node.data is AITreeDefinition)
                    {
                        var root = ConstructorTreeFromDef(node);
                        Debug.Log(root.ToString());
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        protected override void DrawSideBar()
        {
            foreach (var node in save.Nodes)
            {
                if (node?.data is AITreeDefinition def)
                {
                    var root = ConstructorTreeFromDef(node, out var treeToEditorNodeMap);
                    if (GUILayout.Button(def.name))
                    {
                        JumpCameraToNode(node);
                    }
                    DrawAINodeEntry(root, treeToEditorNodeMap, 1);
                    GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
                }
            }

            if (log != null)
            {
                GUILayout.Label(log.ToString());
            }
        }

        private void DrawAINodeEntry(AITreeNode node, Dictionary<AITreeNode, NodeEditorNode> treeToEditorNodeMap, int indentLevel)
        {
            if (node == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10 * indentLevel);
                if (GUILayout.Button(node.methodBinding.ToString()))
                {
                    JumpCameraToNode(treeToEditorNodeMap[node]);
                }
            }
            GUILayout.EndHorizontal();
            foreach (var child in node.children)
            {
                DrawAINodeEntry(child, treeToEditorNodeMap, indentLevel + 1);
            }
        }

        private AITreeNode ConstructorTreeFromDef(NodeEditorNode definitionNode)
        {
            return ConstructorTreeFromDef(definitionNode, out _);
        }

        private AITreeNode ConstructorTreeFromDef(NodeEditorNode definitionNode, out Dictionary<AITreeNode, NodeEditorNode> resultDictionary)
        {
            var children = definitionNode.GetChildren();
            if (children.Length > 0)
            {
                return ConstructTree(children[0], out resultDictionary);
            }
            else
            {
                resultDictionary = null;
                return null;
            }
        }

        private AITreeNode ConstructTree(NodeEditorNode node)
        {
            return ConstructTree(node, out _);
        }

        private AITreeNode ConstructTree(NodeEditorNode node, out Dictionary<AITreeNode, NodeEditorNode> resultDictionary)
        {
            resultDictionary = new Dictionary<AITreeNode, NodeEditorNode>();
            return ConstructTree(node, resultDictionary);
        }

        private AITreeNode ConstructTree(NodeEditorNode node, Dictionary<AITreeNode, NodeEditorNode> resultDictionary)
        {
            if (node.data is MethodBinding methodBinding)
            {
                AITreeNode output = new AITreeNode(
                    methodBinding,
                    node.GUID,
                    node.GetChildren().Select(x => ConstructTree(x, resultDictionary)).ToArray()); ;
                resultDictionary[output] = node;
                return output;
            }
            return null;
        }

        protected override void OnSave(string filename)
        {
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            string dirName = Path.GetDirectoryName(filename);

            foreach (var node in save.Nodes)
            {
                if (node.data is AITreeDefinition def)
                {
                    AITreeNode treeNode = ConstructorTreeFromDef(node);
                    string treePath = $"{dirName}/{filenameWithoutExtension}.{def.name}.asset";
                    AITreeAsset aITreeAsset = CreateInstance<AITreeAsset>();
                    aITreeAsset.root = treeNode;
                    aITreeAsset.sourceFileName = filename;
                    aITreeAsset.definition = def;

                    AssetManagement.CreateOrOverwriteAsset(treePath, aITreeAsset);
                }
            }
        }

        protected override void DrawAllLines(Vector2 offset)
        {
            base.DrawAllLines(offset);
            if (log != null)
            {
                var guidMap = save.Nodes.ToDictionary(x => x.GUID, x => x);
                foreach ((var current, var next) in log.Entires.ForeachElementAndNext())
                {

                    Color arrowColor = next.FrameNumber == Time.frameCount ? Color.green : Color.red;

                    var nodeA = guidMap[current.GUID];
                    var nodeB = guidMap[next.GUID];
                    if (nodeA != null && nodeB != null)
                    {
                        Vector2 posA = nodeA.Position;
                        Vector2 posB = nodeB.Position;
                        Vector2 mid = Vector2.Lerp(posA, posB, 0.5f);

                        Vector2 dir = posB - posA;
                        dir = new Vector2(-dir.y, dir.x);
                        mid = mid + dir * 0.5f;

                        var curve = Bezier.GetBezier2(new Vector2[] { posA, mid, posB }, 10);

                        foreach ((var start, var end) in curve.ForeachElementAndNext())
                        {
                            Vector2 a = start + cameraPosition + offset;
                            Vector2 b = end + cameraPosition + offset;

                            if (CenterRectLastFrame.Contains(a) && CenterRectLastFrame.Contains(b))
                            {
                                Drawing.DrawLine(a, b, arrowColor, 2f);
                            }
                        }

                        {
                            Vector2 lastPoint = curve[curve.Length - 1] + cameraPosition + offset;
                            Vector2 previousPoint = curve[curve.Length - 2] + cameraPosition + offset;

                            Vector2 dir2 = (lastPoint - previousPoint).normalized;
                            Vector2 dir3 = new Vector2(-dir2.y, dir2.x);

                            Vector2 arrowPointA = lastPoint - dir2 * 20f + dir3 * 20f;
                            Vector2 arrowPointB = lastPoint - dir2 * 20f - dir3 * 20f;

                            if (CenterRectLastFrame.Contains(lastPoint) && CenterRectLastFrame.Contains(arrowPointA))
                            {
                                Drawing.DrawLine(lastPoint, arrowPointA, arrowColor, 2f);
                            }

                            if (CenterRectLastFrame.Contains(lastPoint) && CenterRectLastFrame.Contains(arrowPointB))
                            {
                                Drawing.DrawLine(lastPoint, arrowPointB, arrowColor, 2f);
                            }
                        }
                    }
                }
            }
        }

        public static NodeEditor OpenFile(string filename, AIExecutionLog log)
        {
            var output = OpenFile(filename);
            if (output is BehaviourTreeEditor behaviourTreeEditor)
            {
                behaviourTreeEditor.log = log;
            }
            return output;
        }
    }
}
