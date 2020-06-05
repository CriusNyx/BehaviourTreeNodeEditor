using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GameEngine.AI
{
    /// <summary>
    /// Executes an AI Tree at runtime
    /// </summary>
    public class AIExecutor : MonoBehaviour
    {
        TaskCompletionSource<AIResult> result;
        IEnumerator<AIResult> executionTree;
        AIResult lastResult;

        public AIExecutionLog Log { get; } = new AIExecutionLog();
        public AITreeAsset asset;

        private Dictionary<string, object> arguments;

        public static Task<AIResult> Exec(GameObject gameObject, AITreeAsset asset, Dictionary<string, object> arguments)
        {
            return Exec(gameObject, asset.root, arguments, asset);
        }

        public static Task<AIResult> Exec(GameObject gameObject, AITreeNode tree, Dictionary<string, object> arguments, AITreeAsset asset = null)
        {
            if (gameObject.GetComponent<BaseAIBehaviours>() == null)
            {
                gameObject.AddComponent<BaseAIBehaviours>();
            }

            TaskCompletionSource<AIResult> result = new TaskCompletionSource<AIResult>();

            var executor = gameObject.AddComponent<AIExecutor>();
            executor.result = result;
            executor.asset = asset;
            executor.arguments = arguments;

            executor.executionTree =
                tree
                    .Call(
                        new AIExecutionContext(
                            gameObject,
                                executor.Log,
                                null,
                                new TreeArguments(
                                    executor.arguments,
                                    gameObject.GetComponent<AIMemory>())))
                    .GetEnumerator();

            return result.Task;
        }

        private void Update()
        {
            if (executionTree != null)
            {
                if (executionTree.MoveNext())
                {
                    lastResult = executionTree.Current;

                }
                else
                {
                    result.TrySetResult(lastResult);
                    Destroy(this);
                }
            }
        }
    }
}