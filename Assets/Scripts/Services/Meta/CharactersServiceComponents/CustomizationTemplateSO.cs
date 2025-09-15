using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.Meta
{
    [CreateAssetMenu(fileName = "CharacterCustomizationTemplate", menuName = "Customization/Character Template", order = 1)]
    public class CustomizationTemplateSO : ScriptableObject
    {
        [SerializeField] private string templateId;
        [SerializeField] private CharacterGender characterGender;
        [SerializeField] private List<CustomizationSlotSO> customizationSlots = new List<CustomizationSlotSO>();
        [SerializeField] private string characterModelAddressableId;
        
        [SerializeField] private List<DefaultItemData> defaultItemsList = new List<DefaultItemData>();
        
        public string TemplateId => templateId;
        public CharacterGender CharacterGender => characterGender;
        public IReadOnlyList<CustomizationSlotSO> CustomizationSlots => customizationSlots;
        public string CharacterModelAddressableId => characterModelAddressableId;
        
        public CustomizationSlotSO GetSlotById(string slotId)
        {
            foreach (var slot in customizationSlots)
            {
                if (slot.SlotId == slotId)
                    return slot;
            }
            
            throw new ArgumentException($"Slot with id {slotId} is not found!");
        }
        
        public bool HasSlot(string slotId)
        {
            return GetSlotById(slotId) != null;
        }
        
        public void AddSlot(CustomizationSlotSO slot)
        {
            if (!customizationSlots.Contains(slot))
            {
                customizationSlots.Add(slot);
            }
        }
        
        public void RemoveSlot(CustomizationSlotSO slot)
        {
            if (customizationSlots.Contains(slot))
            {
                customizationSlots.Remove(slot);
                
                // Also remove default item for this slot if it exists
                if (slot != null && !string.IsNullOrEmpty(slot.SlotId))
                {
                    RemoveDefaultItemForSlot(slot.SlotId);
                }
            }
        }
        
        public CharacterItemData GetDefaultItemForSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
                return null;
                
            foreach (var data in defaultItemsList)
            {
                if (data.slotId == slotId && data.item != null)
                    return data.item;
            }
            
            return null;
        }
        
        public void SetDefaultItemForSlot(string slotId, CharacterItemData item)
        {
            if (string.IsNullOrEmpty(slotId))
                return;
                
            // Check that slot exists
            if (!HasSlot(slotId))
                return;
                
            // Check that item fits the slot
            if (item != null && item.SlotId != slotId)
                return;
            
            // First remove existing item with this slotId if any
            RemoveDefaultItemForSlot(slotId);
            
            // If null is passed, just remove item (already done above)
            if (item == null)
                return;
                
            // Add new item
            var newData = new DefaultItemData
            {
                slotId = slotId,
                item = item
            };
            
            defaultItemsList.Add(newData);
        }
        
        private void RemoveDefaultItemForSlot(string slotId)
        {
            for (int i = defaultItemsList.Count - 1; i >= 0; i--)
            {
                if (defaultItemsList[i].slotId == slotId)
                {
                    defaultItemsList.RemoveAt(i);
                    break;
                }
            }
        }
        
        [System.Serializable]
        public class DefaultItemData
        {
            public string slotId;
            public CharacterItemData item;
        }
        
    }
} 