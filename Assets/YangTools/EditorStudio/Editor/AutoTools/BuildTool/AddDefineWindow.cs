using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class EditorDefineSymbolWindow : ScriptableWizard
{
    public DefineSymbol symbol;
    private Action onCreateCallBack;

    public static void DisplayWizard(string title, string btnStr, DefineSymbol symbol, Action onCreate)
    {
        var wizard = ScriptableWizard.DisplayWizard<EditorDefineSymbolWindow>(title, btnStr);
        wizard.symbol = symbol;
        wizard.onCreateCallBack = onCreate;
    }

    void OnWizardCreate()
    {
        if (symbol != null)
        {
            onCreateCallBack?.Invoke();
        }
    }
}