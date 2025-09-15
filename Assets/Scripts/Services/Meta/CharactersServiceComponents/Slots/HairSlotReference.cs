using System;
using UnityEngine;
using Game.Services.Common;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    
    [Serializable]
    [SlotType(ItemType.Hair)]
    public class HairSlotReference : ObjectSlotReference, IInitializableSlot
    {
        [SerializeField] private string hatSlotId;
        
        private CharacterHairView characterHairView; 
        
        private CharacterCustomizationView customizationView;
        private bool isInitialized;
        
        public HairSlotReference(string slotId, Transform slotTransform) : base(slotId, slotTransform)
        {
        }
        
        public void Initialize(CharacterCustomizationView view)
        {
            if (isInitialized)
                return;
                
            customizationView = view;
            
            if (customizationView != null)
            {
                customizationView.OnSlotItemChanged += OnSlotItemChanged;
                
                void OnSlotItemChanged(string slotId, string itemId, Color color)
                {
                    if (slotId == hatSlotId)
                        UpdateHairVisibilityState();
                }
            }
            
            isInitialized = true;
        }
        
        private void UpdateHairVisibilityState()
        {
            if (!IsHairObjectExists() || customizationView == null)
                return;
            
            if (!customizationView.TryGetSlot<ObjectSlotReference>(hatSlotId, out var hatSlot))
                return;

            bool isHatEquipped = IsHatEquipped();
            bool IsHatEquipped()
            {
                return !string.IsNullOrEmpty(hatSlot.GetCurrentItemId());
            }
            
            ToggleHairRenderers(!isHatEquipped);
            void ToggleHairRenderers(bool isVisible)
            {
                if (characterHairView.ExtraElements == null)
                    return;

                foreach (var gameObject in characterHairView.ExtraElements)
                {
                    if (gameObject == null)
                    {
                        Logs.Warning("HairSlotReference: ExtraElements contains null gameObject");
                        continue;
                    }

                    gameObject.SetActive(isVisible);
                }
            }
        
            HatPositionCorrecton(isHatEquipped);
            void HatPositionCorrecton(bool isHatEquipped)
            {
                if (!isHatEquipped)
                    return;
                
                var (hatPos, hatRot) = characterHairView.GetHairViewData(hatSlot.GetCurrentItemId());
                hatSlot.CurrentObject.transform.localPosition = hatPos;
                hatSlot.CurrentObject.transform.localRotation = Quaternion.Euler(hatRot);
            }
        
        }
        
        public override async UniTask ApplyItem(CharacterItemData itemData, IAssetsService assetsService)
        {
            await base.ApplyItem(itemData, assetsService);

            if (currentObject != null)
                characterHairView = currentObject.GetComponent<CharacterHairView>();
            
            if (characterHairView == null)
                Logs.Warning($"HairSlotReference: CharacterHairView not found on {currentObject.name}!");
            
            UpdateHairVisibilityState();
        }
        
        private bool IsHairObjectExists()
        {
            return currentObject != null && characterHairView != null;
        }
    
    }
} 