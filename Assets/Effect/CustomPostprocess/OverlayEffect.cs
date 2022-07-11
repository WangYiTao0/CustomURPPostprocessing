using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{
    [Serializable, VolumeComponentMenu("Custom/OverlayEffect")]
    public class OverlayEffect : VolumeComponent, IPostProcessComponent
    {
        // For example, an intensity parameter that goes from 0 to 1
        public ClampedFloatParameter Intensity = new ClampedFloatParameter(value: 0, min: 0, max: 1, overrideState: true);
        // A color that is constant even when the weight changes
        public NoInterpColorParameter OverlayColor = new NoInterpColorParameter(Color.cyan);

        public bool IsActive() => Intensity.value > 0;

        public bool IsTileCompatible() => true;
    }
}