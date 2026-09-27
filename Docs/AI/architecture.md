# Architecture

Parent: [`PROJECT.md`](PROJECT.md). The big-picture structure that takes several files to see.
The rules for *new* code (where things go, how they talk) are in
[`module-rules.md`](module-rules.md); this file describes what already exists.

## Managers (singletons)

Almost every system is a `SingletonMono<T>` (`Assets/Scripts/Base/Framework/SingletonMono.cs`).
Reading `Instance` **auto-creates** the GameObject if it doesn't exist. Singletons are persistent
(`DontDestroyOnLoad`) by default. Override `IsPersistent => false` for one that belongs to a
scene (today: `UnitManager`, `UnitRenderManager`). After a persistent singleton is destroyed,
`Instance` returns null (the `isDestroyed` flag), so null-check it in shutdown and teardown code.

The core managers are in `Assets/Scripts/Manager/`:
- `GameManager`: battle flow, win/loss, rewards.
- `PlayerDataManager`: currency, decks, buildings, artifacts. The territory tile and synergy logic
  lives in its `TileDataHandler`.
- `DataManager`: static game data.
- `BackendManager`: UGS.
- `UnitManager`: the list of live `BaseCharacter`s used for targeting (scene-owned).
- `UIManager`, `ObjectPoolManager`, `EventManager`, `InputManager`, `AudioManager`, `AdManager`,
  `SettingDataManager`.

`SceneLoader` and `FadeManager` (`Assets/GlobalScripts/`) are singletons too.

## Scene transitions

Use `SceneLoader.Instance.StartLoadScene(SceneState.X)` (`Assets/GlobalScripts/SceneLoader.cs`).
It fades out, loads `EmptyScene`, calls `OnSceneReset()` on every registered `ISceneResettable`
(such as `UIManager` and `ObjectPoolManager`), unloads unused assets, runs GC, and then loads the
target scene. `SceneLoader.IsChange` is true during a transition so input can be ignored.
`Manager/SceneLoadManager.cs` and the `SceneBase` classes are an older path that nothing uses.
To add a scene, add it to the `SceneState` enum, `sceneNames`, and Build Settings. The
per-scene bootstrap MonoBehaviours are in `Scripts/Scenes/SceneLoader/`. `SceneLoaderStart`
runs the internet check and `BackendManager.EnsureInstanceAndInitializedAsync()`.

## Static game data (Excel → ScriptableObject)

- The design tables are `.xlsx` files in `Assets/Excel/`. When one is imported, the editor importer
  (`Assets/Externals/ExcelImporter`) regenerates the matching SO in `Assets/Resources/DB/`, for
  every SO class that carries `[ExcelAsset(AssetPath = "Resources/DB")]`. Its public `List<>` field
  names must match the sheet names, and the fields of the row class must match the column headers.
  **Exception:** in `BuildingUpgradeSO` the attribute is commented out ("수동으로 임포터 해야해서").
  Importing `BuildingUpgradeSO.xlsx` does not update that SO, so it has to be imported by hand.
- A reimport (including the first import of a fresh clone) rewrites the SO from the row class, so
  a field added to a row class shows up as a diff in the `.asset` even when no sheet changed.
- Each SO derives from `MonoSO<TData>` (`DB/SO/`) and implements `SetData(Dictionary<int, TData>)`.
  One SO can merge several sheets into a single dictionary keyed by `idNumber`. `GetList()` is
  deprecated.
- `DataBase<TData, TSO>` loads `Resources.Load("DB/<SOName>")`, and `DataManager` exposes each
  table as a lazily created `DataBase` property. To add a table, create the xlsx, the data class in
  `DB/Data/`, the SO in `DB/SO/`, and a `DataManager` property.
- Shared string keys and constants are in `DB/Constants.cs`.

## Backend (Unity Gaming Services)

- Player progress lives on the server: Authentication (anonymous), Cloud Save (player data, gacha
  pity), Economy (currencies), Cloud Code (currency changes and gacha, as anti-cheat), Remote
  Config, and Analytics. Device-local preferences stay in `PlayerPrefs`: audio volumes, the control
  layout (`SettingDataManager`), and "don't ask again" flags.
- Network calls go through `BackendManager`'s static async (UniTask) API. The data calls (Cloud
  Save, Economy, gacha, mail) check `CanCommunicateAsync` and then go through `EnqueueRequestAsync`,
  a sequential queue with retry. The initialization and connectivity calls don't queue, and
  neither does `WatchAdAndGetReward`, which checks connectivity and then shows the ad.
  An Economy write-lock conflict makes it reload balances and retry. Don't call UGS SDKs directly
  from other code. Two existing calls bypass the queue, and they get no ordering or retry:
  `UIMenu` fires the Cloud Code `WakeUpServer()` without awaiting it, and `GameManager` records
  the stage-result Analytics event directly.
- The Cloud Code C# modules are at the repo root in `Economy/` and `GachaV2/`. `Gacha/`,
  `Gatcha/`, and `HelloWorld/` are old or sample code. The modules are referenced by the `.ccmr`
  files in `Assets/Scripts/Cloud/`. Their `.sln`/`.csproj` files are gitignored, so a fresh clone
  can't build them. The client calls them through the generated bindings in
  `Assets/CloudCode/GeneratedModuleBindings` (`EconomyModuleBindings`, `GachaModuleV2Bindings`).
  Regenerate the bindings after changing a module's signatures.

