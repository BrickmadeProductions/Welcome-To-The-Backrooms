using UnityEngine;
using UnityEditor;
using System.Collections;
#if (UNITY_EDITOR) 
[InitializeOnLoad]
public static class LoadSceneOnPressingPlay
{

    [SerializeField]
    public static string oldScene;

    static LoadSceneOnPressingPlay()
    {
        EditorApplication.playmodeStateChanged += StateChange;
    }

    static void StateChange()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.playmodeStateChanged -= StateChange;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //We're in playmode, just about to change playmode to Editor
                Debug.Log("Loading original level");
                EditorApplication.OpenScene(oldScene);
            }
            else
            {
                //We're in playmode, right after having pressed Play
                oldScene = EditorApplication.currentScene;
                Debug.Log("Loading first level");
                Application.LoadLevel(0);
            }
        }
    }
}
#endif