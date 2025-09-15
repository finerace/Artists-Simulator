using UnityEngine;
using Game.Additional.MagicAttributes;

namespace Game.Services.Meta
{
    
    public class CharacterItemColorizeView : MonoBehaviour
    {
        [SerializeField] private Material sourceMaterial;
        [SerializeField] private Renderer sourceRenderer;
        
        [Space]
        
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private int materialIndex = 0;
        
        public static readonly int Tex2Color = Shader.PropertyToID("_Tex2Color");
        
        private Material materialCopy;

        private void Awake()
        {
            if (sourceMaterial == null && sourceRenderer != null)
                sourceMaterial = sourceRenderer.material;

            if (sourceMaterial != null)
            {
                materialCopy = new Material(sourceMaterial);
                ApplyMaterialToRenderers();
            }
        }

        public void SetColor(Color color)
        {
            if (materialCopy == null)
                return;
            
            materialCopy.SetColor(Tex2Color, color);
        }
        
        public void SetMaterial(Material newMaterial)
        {
            if (newMaterial == null)
                return;
            
            materialCopy = new Material(newMaterial);
            
            ApplyMaterialToRenderers();
        }
        
        private void ApplyMaterialToRenderers()
        {
            if (materialCopy == null || targetRenderers == null || targetRenderers.Length == 0)
                return;
            
            foreach (var renderer in targetRenderers)
            {
                if (renderer == null)
                    continue;
                
                if (renderer.materials.Length > 1 && materialIndex < renderer.materials.Length)
                {
                    Material[] materials = renderer.materials;
                    materials[materialIndex] = materialCopy;
                    renderer.materials = materials;
                }
                else
                {
                    renderer.material = materialCopy;
                }
            }
        }
    }
}
