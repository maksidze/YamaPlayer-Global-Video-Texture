# YamaPlayer Global Video Texture

Модуль для [YamaPlayer](https://github.com/koorimizuw/YamaPlayer), публикующий видеотекстуру и состояние проигрывателя в глобальные переменные шейдера, совместимые с ProTV.

## Возможности

- `_Udon_VideoTex` — текущая видеотекстура.
- `_Udon_VideoTex_ST` — преобразование UV.
- `_Udon_VideoTex_TexelSize` — размер текстуры и текселя.
- `_Udon_VideoData` — состояние, ошибка, громкость, позиция, скорость, mute, live и loading.
- События `_EnableGlobalTexture`, `_DisableGlobalTexture`, `_ToggleGlobalTexture`.
- Совместимые алиасы ProTV: `_EnableGSV`, `_DisableGSV`, `_ToggleGSV`.

## Требования

- Unity 2022.3.
- VRChat Worlds SDK 3.8.1 или новее.
- YamaPlayer 2.0.0 или новее.

## Использование

1. Выберите YamaPlayer в сцене.
2. Откройте `Module Manager`.
3. Нажмите `Refresh`.
4. Добавьте модуль `Глобальная видеотекстура` / `Global Video Texture`.

Настройка не требуется: публикация глобальной текстуры включена по умолчанию.

Одновременно используйте только один источник `_Udon_VideoTex`. Если в сцене есть ProTV, отключите у него `Global Video Texture`, иначе проигрыватели будут перезаписывать глобальные значения друг друга.

## Формат `_Udon_VideoData`

Матрица использует совместимую с ProTV раскладку:

| Поле | Значение |
| --- | --- |
| `m00` | Флаги mute, live и loading |
| `m01` | Состояние: waiting/stopped/playing/paused |
| `m02` | Состояние ошибки |
| `m03` | Готовность модуля |
| `m10` | Громкость |
| `m11` | Позиция воспроизведения от 0 до 1 |
| `m12` | Скорость воспроизведения |
| `m30` | Режим 3D, сейчас всегда 2D |
