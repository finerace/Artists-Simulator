using System;
using Game.Services.Common;
using Game.Infrastructure.Configs;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Services.Meta
{
    [Serializable]
    public abstract class SlotReferenceBase
    {
        [SerializeField] protected string slotId;
        
        [NonSerialized] protected CharacterItemData currentItem;
        
        public string SlotId => slotId;
        public CharacterItemData CurrentItem => currentItem;
        
        public abstract void ClearItem(IAssetsService assetsService);
        
        public abstract UniTask ApplyItem(CharacterItemData itemData, IAssetsService assetsService);
        
        public string GetCurrentItemId()
        {
            return currentItem?.ItemId ?? string.Empty;
        }
    }
} 