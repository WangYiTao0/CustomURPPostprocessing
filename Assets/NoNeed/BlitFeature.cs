using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitFeature  : ScriptableRendererFeature
{
    class BlitRenderPass : ScriptableRenderPass
    {
        /// <summary>
        /// used to label this pass in Unity's Frame Debug utility
        /// </summary>
        string _profilerTag;
        
        Material _materialToBlit;
        RenderTargetIdentifier _cameraColorTargetIdent;
        RenderTargetHandle _tempTexture;
        
        public BlitRenderPass(string profilerTag,
            RenderPassEvent renderPassEvent, Material materialToBlit)
        {
            this._profilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this._materialToBlit = materialToBlit;
        }

        
        // This isn't part of the ScriptableRenderPass class and is our own addition.
        // For this custom pass we need the camera's color target, so that gets passed in.
        public void 
            Setup(RenderTargetIdentifier cameraColorTargetIdent)
        {
            _cameraColorTargetIdent = cameraColorTargetIdent;
        }
        
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cmd.GetTemporaryRT(_tempTexture.id, renderingData.cameraData.cameraTargetDescriptor);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // fetch a command buffer to use
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            cmd.Clear();
            // the actual content of our custom render pass!
            // we apply our material while blitting to a temporary texture
            cmd.Blit(_cameraColorTargetIdent, _tempTexture.Identifier(), _materialToBlit, 0);

            // ...then blit it back again 
            cmd.Blit(_tempTexture.Identifier(), _cameraColorTargetIdent);
            // don't forget to tell ScriptableRenderContext to actually execute the commands
            context.ExecuteCommandBuffer(cmd);

            // tidy up after ourselves
            cmd.Clear();
            CommandBufferPool.Release(cmd);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tempTexture.id);
        }
    }

    [System.Serializable]
    public class FeatureSettings
    {
        // we're free to put whatever we want here, public fields will be exposed in the inspector
        public bool IsEnabled = true;
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRenderingTransparents;
        public Material MaterialToBlit;
    }
    
    public FeatureSettings settings = new FeatureSettings();
    RenderTargetHandle renderTextureHandle;
    BlitRenderPass _blitRenderPass;
    /// <inheritdoc/>
    public override void Create()
    {
        _blitRenderPass = new BlitRenderPass("custom pass",settings.WhenToInsert,settings.MaterialToBlit);


    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.IsEnabled)
        {
            // we can do nothing this frame if we want
            return;
        }
        
        // Gather up and pass any extra information our pass will need.
        // In this case we're getting the camera's color buffer target
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        _blitRenderPass.Setup(cameraColorTargetIdent);

        // Ask the renderer to add our pass.
        // Could queue up multiple passes and/or pick passes to use
        renderer.EnqueuePass(_blitRenderPass);
        
    }
}


