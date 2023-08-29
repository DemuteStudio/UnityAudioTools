using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Demute/Static Mesh Audio Emitters/Audio Mesh Dictionary", fileName = "New Audio Mesh Dictionary")]
public class StaticMeshAudioDictionary : ScriptableObject
{
    public List<AudioMeshEntry> audioMeshDictionary;
}
[System.Serializable]
public struct AudioMeshEntry
{
    public Mesh mesh;
    public AudioClip audioClip;
}
