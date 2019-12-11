using System.Collections;
using UnityEngine;

/// <summary>
/// Author: 		Sebastiaan Fehr (Seb@TheBinaryMill.com)
/// Date: 			March 2013
/// Summary:		Creates MonoBehaviour instance through which 
///                 static classes can call StartCoroutine.
/// Description:    Classes that do not inherit from MonoBehaviour, or static 
///                 functions within MonoBehaviours are inertly unable to 
///                 call StartCoroutene, as this function is not static and 
///                 does not exist on Object. This Class creates a proxy though
///                 which StartCoroutene can be called, and destroys it when 
///                 no longer needed.
/// Extended:       July 2018
///                 - Added StopCoroutine.
/// </summary>
public class Coroutiner {

    public static Coroutine StartCoroutine(IEnumerator iterationResult) {
        var routeneHandler = CreateCoroutinerInstance();
        return routeneHandler.ProcessWork(iterationResult);
    }

    private static CoroutinerInstance CreateCoroutinerInstance() {
        //Create GameObject with MonoBehaviour to handle task.
        GameObject routeneHandlerGo = new GameObject("Coroutiner");
        var routeneHandler = routeneHandlerGo
            .AddComponent(typeof(CoroutinerInstance))
                as CoroutinerInstance;
        return routeneHandler;
    }

}

public class CoroutinerInstance : MonoBehaviour {

    public Coroutine _routine;

    public void StopCoroutine() {
        base.StopCoroutine(_routine);
        Destroy(gameObject);
    }

    public Coroutine ProcessWork(IEnumerator iterationResult) {
        _routine = StartCoroutine(DestroyWhenComplete(iterationResult));
        return _routine;
    }

    public IEnumerator DestroyWhenComplete(IEnumerator iterationResult) {
        yield return StartCoroutine(iterationResult);
#if UNITY_EDITOR
        DestroyImmediate(gameObject);
#else
        Destroy(gameObject);
#endif

    }

}
