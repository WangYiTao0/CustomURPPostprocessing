using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static  class ShaderString
{
    
    public static readonly int Overlay_IntensityID = Shader.PropertyToID("_Intensity");
    public static readonly int Overlay_OverlayColorID = Shader.PropertyToID("_OverlayColor");

    public static readonly int BoxBlur_BlurSizeID = Shader.PropertyToID("_BlurSize");

    public static readonly int RadiusBlur_BlurRangeID = Shader.PropertyToID("_BlurRange");
    public static readonly int RadiusBlur_BlurCenterID = Shader.PropertyToID("_BlurCenter");
    public static readonly int RadiusBlur_SampleCountID = Shader.PropertyToID("_SampleCount");
    public static readonly int RadiusBlur_BlurPowerID = Shader.PropertyToID("_BlurPower");
}
