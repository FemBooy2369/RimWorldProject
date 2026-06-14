# 🎮 RimWorld Mod Manager

> Удобный менеджер модов для RimWorld на .NET MAUI

---

## ✨ Возможности

### 🔍 Поиск модов
- Поиск через **Steam Workshop API** с реальными результатами
- Карточки модов с фото, названием и описанием
- Три ссылки в каждой карточке — Steam, top-mods.ru, playground.ru (Google)
- История последних 20 поисков

### ⭐ Избранное
- Сохранение любимых модов локально
- Быстрый доступ к ссылкам из избранного
- Сердечко меняется под цвет темы (💚🧡❤️💟)

### 📁 Мои моды
- Подключение локальной папки Mods из RimWorld
- Автоматическое сканирование и чтение `About.xml`
- Отображение названия, автора и версии каждого мода

### 🌿 Vanilla Expanded
- Полный каталог модов от организации Vanilla-Expanded
- Загрузка напрямую с GitHub API
- Сортировка по звёздам

---

## 🎨 Темы оформления

| Тема | Акцент |
|------|--------|
| 🌑 Тёмная | фон #0A0A0A |
| ☀️ Светлая | фон #F5F5F5 |

### Цвета акцента
- 💚 Зелёный `#00FF7F`
- 🧡 Оранжевый `#FF8C00`
- ❤️ Красный `#FF3B3B`
- 💟 PeackMe `#FF69B4`

---

## 🏗️ Архитектура
RimworldModManager/

├── Pages/

│   ├── MainPage          # Поиск + карточки модов

│   ├── FavoritesPage     # Избранное

│   ├── MyModsPage        # Локальные моды

│   └── VanillaPage       # Vanilla Expanded каталог

├── ViewModels/

│   ├── MainViewModel     # Логика поиска и команды

│   ├── FavoritesViewModel

│   ├── MyModsViewModel

│   └── ThemeViewModel    # Реактивные цвета для биндинга

└── Services/

├── SteamApiService   # Steam Workshop API

├── GithubService     # GitHub API

├── LinkService       # Генерация ссылок

├── StorageService    # JSON хранилище

├── ModFolderService  # Сканирование папки модов

└── ThemeService      # Темы и акцент цвета

---

## 🛠️ Стек

- **.NET 10** / **MAUI**
- **Steam Web API** — поиск модов
- **GitHub API** — каталог Vanilla Expanded
- **MVVM** архитектура
- **JSON** локальное хранилище

---

## 🚀 Запуск

1. Клонировать репозиторий
2. Открыть `RimworldModManager.csproj` в Visual Studio 2026
3. Вставить Steam API ключ в `SteamApiService.cs`
4. Запустить на платформе Windows

---

## 📌 Планы

- [ ] Очистка мусорного кода
- [ ] Поиск по локальным модам
- [ ] Сортировка и фильтрация
- [ ] Экспорт списка модов
- [ ] Кэширование Steam API
- [ ] Заметки к модам в избранном
- [ ] Счётчик модов

---

*Сделано с ❤️ для сообщества RimWorld*
