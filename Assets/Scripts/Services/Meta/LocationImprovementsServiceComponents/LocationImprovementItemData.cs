using UnityEngine;

namespace Game.Services.Meta
{
    [CreateAssetMenu(fileName = "LocImprovementItem", menuName = "Create LocImprovementItem", order = 1)]
    public class LocationImprovementItemData : ScriptableObject
    {
        [SerializeField] private string id;

        [Space]
        
        [SerializeField] private string nameId;

        [Space] 
        
        [SerializeField] private int price;
        [SerializeField] private CurrencyType currencyType;
        
        [Space] 
        
        [SerializeField] private string prefabPath;

        public string Id => id;

        public string NameId => nameId;

        public int Price => price;

        public string PrefabPath => prefabPath;

        public CurrencyType CurrencyType => currencyType;
    }
    
    public enum CurrencyType
    {
        Coins,
        Crystals
    }
    
}