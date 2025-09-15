using UnityEngine;
using Game.Additional.MagicAttributes;
using System;
using Game.Services.Meta;
using System.Linq;


public class CharacterHairView : MonoBehaviour
{
    [SerializeField] private GameObject[] extraElements;
    
    
    [Header("Hair Default Data")]
    [SerializeField] private Vector3 hatsPosModificator;
    [SerializeField] private Vector3 rotationModificator;
    
    [Header("Hair Unique Data")]
    [SerializeField] private HairViewData[] hairViewData;
    
    public GameObject[] ExtraElements => extraElements;
    
    public Vector3 HatsPosModificator => hatsPosModificator;
    public Vector3 RotationModificator => rotationModificator;
    
    public (Vector3, Vector3) GetHairViewData(string itemId)
    {
        var data = hairViewData.FirstOrDefault(data => data.ItemData.ItemId == itemId);
        
        if (data.ItemData == null)
            return (hatsPosModificator, rotationModificator);

        return (data.HatsPosModificator, data.RotationModificator);
    }

    [Serializable]
    public struct HairViewData
    {
        public CharacterItemData ItemData;
        public Vector3 HatsPosModificator;
        public Vector3 RotationModificator;
    }

}
