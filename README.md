# Syncing-Battleship

Syncing-Battleship — микросервис на C# для синхронизации игровых сессий в многопользовательских играх. Сервис реализует обмен состояниями между клиентами через Riptide Networking и может работать с любыми игровыми моделями, подключаемыми в виде плагинов (DLL). 

## Возможности

- Изоляция игровых сессий, поддержка одновременных игр
- Гибкое расширение за счет подключения новых моделей данных (игр) без изменения основного кода
- Эффективная сетевая коммуникация на основе Riptide Networking
- Контроль и управление сессиями через отдельный gRPC-интерфейс

## Роль в архитектуре

Сервис работает в составе архитектуры Magicthirst как центральный компонент для синхронизации состояний между игровыми клиентами. Взаимодействует с сервисами gateway и hosts через gRPC и HTTP, а игровые клиенты подключаются напрямую по WebSocket.

## Запуск

1. Убедитесь, что установлен .NET 9.0 SDK.
2. Соберите проекты:
   ```sh
   dotnet restore
   dotnet build "Magicthrist - Green/Magicthrist - Green.csproj" -c Debug -f net9.0
   dotnet build "Syncing_Battleship/Syncing_Battleship.csproj" -c Debug -f net9.0
   ```
3. Запустите сервис вручную (пример):
   ```sh
   dotnet "Syncing_Battleship/bin/Debug/net9.0/Syncing_Battleship.dll" \
     "Magicthrist - Green/bin/Debug/net9.0/Magicthrist___Green.dll" \
     "Magicthrist___Green.MagicthirstDataBehaviour"
   ```
   Либо используйте заранее настроенный запуск через Rider: `/Run behaviours/Run with Magicthirst Behaviour.run.xml`

## Пример использования

- При запуске сервис инициализирует игровые модели из переданного плагина.
- Новый игровой сеанс создается через gRPC-команду от сервиса gateway.
- Игровые клиенты подключаются через Riptide Networking, получают и отправляют обновления состояния в рамках своей сессии.
- Все игровые действия и обмены проходят через строго типизированные сообщения, определяемые моделью данных.

## Зависимости

- [.NET 9.0](https://dotnet.microsoft.com/) (или выше)
- [Riptide Networking](https://github.com/CMiranda/Riptide)
- Humanizer (для форматирования данных)
- Microsoft.AspNetCore.App (для HTTP/gRPC-интерфейса)

## TODO

- [ ] Документация API
