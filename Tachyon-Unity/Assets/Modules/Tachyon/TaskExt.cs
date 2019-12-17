using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualSerializer
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
            while (task.Status != TaskStatus.RanToCompletion)
                yield return null;

            if (task.IsFaulted)
                if (task.Exception != null)
                    throw task.Exception;

            if (!task.IsCompleted) yield break;

            if (task.Status == TaskStatus.RanToCompletion)
            {
                then?.Invoke(task.Result);
            } else {
                Debug.LogWarning( "Task finished with " + task.Status);
            }

        }

        private static async Task<TOutput> WaitAsync<TOutput>(Task<TOutput> task)
        {
            var result = await task;
            UnityEngine.Debug.Log(result);
            return result;
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