using System;
using UnityEngine;

namespace Game.Services.Meta
{
    public class LocationImprovementView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] meshRenderers;
        [SerializeField] private Material hideMaterial;

        private Material[] defaultMaterials;

        [NonSerialized] public LocationImprovementItemData locationImprovementItemData;
        
        private void Awake()
        {
            SaveDefaultMaterials();
        }

        private void SaveDefaultMaterials()
        {
            defaultMaterials = new Material[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                defaultMaterials[i] = meshRenderers[i].material;
            }
        }
        
        public void Show()
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = defaultMaterials[i];
            }
        }
        
        public void Hide()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.material = hideMaterial;
            }
        }

    }
}