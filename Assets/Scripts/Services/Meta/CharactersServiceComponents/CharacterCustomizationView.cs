using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;
using Zenject;
using YG;

namespace Game.Services.Meta
{
    
    public class CharacterCustomizationView : MonoBehaviour
    {
        [SerializeField] private CustomizationTemplateSO customizationTemplate;
        
        [SerializeReference] private List<SlotReferenceBase> allSlotReferences = new List<SlotReferenceBase>();
        
        private Dictionary<string, SlotReferenceBase> slotsDictionary = new Dictionary<string, SlotReferenceBase>();
        
        private IAssetsService assetsService;
        
        public CustomizationTemplateSO CustomizationTemplate => customizationTemplate;
        
        public event Action<string, string, Color> OnSlotItemChanged;
        
        [Inject]
        private void Construct(IAssetsService assetsService)
        {
            this.assetsService = assetsService;
            Initialize();
        }
        
        private void Initialize()
        {
            slotsDictionary.Clear();
            
            foreach (var slotReference in allSlotReferences)
            {
                if (string.IsNullOrEmpty(slotReference.SlotId))
                {
                    Logs.Warning("Found slot with empty SlotId, skipping");
                    continue;
                }
                
                slotsDictionary[slotReference.SlotId] = slotReference;
                
                if (slotReference is IInitializableSlot initializableSlot)
                    initializableSlot.Initialize(this);
            }
        }
        
        public bool TryGetSlot<T>(string slotId, out T slotReference) where T : class
        {
            if (slotsDictionary.TryGetValue(slotId, out var slot) && slot is T typedSlot)
            {
                slotReference = typedSlot;
                return true;
            }
            
            slotReference = null;
            return false;
        }
        
        public void ClearSlot(string slotId)
        {
            Logs.Debug($"CLEAR {slotId}");
            
            if (slotsDictionary.TryGetValue(slotId, out var slot))
            {
                slot.ClearItem(assetsService);
                OnSlotItemChanged?.Invoke(slotId, string.Empty, Color.white);
            }
            else
                Logs.Warning($"Slot not found: {slotId}");
            
        }
        
        public async UniTask ApplyItemToSlot(CharacterItemData itemData)
        {
            Logs.Debug($"Set new item {itemData.ItemId}");
            
            if (itemData == null)
            {
                Logs.Warning("Attempted to apply null itemData");
                return;
            }
            
            if (!slotsDictionary.TryGetValue(itemData.SlotId, out var slot))
            {
                Logs.Error($"Slot not found: {itemData.SlotId}");
                return;
            }
            
            if (slot.GetCurrentItemId() == itemData.ItemId)
            {
                Logs.Debug($"Item already applied to slot: {itemData.ItemId}");
                return;
            }
            
            await slot.ApplyItem(itemData, assetsService);
            
            ItemChangeEventEmit();
            void ItemChangeEventEmit()
            {
                var color = Color.clear;
            
                if (slot is IColorizedSlot colorizedSlot)
                    color = colorizedSlot.GetCurrentColor();
            
                OnSlotItemChanged?.Invoke(itemData.SlotId, itemData.ItemId, color);
            }
        }
        
        public bool SetSlotColor(string slotId, Color color)
        {
            if (slotsDictionary.TryGetValue(slotId, out var slot) && slot is IColorizedSlot colorizedSlot)
            {
                bool result = colorizedSlot.SetColor(color);
                
                if (result)
                {
                    OnSlotItemChanged?.Invoke(slotId, slot.GetCurrentItemId(), color);
                }
                
                return result;
            }
            
            Logs.Warning($"Colorized slot not found: {slotId}");
            return false;
        }
        
        public List<SavesYG.CharacterSlotData> GetSlotsData()
        {
            List<SavesYG.CharacterSlotData> result = new List<SavesYG.CharacterSlotData>();
            
            foreach (var slotPair in slotsDictionary)
            {
                string slotId = slotPair.Key;
                var slot = slotPair.Value;
                
                string itemId = slot.GetCurrentItemId();
                Color color = Color.clear;

                if (slot is IColorizedSlot colorizedSlot)
                    color = colorizedSlot.GetCurrentColor();
                
                result.Add(new SavesYG.CharacterSlotData(slotId, itemId, color));
            }
            
            return result;
        }

        public bool IsItemEquipped(string itemId)
        {
            foreach (var slotPair in slotsDictionary)
            {
                var slot = slotPair.Value;
                
                if (slot.GetCurrentItemId() == itemId)
                    return true;
            }
            
            return false;
        }
        
        public bool IsSlotRemovable(string slotId)
        {
            var slot = customizationTemplate.GetSlotById(slotId);
            return slot.IsRemovable;
        }
        
        private void OnDestroy()
        {
            foreach (var slot in allSlotReferences)
            {
                if (slot != null)
                    slot.ClearItem(assetsService);
            }
            
            slotsDictionary.Clear();
        }
        
        public async UniTask SetSlotsSavedData(List<SavesYG.CharacterSlotData> saveData)
        {
            if (saveData == null)
            {
                Logs.Warning("Attempted to load null save data");
                return;
            }
            
            foreach (var slotData in saveData)
            {
                if (string.IsNullOrEmpty(slotData.ItemId))
                {
                    ClearSlot(slotData.SlotId);
                    continue;
                }
                
                var itemData = ConfigsProxy.CharactersAndShopConfig.GetItemById(slotData.ItemId);
                if (itemData == null)
                    continue;
                
                if (itemData.CharacterGender != CharacterGender.All && 
                    customizationTemplate.CharacterGender != itemData.CharacterGender)
                    continue;
                
                await ApplyItemToSlot(itemData);
                if (itemData.IsCanColorize)
                    SetSlotColor(slotData.SlotId, slotData.ItemColor);
            }
        }
        
        public async UniTask ApplyDefaultItems()
        {
            
            if (customizationTemplate == null)
            {
                Logs.Warning("CustomizationTemplate is null, cannot apply default items");
                return;
            }
            
            foreach (var slot in customizationTemplate.CustomizationSlots)
            {
                
                if (slot == null)
                    continue;
                    
                var defaultItem = customizationTemplate.GetDefaultItemForSlot(slot.SlotId);
                if (defaultItem != null)
                {
                    await ApplyItemToSlot(defaultItem);
                }
            }
        }
    }
}