using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StaticMeshAudioEmitters_EditorWindow : EditorWindow
{

    private static SerializedObject data;
    
    private Vector2 scrollPosition = Vector2.zero;
    private string consoleText;
    
    [MenuItem("Demute/Static Mesh Audio Emitters")]
    private static void Init()
    {
        StaticMeshAudioEmitters_EditorWindow editorWindow =
            (StaticMeshAudioEmitters_EditorWindow)EditorWindow.GetWindow(typeof(StaticMeshAudioEmitters_EditorWindow));
        editorWindow.Show();
    }

    public List<StaticMeshAudioDictionary> dictionaries;

    private void OnGUI()
    {
        if (data == null || data.targetObject == null)
        {
            StaticMeshAudioEmittersData dataAsset =
                Resources.Load("StaticMeshAudioEmitters_WindowData") as StaticMeshAudioEmittersData;

            if (dataAsset == null)
            {
                dataAsset = CreateInstance<StaticMeshAudioEmittersData>();
                MonoScript editorWindowScript = MonoScript.FromScriptableObject(this);
                AssetDatabase.CreateAsset(dataAsset, AssetDatabase.GetAssetPath(editorWindowScript).
                    Replace(editorWindowScript.name+".cs", "/Resources/StaticMeshAudioEmitters_WindowData.asset"));
            }
            data = new SerializedObject(dataAsset);
        }
        
        data.Update();
        EditorGUILayout.PropertyField(data.FindProperty("dictionaries"));
        EditorGUILayout.PropertyField(data.FindProperty("emitterPrefab"));
        if (GUILayout.Button("Generate Emitters"))
        {
            GenerateEmitters();
        }

        if (GUILayout.Button("Delete Matching Emitters"))
        {
            DeleteMatchingEmitters();
        }

        if (GUILayout.Button("Delete All Emitters"))
        {
            DeleteAllEmitters();
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition,false,true, GUILayout.MaxHeight(100));

        GUILayout.Label(consoleText, GUILayout.ExpandHeight(true));
        
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Check existing emitters"))
        {
            CheckExistingEmitters();
        }

        if (GUILayout.Button("Reposition all emitters"))
        {
            RepositionAllEmitters();
        }

        if (GUILayout.Button("Delete all unlinked all emitters"))
        {
            DeleteAllUnlinkedEmitters();
        }
        
        data.ApplyModifiedProperties();
    }

    private static void GenerateEmitters()
    {
        Undo.IncrementCurrentGroup();
        
        StaticMeshAudioEmittersData serializedData = data.targetObject as StaticMeshAudioEmittersData;
        if (serializedData == null) return;

        if (serializedData.emitterPrefab.GetComponent<StaticMeshAudioEmitter>() == null)
        {
            Debug.LogError("Static Mesh Audio Emitters : the prefab specified doesn't have a static mesh audio emitter component. Aborting Generation.");
            return;
        }
        
        DeleteMatchingEmitters();
        
        MeshFilter[] staticMeshes = FindObjectsOfType<MeshFilter>();
        foreach (MeshFilter staticMesh in staticMeshes)
        {
            foreach (StaticMeshAudioDictionary dictionary in serializedData.dictionaries)
            {
                List<AudioMeshEntry> matchingMeshEntries = dictionary.audioMeshDictionary.FindAll(x => x.mesh == staticMesh.sharedMesh);

                foreach (AudioMeshEntry meshEntry in matchingMeshEntries)
                {
                    StaticMeshAudioEmitter newAudioEmitter = Instantiate(serializedData.emitterPrefab, GetAudioEmittersParent())
                        .GetComponent<StaticMeshAudioEmitter>();
                    newAudioEmitter.transform.SetPositionAndRotation(staticMesh.transform.position, staticMesh.transform.rotation);
                    newAudioEmitter.autoGenerated = true;
                    newAudioEmitter.meshName = meshEntry.mesh.name;
                    newAudioEmitter.associatedAudioClip = meshEntry.audioClip;
                    newAudioEmitter.linkedMeshFilter = staticMesh;
                    newAudioEmitter.gameObject.name = "SMA_" + staticMesh.gameObject.name;
                    newAudioEmitter.ReferencePosition = staticMesh.transform.position;

                    Undo.RegisterCreatedObjectUndo(newAudioEmitter.gameObject, "Create Emitter");
                }
            }
        }
        
        Undo.SetCurrentGroupName("Generate audio emitters");
    }

    private static Transform GetAudioEmittersParent()
    {
        GameObject audioEmittersParent = GameObject.Find("Static Mesh Audio Emitters");

        if (audioEmittersParent == null)
        {
            audioEmittersParent = new GameObject("Static Mesh Audio Emitters");
        }

        return audioEmittersParent.transform;
    }

    private static void DeleteMatchingEmitters()
    {
        Undo.IncrementCurrentGroup();
        
        StaticMeshAudioEmittersData serializedData = data.targetObject as StaticMeshAudioEmittersData;
        if (serializedData == null) return;

        StaticMeshAudioEmitter[] audioEmitters = FindObjectsOfType<StaticMeshAudioEmitter>();
        
        foreach (StaticMeshAudioEmitter audioEmitter in audioEmitters)
        {
            if(audioEmitter.autoGenerated == false) continue;
            foreach (StaticMeshAudioDictionary dictionary in serializedData.dictionaries)
            {
                foreach (AudioMeshEntry audioMeshEntry in dictionary.audioMeshDictionary)
                {
                    if (audioMeshEntry.mesh.name == audioEmitter.meshName
                        && audioMeshEntry.audioClip == audioEmitter.associatedAudioClip)
                    {
                        //We have a match
                        Undo.DestroyObjectImmediate(audioEmitter.gameObject);
                    }
                }
            }
        }
        Undo.SetCurrentGroupName("Delete matching audio emitters");
    }

    private static void DeleteAllEmitters()
    {
        Undo.IncrementCurrentGroup();

        StaticMeshAudioEmitter[] audioEmitters = FindObjectsOfType<StaticMeshAudioEmitter>();
        
        foreach (StaticMeshAudioEmitter audioEmitter in audioEmitters)
        {
            Undo.DestroyObjectImmediate(audioEmitter.gameObject);
        }
        
        Undo.SetCurrentGroupName("Delete all audio emitters");
    }

    private void CheckExistingEmitters()
    {
        consoleText = "";
        
        StaticMeshAudioEmitter[] audioEmitters = FindObjectsOfType<StaticMeshAudioEmitter>();
        
        foreach (StaticMeshAudioEmitter audioEmitter in audioEmitters)
        {
            if (audioEmitter.linkedMeshFilter == null)
            {
                consoleText = consoleText + string.Format("The mesh linked to {0} doesn't exist anymore.\n",
                    audioEmitter.gameObject.name);
            }
            
            else if (audioEmitter.linkedMeshFilter.transform.position != audioEmitter.ReferencePosition)
            {
                consoleText = consoleText + string.Format("The mesh {0} linked to {1} has changed location.\n", audioEmitter.linkedMeshFilter.gameObject.name,
                    audioEmitter.gameObject.name);
            }
        }
    }

    private void RepositionAllEmitters()
    {
        Undo.IncrementCurrentGroup();
        
        consoleText = "";
        
        StaticMeshAudioEmitter[] audioEmitters = FindObjectsOfType<StaticMeshAudioEmitter>();
        
        foreach (StaticMeshAudioEmitter audioEmitter in audioEmitters)
        {
            if (audioEmitter.linkedMeshFilter == null)
            {
                continue;
            }
            
            else if (audioEmitter.linkedMeshFilter.transform.position != audioEmitter.ReferencePosition)
            {
                Undo.RecordObject(audioEmitter.transform, "Reposition emitter");
                consoleText = consoleText + string.Format("Repositioning emitter {1} to match changed position of object {0}.\n", audioEmitter.linkedMeshFilter.gameObject.name,
                    audioEmitter.gameObject.name);
                audioEmitter.ReferencePosition = audioEmitter.linkedMeshFilter.transform.position;
            }
        }
        
        Undo.SetCurrentGroupName("Reposition all audio emitters");

    }

    private void DeleteAllUnlinkedEmitters()
    {
        Undo.IncrementCurrentGroup();
        
        consoleText = "";
        
        StaticMeshAudioEmitter[] audioEmitters = FindObjectsOfType<StaticMeshAudioEmitter>();
        
        foreach (StaticMeshAudioEmitter audioEmitter in audioEmitters)
        {
            if (audioEmitter.linkedMeshFilter == null)
            {
                consoleText = consoleText + string.Format("Deleting emitter {0} with deleted linked object.\n",
                    audioEmitter.gameObject.name);
                Undo.DestroyObjectImmediate(audioEmitter.gameObject);
            }
        }
        
        Undo.SetCurrentGroupName("Delete all unlinked audio emitters");

    }
}
