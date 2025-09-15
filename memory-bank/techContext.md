# Tech Context: Artist's Simulator

## Стек
Unity 6000.0.43f1 + URP + WebGL → Яндекс.Игры

## Зависимости
- **Zenject**: DI container
- **UniTask**: Async/await для Unity
- **Addressables**: Asset management + lazy loading
- **YandexGame SDK**: Платформенная интеграция
- **DOTween**: Анимации UI
- **TextMesh Pro**: UI текст
- **MagicAttributes (Fody)**: LogMethod, SafeExecution для error handling

## Структура проекта
```
Assets/Scripts/
├── Infrastructure/    # DI, State Machine, Configs
│   ├── Main/         # GameStateMachine, GameBootstrapper
│   ├── Configs/      # ScriptableObjects, ConfigsProxy
│   └── Installers/   # Zenject bindings
├── Services/         # Бизнес-логика
│   ├── Core/         # PaintingService, Gameplay
│   ├── Meta/         # CurrenciesService, Characters
│   └── Common/       # SaveService, AssetsProvider
├── Cosmetic/         # UI, анимации, локализация
└── Additional/       # MagicAttributes, Logs
```

## Ключевые сервисы
- `PaintingService`: Основная логика рисования + Magic Attributes
- `CurrenciesService`: Экономика (монеты/кристаллы) + Magic Attributes
- `SaveService`: Debounced сохранения + Magic Attributes
- `CharactersService`: Кастомизация персонажей + Magic Attributes
- `PaintGameplayGenerationService`: Генерация геймплея + Magic Attributes

## 🏗️ АРХИТЕКТУРА КАСТОМИЗАЦИИ (Завершена)

### ✅ Реализованная архитектура (3 рефакторинга):
- `SlotReferenceBase` - абстрактный базовый класс с полиморфизмом
- `IColorizedSlot`, `IInitializableSlot` - интерфейсы для типизации
- `HairSlotReference` - специальный слот с решением Hair-Hat конфликтов
- `SlotTypeAttribute` - атрибут для автоматической регистрации типов
- `SlotTypeRegistry` - reflection-based система регистрации

### 🎯 Ключевые решения:
- **Полиморфизм**: `List<SlotReferenceBase>` вместо отдельных списков
- **[SerializeReference]**: Unity сериализация полиморфных объектов
- **Атрибутная система**: `[SlotType(ItemType.Hair)]` для автоматической регистрации
- **KISS принцип**: Упрощение HairSlotReference, убран переинженеринг
- **Event-driven**: Реакция на события вместо сложной логики

### 📋 Архитектурные паттерны:
- **Strategy Pattern**: Разные типы слотов через полиморфизм
- **Registry Pattern**: SlotTypeRegistry для новых типов
- **Composition over Inheritance**: HairSlotReference расширяет ObjectSlotReference
- **Open/Closed Principle**: Новые слоты без изменения Editor кода

## Сохранения
JSON → YandexGame.savesData (облако + локально, debounce 0.2с/5с)

## Performance
- **Addressables**: Lazy loading ассетов
- **URP**: Оптимизация для WebGL
- **Качество графики**: Адаптивные настройки
- **Debounced saves**: Избежание частых записей
- **Magic Attributes**: Runtime error protection без performance impact

## WebGL особенности
- Сборка для браузера
- Оптимизация размера билда
- Кроссплатформенный input (мышь + тач)
- Magic Attributes совместимы с WebGL IL2CPP 