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
    [CustomEditor(typeof(TextLocalizer))]
    public class TextLocalizerEditor : Editor
    {
        private TextLocalizer textLocalizer;
        private SerializedProperty textItemsProperty;
        
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
        private bool autoAddTextsOnSelection = true;
        private bool groupTextsByCategory = true;
        
        private void OnEnable()
        {
            textLocalizer = (TextLocalizer)target;
            textItemsProperty = serializedObject.FindProperty("textItems");
            
            // Загружаем настройки редактора
            keySearchFilter = EditorPrefs.GetString("TextLocalizerEditor_KeySearchFilter", "");
            showSearchField = EditorPrefs.GetBool("TextLocalizerEditor_ShowSearchField", true);
            autoAddTextsOnSelection = EditorPrefs.GetBool("TextLocalizerEditor_AutoAddTexts", true);
            groupTextsByCategory = EditorPrefs.GetBool("TextLocalizerEditor_GroupByCategory", true);
            selectedCategoryIndex = EditorPrefs.GetInt("TextLocalizerEditor_SelectedCategory", -1);
            
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
            
            // Отображаем панель поиска ключей
            if (showSearchField)
            {
                DrawSearchPanel();
            }
            
            EditorGUILayout.Space(10);
            
            // Кнопки для управления TextItems
            DrawTextItemsManagerButtons();
            
            EditorGUILayout.Space(5);
            
            // Отображаем элементы локализации
            DrawTextItemsList();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawSettingsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Настройки редактора", EditorStyles.boldLabel);
            
            bool newAutoAddTexts = EditorGUILayout.Toggle("Автодобавление текстов", autoAddTextsOnSelection);
            if (newAutoAddTexts != autoAddTextsOnSelection)
            {
                autoAddTextsOnSelection = newAutoAddTexts;
                EditorPrefs.SetBool("TextLocalizerEditor_AutoAddTexts", autoAddTextsOnSelection);
            }
            
            bool newGroupByCategory = EditorGUILayout.Toggle("Группировать по категориям", groupTextsByCategory);
            if (newGroupByCategory != groupTextsByCategory)
            {
                groupTextsByCategory = newGroupByCategory;
                EditorPrefs.SetBool("TextLocalizerEditor_GroupByCategory", groupTextsByCategory);
            }
            
            bool newShowSearchField = EditorGUILayout.Toggle("Показывать поиск", showSearchField);
            if (newShowSearchField != showSearchField)
            {
                showSearchField = newShowSearchField;
                EditorPrefs.SetBool("TextLocalizerEditor_ShowSearchField", showSearchField);
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
                EditorPrefs.SetString("TextLocalizerEditor_KeySearchFilter", keySearchFilter);
                UpdateFilteredKeys();
            }
            
            if (GUILayout.Button("Очистить", GUILayout.Width(70)))
            {
                keySearchFilter = "";
                EditorPrefs.SetString("TextLocalizerEditor_KeySearchFilter", "");
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
                EditorPrefs.SetInt("TextLocalizerEditor_SelectedCategory", selectedCategoryIndex);
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
        
        private void DrawTextItemsManagerButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Добавить элемент", GUILayout.Height(25)))
            {
                AddNewTextItem();
            }
            
            if (GUILayout.Button("Автопоиск текстов на объекте", GUILayout.Height(25)))
            {
                AutoFindTextsOnObject();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Удалить пустые элементы"))
            {
                RemoveEmptyItems();
            }
            
            if (GUILayout.Button("Сортировать по имени"))
            {
                SortItemsByName();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTextItemsList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Элементы локализации", EditorStyles.boldLabel);
            
            if (textItemsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Нет элементов для локализации. Нажмите 'Добавить элемент' или 'Автопоиск текстов'.", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < textItemsProperty.arraySize; i++)
                {
                    SerializedProperty itemProperty = textItemsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty textProperty = itemProperty.FindPropertyRelative("text");
                    SerializedProperty textIDProperty = itemProperty.FindPropertyRelative("textID");
                    SerializedProperty prefixProperty = itemProperty.FindPropertyRelative("prefix");
                    SerializedProperty suffixProperty = itemProperty.FindPropertyRelative("suffix");
                    
                    // Получаем объект текста и его текущее содержимое
                    TextMeshProUGUI textComponent = textProperty.objectReferenceValue as TextMeshProUGUI;
                    string currentTextContent = textComponent != null ? textComponent.text : "";
                    
                    // Ограничиваем длину для аккуратного отображения
                    if (currentTextContent.Length > 50)
                        currentTextContent = currentTextContent.Substring(0, 50) + "...";
                    
                    // Создаем стиль с цветовым выделением
                    GUIStyle textContentStyle = new GUIStyle(EditorStyles.helpBox);
                    textContentStyle.normal.textColor = new Color(1,1,1); // Зеленый цвет для текста
                    textContentStyle.fontStyle = FontStyle.Bold;
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Добавляем строку с текущим содержимым
                    if (!string.IsNullOrEmpty(currentTextContent))
                    {
                        EditorGUILayout.LabelField("📝 " + currentTextContent, textContentStyle);
                    }
                    
                    // Верхняя строка с TextMeshPro и кнопками
                    EditorGUILayout.BeginHorizontal();
                    
                    // Поле для TextMeshProUGUI
                    EditorGUILayout.PropertyField(textProperty, GUIContent.none);
                    
                    // Кнопка для поиска компонента на дочерних объектах
                    if (GUILayout.Button("Найти", GUILayout.Width(60)))
                    {
                        FindTextOnChildren(textProperty);
                    }
                    
                    // Кнопка для удаления элемента
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        textItemsProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        i--;
                        continue;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
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
        
        private void AddNewTextItem()
        {
            textItemsProperty.arraySize++;
            SerializedProperty newItem = textItemsProperty.GetArrayElementAtIndex(textItemsProperty.arraySize - 1);
            
            // Очищаем поля нового элемента
            SerializedProperty textProperty = newItem.FindPropertyRelative("text");
            SerializedProperty textIDProperty = newItem.FindPropertyRelative("textID");
            SerializedProperty prefixProperty = newItem.FindPropertyRelative("prefix");
            SerializedProperty suffixProperty = newItem.FindPropertyRelative("suffix");
            
            textProperty.objectReferenceValue = null;
            textIDProperty.stringValue = "";
            prefixProperty.stringValue = "";
            suffixProperty.stringValue = "";
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void AutoFindTextsOnObject()
        {
            // Находим все TextMeshProUGUI компоненты на объекте и дочерних объектах
            TextMeshProUGUI[] texts = textLocalizer.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            if (texts.Length == 0)
            {
                EditorUtility.DisplayDialog("Автопоиск", "На объекте и дочерних объектах не найдено компонентов TextMeshProUGUI", "ОК");
                return;
            }
            
            // Создаем список уже добавленных текстов
            List<TextMeshProUGUI> existingTexts = new List<TextMeshProUGUI>();
            for (int i = 0; i < textItemsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = textItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty textProperty = itemProperty.FindPropertyRelative("text");
                
                if (textProperty.objectReferenceValue != null)
                {
                    existingTexts.Add(textProperty.objectReferenceValue as TextMeshProUGUI);
                }
            }
            
            // Добавляем только новые тексты
            int addedCount = 0;
            foreach (TextMeshProUGUI text in texts)
            {
                if (!existingTexts.Contains(text))
                {
                    textItemsProperty.arraySize++;
                    SerializedProperty newItem = textItemsProperty.GetArrayElementAtIndex(textItemsProperty.arraySize - 1);
                    
                    SerializedProperty textProperty = newItem.FindPropertyRelative("text");
                    SerializedProperty textIDProperty = newItem.FindPropertyRelative("textID");
                    SerializedProperty prefixProperty = newItem.FindPropertyRelative("prefix");
                    SerializedProperty suffixProperty = newItem.FindPropertyRelative("suffix");
                    
                    textProperty.objectReferenceValue = text;
                    textIDProperty.stringValue = "";
                    prefixProperty.stringValue = "";
                    suffixProperty.stringValue = "";
                    
                    addedCount++;
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.DisplayDialog("Автопоиск", $"Добавлено {addedCount} новых текстовых компонентов", "ОК");
        }
        
        private void FindTextOnChildren(SerializedProperty textProperty)
        {
            TextMeshProUGUI[] texts = textLocalizer.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            if (texts.Length == 0)
            {
                EditorUtility.DisplayDialog("Поиск текста", "На объекте и дочерних объектах не найдено компонентов TextMeshProUGUI", "ОК");
                return;
            }
            
            // Создаем умное меню с иерархией на основе путей объектов
            GenericMenu menu = new GenericMenu();
            
            // Группируем тексты по родительским объектам
            Dictionary<Transform, List<TextMeshProUGUI>> textsByParent = new Dictionary<Transform, List<TextMeshProUGUI>>();
            
            // Спец-меню для быстрого доступа к часто используемым группам
            menu.AddItem(new GUIContent("Быстрый доступ/Все текстовые объекты"), false, () => {
                ShowAllTextsMenu(textProperty, texts);
            });
            
            // Добавляем опцию поиска по самому тексту
            menu.AddItem(new GUIContent("Быстрый доступ/Поиск по содержимому текста..."), false, () => {
                ShowTextSearchPopup(textProperty, texts);
            });
            
            menu.AddSeparator("");
            
            // Строим дерево объектов
            BuildObjectHierarchyMenu(menu, textLocalizer.transform, texts, textProperty);
            
            menu.ShowAsContext();
        }
        
        // Новый метод для отображения всех текстов в одном меню
        private void ShowAllTextsMenu(SerializedProperty textProperty, TextMeshProUGUI[] texts)
        {
            GenericMenu menu = new GenericMenu();
            
            // Сортируем тексты по содержимому для удобного поиска
            var sortedTexts = texts.OrderBy(t => t.text).ToArray();
            
            foreach (TextMeshProUGUI text in sortedTexts)
            {
                string displayText = string.IsNullOrEmpty(text.text) ? "<Пустой текст>" : text.text;
                if (displayText.Length > 30)
                {
                    displayText = displayText.Substring(0, 30) + "...";
                }
                
                string path = GetRelativePath(textLocalizer.transform, text.transform);
                
                menu.AddItem(
                    new GUIContent($"{displayText} ({path})"), 
                    textProperty.objectReferenceValue == text, 
                    () => {
                        textProperty.objectReferenceValue = text;
                        serializedObject.ApplyModifiedProperties();
                    }
                );
            }
            
            menu.ShowAsContext();
        }
        
        // Новый метод для поиска по содержимому текста
        private void ShowTextSearchPopup(SerializedProperty textProperty, TextMeshProUGUI[] texts)
        {
            // Это отдельное окно поиска
            TextSearchWindow.Show(texts, textLocalizer.transform, (selectedText) => {
                textProperty.objectReferenceValue = selectedText;
                serializedObject.ApplyModifiedProperties();
            });
        }
        
        // Рекурсивный метод построения иерархического меню объектов
        private void BuildObjectHierarchyMenu(GenericMenu menu, Transform current, TextMeshProUGUI[] allTexts, SerializedProperty textProperty)
        {
            // Проверяем, есть ли тексты на текущем объекте или его потомках
            bool hasTextsInChildren = HasTextComponentsInChildren(current, allTexts);
            if (!hasTextsInChildren)
                return;
            
            // Добавляем элементы меню для каждого дочернего объекта с текстами
            foreach (Transform child in current)
            {
                bool hasDirectTexts = HasDirectTextComponents(child, allTexts);
                bool hasChildTexts = HasTextComponentsInChildren(child, allTexts);
                
                if (hasDirectTexts)
                {
                    // Если на этом объекте есть текст - добавляем опцию "Выбрать все тексты объекта"
                    var textsOnObject = allTexts.Where(t => t.transform.parent == child).ToArray();
                    if (textsOnObject.Length > 1)
                    {
                        string path = GetRelativePath(textLocalizer.transform, child);
                        menu.AddItem(
                            new GUIContent($"{path}/► Выбрать любой из {textsOnObject.Length} текстов"),
                            false,
                            () => {
                                ShowTextsOnObjectMenu(textProperty, textsOnObject);
                            }
                        );
                    }
                    
                    // Добавляем каждый текст на этом объекте
                    foreach (var text in textsOnObject)
                    {
                        string displayText = string.IsNullOrEmpty(text.text) ? "<Пустой текст>" : text.text;
                        if (displayText.Length > 30)
                        {
                            displayText = displayText.Substring(0, 30) + "...";
                        }
                        
                        string path = GetRelativePath(textLocalizer.transform, child);
                        menu.AddItem(
                            new GUIContent($"{path}/{text.name} [{displayText}]"),
                            textProperty.objectReferenceValue == text,
                            () => {
                                textProperty.objectReferenceValue = text;
                                serializedObject.ApplyModifiedProperties();
                            }
                        );
                    }
                }
                
                // Рекурсивно обрабатываем дочерние объекты, только если в них есть тексты
                if (hasChildTexts)
                {
                    BuildObjectHierarchyMenu(menu, child, allTexts, textProperty);
                }
            }
        }
        
        // Проверяет, есть ли текстовые компоненты на объекте или в его потомках
        private bool HasTextComponentsInChildren(Transform obj, TextMeshProUGUI[] allTexts)
        {
            return allTexts.Any(t => t.transform == obj || IsChildOf(t.transform, obj));
        }
        
        // Проверяет, есть ли текстовые компоненты на самом объекте (непосредственно)
        private bool HasDirectTextComponents(Transform obj, TextMeshProUGUI[] allTexts)
        {
            return allTexts.Any(t => t.transform.parent == obj);
        }
        
        // Проверяет, является ли один трансформ потомком другого
        private bool IsChildOf(Transform child, Transform parent)
        {
            Transform current = child;
            while (current != null)
            {
                if (current == parent)
                    return true;
                current = current.parent;
            }
            return false;
        }
        
        // Показывает меню со всеми текстами на конкретном объекте
        private void ShowTextsOnObjectMenu(SerializedProperty textProperty, TextMeshProUGUI[] texts)
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (var text in texts)
            {
                string displayText = string.IsNullOrEmpty(text.text) ? "<Пустой текст>" : text.text;
                if (displayText.Length > 30)
                {
                    displayText = displayText.Substring(0, 30) + "...";
                }
                
                menu.AddItem(
                    new GUIContent($"{text.name} [{displayText}]"),
                    textProperty.objectReferenceValue == text,
                    () => {
                        textProperty.objectReferenceValue = text;
                        serializedObject.ApplyModifiedProperties();
                    }
                );
            }
            
            menu.ShowAsContext();
        }
        
        // Добавляем класс окна поиска
        public class TextSearchWindow : EditorWindow
        {
            private TextMeshProUGUI[] allTexts;
            private string searchText = "";
            private Vector2 scrollPosition;
            private System.Action<TextMeshProUGUI> onTextSelected;
            private Transform rootTransform;
            
            public static void Show(TextMeshProUGUI[] texts, Transform root, System.Action<TextMeshProUGUI> callback)
            {
                TextSearchWindow window = GetWindow<TextSearchWindow>(true, "Поиск текстового компонента");
                window.allTexts = texts;
                window.onTextSelected = callback;
                window.rootTransform = root;
                window.minSize = new Vector2(400, 300);
                window.Show();
            }
            
            private void OnGUI()
            {
                GUILayout.Label("Поиск по содержимому текста", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Поиск:", GUILayout.Width(50));
                string newSearch = EditorGUILayout.TextField(searchText);
                if (newSearch != searchText)
                {
                    searchText = newSearch;
                    GUI.FocusControl(null);
                }
                
                if (GUILayout.Button("Очистить", GUILayout.Width(70)))
                {
                    searchText = "";
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Фильтруем тексты по запросу
                var filteredTexts = allTexts;
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredTexts = allTexts.Where(t => 
                        t.text != null && 
                        t.text.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                    ).ToArray();
                }
                
                GUILayout.Label($"Найдено: {filteredTexts.Length} из {allTexts.Length}");
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var text in filteredTexts)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    string displayText = string.IsNullOrEmpty(text.text) ? "<Пустой текст>" : text.text;
                    string path = GetRelativePath(rootTransform, text.transform);
                    
                    if (GUILayout.Button(displayText, EditorStyles.label, GUILayout.ExpandWidth(true)))
                    {
                        onTextSelected(text);
                        Close();
                    }
                    
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel, GUILayout.Width(150));
                    
                    if (GUILayout.Button("Выбрать", GUILayout.Width(60)))
                    {
                        onTextSelected(text);
                        Close();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private static string GetRelativePath(Transform root, Transform target)
        {
            if (target == root)
                return root.name;
                
            string path = target.name;
            Transform parent = target.parent;
            
            while (parent != null && parent != root)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        private void RemoveEmptyItems()
        {
            bool removed = false;
            
            for (int i = textItemsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty itemProperty = textItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty textProperty = itemProperty.FindPropertyRelative("text");
                SerializedProperty textIDProperty = itemProperty.FindPropertyRelative("textID");
                SerializedProperty prefixProperty = itemProperty.FindPropertyRelative("prefix");
                SerializedProperty suffixProperty = itemProperty.FindPropertyRelative("suffix");
                
                if (textProperty.objectReferenceValue == null || string.IsNullOrEmpty(textIDProperty.stringValue))
                {
                    textItemsProperty.DeleteArrayElementAtIndex(i);
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
        
        private void SortItemsByName()
        {
            // Это сложная задача для SerializedProperty, поэтому собираем данные
            // в промежуточную структуру, сортируем и затем восстанавливаем
            List<TextInfo> items = new List<TextInfo>();
            
            for (int i = 0; i < textItemsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = textItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty textProperty = itemProperty.FindPropertyRelative("text");
                SerializedProperty textIDProperty = itemProperty.FindPropertyRelative("textID");
                SerializedProperty prefixProperty = itemProperty.FindPropertyRelative("prefix");
                SerializedProperty suffixProperty = itemProperty.FindPropertyRelative("suffix");
                
                TextMeshProUGUI text = textProperty.objectReferenceValue as TextMeshProUGUI;
                string textID = textIDProperty.stringValue;
                
                items.Add(new TextInfo { Text = text, TextID = textID, Prefix = prefixProperty.stringValue, Suffix = suffixProperty.stringValue });
            }
            
            // Сортируем по имени TextMeshProUGUI, если оно есть
            items = items.OrderBy(item => item.Text != null ? item.Text.name : "").ToList();
            
            // Пересоздаем все элементы в новом порядке
            textItemsProperty.ClearArray();
            
            foreach (var item in items)
            {
                textItemsProperty.arraySize++;
                SerializedProperty newItem = textItemsProperty.GetArrayElementAtIndex(textItemsProperty.arraySize - 1);
                
                SerializedProperty textProperty = newItem.FindPropertyRelative("text");
                SerializedProperty textIDProperty = newItem.FindPropertyRelative("textID");
                SerializedProperty prefixProperty = newItem.FindPropertyRelative("prefix");
                SerializedProperty suffixProperty = newItem.FindPropertyRelative("suffix");
                
                textProperty.objectReferenceValue = item.Text;
                textIDProperty.stringValue = item.TextID;
                prefixProperty.stringValue = item.Prefix;
                suffixProperty.stringValue = item.Suffix;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private class TextInfo
        {
            public TextMeshProUGUI Text;
            public string TextID;
            public string Prefix;
            public string Suffix;
        }
    }
}
#endif 