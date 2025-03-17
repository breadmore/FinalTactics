using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneNavigator : EditorWindow
{
    private string[] scenePaths;
    private string[] sceneNames;
    private Vector2 scrollPos;
    private const string scenesFolder = "Assets/Scenes/";

    [MenuItem("Window/Scene Navigator")]
    public static void ShowWindow()
    {
        GetWindow<SceneNavigator>("Scene Navigator");
    }

    private void OnEnable()
    {
        LoadScenes();
    }

    private void LoadScenes()
    {
        scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.StartsWith(scenesFolder))
            .ToArray();
        sceneNames = scenePaths.Select(path => System.IO.Path.GetFileNameWithoutExtension(path)).ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("æ¿ º±≈√ (Assets/Scenes)", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < sceneNames.Length; i++)
        {
            if (GUILayout.Button(sceneNames[i]))
            {
                OpenScene(scenePaths[i]);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
