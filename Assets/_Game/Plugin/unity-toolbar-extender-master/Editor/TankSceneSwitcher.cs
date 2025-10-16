#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class TankSceneSwitcher
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Normal,
                fixedWidth = 70,
            };
        }
    }

    static TankSceneSwitcher()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI_Right);
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Splash", "Start Scene Splash"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("Splash");
        }
        if (GUILayout.Button(new GUIContent("LoadingScene", "Start Scene Loading Scene"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("LoadingScene");
        }

        if (GUILayout.Button(new GUIContent("HomeScene", "Start Scene Main UI"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("HomeScene");
        }
        //if (GUILayout.Button(new GUIContent("BRImporter", "Start Scene BRImporterTest"), ToolbarStyles.commandButtonStyle))
        //{
        //    SceneHelper.StartScene("BRImporterTest");
        //}

    }

    static void OnToolbarGUI_Right()
    {
        if (GUILayout.Button(new GUIContent("GameScene", "Start Scene Gameplay"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("GameScene");
        }
        if (GUILayout.Button(new GUIContent("ToolScene", "Start Scene Gameplay"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("ToolScene");
        }
        if (GUILayout.Button(new GUIContent("TestScene", "Start Scene Gameplay"), ToolbarStyles.commandButtonStyle))
        {
            SceneHelper.StartScene("TestScene");
        }
    }

    static class SceneHelper
    {
        static string sceneToOpen;

        public static void StartScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            sceneToOpen = sceneName;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // need to get scene via search because the path to the scene
                // file contains the package version so it'll change over time
                string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
                if (guids.Length == 0)
                {
                    Debug.LogWarning("Couldn't find scene file");
                }
                else
                {
                    string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    EditorSceneManager.OpenScene(scenePath);
                    //EditorApplication.isPlaying = true;
                }
            }
            sceneToOpen = null;
        }
    }
}

#endif