## Resources-path conventions

- **UI**: `UIManager.Instance.OpenUI<T>()` / `GetUI<T>()` loads `Resources/Prefabs/UI/<TypeName>`.
  The prefab name must equal the class name, and the class must derive from `BaseUI`. Popups derive
  from `BasePopUpUI`. Back-button handling uses a UI stack of `IBackButtonHandler`s. Popups push and
  pop it through `UIManager`'s static `PubishAddUIStackEvent`/`PublishRemoveUIStackEvent`. Despite
  the names, these call `PushUI`/`PopUI` directly, because their event publication is commented
  out. `UIManager` still subscribes to `AddUIStackEvent`/`RemoveUIStackEvent`, but nothing
  publishes them.
- **Pooling**: add a `PoolType` enum value, following the ordered regions and comments in the enum.
  Put a prefab with the same name in `Resources/Prefabs/ObjPooling/`, with a `BasePoolable` or a
  subclass on it. Get objects with `ObjectPoolManager.Instance.Get(PoolType)`, and always return
  them with `ReleaseSelf()`.

## Events (`Assets/Scripts/Manager/EventManager.cs`)

A typed pub/sub keyed by **struct** event types. Its instance is hidden. The whole public API is
two static methods:

- `EventManager.GetPublisher<T>()` → `IEventPublisher<T>` with `Publish(in T)` and `Publish()`
  (a default `T`).
- `EventManager.GetSubscriber<T>()` → `IEventSubscriber<T>` with `Subscribe(Action<T>)` and
  `Unsubscribe(Action<T>)`.

`Subscribe` removes the same delegate before adding it, so subscribing twice with a method
reference is harmless. On destroy the manager clears every channel. The idiom in the codebase is to
fetch the publisher or subscriber once into a field (in `Awake`/`OnEnable`), subscribe in
`OnEnable`, and `?.Unsubscribe` in `OnDisable` (see `GachaUIPanel`, `SynergyOuitlineManager`).
Event structs are usually declared in the publisher's file (for example, `PlayerDataManager.cs`
declares the facts it publishes). Some sit next to a subscriber instead: `BattleEndedEvent` is
published by `GameManager` but lives in `UI/BattleScene/`. The usage rules are in
[`module-rules.md`](module-rules.md), under *Rules → C — EventManager channels*.

## Combat

- Class hierarchy (`Base/Character/`): `BaseCharacter` → `BaseUnit`/`BaseHQ`, with
  `BaseController` → `BaseUnitController`. The role-specific controllers for melee splash, ranged
  splash, healer, and healer splash each have a Player version in `Player/PlayerUnit/` and an
  Enemy version in `Enemy/`. `GolemAIController` exists only on the Player side. So does
  `SummonedUnitController`, which despite its name derives from `PlayerUnit`, not from a
  controller.
- Unit AI: target search runs in a `TargetingRoutine` coroutine, and attacks run per frame in
  `AttackUpdate()`, which `Update()` calls. The old `AttackRoutine` coroutines are commented out
  in the unit controllers. Only `PlayerController` still starts both a `TargetingRoutine` and an
  `AttackRoutine` coroutine. Targeting scans the lists in
  `UnitManager` by x-distance (no physics colliders), and removal uses swap-back.
- Enemy spawning:
  - `EnemyHQ.SpawnUnitRoutine` is the regular spawn loop. It takes the first enemy type whose
    cooldown has expired, then waits `spawnInterval`. `EnemyCoolTimeRoutin` only toggles that type's
    `enemyUnitCanSpawn` flag, and every cooldown already starts in `Awake`.
  - `EnemyWaveSystem` spawns timed waves itself. It pauses the HQ loop for each wave with
    `SetSpawnEnemyActive(false)` and restarts it afterwards.
  - A one-time defense wave (`SpawnDefenseWave`) fires when the HQ drops to 70% HP. It runs
    alongside the HQ loop.
  `PlayerHQ.SpawnUnit` summons the player's units.
- Battle end:
  - `EnemyHQ.Dead()` always calls `GameManager.ClearStage()`. Once the tutorial is complete, it
    opens the stage-clear artifact select (`GameManager.OpenSelectArtifactUI()`, which stops time).
    The victory `ShowResultUI(true)` runs later, from `UIStageClearArtifactSelect` after the player
    picks. During the tutorial, `Dead()` calls `ShowResultUI(true)` directly.
  - Defeat is `ShowResultUI(false)`. It is called from `PlayerHQ.Dead()`, `Player`, and
    `UIGiveUpPanel`.
- Buffs go through `BuffController`, and stage modifiers go through `Modifiercalculator`.

## Third-party

DOTween (`Plugins/Demigiant`), UniTask (`Plugins/UniTask`), Google Mobile Ads, and art/FX packs in
`Assets/Externals`. Don't modify vendor code. `Assets/Z_ForTest` and `UI/SelectCard(Old)` are test
or legacy code.
