using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(fileName = "CustomPostProcessingMaterials", menuName = "Effect/CustomPostProcessingMaterials", order = 0)]
    public class CustomPostProcessingMaterials : ScriptableObject
    {
        public Material OverlayEffect;
        public Material BoxBlurEffect;
        public Material RadiusBlurEffect;
        
        //---Accessing the data from the Pass---
        static CustomPostProcessingMaterials _instance;
        
        public static CustomPostProcessingMaterials Instance
        {
            get
            {
                if (_instance != null) return _instance;
                // TODO check if application is quitting
                // and avoid loading if that is the case

                _instance = UnityEngine.Resources.Load("CustomPostProcessingMaterials") as CustomPostProcessingMaterials;
                return _instance;
            }
        }
    }
}