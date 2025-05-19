
# Server Panel

**Server Panel** — это веб-панель управления игровыми серверами, которая позволяет пользователю создавать, настраивать и запускать выделенные игровые серверы через простой и удобный интерфейс.

## 🚀 Возможности

- Поддержка различных игр: Don't Starve Together, Terraria, Minecraft (расширяется)
- Создание и конфигурация серверов через UI
- Поддержка модов и генерации конфигов
- Консоль сервера в реальном времени (через WebSocket)
- Подключение к PostgreSQL и Redis
- Docker-контейнеризация всех компонентов

## 📦 Архитектура

Проект состоит из следующих компонентов:

- **Backend** — ASP.NET Core Web API (порт 5000)
- **Frontend** — React SPA (порт 3000)
- **PostgreSQL** — база данных пользователей и серверов
- **Redis** — кеш
- **Docker Compose** — оркестрация всех сервисов

## 🛠️ Установка

### Требования

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- Git

### Клонирование репозитория

```bash
git clone https://github.com/lDubsterl/Server-Panel.git
cd Server-Panel
```

### Запуск

```bash
docker-compose up --build
```

Это запустит:

- `frontend`: http://localhost:3000
- `backend`: http://localhost:5000
- PostgreSQL на `localhost:5432`
- Redis на `localhost:6379`

> 📂 По умолчанию игровые серверы монтируются в `D:/Servers`. Измените в `docker-compose.yml`, если нужно другое расположение.