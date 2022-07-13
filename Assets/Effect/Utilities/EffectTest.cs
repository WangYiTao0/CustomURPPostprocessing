using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Effect;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectTest : MonoBehaviour
{
    #region Inspector
    [SerializeField] private float _blurTime = 0.5f;
    [SerializeField][ColorUsageAttribute(true,true)] private Color _targetColor;
    #endregion
    
    private Volume _volume;
    private RadiusBlurEffect _radiusBlurEffect;
    private BoxBlurEffect _boxBlurEffect;
    private ColorAdjustments _colorAdjustments;
    [SerializeField] private bool _isBoxBlur = false;
    [SerializeField] private bool _isRadiusBlur = false;

    private Sequence _boxBlurSq;

    void Start()
    {
        _volume = GetComponent<Volume>();
        VolumeProfile volumeProfile = _volume.profile;

        if (!volumeProfile.TryGet(out _radiusBlurEffect))
        {
            _radiusBlurEffect = volumeProfile.Add<RadiusBlurEffect>();
        }

        if (!volumeProfile.TryGet(out _boxBlurEffect))
        {
            _boxBlurEffect = volumeProfile.Add<BoxBlurEffect>();
        }
        
        if (!volumeProfile.TryGet(out _colorAdjustments))
        {
            _colorAdjustments = volumeProfile.Add<ColorAdjustments>();
        }
        
        // SetupBoxBlurSequence();
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

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DoBoxBlur();
        }
        
        if (Input.GetKey(KeyCode.Alpha4) && !_isRadiusBlur)
        {
            DoRadiusBlur();
        }

        if (Input.GetKeyUp(KeyCode.Alpha4)&& _isRadiusBlur)
        {
            ResetRadiusBlur();
        }
    }

    private void ResetRadiusBlur()
    {
        _isRadiusBlur = false;
        DOTween.To(() =>  _radiusBlurEffect.BlurRange.value, (x) => _radiusBlurEffect.BlurRange.value = x, 
            0.0f, _blurTime);
        _colorAdjustments.colorFilter.value = Color.white;
    }

    private void DoRadiusBlur()
    {
        _isRadiusBlur = true;
        DOTween.To(() =>  _radiusBlurEffect.BlurRange.value, (x) => _radiusBlurEffect.BlurRange.value = x, 
            0.6f, _blurTime);
        _colorAdjustments.colorFilter.value = _targetColor;
    }

    private void DoBoxBlur()
    {
        if (_isBoxBlur)
            return;
        _isBoxBlur = true;
        SetupBoxBlurSequence();
        //_boxBlurSq.Play();
    }

    private void SetupBoxBlurSequence()
    {
        _boxBlurSq = DOTween.Sequence();

        var to = DOTween.To(() => _boxBlurEffect.BlurSize.value, (x) => _boxBlurEffect.BlurSize.value = x, 
            20, _blurTime);
        var toColor = DOTween.To(() => _colorAdjustments.colorFilter.value, (x) => _colorAdjustments.colorFilter.value = x,
            _targetColor,_blurTime);
        var back =DOTween.To(() => _boxBlurEffect.BlurSize.value, (x) => _boxBlurEffect.BlurSize.value = x, 0,
            _blurTime);
        var backColor = DOTween.To(() => _colorAdjustments.colorFilter.value, (x) => _colorAdjustments.colorFilter.value = x,
            Color.white, _blurTime);
        _boxBlurSq.Append(to).Join(toColor).Append(back).Join(backColor).OnComplete(() => { _isBoxBlur = false; });

        //_boxBlurSq.Pause();
        //_boxBlurSq.SetAutoKill(false);
    }

    private void OnDestroy()
    {
        if(_boxBlurSq != null)
            _boxBlurSq.Kill();
    }
}
