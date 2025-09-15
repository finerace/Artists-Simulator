using System;

namespace Game.Services.Meta
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SlotTypeAttribute : Attribute
    {
        public ItemType ItemType { get; }
        
        public SlotTypeAttribute(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
} 