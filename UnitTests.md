# RimWorld Mod Manager

**Кросс-платформенное приложение для управления модами RimWorld**  
**Курсовая работа** | .NET MAUI | C#

## Описание проекта

**RimWorld Mod Manager** — удобный инструмент для игроков RimWorld, который помогает эффективно работать с модами. Приложение позволяет искать моды в Steam Workshop и Nexus Mods, просматривать коллекцию Vanilla Expanded, сканировать локальную папку модов, вести избранное и получать персонализированные рекомендации через систему **Mod Radar**.

### Основные возможности
- Поиск модов в Steam + переход на Nexus Mods
- Просмотр Vanilla Expanded модов с GitHub
- Сканирование и анализ локальной папки `Mods`
- Система избранного и история поиска
- **Mod Radar** — умные рекомендации на основе ваших модов
- Поддержка тёмной и светлой темы + 4 акцентных цвета
- Красивый адаптивный интерфейс с анимациями

**Платформы:** Windows, macOS

---

## Unit Testing

В рамках курсовой работы было разработано **10 unit-тестов**, покрывающих ключевую бизнес-логику приложения.

### Результаты тестирования

| № | Класс / Метод | Название теста | Описание | Результат | Комментарий |
|---|---------------|----------------|----------|-----------|-------------|
| 1 | ModFolderService | ScanValidModFolder_ReturnsCorrectMods | Сканирование папки с корректными About.xml | **ПРОШЁЛ** | Возвращаются правильные ModInfo |
| 2 | ModFolderService | AutoAssignTags_CorrectlyDetectsCategories | Автоопределение тегов по ключевым словам | **ПРОШЁЛ** | Корректная работа Regex |
| 3 | StorageService | SaveAndLoadFavorites_PersistsData | Сохранение и загрузка избранного | **ПРОШЁЛ** | JSON сериализация работает |
| 4 | StorageService | AddToHistory_LimitsTo20AndMovesToTop | Добавление в историю с ограничением | **ПРОШЁЛ** | Лимит 20 и перемещение наверх |
| 5 | MainViewModel | CalculateMatchPercent_LogicVerification | Расчёт процента совпадения | **ПРОШЁЛ** | Логика рекомендаций верна |
| 6 | MainViewModel | LoadModRadar_GeneratesRecommendations | Генерация рекомендаций в Radar | **ПРОШЁЛ** | Рекомендации формируются корректно |
| 7 | ThemeService + ThemeViewModel | ThemeAndAccent_ChangeNotifiesViewModel | Смена темы и уведомление UI | **ПРОШЁЛ** | Binding и события работают |
| 8 | SteamApiService | SearchMods_HandlesApiResponse | Обработка ответа Steam API | **ПРОШЁЛ** | Парсинг данных успешен |
| 9 | GithubService | GetVanillaMods_ParsesMultiplePages | Загрузка модов Vanilla Expanded | **ПРОШЁЛ** | Постраничная загрузка работает |
| 10 | Converters | GreaterThanZeroConverter_And_InverseBool | Тестирование value converters | **ПРОШЁЛ** | Конвертеры для XAML корректны |

**Общий результат:**  
**✅ Все 10 тестов успешно прошли**

**Покрытие:** Основная бизнес-логика (Services + ViewModels) покрыта качественно.

---

## Технический стек
- **.NET MAUI** (net10.0)
- MVVM архитектура
- CommunityToolkit (Command, Observable)
- HttpClient + JSON
- Preferences + Local File Storage
- XAML + Data Binding

---

## Вывод по курсовой работе

Приложение **RimWorld Mod Manager** демонстрирует современный подход к разработке кросс-платформенных приложений. Реализована чистая архитектура MVVM, качественная обработка данных, работа с внешними API (Steam, GitHub), локальное хранилище и удобный UI. 

Все ключевые модули протестированы. Приложение готово к использованию и дальнейшему развитию (добавление установки модов, экспорт сборок и т.д.).

**Дата:** 23.06.2026
