using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ConsoleClearer 
{
    public static void ClearConsole()
    {
        // This uses reflection to find the internal LogEntries class and calls its Clear method
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }
}
