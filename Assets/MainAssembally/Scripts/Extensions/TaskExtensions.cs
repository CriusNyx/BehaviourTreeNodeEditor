using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskExtensions
{
    public static Task Then(this Task task, Action action)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
                action();
        });
    }

    public static Task<T> Then<T>(this Task task, Func<T> func)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
                return func();
            else
                return default;
        });
    }

    public static Task Then<T>(this Task<T> task, Action<T> action)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
            {
                var result = task.Result;
                action(result);
            }
        });
    }

    public static Task<U> Then<T, U>(this Task<T> task, Func<T, U> func)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
            {
                var result = task.Result;
                return func(result);
            }
            else
                return default;
        });
    }

    public static Task ThenOnMainThread(this Task task, Action action)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
                MainThreadDispatcher.Dispatch(action).Wait();
        });
    }

    public static Task<T> ThenOnMainThread<T>(this Task task, Func<T> func)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
                return MainThreadDispatcher.Dispatch(func).Result;
            return default;
        });
    }

    public static Task ThenOnMainThread<T>(this Task<T> task, Action<T> action)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
            {
                var result = task.Result;
                MainThreadDispatcher.Dispatch(() => action(result)).Wait();
            }
        });
    }

    public static Task<U> ThenOnMainThread<T, U>(this Task<T> task, Func<T, U> func)
    {
        return Task.Run(() =>
        {
            task.Wait();
            if (task.IsCompleted)
            {
                var result = task.Result;
                return MainThreadDispatcher.Dispatch(() => func(result)).Result;
            }
            else
            {
                return default;
            }
        });
    }
}