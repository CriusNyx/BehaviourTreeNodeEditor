using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void CreateMainThraedDispatcher()
    {
        instance = new GameObject("Main Thread Dispatcher").AddComponent<MainThreadDispatcher>();
    }

    private static MainThreadDispatcher instance;

    private object queueLock = new object();

    private Queue<Action>
        queue = new Queue<Action>(),
        processingQueue = new Queue<Action>();

    private void Update()
    {
        lock (queueLock)
        {
            while (queue.Count > 0)
            {
                processingQueue.Enqueue(queue.Dequeue());
            }
        }
        while (processingQueue.Count > 0)
        {
            try
            {
                processingQueue.Dequeue()();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public static Task Dispatch(Action action)
    {
        TaskCompletionSource<object> source = new TaskCompletionSource<object>();
        lock (instance.queueLock)
        {
            instance.queue.Enqueue(() =>
           {
               action();
               source.SetResult(null);
           });
        }
        return source.Task;
    }

    public static Task<T> Dispatch<T>(Func<T> func)
    {
        TaskCompletionSource<T> source = new TaskCompletionSource<T>();

        lock (instance.queueLock)
        {
            instance.queue.Enqueue(() => source.SetResult(func()));
        }

        return source.Task;
    }
}