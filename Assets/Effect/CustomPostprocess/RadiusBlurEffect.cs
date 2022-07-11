using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{

    [Serializable, VolumeComponentMenu("Custom/RadiusEffect")]
    public class RadiusBlurEffect : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter BlurRange = new ClampedFloatParameter(0, 0.0f, 0.6f);
        public NoInterpColorParameter BlurColor = new NoInterpColorParameter(Color.white);
        public ClampedIntParameter SampleCount = new ClampedIntParameter(4, 4, 16);
        public Vector2Parameter BlurCenter = new Vector2Parameter(new Vector2(0.5f,0.5f));
        public ClampedFloatParameter BlurPower = new ClampedFloatParameter(3, 0.0f, 10.0f);

        public bool IsActive() => BlurRange.value > 0;

        public bool IsTileCompatible() => true;
    }
}