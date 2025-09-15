using UnityEditor;
using UnityEngine;
using Game.Infrastructure.Configs;
using System;
using System.Collections.Generic;

namespace Game.Services.Meta.Editor
{
    /// <summary>
    /// Утилитный класс для обработки выпадающих списков ID слотов в Editor
    /// </summary>
    public static class SlotIdEditorUtility
    {
        private static Dictionary<ItemType, string[]> cachedSlotIdsByType = new Dictionary<ItemType, string[]>();
        private static Dictionary<ItemType, GUIContent[]> cachedSlotIdContentsByType = new Dictionary<ItemType, GUIContent[]>();
        private const string CustomOptionText = "+ Добавить новый...";
        
        /// <summary>
        /// Отрисовка выпадающего списка ID слотов для редактора
        /// </summary>
        /// <param name="position">Позиция элемента</param>
        /// <param name="label">Метка элемента</param>
        /// <param name="selectedSlotId">Текущий выбранный ID слота</param>
        /// <param name="itemType">Тип слота (Object или Material)</param>
        /// <param name="allowCustomValues">Разрешить ввод пользовательских значений</param>
        /// <returns>Выбранный ID слота</returns>
        public static string SlotIdPopup(Rect position, GUIContent label, string selectedSlotId, ItemType itemType, bool allowCustomValues = true)
        {
            var slotIds = GetSlotIdsByType(itemType);
            var slotIdContents = GetSlotIdContentsByType(itemType);
            
            if (slotIds == null || slotIds.Length == 0)
            {
                return EditorGUI.TextField(position, label, selectedSlotId);
            }
            
            // Находим индекс выбранного ID в массиве
            int selectedIndex = -1;
            for (int i = 0; i < slotIds.Length; i++)
            {
                if (slotIds[i] == selectedSlotId)
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            // Если используется пользовательское значение, которого нет в списке
            bool isCustomValue = selectedIndex == -1;
            
            // Создаем массив с дополнительным элементом в конце для опции "Добавить новый"
            string[] displaySlotIds;
            GUIContent[] displayContents;
            
            if (allowCustomValues)
            {
                displaySlotIds = new string[slotIds.Length + 1];
                displayContents = new GUIContent[slotIdContents.Length + 1];
                
                Array.Copy(slotIds, displaySlotIds, slotIds.Length);
                Array.Copy(slotIdContents, displayContents, slotIdContents.Length);
                
                displaySlotIds[slotIds.Length] = "";
                displayContents[slotIdContents.Length] = new GUIContent(CustomOptionText);
            }
            else
            {
                displaySlotIds = slotIds;
                displayContents = slotIdContents;
            }
            
            // Если выбрано пользовательское значение, которого нет в списке
            if (isCustomValue)
            {
                int customIndex = -1;
                
                // Создаем новый дисплейный массив с пользовательским значением
                string[] customDisplaySlotIds = new string[displaySlotIds.Length + 1];
                GUIContent[] customDisplayContents = new GUIContent[displayContents.Length + 1];
                
                // Вставляем пользовательское значение в начало списка
                customDisplaySlotIds[0] = selectedSlotId;
                
                if(!String.IsNullOrEmpty(selectedSlotId))
                    customDisplayContents[0] = new GUIContent(selectedSlotId + " (Текущее)");
                else
                    customDisplayContents[0] = new GUIContent("Не назначено!");
                
                // Копируем остальные элементы
                for (int i = 0; i < displaySlotIds.Length; i++)
                {
                    customDisplaySlotIds[i + 1] = displaySlotIds[i];
                    customDisplayContents[i + 1] = displayContents[i];
                }
                
                displaySlotIds = customDisplaySlotIds;
                displayContents = customDisplayContents;
                customIndex = 0;
                selectedIndex = customIndex;
            }
            else if (selectedIndex == -1)
            {
                // Если ничего не выбрано, выбираем первый элемент
                selectedIndex = 0;
            }
            
            // Отображаем выпадающий список
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label, selectedIndex, displayContents);
            if (EditorGUI.EndChangeCheck())
            {
                // Если выбрана опция "Добавить новый..."
                if (allowCustomValues && (
                        (isCustomValue && newIndex == displaySlotIds.Length - 1) || 
                        (!isCustomValue && newIndex == displaySlotIds.Length - 1)))
                {
                    // Создаем окно для ввода нового значения
                    string newValue = EditorInputDialog.Show("Добавить новый ID слота", "Введите ID слота:", "");
                    if (!string.IsNullOrEmpty(newValue))
                    {
                        return newValue;
                    }
                    
                    return selectedSlotId; // Возвращаем текущее значение, если ввод отменен
                }
                
                // Если выбрано пользовательское значение
                if (isCustomValue && newIndex == 0)
                {
                    return selectedSlotId;
                }
                
                // Корректируем индекс при наличии пользовательского значения в списке
                int targetIndex = isCustomValue ? newIndex - 1 : newIndex;
                
                // Получаем ID слота из оригинального списка
                if (targetIndex >= 0 && targetIndex < slotIds.Length)
                {
                    return slotIds[targetIndex];
                }
            }
            
            return selectedSlotId;
        }
        
        /// <summary>
        /// Сохраняем предыдущую версию метода для обратной совместимости
        /// </summary>
        public static string SlotIdPopup(Rect position, GUIContent label, string selectedSlotId, bool allowCustomValues = true)
        {
            // Показываем все слоты (и Object и Material)
            return SlotIdPopup(position, label, selectedSlotId, ItemType.Object, allowCustomValues);
        }
        
