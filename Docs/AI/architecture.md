# Architecture

Parent: [`PROJECT.md`](PROJECT.md). This file covers the cross-cutting structure that takes several
files to see, and it routes to one document per game system. The rules for *new* code (where
things go, how they talk) are in [`module-rules.md`](module-rules.md). This file and the system
documents describe what already exists, including the parts that are broken.

## System documents

Open the one whose system your task touches. Each document lists its files, its runtime flow, its
data and save shape, common changes, and its gotchas (dead code and suspected bugs).

```
Docs/AI/architecture.md  [router]          ← you are here: cross-cutting infrastructure
├─ Docs/AI/systems/app-shell.md            start scene and first launch, tutorial, main menu hub, settings/pause, audio, input, shared popups
├─ Docs/AI/systems/player-data.md          PlayerDataManager: the save shape, load order, saving, currencies, its events
├─ Docs/AI/systems/territory.md            main-screen 5×5 grid: buildings, costs, repair and move, building synergies
├─ Docs/AI/systems/deck-and-units.md       unit roster and cards, deck presets, unit synergies, card picker
├─ Docs/AI/systems/artifacts.md            active/passive artifacts: inventory, equip, merge/upgrade, stats, active skills, stage-clear pick
├─ Docs/AI/systems/gacha-and-rewards.md    gacha (Cloud Code GachaV2), pity, mail/post box, ad rewards, reward panel, exactly-once risks
├─ Docs/AI/systems/stages.md               stage ids and unlocks, stage select, waves, stage rewards, destiny and challenge modifiers
└─ Docs/AI/systems/battle.md               battle scene: start sequence, food/spawning, hero, HQ skills, units and AI, damage/buffs, battle end
```

## Managers (singletons)

Almost every system is a `SingletonMono<T>` (`Assets/Scripts/Base/Framework/SingletonMono.cs`).

- Reading `Instance` **auto-creates** the GameObject. No production singleton is placed in a build
  scene, so every one of them is created on its first `Instance` read. `SceneLoader`,
  `FadeManager`, and `AudioManager` are created even earlier, by
  `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`. `UIManager.Awake` forces `InputManager` to
  exist.
- Singletons are persistent (`DontDestroyOnLoad`) by default. A scene-owned one overrides
  `IsPersistent => false`: today `UnitManager` and `UnitRenderManager`.
- **Destroy hazard.** `SingletonMono.OnDestroy` sets the per-type static `isDestroyed` and nulls
  `instance` whenever *any* persistent copy of that type is destroyed. That includes a duplicate
  that `Awake` destroys, and a call to `Release()`. From then on `Instance` returns null for the
  rest of the run. Null-check `Instance` in shutdown and teardown code, and never place a second
  copy of a persistent manager in a scene.
- `EventManager` and `UnitRenderManager` hide `Instance` (`private new static`) and expose static
  members instead. `MainScreenBuildingController.Instance` is a hand-written scene singleton, not
  a `SingletonMono`.

| Singleton | Where | Owns | System doc |
|---|---|---|---|
| `GameManager` | `Assets/Scripts/Manager/` | Battle flow, win/loss, stage-result rewards, `IsTutorialCompleted`, `LoadMain` | battle, stages |
| `PlayerDataManager` | `Assets/Scripts/Manager/` | All player state: currencies, unit cards, decks, stage clears, territory grid (`TileDataHandler`), artifacts, pity, level/EXP, battle food | player-data |
| `SettingDataManager` | `Assets/Scripts/Manager/` | The runtime stage-unlock table (`MainStageData`), battle speed, the control layout | stages, app-shell |
| `DataManager` | `Assets/Scripts/Manager/` | Static tables (see *Static game data*), icon sprite caches, `AudioData` | — |
| `BackendManager` | `Assets/Scripts/Manager/` | UGS: login, request queue, Cloud Save, Economy, Cloud Code, Remote Config, Analytics | player-data, gacha-and-rewards |
| `UIManager` | `Assets/Scripts/Manager/` | UI cache and loading by name, back-button stack, loading overlay | app-shell |
| `ObjectPoolManager` | `Assets/Scripts/Manager/` | Pools keyed by `PoolType` | battle |
| `EventManager` | `Assets/Scripts/Manager/` | The typed fact bus (see *Events*) | — |
| `InputManager` | `Assets/Scripts/Manager/` | The back-button input action | app-shell |
| `AudioManager` | `Assets/Scripts/Manager/` | BGM per scene, SFX, mixer volumes | app-shell |
| `AdManager` | `Assets/Scripts/Manager/` | Google Mobile Ads init and rewarded ads | gacha-and-rewards |
| `UnitManager` (scene) | `Assets/Scripts/Manager/` | Live player and enemy unit lists for targeting | battle |
| `UnitRenderManager` (scene) | `Assets/Scripts/Manager/` | Render textures for the active deck's spawn slots | deck-and-units |
| `SceneLoader`, `FadeManager` | `Assets/GlobalScripts/` | Scene transitions, the fade overlay, in-scene panel switches | — |
| `AnimationData` | `Assets/Scripts/Animation/AnimatonData.cs` | Animator parameter hashes | battle |
| `SceneLoadManager` (legacy) | `Assets/Scripts/Manager/` | Old scene loader. Only a `Assets/Z_ForTest` script still calls it | — |

