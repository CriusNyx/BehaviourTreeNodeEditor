using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DynamicBinding;

namespace GameEngine.AI
{
    public class BaseAIBehaviours : MonoBehaviour
    {
        [AIMethod(tooltip: "Executes each node in order until one fails.")]
        public IEnumerable<AIResult> Sequence(AIExecutionContext context)
        {
            bool failOut = false;

            foreach (var child in context.children)
            {
                foreach (var result in child.Call(context))
                {
                    if (result == AIResult.Running)
                    {
                        yield return AIResult.Running;
                    }
                    else if (result == AIResult.Success)
                    {
                        continue;
                    }
                    else if (result == AIResult.Failure)
                    {
                        failOut = true;
                    }

                    if (failOut)
                        break;
                }

                if (failOut)
                    break;
            }

            yield return failOut ? AIResult.Failure : AIResult.Success;
        }

        [AIMethod(tooltip: "Executes each node in sequence until one succeeds")]
        public IEnumerable<AIResult> Selector(AIExecutionContext context)
        {
            bool succeedOut = false;

            foreach (var child in context.children)
            {
                foreach (var result in child.Call(context))
                {
                    if (result == AIResult.Running)
                    {
                        yield return AIResult.Running;
                    }
                    else if (result == AIResult.Success)
                    {
                        succeedOut = true;
                    }
                    else if (result == AIResult.Failure)
                    {
                        continue;
                    }

                    if (succeedOut)
                        break;
                }

                if (succeedOut)
                    break;
            }

            yield return succeedOut ? AIResult.Success : AIResult.Failure;
        }

        [AIMethod(tooltip: "Get's the result from the first child. If true executes the second child, otherwise executes the third")]
        public IEnumerable<AIResult> IfElse(AIExecutionContext context)
        {
            GameObject gameObject = context.gameObject;
            AITreeNode[] children = context.children;
            AIExecutionLog log = context.log;
            IReadOnlyDictionary<object, object> memoryMap = context.memoryMap;

            AITreeNode ifChild = null, thenChild = null, elseChild = null;
            if (children.Length >= 1)
            {
                ifChild = children[0];
            }
            if (children.Length >= 2)
            {
                thenChild = children[1];
            }
            if (children.Length >= 3)
            {
                elseChild = children[2];
            }

            bool? success = false;

            if (ifChild != null)
            {
                bool breakIfElseLoop = false;
                foreach (var result in ifChild.Call(context))
                {
                    switch (result)
                    {
                        case AIResult.Success:
                            success = true;
                            breakIfElseLoop = true;
                            break;
                        case AIResult.Failure:
                            success = false;
                            breakIfElseLoop = true;
                            break;
                        case AIResult.Running:
                            break;
                    }

                    if (breakIfElseLoop)
                    {
                        break;
                    }
                }
            }

            if (success == null)
            {
                yield return AIResult.Failure;
            }
            else
            {
                foreach (var result in ProcessIfThenChild(success.Value ? thenChild : elseChild, context))
                {
                    // Process if then child method will ensure appropriate returns
                    yield return result;
                }
            }
        }

        private IEnumerable<AIResult> ProcessIfThenChild(AITreeNode node, AIExecutionContext context)
        {
            if (node == null)
            {
                yield return AIResult.Failure;
            }

            bool breakLoop = false;

            foreach (var result in node.Call(context))
            {
                switch (result)
                {
                    case AIResult.Success:
                        yield return AIResult.Success;
                        breakLoop = true;
                        break;
                    case AIResult.Failure:
                        yield return AIResult.Failure;
                        breakLoop = true;
                        break;
                    case AIResult.Running:
                        yield return AIResult.Running;
                        break;
                }

                if (breakLoop)
                {
                    break;
                }
            }
        }

        [AIMethod(tooltip: "Node always succeeds")]
        public IEnumerable<AIResult> Succeed(AIExecutionContext context)
        {
            foreach (var child in context.children)
            {
                foreach (var result in child.Call(context))
                {
                    if (result == AIResult.Running)
                    {
                        yield return result;
                    }
                }
            }
            yield return AIResult.Success;
        }

        [AIMethod(tooltip: "Node always fails")]
        public IEnumerable<AIResult> Failure(AIExecutionContext context)
        {
            Debug.Log("Failure");
            yield return AIResult.Failure;
        }

        [AIMethod(tooltip: "Node pauses tree execution x frames")]
        public IEnumerable<AIResult> Running(AIExecutionContext context, int frameCount, float timeCount)
        {
            var children = context.children;
            var log = context.log;
            var memoryMap = context.memoryMap;
            var gameObject = context.gameObject;

            float startTime = Time.time;

            for (int i = 0; i < frameCount; i++)
            {
                yield return AIResult.Running;
            }

            while (Time.time <= startTime + timeCount)
            {
                yield return AIResult.Running;
            }

            if (children.Length > 0)
            {
                var firstChild = children[0];
                if (firstChild != null)
                {
                    bool breakChildLoop = false;

                    foreach (var result in firstChild.Call(context))
                    {
                        switch (result)
                        {
                            case AIResult.Success:
                            case AIResult.Failure:
                                yield return result;
                                breakChildLoop = true;
                                break;
                            case AIResult.Running:
                                yield return AIResult.Running;
                                break;
                        }
                        if (breakChildLoop)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [AIMethod("Prints the cell. Returns success if the cell is not null.")]
        public IEnumerable<AIResult> PrintMemoryCell(AIExecutionContext context, object cell)
        {
            Debug.Log($"Printing Cell: {cell?.ToString()}");
            if (cell != null)
            {
                yield return AIResult.Success;
            }
            else
            {
                yield return AIResult.Failure;
            }
        }

        [AIMethod("Executes it's children x number of times")]
        public IEnumerable<AIResult> Loop(AIExecutionContext context, int count)
        {
            var children = context.children;
            var log = context.log;
            var memoryMap = context.memoryMap;
            var gameObject = context.gameObject;

            bool failBreak = false;
            for (int i = 0; i < count; i++)
            {
                foreach (var child in children)
                {
                    foreach (var result in child.Call(context))
                    {
                        switch (result)
                        {
                            case AIResult.Success:
                                continue;
                            case AIResult.Failure:
                                failBreak = true;
                                break;
                            case AIResult.Running:
                                yield return AIResult.Running;
                                break;
                        }
                        if (failBreak)
                            break;
                    }
                    if (failBreak)
                        break;
                }
                if (failBreak)
                    break;
            }

            yield return failBreak ? AIResult.Failure : AIResult.Success;
        }

        [AIMethod("Call another tree")]
        [ParamsDataSource(typeof(BaseAIBehaviours), nameof(GetCallParams))]
        public IEnumerable<AIResult> Call(AIExecutionContext context, AITreeAsset asset, params (string argName, object argValue)[] arguments)
        {
            return asset.root.Call(new AIExecutionContext(context.gameObject, context.log, null, new TreeArguments(arguments.ToDictionary(x => x.argName, x => x.argValue), context.memoryMap.aiMemory)));
        }

        private static IEnumerable<(string name, Type type)> GetCallParams(AITreeAsset asset)
        {
            if (asset == null || asset?.definition?.arguments == null)
            {
                return new (string, Type)[] { };
            }
            else
            {
                return asset.definition.arguments.Select(x => (x.value, TypeEnumAttribute.ResolveType(x.type)));
            }
        }

        public static IEnumerable<AIResult> WrapTask(Task<AIResult> task)
        {
            while (!task.IsCompleted)
            {
                yield return AIResult.Running;
            }
            yield return task.Result;
        }
    }
}