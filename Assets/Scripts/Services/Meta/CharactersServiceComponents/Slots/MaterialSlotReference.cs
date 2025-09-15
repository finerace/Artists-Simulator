using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Services.Common;
using Game.Infrastructure.Configs;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    [Serializable]
    [SlotType(ItemType.Material)]
    public class MaterialSlotReference : SlotReferenceBase, IColorizedSlot
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private int materialIndex;
        
        [NonSerialized] private List<Material> materialCopies = new List<Material>();
        [NonSerialized] private Color currentColor = Color.white;
        
        public Renderer[] Renderers => renderers;
        public int MaterialIndex => materialIndex;
        
        public MaterialSlotReference(string slotId, Renderer[] renderers, int materialIndex = 0)
        {
            this.slotId = slotId;
            this.renderers = renderers;
            this.materialIndex = materialIndex;
            
            materialCopies = new List<Material>();
        }
        
        public void SetItem(CharacterItemData itemData)
        {
            currentItem = itemData;
            currentColor = itemData?.DefaultColor ?? Color.white;
        }
        
        public void AddMaterialCopy(Material materialCopy)
        {
            CheckMaterialCopiesForNull();

            if (materialCopy != null && !materialCopies.Contains(materialCopy))
                materialCopies.Add(materialCopy);
        }
        
        public override void ClearItem(IAssetsService assetsService)
        {
            CheckMaterialCopiesForNull();
            
            foreach (var materialCopy in materialCopies)
            {
                if (materialCopy != null)
                {
                    UnityEngine.Object.DestroyImmediate(materialCopy);
                }
            }
            
            materialCopies.Clear();
            currentItem = null;
            currentColor = Color.white;
        }
        
        public override async UniTask ApplyItem(CharacterItemData itemData, IAssetsService assetsService)
        {
            if (itemData == null)
            {
                Logs.Warning("Attempted to apply null itemData to MaterialSlot");
                return;
            }
            
            ClearItem(assetsService);
            
            if (renderers == null || renderers.Length == 0 || itemData.ItemMaterial == null)
            {
                Logs.Warning($"Invalid material data for slot: {slotId}");
                return;
            }
            
            SetItem(itemData);
            
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                    continue;
                
                if (materialIndex < renderer.materials.Length)
                {
                    Material[] materials = renderer.materials;
                    
                    Material materialCopy = new Material(itemData.ItemMaterial);
                    materials[materialIndex] = materialCopy;
                    renderer.materials = materials;
                    
                    AddMaterialCopy(materialCopy);
                    
                    if (itemData.IsCanColorize)
                    {
                        materialCopy.SetColor(CharacterItemColorizeView.Tex2Color, itemData.DefaultColor);
                    }
                }
                else if (renderer.materials.Length == 1)
                {
                    Material materialCopy = new Material(itemData.ItemMaterial);
                    renderer.material = materialCopy;
                    
                    AddMaterialCopy(materialCopy);
                    
                    if (itemData.IsCanColorize)
                    {
                        materialCopy.SetColor(CharacterItemColorizeView.Tex2Color, itemData.DefaultColor);
                    }
                }
            }
        }
        
        public bool SetColor(Color color)
        {
            if (materialCopies.Count == 0)
                return false;
            
            foreach (var material in materialCopies)
            {
                if (material != null)
                {
                    material.SetColor(CharacterItemColorizeView.Tex2Color, color);
                }
            }
            
            currentColor = color;
            return true;
        }
        
        public Color GetCurrentColor()
        {
            return currentColor;
        }

        private void CheckMaterialCopiesForNull()
        {
            if (materialCopies == null)
                materialCopies = new List<Material>();
        }

    }
} 