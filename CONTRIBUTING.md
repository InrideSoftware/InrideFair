# Руководство для контрибьюторов

Приветствуем вас в проекте **Inride Fair**! 🎉

Мы рады любому вкладу в развитие проекта. Это руководство поможет вам начать работу.

## 📋 Содержание

- [Как помочь](#как-помочь)
- [Начало работы](#начало-работы)
- [Структура проекта](#структура-проекта)
- [Правила кода](#правила-кода)
- [Pull Request](#pull-request)
- [Code Review](#code-review)

---

## 🤝 Как помочь

Есть несколько способов внести вклад:

### 1. Сообщить об ошибке
- Проверьте существующие [Issues](https://github.com/InrideSoftware/InrideFair/issues)
- Создайте новый issue с описанием проблемы
- Приложите логи из папки `Logs/`

### 2. Предложить улучшение
- Опишите предлагаемое изменение
- Объясните, почему это полезно
- Приведите примеры использования

### 3. Исправить баг
- Найдите open issue с багом
- Напишите комментарий, что берётесь за исправление
- Создайте Pull Request

### 4. Добавить новую сигнатуру чита
- Откройте `Config/CheatSignatures.cs`
- Добавьте сигнатуру в соответствующий массив
- Создайте Pull Request с обоснованием

### 5. Улучшить документацию
- Исправьте опечатки
- Дополните README
- Добавьте примеры

---

## 🚀 Начало работы

### Требования

- **.NET 11.0 SDK** или выше
- **Visual Studio 2022** или **VS Code**
- **Git**

### Установка

```bash
# Клонирование репозитория
git clone https://github.com/InrideSoftware/InrideFair.git
cd InrideFair

# Установка зависимостей
dotnet restore

# Сборка Debug
dotnet build InrideFair/InrideFair.csproj -c Debug

# Запуск
dotnet run --project InrideFair/InrideFair.csproj
```

### Ветка для разработки

```bash
# Создайте ветку от master
git checkout master
git pull origin master
git checkout -b feature/my-feature
```

---

## 📁 Структура проекта

```
InrideFair/
├── InrideFair.sln              # Решение Visual Studio
├── InrideFair/                 # Основной проект
│   ├── App.xaml                # Точка входа WPF
│   ├── Program.cs              # Резервная точка входа
│   ├── Config/                 # Константы и сигнатуры
│   │   ├── CheatSignatures.cs  # Сигнатуры читов
│   │   └── AnalysisConstants.cs # Константы анализа
│   ├── Models/                 # Модели данных
│   ├── Database/               # База данных сигнатур
│   ├── Utils/                  # Утилиты
│   ├── Checkers/               # Модули проверки
│   │   ├── ProcessChecker.cs   # Проверка процессов
│   │   ├── FileSystemChecker.cs # Проверка файлов
│   │   ├── ArchiveChecker.cs   # Проверка архивов
│   │   ├── BrowserChecker.cs   # Проверка браузеров
│   │   └── RegistryChecker.cs  # Проверка реестра
│   ├── Scanner/                # Сервис сканирования
│   ├── Services/               # Сервисы (Logging, Reports)
│   └── UI/                     # WPF интерфейс
├── config.json                 # Конфигурация
├── build_release.ps1           # Скрипт сборки
└── README.md                   # Документация
```

---

## 📝 Правила кода

### Стиль кода

Проект использует `.editorconfig` для автоматического форматирования. Основные правила:

- **Отступы**: 4 пробела
- **Стиль именования**: 
  - `PascalCase` для классов, методов, свойств
  - `camelCase` для локальных переменных, параметров
  - `_camelCase` для приватных полей
- **Nullable reference types**: включены

### Пример кода

```csharp
// ✅ Правильно
public class ProcessChecker
{
    private readonly CheatDatabase _cheatDb;
    
    public ProcessChecker(CheatDatabase cheatDb)
    {
        _cheatDb = cheatDb;
    }
    
    public int CheckProcesses()
    {
        var processes = ProcessUtils.GetRunningProcesses();
        // ...
    }
}

// ❌ Неправильно
public class processChecker  // должно быть PascalCase
{
    public CheatDatabase cheatDb;  // должно быть _camelCase
}
```

### Комментарии

- Пишите `/// XML-комментарии` для публичных API
- Избегайте очевидных комментариев
- Объясняйте **почему**, а не **что**

```csharp
// ✅ Хорошо
/// <summary>
/// Проверяет процессы на наличие известных сигнатур.
/// </summary>
public int CheckProcesses() { ... }

// ❌ Плохо
// Перебираем процессы и проверяем
foreach (var proc in processes) { ... }
```

### Логирование

Используйте `ILoggingService` для логирования:

```csharp
_logger.Debug("Подробная информация");
_logger.Info("Важное событие");
_logger.Warning("Предупреждение");
_logger.Error("Ошибка", exception);
_logger.Fatal("Критическая ошибка", exception);
```

---

## 🔀 Pull Request

### Перед созданием PR

1. **Проверьте код**
   ```bash
   dotnet build -c Release
   dotnet run --project InrideFair/InrideFair.csproj
   ```

2. **Убедитесь, что код отформатирован**
   ```bash
   dotnet format InrideFair.sln
   ```

3. **Обновите ветку**
   ```bash
   git fetch origin
   git rebase origin/master
   ```

### Создание PR

1. **Запушьте ветку**
   ```bash
   git push origin feature/my-feature
   ```

2. **Создайте Pull Request** на GitHub

3. **Опишите изменения**:
   - Что было изменено
   - Почему это нужно
   - Как тестировать

### Шаблон PR

```markdown
## Описание
Краткое описание изменений.

## Тип изменений
- [ ] Исправление бага
- [ ] Новая функция
- [ ] Улучшение документации
- [ ] Рефакторинг

## Проверка
- [ ] Код компилируется
- [ ] Протестировано вручную
- [ ] Нет предупреждений компилятора

## Скриншоты (если применимо)
```

---

## 🔍 Code Review

### Чек-лист ревьюера

- [ ] Код следует стилю проекта
- [ ] Нет дублирования кода
- [ ] Логирование добавлено где нужно
- [ ] Обработка ошибок присутствует
- [ ] Нет хардкода (константы вынесены)
- [ ] XML-комментарии для публичных API
- [ ] Изменения протестированы

### Время ревью

- **Малые PR** (< 100 строк): 1-2 дня
- **Средние PR** (100-500 строк): 3-5 дней
- **Большие PR** (> 500 строк): 5-7 дней

---

## 📞 Контакты

| | |
|---|---|
| **Email** | inridesoftware@gmail.com |
| **GitHub** | https://github.com/InrideSoftware |
| **Website** | in progress... |

---

**Спасибо за ваш вклад!** 🙏

Ваш код поможет сделать Inride Fair лучше для всех пользователей.
