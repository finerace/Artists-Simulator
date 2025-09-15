#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using TMPro;
using Game.Infrastructure.Configs;
using Game.Cosmetic.UI;

namespace Game.Cosmetic.EditorUI
{
    [CustomEditor(typeof(DropdownLocalizer))]
    public class DropdownLocalizerEditor : Editor
    {
        private DropdownLocalizer dropdownLocalizer;
        private SerializedProperty dropdownProperty;
        private SerializedProperty dropdownItemsProperty;
        
        // Поля для фильтрации ключей
        private string keySearchFilter = "";
        private bool showSearchField = true;
        private Vector2 scrollPosition;
        
        // Кэш для ключей локализации
        private List<string> allKeys = new List<string>();
        private List<string> filteredKeys = new List<string>();
        
        // Кэш категорий ключей
        private List<string> keyCategories = new List<string>();
        private int selectedCategoryIndex = -1; // -1 = All categories
        
        // Настройки для редактора
        private bool autoFindDropdown = true;
        private bool groupTextsByCategory = true;
        
        private void OnEnable()
        {
            dropdownLocalizer = (DropdownLocalizer)target;
            dropdownProperty = serializedObject.FindProperty("dropdown");
            dropdownItemsProperty = serializedObject.FindProperty("dropdownItems");
            
            // Загружаем настройки редактора
            keySearchFilter = EditorPrefs.GetString("DropdownLocalizerEditor_KeySearchFilter", "");
            showSearchField = EditorPrefs.GetBool("DropdownLocalizerEditor_ShowSearchField", true);
            autoFindDropdown = EditorPrefs.GetBool("DropdownLocalizerEditor_AutoFindDropdown", true);
            groupTextsByCategory = EditorPrefs.GetBool("DropdownLocalizerEditor_GroupByCategory", true);
            selectedCategoryIndex = EditorPrefs.GetInt("DropdownLocalizerEditor_SelectedCategory", -1);
            
            // Загружаем ключи локализации
            LoadLocalizationKeys();
        }
        
        private void LoadLocalizationKeys()
        {
            // Находим конфиг локализации в проекте
            LocalizationConfig config = GetLocalizationConfig();
            
            if (config != null)
            {
                allKeys = config.GetAllKeys().ToList();
                
                // Извлекаем категории из ключей (формат: CATEGORY_NAME)
                keyCategories = new List<string> { "Все категории" };
                HashSet<string> uniqueCategories = new HashSet<string>();
                
                foreach (var key in allKeys)
                {
                    string category = GetCategoryFromKey(key);
                    if (!string.IsNullOrEmpty(category))
                    {
                        uniqueCategories.Add(category);
                    }
                }
                
                keyCategories.AddRange(uniqueCategories.OrderBy(c => c));
                
                // Применяем фильтры
                UpdateFilteredKeys();
            }
            else
            {
                allKeys = new List<string>();
                keyCategories = new List<string> { "Все категории" };
            }
        }
        
