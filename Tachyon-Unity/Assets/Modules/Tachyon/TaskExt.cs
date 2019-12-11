using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace DefaultNamespace
{
    public static class TaskExt
    {
        public static Coroutine Then(this Task task, Action then)
        {
            return Coroutiner.StartCoroutine(WaitForTask(task, then));
        }

        public static Coroutine Then<TOutput>(this Task<TOutput> task, Action<TOutput> then)
        {
            return Coroutiner.StartCoroutine(WaitForTask(task, then));
        }

        static IEnumerator WaitForTask<TOutput>(Task<TOutput> task, Action<TOutput> then)
        {
            
            while (task.Status == TaskStatus.Running)
                yield return null;

            if (task.IsFaulted)
                if (task.Exception != null)
                    throw task.Exception;

            if (!task.IsCompleted) yield break;
            
            TOutput result = task.Result;
            then?.Invoke(result);
        }

        static IEnumerator WaitForTask(Task task, Action then)
        {
            while (task.Status == TaskStatus.Running) {
                yield return null;
            }
            
            then.Invoke();
        }
    }
}