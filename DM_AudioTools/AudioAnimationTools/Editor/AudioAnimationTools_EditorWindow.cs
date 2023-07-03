using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AudioAnimationTools_EditorWindow : EditorWindow
{
    private static SerializedObject data;
    
    public List<AnimationClip> AnimationClips;

    [MenuItem("Demute/Audio Animation Tools")]
    private static void Init()
    {
        AudioAnimationTools_EditorWindow editorWindow =
            (AudioAnimationTools_EditorWindow)EditorWindow.GetWindow(typeof(AudioAnimationTools_EditorWindow));
        editorWindow.Show();
    }

    private void OnGUI()
    {
        if (data == null || data.targetObject == null)
        {
            AudioAnimationToolsData dataAsset =
                Resources.Load("AudioAnimationTools_WindowData") as AudioAnimationToolsData;

            if (dataAsset == null)
            {
                dataAsset = CreateInstance<AudioAnimationToolsData>();
                MonoScript editorWindowScript = MonoScript.FromScriptableObject(this);
                AssetDatabase.CreateAsset(dataAsset, AssetDatabase.GetAssetPath(editorWindowScript).
                    Replace(editorWindowScript.name+".cs", "/Resources/AudioAnimationTools_WindowData.asset"));
            }
            data = new SerializedObject(dataAsset);
            
            //data = new SerializedObject(this);
        }
        data.Update();
        EditorGUILayout.PropertyField(data.FindProperty("AnimationClips"));
        if (GUILayout.Button("Append Selected Animation Clips"))
        {
            AppendSelectedAnimationClips();
        }
        if (GUILayout.Button("Clear Animation Clips list"))
        {
            ClearAnimationClipsList();
        }
        data.ApplyModifiedProperties();
    }

    private static void AppendSelectedAnimationClips()
    {
        AudioAnimationToolsData serializedData = data.targetObject as AudioAnimationToolsData;
        if (serializedData == null) return;
        Object[] selectedObjects = Selection.objects;

        foreach (Object selectedObject in selectedObjects)
        {
            Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(selectedObject));
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip animationClip)
                {
                    serializedData.AnimationClips.Add(animationClip);
                    EditorUtility.SetDirty(serializedData);
                }
            }
        }
    }

    private static void ClearAnimationClipsList()
    {
        AudioAnimationToolsData serializedData = data.targetObject as AudioAnimationToolsData;
        if (serializedData == null) return;
        
        serializedData.AnimationClips.Clear();
        EditorUtility.SetDirty(serializedData);
    }
}

