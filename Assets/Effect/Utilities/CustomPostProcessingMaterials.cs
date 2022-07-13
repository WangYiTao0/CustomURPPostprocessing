using UnityEngine;

namespace Effect
{
    /// <summary>
    /// Eazy Way to Access Material for CustomRenderPass
    /// </summary>
    [CreateAssetMenu(fileName = "CustomPostProcessingMaterials", menuName = "Effect/CustomPostProcessingMaterials", order = 0)]
    public class CustomPostProcessingMaterials : ScriptableObject
    {
        public Material OverlayEffect;
        public Material BoxBlurEffect;
        public Material RadiusBlurEffect;
        
        static CustomPostProcessingMaterials _instance;
        
        public static CustomPostProcessingMaterials Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                //TODO Check Application is Quitting
                _instance = UnityEngine.Resources.Load("CustomPostProcessingMaterials") as CustomPostProcessingMaterials;
                return _instance;
            }
        }
    }
}