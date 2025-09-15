using UnityEngine;

namespace Game.Services.Meta
{
    [CreateAssetMenu(fileName = "CustomizationSlot", menuName = "Customization/Slot", order = 2)]
    public class CustomizationSlotSO : ScriptableObject
    {
        [SerializeField] private string slotId;
        [SerializeField] private ItemType slotType;
        [SerializeField] private bool isRemovable = true;
        
        public string SlotId => slotId;
        public ItemType SlotType => slotType;
        public bool IsRemovable => isRemovable;
        
        public bool CanAcceptItem(CharacterItemData item)
        {
            if (item == null)
                return isRemovable;
            
            return item.SlotId == slotId;
        }
        
        public void Initialize(string slotId, ItemType slotType, bool isRemovable)
        {
            this.slotId = slotId;
            this.slotType = slotType;
            this.isRemovable = isRemovable;
        }
    }
} 