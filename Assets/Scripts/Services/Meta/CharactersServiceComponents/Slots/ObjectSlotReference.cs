using System;
using UnityEngine;
using Game.Services.Common;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    
    [Serializable]
    [SlotType(ItemType.Object)]
    public class ObjectSlotReference : SlotReferenceBase, IColorizedSlot
    {
        [SerializeField] protected Transform slotTransform;
        [SerializeField] protected Transform transformTemplate;
        
        [NonSerialized] protected GameObject currentObject;
        [NonSerialized] protected Color currentColor = Color.white;
        
        public Transform SlotTransform => slotTransform;
        public Transform TransformTemplate => transformTemplate;
        public GameObject CurrentObject => currentObject;
        
        public ObjectSlotReference(string slotId, Transform slotTransform, Transform transformTemplate = null)
        {
            this.slotId = slotId;
            this.slotTransform = slotTransform;
            this.transformTemplate = transformTemplate;
        }
        
        public void SetItem(GameObject gameObject, CharacterItemData itemData)
        {
            currentObject = gameObject;
            currentItem = itemData;
            currentColor = itemData?.DefaultColor ?? Color.white;
        }
        
        public override void ClearItem(IAssetsService assetsService)
        {
            if (currentObject != null)
                assetsService.ReleaseAsset(currentObject);
            
            currentObject = null;
            currentItem = null;
            currentColor = Color.white;
        }
        
        public override async UniTask ApplyItem(CharacterItemData itemData, IAssetsService assetsService)
        {
            if (itemData == null)
            {
                Logs.Warning("Attempted to apply null itemData to ObjectSlot");
                return;
            }
            
            ClearItem(assetsService);
            
            if (string.IsNullOrEmpty(itemData.ItemObjectAddressableId))
                return;
            
            var instantiatedObject = await assetsService.GetAsset<Transform>(itemData.ItemObjectAddressableId);
            
            if (instantiatedObject != null)
            {
                var itemGO = instantiatedObject.gameObject;
                var itemT = itemGO.transform;
                
                itemT.SetParent(slotTransform);
                
                if (transformTemplate == null)
                {
                    itemT.localPosition = Vector3.zero;
                    itemT.localRotation = Quaternion.identity;
                    itemT.localScale = Vector3.one;
                }
                else
                {
                    itemT.localPosition = transformTemplate.localPosition;
                    itemT.localRotation = transformTemplate.localRotation;
                    itemT.localScale = transformTemplate.localScale;
                }
                
                SetItem(itemGO, itemData);
                
                if (itemData.IsCanColorize)
                {
                    var colorizeView = itemGO.GetComponent<CharacterItemColorizeView>();
                    if (colorizeView != null)
                    {
                        colorizeView.SetColor(itemData.DefaultColor);
                    }
                    else
                        Logs.Warning($"CharacterItemColorizeView not found on {itemGO.name}");
                }
            }
            else
                Logs.Error($"Failed to load asset: {itemData.ItemObjectAddressableId}");
        }
        
        public bool SetColor(Color color)
        {
            if (currentObject == null)
                return false;
            
            var colorizeView = currentObject.GetComponent<CharacterItemColorizeView>();

            if (colorizeView != null)
            {
                colorizeView.SetColor(color);
                currentColor = color;

                return true;
            }
            
            return false;
        }
        
        public Color GetCurrentColor()
        {
            return currentColor;
        }
    }
} 