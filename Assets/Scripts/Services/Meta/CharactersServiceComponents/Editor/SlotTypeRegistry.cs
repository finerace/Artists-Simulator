using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game.Services.Meta
{
    public static class SlotTypeRegistry
    {
        private static readonly Dictionary<ItemType, Type> itemTypeToSlotType = new Dictionary<ItemType, Type>();
        private static readonly Dictionary<Type, ItemType> slotTypeToItemType = new Dictionary<Type, ItemType>();
        private static bool isInitialized = false;
        
        public static Type GetSlotType(ItemType itemType)
        {
            EnsureInitialized();
            return itemTypeToSlotType.TryGetValue(itemType, out var slotType) ? slotType : typeof(ObjectSlotReference);
        }
        
        public static ItemType GetItemType(Type slotType)
        {
            EnsureInitialized();
            return slotTypeToItemType.TryGetValue(slotType, out var itemType) ? itemType : ItemType.Object;
        }
        
        public static string GetSlotTypeName(ItemType itemType)
        {
            return GetSlotType(itemType).Name;
        }
        
        public static string GetDisplayName(ItemType itemType)
        {
            return itemType.ToString();
        }
        
        public static SlotReferenceBase CreateSlotInstance(string slotTypeName)
        {
            EnsureInitialized();
            var slotType = itemTypeToSlotType.Values.FirstOrDefault(t => t.Name == slotTypeName);
            if (slotType == null)
                return new ObjectSlotReference("", null);
                
            return CreateSlotInstanceByType(slotType);
        }
        
        private static SlotReferenceBase CreateSlotInstanceByType(Type slotType)
        {
            try
            {
                if (slotType == typeof(MaterialSlotReference))
                    return (SlotReferenceBase)Activator.CreateInstance(slotType, "", null, 0);
                else
                    return (SlotReferenceBase)Activator.CreateInstance(slotType, "", null);
            }
            catch
            {
                return new ObjectSlotReference("", null);
            }
        }
        
        private static void EnsureInitialized()
        {
            if (isInitialized)
                return;
                
            InitializeRegistry();
            isInitialized = true;
        }
        
        private static void InitializeRegistry()
        {
            FindAndRegisterSlotTypes();
            
            void FindAndRegisterSlotTypes()
            {
                var slotTypes = FindSlotTypesWithAttributes();
                
                foreach (var slotType in slotTypes)
                    RegisterSlotType(slotType);
            }
            
            IEnumerable<Type> FindSlotTypesWithAttributes()
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.Load("Assembly-CSharp");
                }
                catch
                {
                    assembly = Assembly.GetExecutingAssembly();
                }
                
                return assembly.GetTypes().Where(IsValidSlotType);
            }
            
            bool IsValidSlotType(Type type)
            {
                return type.IsSubclassOf(typeof(SlotReferenceBase)) && 
                       !type.IsAbstract && 
                       type.GetCustomAttribute<SlotTypeAttribute>() != null;
            }
            
            void RegisterSlotType(Type slotType)
            {
                var attribute = slotType.GetCustomAttribute<SlotTypeAttribute>();
                var itemType = attribute.ItemType;
                
                itemTypeToSlotType[itemType] = slotType;
                slotTypeToItemType[slotType] = itemType;
            }
        }
    }
} 