`Assets/Scripts/Manager/ResourceManager.cs` is commented out entirely; its job moved into
`PlayerDataManager`.

## Startup and scene transitions

**First launch.** The first `BackendManager.Instance` read runs `BackendManager.Awake` →
`InitializeAndLoginAsync`: network check → `UnityServices.InitializeAsync` (environment `dev`, or
`usertest` in a `USERTEST` player build) → anonymous sign-in → Analytics → Economy config sync →
`AdManager.InitializeAsync` (skipped on WebGL) → `InitializationTask` completes →
`PlayerDataManager.InitializeResourcesAsync` (Economy balances) → `LoadDataFromCloundAsync` (Cloud
Save) → `UIManager.HideLoading`. A failure opens `ErrorPopUP`. `InitializationTask` completes
*before* player data has loaded, so `EnsureInstanceAndInitializedAsync` does not mean the data is
ready; only the loading overlay gates it. The full load order is in
[`systems/player-data.md`](systems/player-data.md).

**Per-scene bootstrap** MonoBehaviours are in `Assets/Scripts/Scenes/SceneLoader/`.
`SceneLoaderStart.Awake` opens `StartUI`, shows the loading overlay, then awaits
`BackendManager.CheckInterentAsync()` and `EnsureInstanceAndInitializedAsync()`.
`CheckInterentAsync` does nothing on first launch. On a later return to StartScene it re-checks
the network and reloads balances and player data on the live managers. `SceneLoaderMain` opens a
MainScene panel chosen by `GameManager.LoadMain` (`None`, `DeckPresetController`,
`UIDestinyRoullette`, `TutorialInWisdom`). `SceneLoaderBattle` loads the map prefab and activates
`UnitManager`.

**Transitions.** Use `SceneLoader.Instance.StartLoadScene(SceneState.X)`
(`Assets/GlobalScripts/SceneLoader.cs`). `SceneLoader.NextSceneSequence` runs:
`FadeManager.FadeOut()` → load `EmptyScene` → `OnSceneReset()` on every registered
`ISceneResettable` (only `UIManager` and `ObjectPoolManager`; both register in `Start`) in the same
frame, before the old scene has actually unloaded → `Resources.UnloadUnusedAssets()` → GC → load the
target. The fade-in runs from `OnSceneLoaded`.

- Input during a transition is blocked by `FadeManager`'s overlay canvas (its image's
  `raycastTarget`), not by `SceneLoader.IsChange`. `IsChange` is set only after the fade-out and
  nothing reads it.
