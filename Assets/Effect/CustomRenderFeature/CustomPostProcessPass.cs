using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effect
{
    public class CustomPostProcessPass : ScriptableRenderPass
    {
        RenderTargetIdentifier source;
        RenderTargetIdentifier destinationA;
        RenderTargetIdentifier destinationB;
        RenderTargetIdentifier latestDest;
        
        readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
        readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");
        
        public CustomPostProcessPass(RenderPassEvent settingsWhenToInsert)
        {
            // Set the render pass event
            renderPassEvent = settingsWhenToInsert;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //Grab the camera target descriptor. We will use this when creating a temporary render texture.
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;
            
            // Create a temporary render texture using the descriptor from above.
            cmd.GetTemporaryRT(temporaryRTIdA , descriptor, FilterMode.Bilinear);
            destinationA = new RenderTargetIdentifier(temporaryRTIdA);
            cmd.GetTemporaryRT(temporaryRTIdB , descriptor, FilterMode.Bilinear);
            destinationB = new RenderTargetIdentifier(temporaryRTIdB);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Skipping post processing rendering inside the scene View
            if(renderingData.cameraData.isSceneViewCamera)
                return;
            
            // Here you get your materials from your custom class
            // (It's up to you! But here is how I did it)
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
                var first = latestDest;
                var last = first == destinationA ? destinationB : destinationA;
                Blit(cmd, first, last, mat, pass);

                latestDest = last;
            }
            #endregion
            // Starts with the camera source
            latestDest = source;

            #region OverlayEffect
   
            //---Custom effect here---
            var OverlayEffect = stack.GetComponent<OverlayEffect>();
            // Only process if the effect is active
            if (OverlayEffect.IsActive())
            {
                var material = materials.OverlayEffect;
                // P.s. optimize by caching the property ID somewhere else
                material.SetFloat(Shader.PropertyToID("_Intensity"), OverlayEffect.Intensity.value);
                material.SetColor(Shader.PropertyToID("_OverlayColor"), OverlayEffect.OverlayColor.value);
            
                BlitTo(material);
            } 

            #endregion
            
            // Add any other custom effect/component you want, in your preferred order
            // Custom effect 2, 3 , ...
            
            #region BoxBlurEffect

            var boxBlurEffect = stack.GetComponent<BoxBlurEffect>();
            // Only process if the effect is active
            if (boxBlurEffect.IsActive())
            {
                var material = materials.BoxBlurEffect;
                // P.s. optimize by caching the property ID somewhere else
                material.SetFloat(Shader.PropertyToID("_BlurSize"), boxBlurEffect.BlurSize.value);
                material.SetColor(Shader.PropertyToID("_BlurColor"), boxBlurEffect.BlurColor.value);
            
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
                material.SetFloat(Shader.PropertyToID("_BlurRange"), radiusBlurEffect.BlurRange.value);
                material.SetColor(Shader.PropertyToID("_BlurColor"), radiusBlurEffect.BlurColor.value);
                material.SetVector(Shader.PropertyToID("_BlurCenter"), radiusBlurEffect.BlurCenter.value);
                material.SetInt(Shader.PropertyToID("_SampleCount"), radiusBlurEffect.SampleCount.value);
                material.SetFloat(Shader.PropertyToID("_BlurPower"), radiusBlurEffect.BlurPower.value);
                BlitTo(material);
            } 

            #endregion
 
            
  
            
            // DONE! Now that we have processed all our custom effects, applies the final result to camera
            Blit(cmd, latestDest, source);
            
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
            cmd.ReleaseTemporaryRT(temporaryRTIdA);
            cmd.ReleaseTemporaryRT(temporaryRTIdB);
        }
    }
}