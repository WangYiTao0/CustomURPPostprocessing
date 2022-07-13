using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{
    public class CustomPostProcessPass : ScriptableRenderPass
    {
        RenderTargetIdentifier _source;
        RenderTargetIdentifier _destinationA;
        RenderTargetIdentifier _destinationB;
        RenderTargetIdentifier _latestDest;
        
        readonly int _temporaryRTIdA = Shader.PropertyToID("_TempRTA");
        readonly int _temporaryRTIdB = Shader.PropertyToID("_TempRTB");
        
        public CustomPostProcessPass(RenderPassEvent settingsWhenToInsert)
        {
            // Set the render pass event
            renderPassEvent = settingsWhenToInsert;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //Grab the camera target descriptor.
            //We will use this when creating a temporary render texture.
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            var renderer = renderingData.cameraData.renderer;
            _source = renderer.cameraColorTarget;
            
            // Create a temporary render texture using the descriptor from above.
            cmd.GetTemporaryRT(_temporaryRTIdA , descriptor, FilterMode.Bilinear);
            _destinationA = new RenderTargetIdentifier(_temporaryRTIdA);
            cmd.GetTemporaryRT(_temporaryRTIdB , descriptor, FilterMode.Bilinear);
            _destinationB = new RenderTargetIdentifier(_temporaryRTIdB);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Skipping post processing rendering inside the scene View
            if(renderingData.cameraData.isSceneViewCamera)
                return;
            
            // Here you get your materials from your custom class
            var materials = CustomPostProcessingMaterials.Instance;
            if (materials == null)
            {
                Debug.LogError("Custom Post Processing Materials instance is null");
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
            cmd.Clear();
            
            // This holds all the current Volumes information
            // which we will need later
            var stack = VolumeManager.instance.stack;
            
            #region Local Methods
            // Swaps render destinations back and forth, so that
            // we can have multiple passes and similar with only a few textures
            void BlitTo(Material mat, int pass = 0)
            {
                var first = _latestDest;
                var last = first == _destinationA ? _destinationB : _destinationA;
                Blit(cmd, first, last, mat, pass);

                _latestDest = last;
            }
            #endregion
            // Starts with the camera source
            _latestDest = _source;
            
            //---Custom effect here---
            #region CustomEffect
            
            #region OverlayEffect
   
            var OverlayEffect = stack.GetComponent<OverlayEffect>();
            // Only process if the effect is active
            if (OverlayEffect.IsActive())
            {
                var material = materials.OverlayEffect;
                material.SetFloat(ShaderString.Overlay_IntensityID, OverlayEffect.Intensity.value);
                material.SetColor(ShaderString.Overlay_OverlayColorID, OverlayEffect.OverlayColor.value);
            
                BlitTo(material);
            } 

            #endregion
            
            #region BoxBlurEffect

            var boxBlurEffect = stack.GetComponent<BoxBlurEffect>();
            
            if (boxBlurEffect.IsActive())
            {
                var material = materials.BoxBlurEffect;
                material.SetFloat(ShaderString.BoxBlur_BlurSizeID, boxBlurEffect.BlurSize.value);
            
                BlitTo(material);
            } 

            #endregion

            #region RadiusBlurEffect

            var radiusBlurEffect = stack.GetComponent<RadiusBlurEffect>();
            // Only process if the effect is active
            if (radiusBlurEffect.IsActive())
            {
                var material = materials.RadiusBlurEffect;
                // P.s. optimize by caching the property ID somewhere else
                material.SetFloat(ShaderString.RadiusBlur_BlurRangeID, radiusBlurEffect.BlurRange.value);
                material.SetVector(ShaderString.RadiusBlur_BlurCenterID, radiusBlurEffect.BlurCenter);
                material.SetInt(ShaderString.RadiusBlur_SampleCountID, radiusBlurEffect.SampleCount.value);
                material.SetFloat(ShaderString.RadiusBlur_BlurPowerID, radiusBlurEffect.BlurPower.value);
                BlitTo(material);
            } 

            #endregion
 
            
              

            #endregion
            
            // DONE! Now that we have processed all our custom effects, applies the final result to camera
            Blit(cmd, _latestDest, _source);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
       
        }
        
        // //TODO Make Auto Config
        // private void  BindCustomEffect<T>(T effect) where T : VolumeComponent,IPostProcessComponent
        // {
        //     // Here you get your materials from your custom class
        //     // (It's up to you! But here is how I did it)
        //     var materials = CustomPostProcessingMaterials.Instance;
        //     if (materials == null)
        //     {
        //         Debug.LogError("Custom Post Processing Materials instance is null");
        //         return;
        //     }
        //     
        //     // This holds all the current Volumes information
        //     // which we will need later
        //     var stack = VolumeManager.instance.stack;
        //     //---Custom effect here---
        //     T customEffect = stack.GetComponent<T>();
        //     // Only process if the effect is active
        //     if (customEffect.IsActive())
        //     {
        //         var material = materials.OverlayEffect;
        //         // P.s. optimize by caching the property ID somewhere else
        //         material.SetFloat(Shader.PropertyToID("_Intensity"), customEffect.Intensity.value);
        //         material.SetColor(Shader.PropertyToID("_OverlayColor"), customEffect.OverlayColor.value);
        //     
        //         BlitTo(material);
        //     } 
        // }
        
        //Cleans the temporary RTs when we don't need them anymore
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_temporaryRTIdA);
            cmd.ReleaseTemporaryRT(_temporaryRTIdB);
        }
    }
}