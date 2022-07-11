using System;
using UnityEngine.Rendering.Universal;

namespace Effect
{

    [Serializable]
    public class CustomPostProcessRenderer:ScriptableRendererFeature
    {
        
        public FeatureSettings settings = new FeatureSettings();
        RenderTargetHandle renderTextureHandle;
        CustomPostProcessPass _customPostProcessPass;
        
        public override void Create()
        {
            _customPostProcessPass = new CustomPostProcessPass(settings.WhenToInsert);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.IsEnabled)
            {
                // we can do nothing this frame if we want
                return;
            }
            renderer.EnqueuePass(_customPostProcessPass);
        }
    }
    
    [System.Serializable]
    public class FeatureSettings
    {
        // we're free to put whatever we want here, public fields will be exposed in the inspector
        public bool IsEnabled = true;
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRenderingTransparents;
        //public Material MaterialToBlit;
    }
}