        /// <summary>
        /// Отрисовка выпадающего списка ID слотов для редактора (версия с SerializedProperty)
        /// </summary>
        public static void SlotIdPopup(Rect position, SerializedProperty property, GUIContent label, ItemType itemType, bool allowCustomValues = true)
        {
            string currentValue = property.stringValue;
            string newValue = SlotIdPopup(position, label, currentValue, itemType, allowCustomValues);
            
            if (newValue != currentValue)
            {
                property.stringValue = newValue;
            }
        }
        
        /// <summary>
        /// Сохраняем предыдущую версию метода для обратной совместимости
        /// </summary>
        public static void SlotIdPopup(Rect position, SerializedProperty property, GUIContent label, bool allowCustomValues = true)
        {
            SlotIdPopup(position, property, label, ItemType.Object, allowCustomValues);
        }
        
        /// <summary>
        /// Возвращает массив доступных ID слотов по типу
        /// </summary>
        private static string[] GetSlotIdsByType(ItemType type)
        {
            if (cachedSlotIdsByType.TryGetValue(type, out var cachedIds) && cachedIds.Length > 0)
                return cachedIds;
                
            var config = GetCharacterShopConfig();
            
            string[] slotIds;
            
            if (config != null)
            {
                // Используем новый метод получения ID слотов по типу из конфига
                slotIds = config.GetSlotIdsByType(type);
                
                // Если массив пустой, проверим слоты из существующих ассетов
                if (slotIds == null || slotIds.Length == 0)
                {
                    slotIds = GetSlotIdsFromAssets(type);
                }
            }
            else
            {
                // Если конфиг не найден, ищем слоты в ассетах
                slotIds = GetSlotIdsFromAssets(type);
                
                // Если и в ассетах не нашли, возвращаем пустой массив
                if (slotIds == null || slotIds.Length == 0)
                {
                    slotIds = new string[0];
                }
            }
            
            cachedSlotIdsByType[type] = slotIds;
            return slotIds;
        }
        
        /// <summary>
        /// Получает ID слотов нужного типа из существующих ассетов CustomizationSlotSO
        /// </summary>
        private static string[] GetSlotIdsFromAssets(ItemType type)
        {
            var slotSOs = FindAllCustomizationSlots();
            var filteredSlotIds = new List<string>();
            
            foreach (var slot in slotSOs)
            {
                if (slot.SlotType == type)
                {
                    filteredSlotIds.Add(slot.SlotId);
                }
            }
            
            return filteredSlotIds.ToArray();
        }
        
        /// <summary>
        /// Возвращает массив всех слотов кастомизации из проекта
        /// </summary>
        private static CustomizationSlotSO[] FindAllCustomizationSlots()
        {
            var guids = AssetDatabase.FindAssets("t:CustomizationSlotSO");
            var slots = new List<CustomizationSlotSO>();
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var slot = AssetDatabase.LoadAssetAtPath<CustomizationSlotSO>(path);
                if (slot != null)
                {
                    slots.Add(slot);
                }
            }
            
            return slots.ToArray();
        }
        
        /// <summary>
        /// Возвращает массив GUIContent для отображения ID слотов по типу
        /// </summary>
        private static GUIContent[] GetSlotIdContentsByType(ItemType type)
        {
            if (cachedSlotIdContentsByType.TryGetValue(type, out var cachedContents) && cachedContents.Length > 0)
                return cachedContents;
                
            var slotIds = GetSlotIdsByType(type);
            var contents = new GUIContent[slotIds.Length];
            
            for (int i = 0; i < slotIds.Length; i++)
            {
                contents[i] = new GUIContent(slotIds[i]);
            }
            
            cachedSlotIdContentsByType[type] = contents;
            return cachedSlotIdContentsByType[type];
        }
        
        /// <summary>
        /// Находит и возвращает CharacterShopConfig из ресурсов проекта
        /// </summary>
        private static CharacterShopConfig GetCharacterShopConfig()
        {
            // Ищем конфиг в ресурсах проекта
            var guids = AssetDatabase.FindAssets("t:CharacterShopConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<CharacterShopConfig>(path);
            }
            
            return null;
        }
        
        /// <summary>
        /// Очищает кэш ID слотов
        /// </summary>
        public static void ClearCache()
        {
            cachedSlotIdsByType.Clear();
            cachedSlotIdContentsByType.Clear();
        }
    }
    
    /// <summary>
    /// Диалоговое окно для ввода текста
    /// </summary>
    public class EditorInputDialog : EditorWindow
    {
        public static string Show(string title, string message, string defaultText)
        {
            var window = ScriptableObject.CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window.messageText = message;
            window.inputText = defaultText;
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 120);
            window.ShowModal();
            
            return window.resultText;
        }
        
        private string messageText = "";
        private string inputText = "";
        private string resultText = "";
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField(messageText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);
            
            GUI.SetNextControlName("InputTextField");
            inputText = EditorGUILayout.TextField(inputText);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Отмена", GUILayout.Width(100)))
            {
                resultText = "";
                Close();
            }
            
            if (GUILayout.Button("OK", GUILayout.Width(100)))
            {
                resultText = inputText;
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.FocusTextInControl("InputTextField");
            
            // Обработка нажатия Enter и Escape
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    resultText = inputText;
                    e.Use();
                    Close();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    resultText = "";
                    e.Use();
                    Close();
                }
            }
        }
    }
} 