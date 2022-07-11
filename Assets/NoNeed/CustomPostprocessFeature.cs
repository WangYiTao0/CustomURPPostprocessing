using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostprocessFeature : ScriptableRendererFeature
{
    public enum BufferType
    {
        CameraColor,
        Custom 
    }
    class CustomPostprocessPass : ScriptableRenderPass
    {
        
        public FilterMode filterMode { get; set; }
        public CustomPostprocessFeature.Settings settings;
        
        RenderTargetIdentifier _source;
        RenderTargetIdentifier _destination;
        
        int _temporaryRTId = Shader.PropertyToID("_TempRT");
            
        int _sourceId;
        int _destinationId;
        bool _isSourceAndDestinationSameTarget;
            
        string _profilerTag;
        
        public CustomPostprocessPass(string tag) : base()
        {
            _profilerTag = tag;
        }
        
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            blitTargetDescriptor.depthBufferBits = 0;
                
            _isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
                                                (settings.sourceType == BufferType.CameraColor || settings.sourceTextureId == settings.destinationTextureId);
                
            var renderer = renderingData.cameraData.renderer;
                
            if (settings.sourceType == BufferType.CameraColor)
            {
                _sourceId = -1;
                _source = renderer.cameraColorTarget;
            }
            else
            {
                _sourceId = Shader.PropertyToID(settings.sourceTextureId);
                cmd.GetTemporaryRT(_sourceId, blitTargetDescriptor, filterMode);
                _source = new RenderTargetIdentifier(_sourceId);
            }
                
            if (_isSourceAndDestinationSameTarget)
            {
                _destinationId = _temporaryRTId;
                cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, filterMode);
                _destination = new RenderTargetIdentifier(_destinationId);
            }
            else if (settings.destinationType == BufferType.CameraColor)
            {
                _destinationId = -1;
                _destination = renderer.cameraColorTarget;
            }
            else
            {
                _destinationId = Shader.PropertyToID(settings.destinationTextureId);
                cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, filterMode);
                _destination = new RenderTargetIdentifier(_destinationId);
            }
        }


        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            // Can't read and write to same color target, create a temp render target to blit. 
            if (_isSourceAndDestinationSameTarget)
            {
                Blit(cmd, _source, _destination, settings.blitMaterial, settings.blitMaterialPassIndex);
                Blit(cmd, _destination, _source);
            }
            else
            {
                Blit(cmd, _source, _destination, settings.blitMaterial, settings.blitMaterialPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (_destinationId != -1)
                cmd.ReleaseTemporaryRT(_destinationId);

            if (_source == _destination && _sourceId != -1)
                cmd.ReleaseTemporaryRT(_sourceId);
        }
    }
    
    
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        public Material blitMaterial = null;
        public string shaderPath = "shaderName";
        public int blitMaterialPassIndex = -1;
        public BufferType sourceType = BufferType.CameraColor;
        public BufferType destinationType = BufferType.CameraColor;
        public string sourceTextureId = "_SourceTexture";
        public string destinationTextureId = "_DestinationTexture";
    }

    public Settings settings = new Settings();
    CustomPostprocessPass _blitPass;

    /// <inheritdoc/>
    public override void Create()
    {
        _blitPass = new CustomPostprocessPass(name);

        // Configures where the render pass should be injected.
        _blitPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
        
        _blitPass.renderPassEvent = settings.renderPassEvent;
        _blitPass.settings = settings;
        renderer.EnqueuePass(_blitPass);
    }
}


