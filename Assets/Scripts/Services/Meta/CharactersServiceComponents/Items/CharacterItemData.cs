using UnityEngine;

namespace Game.Services.Meta
{
    [CreateAssetMenu(fileName = "CharacterItem", menuName = "Create CharacterItem", order = 1)]
    public class CharacterItemData : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string slotId;

        [SerializeField] private int itemNameId;
        [SerializeField] private int itemDescId;
        
        [SerializeField] private bool isItemsPack;
        [SerializeField] private CharacterItemData[] itemsPack;
        
        [SerializeField] private ItemType itemType;

        [SerializeField] private bool itemIsAlwaysUnlocked;
        [SerializeField] private bool isHiddenFromShop;
        [SerializeField] private int coinsPrice = 250;
        [SerializeField] private int crystalsPrice;
        
        [SerializeField] private CharacterGender characterGender = CharacterGender.All;
        
        [SerializeField] private bool isCanColorize = false;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private int colorizeCoinsPrice = 100;
        
        [SerializeField] private string itemObjectAddressableId;
        [SerializeField] private Material itemMaterial;

        [SerializeField] private bool isIconObject = true;
        [SerializeField] private float iconObjectScale = 1;
        [SerializeField] private Vector3 iconObjectLocalPos = Vector3.zero;
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private Texture itemIcon;

        public string ItemId => itemId;
        
        public string SlotId => slotId;

        public int ItemNameId => itemNameId;

        public int ItemDescId => itemDescId;

        public bool IsItemsPack => isItemsPack;

        public CharacterItemData[] ItemsPack => itemsPack;

        public ItemType ItemType => itemType;

        public bool ItemIsAlwaysUnlocked => itemIsAlwaysUnlocked;

        public bool IsHiddenFromShop => isHiddenFromShop;

        public int CoinsPrice => coinsPrice;

        public int CrystalsPrice => crystalsPrice;

        public CharacterGender CharacterGender => characterGender;

        public bool IsCanColorize => isCanColorize;

        public Color DefaultColor => defaultColor;

        public int ColorizeCoinsPrice => colorizeCoinsPrice;
        
        public string ItemObjectAddressableId => itemObjectAddressableId;

        public Material ItemMaterial => itemMaterial;

        public bool IsIconObject => isIconObject;

        public float IconObjectScale => iconObjectScale;

        public Color IconColor => iconColor;

        public Texture ItemIcon => itemIcon;

        public Vector3 IconObjectLocalPos => iconObjectLocalPos;

        public void SetPack(CharacterItemData[] pack)
        {
            SetItemsPack();
            void SetItemsPack()
            {
                itemsPack = pack;
                isItemsPack = true;
            }
        }
    }
}