- UIs are children of the persistent `UIManager`, and the scene reset destroys them. Each
  bootstrap re-creates the UIs its scene needs with `GetUI<T>()` (MainScene's also calls
  `CloseUI()` to pre-create hidden panels; BattleScene's keeps them open).
- `SceneState` is `None, MainScene, BattleScene, EmptyScene, WonJinTestScene, StartScene`, and
  `sceneNames` has no entry for `WonJinTestScene`. To add a scene, add it to the enum,
  `sceneNames`, and Build Settings.
- `FadeManager` also provides `SwitchGameObjects(from, to)` for in-scene panel switches (the main
  menu uses it) and static `FadeInUI`/`FadeOutUI(CanvasGroup, …)`, which `BasePopUpUI` uses.
- `Manager/SceneLoadManager.cs` and the `SceneBase` classes (`Assets/Scripts/Scenes/MainScene.cs`,
  `Assets/Scripts/Scenes/BattleScene.cs`) are an older path. No production code uses them.

## Static game data (Excel → ScriptableObject)

- The design tables are `.xlsx` files in `Assets/Excel/`. Importing one makes the editor importer
  (`Assets/Externals/ExcelImporter`) regenerate the matching SO in `Assets/Resources/DB/`, for every
  SO class that carries `[ExcelAsset(AssetPath = "Resources/DB")]` (12 classes).
  - The xlsx file name must equal the SO class name. The SO's public `List<>` field names must
    match the sheet names, and the row class's fields must match the column headers. Unmatched
    sheets, columns, and fields are **skipped silently**. Rows stop at the first blank cell in
    column A, and rows starting with `#` are comments.
  - A multi-sheet SO needs one public list field per sheet. Adding a sheet to the xlsx alone does
    nothing (`StageWaveSO` drops two sheets today; see [`systems/stages.md`](systems/stages.md)).
  - **Exception:** in `BuildingUpgradeSO` the attribute is commented out ("수동으로 임포터 해야해서",
    i.e. it has to be imported by hand). Importing `BuildingUpgradeSO.xlsx` doesn't update the SO;
    use the menu item *My Tools/Import BuildingUpgrade Data*. That importer and its editor
    (`BuildingUpgradeImporter.cs`, `BuildingUpgradeSOEditor.cs`) are project code that lives
    inside the vendor folder `Assets/Externals/ExcelImporter/Editor/`.
- A reimport (including the first import of a fresh clone) rewrites the SO from the row class, so
  a field added to a row class shows up as an `.asset` diff even when no sheet changed. Today
  `Assets/Resources/DB/PlayerSO.asset` lacks the inherited `ownedCount`/`curLevel` fields, so its
  next reimport adds two lines to every row without changing data.
- Each SO derives from `MonoSO<TData>` (`Assets/Scripts/DB/SO/`) and implements
  `SetData(Dictionary<int, TData>)`. `PlayerSO` is the odd one out: its class is in
  `Assets/Scripts/DB/Data/Unit/`. One SO can merge several sheets into one dictionary. The key is
  usually `idNumber`, but not always: `PlayerUnitSO` registers each row under **both**
  `(int)poolType` and `idNumber` (so `Values` lists every row twice), `EnemyUnitSO` keys by
  `(int)poolType`, `PlayerSO` by `level`, and `SynergyEffectSO` by `synergyType*1000 + synergyGrade`.
  `GetList()` is deprecated.
- `DataBase<TData, TSO>` loads `Resources.Load("DB/<SOName>")`. `DataManager` exposes each table as
  a lazily created `DataBase` property, and the properties mix static and instance members:
  `DataManager.PlayerUnitData`, `PlayerData`, `EnemyUnitData`, `ArtifactData`, `SynergyEffectData`,
  and `AudioData` are static, while `DataManager.Instance.EnemyData`, `RewardData`,
  `BuildingUpgradeData`, `MainStageData`, `SubStageData`, `StageWaveData`, and `StageModifierData`
  are instance members. The `ItemData` property is commented out, but `ItemSO` is still imported.
- To add a table: create the xlsx, the data class in `Assets/Scripts/DB/Data/`, the SO in
  `Assets/Scripts/DB/SO/`, and a `DataManager` property.
- **The `Resources/DB` SOs are not purely static at runtime.** Several systems keep player state on
  the shared row objects: `ownedCount` on `PlayerUnitSO` rows, `curLevel` and appended level rows
  on `ArtifactSO`, unlock flags on the stage rows (`SettingDataManager`), and runtime-loaded sprites
  in serialized row fields. In the Editor these changes can survive leaving Play mode. See
  [`module-rules.md`](module-rules.md) → *Known exceptions* and the system docs.
- Shared string keys and constants are in `Assets/Scripts/DB/Constants.cs`.

## Backend (Unity Gaming Services)

- Player progress lives on the server: Authentication (anonymous), Cloud Save (the whole player
  save under one key, gacha pity, claimed mail), Economy (six currencies), Cloud Code, Remote
  Config, and Analytics. The save shape and load order are in
  [`systems/player-data.md`](systems/player-data.md).
- Device-local preferences stay in `PlayerPrefs`: the three volume keys, the control layout, and
  the "don't ask again" empty-deck flag. Battle speed and FPS are not saved at all. The key list
  is in [`systems/app-shell.md`](systems/app-shell.md).
- **The request queue.** Network calls go through `BackendManager`'s static async (UniTask) API.
  The data calls (Cloud Save, Economy, gacha, mail) check `CanCommunicateAsync` and then go through
  `EnqueueRequestAsync`, a sequential queue with retry. The initialization and connectivity calls
  don't queue, and neither does `WatchAdAndGetReward`, which checks connectivity and then shows the
  ad (it ignores whether the ad succeeded).
  - The queue serializes requests; it doesn't merge them or make them idempotent. A retry after a
    timeout the server already applied repeats the write. Which grants can double or be lost is in
    [`systems/gacha-and-rewards.md`](systems/gacha-and-rewards.md).
  - `CloudCodeException` and `EconomyException` are `RequestFailedException`s, so a server-side
    rejection (for example not enough tickets) shows the network-error retry popup, and Cancel
    returns to StartScene.
  - Mail (`CheckMailAsync`) catches every exception and returns null, so its retry never runs.
  - A Cloud Code 422 (not enough funds) in `InternalChangeEconomyAsync` reloads balances, calls
    `PlayerDataManager.SyncAllResources`, and throws `NOT_ENOUGH_FUNDS`. The Economy write-lock
    conflict branch in `ExecuteWithRetryAsync` is effectively dead: no client write sends a write
    lock, and `writeLocks` is filled but never read.
- **Bypasses.** Don't call UGS SDKs or the ad SDK directly from other code. These existing calls
  skip the queue, so they get no ordering or retry: `UIMenu` fires the Cloud Code `WakeUpServer()`
  without awaiting it, `GameManager` records the stage-result Analytics event directly, and
  `MainScreenBuildingController` and `ConstructionUpgradePanel` call
  `AdManager.Instance.ShowRewardedAd(Action)` directly, without the connectivity check.
- **Cloud Code is not anti-cheat for currencies.** `ChangeEconomyResource` in the Economy module
  applies whatever currency id and signed amount the client sends. Only the GachaV2 ticket charge,
  roll, and pity are server-authoritative. Stage rewards, mail rewards, and gacha units are decided
  or saved by the client.
- Remote Config holds the gacha banners (`GACHA_BANNERS`, read by the GachaV2 module) and the mail
  and notice lists (read by the client). The boot load calls Cloud Code `GetGachaBanners` before
  loading the save, so a GachaV2 failure blocks the whole player-data load (retry popup first; it
  fails if the player cancels).
- The Cloud Code C# modules are at the repo root in `Economy/` and `GachaV2/`. `Gacha/`,
  `Gatcha/`, and `HelloWorld/` are old or sample code. The `.ccmr` files in
  `Assets/Scripts/Cloud/` reference the modules (`Economy.ccmr`, `GachaV2.ccmr`, and the unused
  `Gacha.ccmr`). The modules' `.sln`/`.csproj` files are gitignored, so a fresh clone can't build
  them. The client calls them through the generated bindings in
  `Assets/CloudCode/GeneratedModuleBindings` (`EconomyModuleBindings`, `GachaModuleV2Bindings`; the
  old Gacha bindings are generated too and unused). Regenerate the bindings after changing a
  module's signatures.

## Resources-path conventions

- **UI**: `UIManager.Instance.OpenUI<T>()` / `GetUI<T>()` loads `Resources/Prefabs/UI/<TypeName>`.
  The prefab name must equal the class name, and the class must derive from `BaseUI`. Popups derive
  from `BasePopUpUI`.
  - Only **root** UIs are loaded by name. Many `BaseUI`/`BasePopUpUI` classes are nested inside a
    root prefab and never loaded on their own (for example the card picker and popups inside
    `DeckPresetController`).
  - Known mismatches: `ErrorPopUp.prefab` vs class `ErrorPopUP` (it relies on `Resources.Load`
    matching without regard to case; don't copy it), and `UITestEndPopup`, which has no prefab although `GameManager.ShowResultUI`
    opens it after stage 2-9.
  - `UIManager.ShowLoading`/`HideLoading` toggle the `UI_Loading` overlay, which blocks touches and
    the back button.
- **Back button**: a UI stack of `IBackButtonHandler`s. Root panels push too, not just popups.
  Popups push and pop through `UIManager`'s static `PubishAddUIStackEvent`/`PublishRemoveUIStackEvent`.
  Despite the names, these call `PushUI`/`PopUI` directly, because their event publication is
  commented out. `UIManager` still subscribes to `AddUIStackEvent`/`RemoveUIStackEvent`, but nothing
  publishes them. `PopUI` removes whatever is on top, not the caller's entry.
- **Pooling**: add a `PoolType` enum value. **Its int value is stored in assets**: `poolType` in
  `Assets/Resources/DB/PlayerUnitSO.asset` and `EnemyUnitSO.asset`, the `enemyUnits` overrides in
  the `Assets/Resources/Map` prefabs, and `PlayerHQ.playerUnits`; the unit tables are also keyed by
  `(int)poolType`. Inserting a value before existing ones shifts them and silently re-points those
  assets. The enum's own comments ask you to add values inside its regions (enemies in the enemy
  region, others below the allies); only do that if you also re-check the stored ints, otherwise
  append at the end of the enum.
  Put a prefab with the same name in `Resources/Prefabs/ObjPooling/`, with a `BasePoolable` or a
  subclass on it. `ObjectPoolManager` loads the prefab of **every** `PoolType` on first access. A
  value without a prefab only logs a warning there (many do today), and `Get` for it logs an error
  and returns **null**, so null-check the result. Get objects with `ObjectPoolManager.Instance.Get(PoolType)`,
  and always return them with `ReleaseSelf()`. Pools are cleared on scene reset.
- **Other `Resources.Load` paths** (name or data-driven contracts too): `Map/Map{main}_{sub}`
  (1-based battle map, `SceneLoaderBattle`), `DB/ArtifactSO` (loaded directly by
  `ArtifactRewardGenerator`, bypassing `DataManager`), `Sound/SoundData` and `Sound/GameMixer`,
  `Synergy/<UnitSynergyType>` and `Synergy/Synergy_Background1`/`2`, `Icon/Position`,
  `UnitIcon/<PoolType>` and `GachaHero/<PoolType>` (loaded by `PlayerUnitSO.SetData`), and sprite
  paths stored in table columns (`iconSpritePath`, building sprites, hero cinematic sprites).
  `Assets/Editor/SpriteImportProcessor.cs` imports everything under `Assets/Resources/UnitIcon` as
  sprites.

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
`OnEnable`, and `?.Unsubscribe` in `OnDisable` (see `GachaUIPanel`, and `SynergyOutlineManager` in
`SynergyOuitlineManager.cs`). Event structs are usually declared in the publisher's file (for
example, `PlayerDataManager.cs` declares the facts it publishes). Some have a file of their own
elsewhere: `BattleEndedEvent` is published by `GameManager` but lives in
`Assets/Scripts/UI/BattleScene/BattleEndedEvent.cs`, and its only subscriber is
`PlayerDataManager`.

