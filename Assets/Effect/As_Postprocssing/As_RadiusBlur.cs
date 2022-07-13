using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{

    [AddComponentMenu("As_Postprocess/As_RadiusBlur")]
    [ExecuteAlways]
    public class As_RadiusBlur : MonoBehaviour
    {
        #region Inspector
        [SerializeField] [Range(0.0f,0.6f)][Tooltip("数字が大きいほど、ぼかしの範囲が広くなる")] private float _blurRange = 0.6f;
        [SerializeField] [Range(0f,10f)] [Tooltip("数値が大きいほど、ぼかし効果が強くなります。")]private float _blurPower = 3f;
        [SerializeField] [Range(4,16)] [Tooltip("数値が大きいほど、ぼかし効果が強くなります。")]private int _sampleCount = 4;
        [SerializeField] private Color _blurColor = Color.white;
        [SerializeField] [Range(0.0f,1.0f)]private float _blurCenterX = 0.5f;
        [SerializeField] [Range(0.0f,1.0f)]private float _blurCenterY = 0.5f;
        #endregion
        
        private VolumeProfile _volumeProfile;
        private Volume _volume;
        private RadiusBlurEffect _radiusBlurEffect;
        private ColorAdjustments _colorAdjustments;
        private void Awake()
        {
            if (_volume == null)
            {
                _volume = gameObject.GetComponent<Volume>();
            }
            if (_volumeProfile == null)
            {
                _volumeProfile = _volume.profile;
            }
            if (!_volumeProfile.TryGet(out _radiusBlurEffect))
            {
                _volumeProfile.Add<RadiusBlurEffect>();
            }
            if (!_volumeProfile.TryGet(out _colorAdjustments))
            {
                _volumeProfile.Add<ColorAdjustments>();
            }
        }

        private void Start()
        {
            _radiusBlurEffect.active = true;
            _colorAdjustments.active = true;
            UpdateParameter();
        }

        private void Update()
        {
            UpdateParameter();
        }

        private void UpdateParameter()
        {
            _radiusBlurEffect.BlurRange.value = _blurRange;
            _colorAdjustments.colorFilter.value = _blurColor;
            _radiusBlurEffect.SampleCount.value = _sampleCount;
            _radiusBlurEffect.BlurCenterX.value = _blurCenterX;
            _radiusBlurEffect.BlurCenterY.value = _blurCenterY;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_volume == null)
            {
                _volume = gameObject.AddComponent<Volume>();
            }
            
            _volumeProfile = _volume.profile;
            if (!_volumeProfile.TryGet(out _radiusBlurEffect))
            {
                _volumeProfile.Add<RadiusBlurEffect>();
            }
            if (!_volumeProfile.TryGet(out _colorAdjustments))
            {
                _volumeProfile.Add<ColorAdjustments>();
            }
        }
#endif
    }
}