        private LocalizationConfig GetLocalizationConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:LocalizationConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LocalizationConfig>(path);
            }
            return null;
        }
        
        private string GetCategoryFromKey(string key)
        {
            int index = key.IndexOf('_');
            if (index > 0)
            {
                return key.Substring(0, index);
            }
            return "";
        }
        
        private void UpdateFilteredKeys()
        {
            filteredKeys = allKeys.ToList();
            
            // Применяем фильтр по категории
            if (selectedCategoryIndex > 0 && selectedCategoryIndex < keyCategories.Count)
            {
                string selectedCategory = keyCategories[selectedCategoryIndex];
                filteredKeys = filteredKeys.Where(k => GetCategoryFromKey(k) == selectedCategory).ToList();
            }
            
            // Применяем поисковый фильтр
            if (!string.IsNullOrEmpty(keySearchFilter))
            {
                filteredKeys = filteredKeys.Where(k => k.ToLower().Contains(keySearchFilter.ToLower())).ToList();
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(5);
            
            // Отображаем панель настроек
            DrawSettingsPanel();
            
            EditorGUILayout.Space(10);
            
            // Отображаем выбор dropdown компонента
            DrawDropdownField();
            
            EditorGUILayout.Space(10);
            
            // Отображаем панель поиска ключей
            if (showSearchField)
            {
                DrawSearchPanel();
            }
            
            EditorGUILayout.Space(10);
            
            // Кнопки для управления DropdownItems
            DrawDropdownItemsManagerButtons();
            
            EditorGUILayout.Space(5);
            
            // Отображаем элементы локализации
            DrawDropdownItemsList();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawSettingsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Настройки редактора", EditorStyles.boldLabel);
            
            bool newAutoFindDropdown = EditorGUILayout.Toggle("Автопоиск dropdown", autoFindDropdown);
            if (newAutoFindDropdown != autoFindDropdown)
            {
                autoFindDropdown = newAutoFindDropdown;
                EditorPrefs.SetBool("DropdownLocalizerEditor_AutoFindDropdown", autoFindDropdown);
            }
            
            bool newGroupByCategory = EditorGUILayout.Toggle("Группировать по категориям", groupTextsByCategory);
            if (newGroupByCategory != groupTextsByCategory)
            {
                groupTextsByCategory = newGroupByCategory;
                EditorPrefs.SetBool("DropdownLocalizerEditor_GroupByCategory", groupTextsByCategory);
            }
            
            bool newShowSearchField = EditorGUILayout.Toggle("Показывать поиск", showSearchField);
            if (newShowSearchField != showSearchField)
            {
                showSearchField = newShowSearchField;
                EditorPrefs.SetBool("DropdownLocalizerEditor_ShowSearchField", showSearchField);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDropdownField()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Выпадающий список", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PropertyField(dropdownProperty, GUIContent.none);
            
            if (GUILayout.Button("Найти", GUILayout.Width(60)))
            {
                FindDropdownOnObject();
            }
            
            EditorGUILayout.EndHorizontal();
            
            TMP_Dropdown dropdown = dropdownProperty.objectReferenceValue as TMP_Dropdown;
            if (dropdown == null)
            {
                EditorGUILayout.HelpBox("Выберите компонент TMP_Dropdown или нажмите 'Найти' для автоматического поиска на объекте.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Текущий dropdown: {dropdown.name}");
                
                if (dropdown.options != null && dropdown.options.Count > 0)
                {
                    EditorGUILayout.LabelField($"Количество опций: {dropdown.options.Count}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSearchPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Поиск и фильтрация ключей
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Поиск ключа:", GUILayout.Width(80));
            
            string newSearchFilter = EditorGUILayout.TextField(keySearchFilter);
            if (newSearchFilter != keySearchFilter)
            {
                keySearchFilter = newSearchFilter;
                EditorPrefs.SetString("DropdownLocalizerEditor_KeySearchFilter", keySearchFilter);
                UpdateFilteredKeys();
            }
            
            if (GUILayout.Button("Очистить", GUILayout.Width(70)))
            {
                keySearchFilter = "";
                EditorPrefs.SetString("DropdownLocalizerEditor_KeySearchFilter", "");
                UpdateFilteredKeys();
            }
            EditorGUILayout.EndHorizontal();
            
            // Выбор категории
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Категория:", GUILayout.Width(80));
            
            int newCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex + 1, keyCategories.ToArray()) - 1;
            if (newCategoryIndex != selectedCategoryIndex)
            {
                selectedCategoryIndex = newCategoryIndex;
                EditorPrefs.SetInt("DropdownLocalizerEditor_SelectedCategory", selectedCategoryIndex);
                UpdateFilteredKeys();
            }
            
            if (GUILayout.Button("Обновить ключи", GUILayout.Width(120)))
            {
                LoadLocalizationKeys();
            }
            EditorGUILayout.EndHorizontal();
            
            // Отображаем количество найденных ключей
            EditorGUILayout.LabelField($"Найдено ключей: {filteredKeys.Count} из {allKeys.Count}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDropdownItemsManagerButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Добавить элемент", GUILayout.Height(25)))
            {
                AddNewDropdownItem();
            }
            
            if (GUILayout.Button("Заполнить из dropdown", GUILayout.Height(25)))
            {
                FillFromDropdown();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Удалить пустые элементы"))
            {
                RemoveEmptyItems();
            }
            
            if (GUILayout.Button("Сдвинуть выделенный вверх", GUILayout.Width(180)))
            {
                MoveSelectedItemUp();
            }
            
            if (GUILayout.Button("Сдвинуть выделенный вниз", GUILayout.Width(180)))
            {
                MoveSelectedItemDown();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawDropdownItemsList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Элементы выпадающего списка", EditorStyles.boldLabel);
            
            if (dropdownItemsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Нет элементов для локализации. Нажмите 'Добавить элемент' или 'Заполнить из dropdown'.", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < dropdownItemsProperty.arraySize; i++)
                {
                    SerializedProperty itemProperty = dropdownItemsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty textIDProperty = itemProperty.FindPropertyRelative("textID");
                    SerializedProperty prefixProperty = itemProperty.FindPropertyRelative("prefix");
                    SerializedProperty suffixProperty = itemProperty.FindPropertyRelative("suffix");
                    
                    // Получаем перевод для предварительного просмотра
                    string previewText = "Не выбран ключ";
                    if (!string.IsNullOrEmpty(textIDProperty.stringValue))
                    {
                        LocalizationConfig config = GetLocalizationConfig();
                        if (config != null)
                        {
                            string localizedText = config.GetTextByKey(textIDProperty.stringValue, SystemLanguage.English);
                            if (localizedText.StartsWith("[") && localizedText.EndsWith("]"))
                                previewText = "Ключ не найден: " + textIDProperty.stringValue;
                            else
                            {
                                string prefix = prefixProperty.stringValue ?? "";
                                string suffix = suffixProperty.stringValue ?? "";
                                previewText = $"{prefix}{localizedText}{suffix}";
                            }
                        }
                    }
                    
                    // Создаем стиль с цветовым выделением
                    GUIStyle textContentStyle = new GUIStyle(EditorStyles.helpBox);
                    textContentStyle.normal.textColor = Color.white;
                    textContentStyle.fontStyle = FontStyle.Bold;
                    textContentStyle.richText = true;
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Индекс элемента и перевод
                    EditorGUILayout.BeginHorizontal();
                    
                    // Получаем текущее имя опции из dropdown, если возможно
                    string itemName = $"Элемент #{i}";
                    TMP_Dropdown dropdown = dropdownProperty.objectReferenceValue as TMP_Dropdown;
                    if (dropdown != null && dropdown.options != null && i < dropdown.options.Count)
                    {
                        itemName = $"{dropdown.options[i].text} <color=#888888>[{i}]</color>";
                    }
                    else if (!string.IsNullOrEmpty(previewText) && previewText != "Не выбран ключ" && !previewText.StartsWith("Ключ не найден:"))
                    {
                        itemName = $"{previewText} <color=#888888>[{i}]</color>";
                    }
                    
                    GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
                    headerStyle.wordWrap = true;
                    headerStyle.richText = true;
                    
                    EditorGUILayout.LabelField(itemName, headerStyle);
                    
                    // Кнопка удаления
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        dropdownItemsProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        i--;
                        continue;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Предварительный просмотр
                    EditorGUILayout.LabelField($"📝 {previewText}", textContentStyle);
                    
                    // Строка с выбором ключа локализации
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField("Ключ:", GUILayout.Width(40));
                    
                    // Текстовое поле для отображения текущего ключа
                    string currentID = textIDProperty.stringValue;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(currentID);
                    EditorGUI.EndDisabledGroup();
                    
                    // Выпадающий список для выбора ключа
                    if (GUILayout.Button("Выбрать", GUILayout.Width(70)))
                    {
                        ShowKeySelectionMenu(textIDProperty);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Добавляем поля для префикса и суффикса
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Префикс:", GUILayout.Width(55));
                    EditorGUILayout.PropertyField(prefixProperty, GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Суффикс:", GUILayout.Width(55));
                    EditorGUILayout.PropertyField(suffixProperty, GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void ShowKeySelectionMenu(SerializedProperty textIDProperty)
        {
            if (filteredKeys.Count == 0)
            {
                EditorUtility.DisplayDialog("Ошибка", "Нет доступных ключей локализации", "ОК");
                return;
            }
            
            GenericMenu menu = new GenericMenu();
            
            if (groupTextsByCategory)
            {
                // Группируем ключи по категориям
                var keysByCategory = filteredKeys.GroupBy(k => GetCategoryFromKey(k))
                    .OrderBy(g => g.Key);
                
                foreach (var group in keysByCategory)
                {
                    string category = string.IsNullOrEmpty(group.Key) ? "Без категории" : group.Key;
                    
                    foreach (var key in group.OrderBy(k => k))
                    {
                        menu.AddItem(new GUIContent($"{category}/{key}"), textIDProperty.stringValue == key, () => {
                            SelectKey(textIDProperty, key);
                        });
                    }
                }
            }
            else
            {
                // Отображаем ключи без группировки
                foreach (var key in filteredKeys.OrderBy(k => k))
                {
                    menu.AddItem(new GUIContent(key), textIDProperty.stringValue == key, () => {
                        SelectKey(textIDProperty, key);
                    });
                }
            }
            
            menu.ShowAsContext();
        }
        
        private void SelectKey(SerializedProperty textIDProperty, string key)
        {
            textIDProperty.stringValue = key;
            serializedObject.ApplyModifiedProperties();
        }
        
        private void AddNewDropdownItem()
        {
            dropdownItemsProperty.arraySize++;
            SerializedProperty newItem = dropdownItemsProperty.GetArrayElementAtIndex(dropdownItemsProperty.arraySize - 1);
            
            // Очищаем поля нового элемента
            SerializedProperty textIDProperty = newItem.FindPropertyRelative("textID");
            SerializedProperty prefixProperty = newItem.FindPropertyRelative("prefix");
            SerializedProperty suffixProperty = newItem.FindPropertyRelative("suffix");
            
            textIDProperty.stringValue = "";
            prefixProperty.stringValue = "";
            suffixProperty.stringValue = "";
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void FillFromDropdown()
        {
            TMP_Dropdown dropdown = dropdownProperty.objectReferenceValue as TMP_Dropdown;
            if (dropdown == null)
            {
                EditorUtility.DisplayDialog("Ошибка", "Сначала выберите компонент TMP_Dropdown", "ОК");
                return;
            }
            
            if (dropdown.options == null || dropdown.options.Count == 0)
            {
                EditorUtility.DisplayDialog("Ошибка", "В выбранном выпадающем списке нет опций", "ОК");
                return;
            }
            
            // Спрашиваем, хочет ли пользователь очистить существующие элементы
            bool clearExisting = EditorUtility.DisplayDialog(
                "Заполнение из dropdown", 
                "Хотите заменить существующие элементы или добавить к ним?", 
                "Заменить", "Добавить");
            
            if (clearExisting)
                dropdownItemsProperty.ClearArray();
            
            int startIndex = dropdownItemsProperty.arraySize;
            int itemsToAdd = dropdown.options.Count;
            
            // Добавляем элементы из dropdown
            for (int i = 0; i < itemsToAdd; i++)
            {
                dropdownItemsProperty.arraySize++;
                SerializedProperty newItem = dropdownItemsProperty.GetArrayElementAtIndex(startIndex + i);
                
                SerializedProperty textIDProperty = newItem.FindPropertyRelative("textID");
                SerializedProperty prefixProperty = newItem.FindPropertyRelative("prefix");
                SerializedProperty suffixProperty = newItem.FindPropertyRelative("suffix");
                
                textIDProperty.stringValue = "";
                prefixProperty.stringValue = "";
                suffixProperty.stringValue = "";
            }
            
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.DisplayDialog("Заполнение из dropdown", 
                $"Добавлено {itemsToAdd} элементов. Теперь выберите ключи локализации для каждого элемента.", 
                "ОК");
        }
        
        private void FindDropdownOnObject()
        {
            // Находим TMP_Dropdown компонент на объекте
            TMP_Dropdown dropdown = dropdownLocalizer.GetComponent<TMP_Dropdown>();
            
            if (dropdown == null)
            {
                // Если не найден на текущем объекте, ищем в дочерних
                dropdown = dropdownLocalizer.GetComponentInChildren<TMP_Dropdown>(true);
            }
            
            if (dropdown != null)
            {
                dropdownProperty.objectReferenceValue = dropdown;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditorUtility.DisplayDialog("Поиск dropdown", 
                    "Компонент TMP_Dropdown не найден на объекте или его дочерних объектах.", 
                    "ОК");
            }
        }
        
        private void RemoveEmptyItems()
        {
            bool removed = false;
            
            for (int i = dropdownItemsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty itemProperty = dropdownItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty textIDProperty = itemProperty.FindPropertyRelative("textID");
                
                if (string.IsNullOrEmpty(textIDProperty.stringValue))
                {
                    dropdownItemsProperty.DeleteArrayElementAtIndex(i);
                    removed = true;
                }
            }
            
            if (removed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.DisplayDialog("Очистка", "Пустые элементы удалены", "ОК");
            }
            else
            {
                EditorUtility.DisplayDialog("Очистка", "Пустых элементов не найдено", "ОК");
            }
        }
        
        private void MoveSelectedItemUp()
        {
            int selectedIndex = GetLastSelectedItemIndex();
            if (selectedIndex <= 0 || selectedIndex >= dropdownItemsProperty.arraySize)
                return;
                
            dropdownItemsProperty.MoveArrayElement(selectedIndex, selectedIndex - 1);
            serializedObject.ApplyModifiedProperties();
        }
        
        private void MoveSelectedItemDown()
        {
            int selectedIndex = GetLastSelectedItemIndex();
            if (selectedIndex < 0 || selectedIndex >= dropdownItemsProperty.arraySize - 1)
                return;
                
            dropdownItemsProperty.MoveArrayElement(selectedIndex, selectedIndex + 1);
            serializedObject.ApplyModifiedProperties();
        }
        
        private int GetLastSelectedItemIndex()
        {
            return EditorGUIUtility.keyboardControl;
        }
    }
}
#endif 