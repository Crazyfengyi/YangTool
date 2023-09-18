#if UNITY_EDITOR
/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-07 
*/
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public static class EditorCoroutineRunner
{
    private static List<EditorCoroutine> editorCoroutineList;
    private static List<IEnumerator> buffer;

    public static IEnumerator StartEditorCoroutine(IEnumerator iterator)
    {
        if (editorCoroutineList == null)
        {
            editorCoroutineList = new List<EditorCoroutine>();
        }
        if (buffer == null)
        {
            buffer = new List<IEnumerator>();
        }
        if (editorCoroutineList.Count == 0)
        {
            EditorApplication.update += Update;
        }

        buffer.Add(iterator);

        return iterator;
    }

    private static bool Find(IEnumerator iterator)
    {
        foreach (EditorCoroutine editorCoroutine in editorCoroutineList)
        {
            if (editorCoroutine.Find(iterator))
            {
                return true;
            }
        }

        return false;
    }

    private static void Update()
    {
        editorCoroutineList.RemoveAll
        (
            coroutine => { return coroutine.MoveNext() == false; }
        );

        if (buffer.Count > 0)
        {
            foreach (IEnumerator iterator in buffer)
            {
                if (!Find(iterator))
                {
                    editorCoroutineList.Add(new EditorCoroutine(iterator));
                }
            }

            buffer.Clear();
        }

        if (editorCoroutineList.Count == 0)
        {
            EditorApplication.update -= Update;
        }
    }
}
#endif
