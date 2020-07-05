using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MogulTech.Utilities
{
    /// <summary>
    /// Coroutines for editor scripting.
    /// It's a little different from regular coroutines. Things like WaitForSeconds don't work, 
    /// it always waits just one frame.But you can "cascade" coroutines by just yielding the 
    /// call to the second coroutine.You can also specify an (optional) OnUpdate method, which 
    /// gets called every time the coroutine gets updated.
    ///
    /// I use this for splitting up a lengthy operation over multiple frames, so I can use 
    /// EditorUtility.DisplayProgressBar to show progress (which I update in the OnUpdate callback).
    /// 
    /// Example Use:
    /// <code>
    /// [InitializeOnLoad]
    /// public static class CoroutineTester
    /// {
    ///    static CoroutineTester()
    ///    {
    ///        EditorCoroutines.Execute(Test());
    ///    }
    ///    
    ///    static IEnumerator Test()
    ///    {
    ///       Debug.Log("Test");
    ///       yield return Test2();
    ///       Debug.Log("Test done");
    ///    }
    ///    
    ///    static IEnumerator Test2()
    ///    {
    ///        Debug.Log("Test2");
    ///        yield return Test3();
    ///        Debug.Log("Test2 done");
    ///    }
    ///    static IEnumerator Test3()
    ///    {
    ///        Debug.Log("Test3");
    ///        yield return 0;
    ///        Debug.Log("Test3 done");
    ///    }
    /// }
    /// </code>
    /// 
    /// Original code from: https://forum.unity.com/threads/editor-coroutines.589504/
    /// </summary>
    public static class EditorCoroutines
    {
        public class Coroutine
        {
            public IEnumerator enumerator;
            public System.Action<bool> OnUpdate;
            public List<IEnumerator> history = new List<IEnumerator>();
        }

        static readonly List<Coroutine> coroutines = new List<Coroutine>();

        public static void Execute(IEnumerator enumerator, System.Action<bool> OnUpdate = null)
        {
            if (coroutines.Count == 0)
            {
                EditorApplication.update += Update;
            }
            var coroutine = new Coroutine { enumerator = enumerator, OnUpdate = OnUpdate };
            coroutines.Add(coroutine);
        }

        static void Update()
        {
            for (int i = 0; i < coroutines.Count; i++)
            {
                var coroutine = coroutines[i];
                bool done = !coroutine.enumerator.MoveNext();
                if (done)
                {
                    if (coroutine.history.Count == 0)
                    {
                        coroutines.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        done = false;
                        coroutine.enumerator = coroutine.history[coroutine.history.Count - 1];
                        coroutine.history.RemoveAt(coroutine.history.Count - 1);
                    }
                }
                else
                {
                    if (coroutine.enumerator.Current is IEnumerator)
                    {
                        coroutine.history.Add(coroutine.enumerator);
                        coroutine.enumerator = (IEnumerator)coroutine.enumerator.Current;
                    }
                }
                if (coroutine.OnUpdate != null) coroutine.OnUpdate(done);
            }
            if (coroutines.Count == 0) EditorApplication.update -= Update;
        }

        internal static void StopAll()
        {
            coroutines.Clear();
            EditorApplication.update -= Update;
        }

    }
}