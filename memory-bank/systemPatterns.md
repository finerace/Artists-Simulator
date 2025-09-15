# System Patterns: Artist's Simulator

## Архитектура
Infrastructure → Services → Cosmetic

## Паттерны
- **State Machine**: GameStateMachine + UIStateMachine
- **DI**: Zenject constructor injection
- **Events**: Action-based развязка
- **Factory**: Assets + States creation
- **Command**: Debounced saves
- **Local Methods**: Дробление сложной логики на вложенные методы для читаемости

## 🏗️ ПЛАНИРУЕМЫЕ ПАТТЕРНЫ (Customization System Refactoring)
- **Strategy Pattern**: ISlotHandler для обработки разных типов слотов
- **Factory Pattern**: SlotHandlerFactory для создания обработчиков
- **Registry Pattern**: SlotHandlerRegistry для регистрации новых типов
- **Dependency Injection**: Интеграция с Zenject для всех новых сервисов

## 🧙‍♂️ Magic Attributes (Fody)

### Доступные атрибуты:
```csharp
using Game.Additional.MagicAttributes;

[LogMethod(LogLevel.Debug)]                      // автологгирование входа/выхода
[LogMethod(LogLevel.Info, LogLevel.None)]       // разные уровни для входа/выхода
                                  // только обработка исключений
```

### ✅ КОГДА использовать `LogMethod`:
- **Рутинные операции**: Initialize, Setup, Configure, Load, Save
- **Технические методы**: InitializeServices, LoadAssets, SetupComponents
- **Когда НЕ важно ЧТО происходит внутри**, только факт вызова

### ❌ КОГДА НЕ использовать `LogMethod`:
- **Где важны ДЕТАЛИ**: сколько потрачено, добавлено, изменено

### ✅ КОГДА использовать `SafeExecution`:
- **ВСЕГДА** для защиты от runtime ошибок
- **В комбинации с ручным логгированием** для важных операций

### 🎯 ГИБРИДНЫЙ ПОДХОД (рекомендуемый):
```csharp
  // Защита от ошибок
public void SpendCoins(int amount)
{
    var oldBalance = coins;
    
    // бизнес-логика
    coins -= amount;
    
    Logs.Info($"Spent {amount} coins. Balance: {oldBalance} → {coins}");
}
```

## Логгирование

### Статический Logs класс
- **Уровни**: Debug, Info, Warning, Error
- **Контекстное логгирование**: Подробная информация о состоянии

### Примеры правильного логгирования:
```csharp
// ✅ ХОРОШО - детальная информация
Logs.Info($"Added {coins} coins. Balance: {oldValue} → {newValue}");
Logs.Warning($"Insufficient funds. Required: {required}, Available: {available}");

// ❌ ПЛОХО - бесполезная информация  
Logs.Info("Method started");
Logs.Info("Operation completed");
```

## Обработка ошибок
- **Graceful Degradation**: Не крашим приложение, логгируем и продолжаем
- **ArgumentException → Logs.Error**: Валидация параметров без исключений
- **Бизнес-логика → Try* методы**: TrySpend вместо throw для пользовательских операций
- **Runtime ошибки → SafeExecution**: Автоматическая обработка с логгированием
- **Fallback операции**: Локальное сохранение если облачное не работает
- **Retry механизмы**: Повторные попытки для нестабильных операций 