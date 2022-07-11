using System;
using System.Collections;
using System.Collections.Generic;
using Effect;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectTest : MonoBehaviour
{
    private Volume _volume;
    private RadiusBlurEffect _radiusBlurEffect;
    private BoxBlurEffect _boxBlurEffect;

    void Start()
    {
        _volume = GetComponent<Volume>();
        VolumeProfile volumeProfile = _volume.profile;

        if (!volumeProfile.TryGet(out _radiusBlurEffect))
        {
            throw new NullReferenceException();
        }
        
        if (!volumeProfile.TryGet(out _boxBlurEffect))
        {
            throw new NullReferenceException();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _radiusBlurEffect.BlurRange.value += 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _radiusBlurEffect.BlurRange.value -= 0.1f;
        }
    }
}
