using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{
    [Serializable, VolumeComponentMenu("Custom/BoxBlurEffect")]
    public class BoxBlurEffect : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter BlurSize = new ClampedFloatParameter(0, 0, 20);
        public NoInterpColorParameter BlurColor = new NoInterpColorParameter(Color.white);
        
        public bool IsActive() => BlurSize.value > 0;

        public bool IsTileCompatible() => true;
    }
}