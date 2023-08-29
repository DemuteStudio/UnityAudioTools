using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StaticMeshAudioEmitter))]
public class StaticMeshAudioPlayer : MonoBehaviour
{

    private StaticMeshAudioEmitter _associatedEmitter;

    private void OnEnable()
    {
        if(_associatedEmitter == null)
            _associatedEmitter = GetComponent<StaticMeshAudioEmitter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Default implementation that just plays the audio on begin play.
        //Rewrite this part if you want to optimise or control the playback of your static mesh audio emitters
        //differently.

#if DM_WWISE_SYMBOLS
        _associatedEmitter.associatedAudioClip.Post(gameObject);
#else
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = _associatedEmitter.associatedAudioClip;
        audioSource.Play();
#endif
    }
}