`PlayerDataManager` also exposes plain C# `event`s that UI subscribes to directly
(`OnResourceChangedEvent`, `OnArtifactOwnedChanged`, `OnArtifactEquippedChanged`). The channel
lists per system are in the system docs, and the usage rules are in
[`module-rules.md`](module-rules.md), under *Rules → C — EventManager channels*.

## Naming traps

These file names differ from the class they hold. Don't "fix" the spelling in passing: renaming a
class that a root UI prefab is loaded by breaks `Resources/Prefabs/UI/<TypeName>` (P5 in
`module-rules.md`), Unity binds a MonoBehaviour script to its class by file name (so a rename can
detach the component from prefabs even though the `.meta` GUID stays), and nobody has checked in the
Editor which of these currently bind:

`AnimatonData.cs` (`AnimationData`), `BlinkText.cs` (`BlinkingText`), `BmResourcePanel.cs`
(`ResourcePanelUI`), `DestoryConfirmPopup.cs` (`DestroyConfirmPopup`), `SynergyOuitlineManager.cs`
(`SynergyOutlineManager`), `UIArtifactStatUnitPage.cs` (`UIArtifactUnitStatPage`),
`UIDestinyRoullete.cs` (`UIDestinyRoullette`), `UITurorialHero.cs` (`UITutorialHero`).

Several methods are misspelled too (`LoadDataFromCloundAsync`, `CheckInterentAsync`,
`PubishAddUIStackEvent`). Search for the misspelling when you look for them.

## Third-party

Vendor code, don't modify: DOTween (`Assets/Plugins/Demigiant`), UniTask
(`Assets/Plugins/UniTask`), Google Mobile Ads (`Assets/GoogleMobileAds`, plus the native
libraries in `Assets/Plugins/Android` and `Assets/Plugins/iOS`), `Assets/ExternalDependencyManager`,
`Assets/MobileDependencyResolver`, Assets/TextMesh Pro, and the packs in `Assets/Externals`
(ExcelImporter, SPUM, art and FX), apart from the two project-written BuildingUpgrade importer
scripts noted above. `Assets/Z_ForTest` holds test scenes and scripts. `UI/SelectCard(Old)` and
the other `(Old)` files are legacy code.
