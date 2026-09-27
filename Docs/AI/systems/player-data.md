# Player data

Parent: [`../architecture.md`](../architecture.md). The player's persistent progress (currencies,
unit cards, decks, stage unlocks, territory grid, artifacts, level/EXP, gacha pity) and how it is
loaded from and saved to Unity Gaming Services.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/Manager/PlayerDataManager.cs` | The persistent singleton that owns all player state. Also declares `PlayerSaveData`, `DeckData`, the `ResourceType`/`TileStatus` enums, and the event structs `SynergyDataUpdatedEvent`, `ClearedStagesUpdatedEvent`, `LimitedPityCountUpdatedEvent`, `StandardPityCountUpdatedEvent`. |
| `Assets/Scripts/Manager/BackendManager.cs` | Cloud Save / Economy / Cloud Code calls (`LoadDataAsync`, `SaveDataAsync`, `LoadEconomyData`, `ChangeEconomyAsync`, `EconomyEnumToId`/`EconomyIdToEnum`), the `AllCloudData` load result, and the startup sequence that triggers the load. |
| `Assets/Scripts/DB/Constants.cs` | Economy currency ids and Cloud Save keys. |
| `Assets/Scripts/UI/MainScreen/ResourceUI.cs` | Currency text bar. Not opened by name: it is a component inside `MainScreenUI`, `UIMenu`, `GachaUIPanel`, `DeckPresetController` (Resources UI prefabs) and `Assets/Prefabs/UI/ResourcePanel.prefab`. |
| `Assets/Scripts/UI/MainScreen/Building/Synergy/TileDataHandler.cs` | Territory grid state held by `PlayerDataManager._TileDataHandler`, plus the `TileDataSnapshot` save class (territory doc). |
| `Assets/Scripts/Manager/ResourceManager.cs` | Dead: fully commented out ("플레이어데이터매니저로 이동" = moved to PlayerDataManager). |

## How it works

**Startup load order** (first launch): `BackendManager.Awake` → `InitializeAndLoginAsync`: network
check → `UnityServices.InitializeAsync` (env `dev`, or `usertest` under the `USERTEST` define) →
anonymous sign-in → Analytics → `EconomyService…SyncConfigurationAsync` → ads init →
`_initializationTcs.TrySetResult(true)` → **`PlayerDataManager.InitializeResourcesAsync()`**
(Economy balances) → **`PlayerDataManager.LoadDataFromCloundAsync()`** (Cloud Save) →
`isInitializationCompeleted = true` → `UIManager.HideLoading()`.
Returning to StartScene later: `SceneLoaderStart.Awake` → `BackendManager.CheckInterentAsync()`
re-runs the same two loads (only when `isInitializationCompeleted`).

`LoadDataFromCloundAsync`:
1. `BackendManager.LoadDataAsync()` → `InternalLoadDataAsync`: calls Cloud Code
   `GachaModuleV2Bindings.GetGachaBanners()` to get banner ids and pity thresholds, then loads the
   Cloud Save keys `PLAYER_DATA`, `PITYCOUNT_<normal bannerId>`, `PITYCOUNT_<pickup bannerId>` in one call.
2. Sets static `StandardGachaPityLimit`/`LimitedGachaPityLimit` from the thresholds.
3. If `PlayerSaveData` is null (new player): `CardGenerate` unlocks ids 100001–100010 and returns.
   `GameManager.IsTutorialCompleted` stays false.
4. Otherwise sets `GameManager.IsTutorialCompleted = true` ("a save exists ⇒ tutorial done"), then in
   order: `SettingDataManager.LoadClearData(UnlockData)` → `UpdateClearedStagesFromServer` →
   `ConvertIntToDeck` → `LoadDeckName` → `ActiveDeckIndex` → `CardGenerate(OwnedCardData)` + apply
   `CardCounts` → `_TileDataHandler.RestoreFromSnapshot(TileGridData)` → `LoadArtifactData` →
   `PlayerLevel`/`CurExp` → pity counts (set directly; **no pity event is published here** —
   `GachaUIPanel` reads the properties when it opens).
5. A `DeserializationException` (matched by type *name*) or `NullReferenceException` falls back to the
   new-player unit set. Other exceptions are only logged.

**Saving** — no dirty flags, no batching, no autosave timer. `SaveDataToCloudAsync()` builds a full
`PlayerSaveData` snapshot and writes the single key `PLAYER_DATA` through the BackendManager queue
(so concurrent saves are serialized, not merged). It is a **no-op until
`GameManager.IsTutorialCompleted`**, and it swallows every exception (callers never learn a save
failed). Callers save after each meaningful action: battle result and `GameManager.ClearStage`,
building build/upgrade/move/destroy/ad-skip (`MainScreenBuildingController`,
`PlayerDataManager.DestroyBuildingAt`), deck edits (`DeckPresetController`), gacha pulls
(`GachaUIPanel`), unit upgrade (`UnitUpgradeService`), artifact upgrade (`ArtifactUpgradeService`),
tutorial skip (`UITutorialDeck`), and `OnApplicationPause(true)` (fire-and-forget). There is no
`OnApplicationQuit` save.

**Currencies** are separate from `PLAYER_DATA`: they live in UGS Economy.
`AddResource(type, amount)` updates the local `_resources` dictionary first (clamped to 99999),
fires `OnResourceChangedEvent`, then (except Food) awaits `BackendManager.ChangeEconomyAsync`, which
calls Cloud Code `EconomyModuleBindings.ChangeEconomyResource`. On a Cloud Code 422 (not enough funds)
BackendManager reloads balances, calls `PlayerDataManager.SyncAllResources`, and throws
`NOT_ENOUGH_FUNDS` after a popup. Gacha deducts tickets server-side; the client mirrors it with
`ChangeResourceOnlyLocal(Ticket, result.UserCurrency)` (sets an absolute value, no server call).

**Events published**
| Channel | Kind | Published by | Subscribers |
|---|---|---|---|
| `SynergyDataUpdatedEvent` | EventManager | `UpdateAllBuildingEffects`, and again at the end of `UpdateAllSynergyEffects` | `SynergyOutlineManager`, `DeckPresetController`, `UIUnitCardSelect` |
| `ClearedStagesUpdatedEvent` | EventManager | `UpdateClearedStagesFromServer`, `MarkLocalStageClear` | `MainScreenBuildingController` |
| `LimitedPityCountUpdatedEvent` / `StandardPityCountUpdatedEvent` (`NewCount`) | EventManager | `UpdateLimitedPityCount` / `UpdateStandardPityCount` (and unused `LoadPityCounts`) | `GachaUIPanel` |
| `OnResourceChangedEvent(ResourceType,int)` | C# `event` | every currency/food mutation, `UpdateAllBuildingEffects` | `ResourceUI`, `BmResourcePanel`, `SupplyUI` |
| `OnArtifactOwnedChanged` / `OnArtifactEquippedChanged` | C# `event` | artifact setters, `LoadArtifactData` | `ArtifactUIPresenter`, `UIArtifactUpgradePresenter` |
| `OnUnitCountChanged(int,int)` | C# `event` | `UnLockUnit`, `CardCountChanged` | **none** |

**Subscribed:** `GridStateChangedEvent` → `UpdateAllSynergyEffects`; `BattleEndedEvent` →
`_TileDataHandler.AdvanceRepairTurn()` and, on defeat after the tutorial, `DamageRandomTile()`.

**Public API by domain**
- Currency: `GetResourceAmount` (returns **-1** for a key not yet loaded), `AddResource`,
  `ChangeResourceOnlyLocal`, `SyncAllResources`, `ApplyResourcePenalty` (5% of Gold/Wood/Iron/MagicStone,
  used on defeat), `InitializeResourcesAsync`.
- Food (battle-only, never saved): `CurrentFood`, `MaxFood`, `CalculatedMaxFood`, `SupplyLevel`,
  `AddFoodOverTime`, `UpgradeSupplyLevel`, `TryGetUpgradeCost`, `ResetFood` (called by
  `GameManager.StartBattle`; it is what first creates `_resources[Food]`).
- Units: `OwnedCardData` (id → `BaseUnitData`, the *same objects* as the `PlayerUnitSO` rows),
  `UnLockUnit` (new card or `ownedCount++`), `CardCountChanged`. No unit level is stored anywhere.
- Decks: `DeckPresets` (keys 1–5, 8 slots, `-1` = empty), `ActiveDeckIndex`, static
  `AppliedDeckUnitSynergies` (runtime only).
- Stages: `SelectedStageIdx` (runtime), `IsStageCleared`, `MarkLocalStageClear` (first clear of
  1-2 grants 10 Tickets), `UpdateClearedStagesFromServer`.
- Territory (summary; territory doc): `_TileDataHandler`, `UpdateAllSynergyEffects`,
  `UpdateAllBuildingEffects` (derives `TotalUnitCooldownReduction`, `RareUnitSlots`, `EpicUnitSlots`,
  max food, the `Synergy*` bonuses, `TileEfficiencyBonuses`, `ActiveSynergies`), `GetBuildableList`,
  `CalculateTotalInvestedCost`, `DestroyBuildingAt` (50% refund, then saves).
- Artifacts (summary; artifact doc): `OwnedArtifacts`, `EquippedArtifacts` (always 8 slots),
  `Add/RemoveOwnedArtifact`, `Set/ClearEquipped*`, `GetEquippedArtifact`, `SetOwned/EquippedArtifacts`.
- Gacha pity: `LimitedGachaPityCount` (pickup/page 0), `StandardGachaPityCount` (normal/page 1),
  static limits, `UpdateLimited/StandardPityCount(int)` fed from the Cloud Code result.
- Destiny/challenge (runtime only): `currentDestiny`, `activeChallenges`,
  `Set/ClearDestinyChallengeCache`, `GetDestinyChallengeBonus`.
- Level: `PlayerLevel` (clamped ≥ 1), `CurExp` — written by `Player` and `GameManager`.

## Data and persistence

Cloud Save keys (`Constants`): `PLAYER_DATA` (the whole `PlayerSaveData`, client-written),
`PITYCOUNT_<bannerId>` (written only by the GachaV2 Cloud Code module; banner keys
`NORMAL_BANNER`/`PICKUP_BANNER`), `ALREADY_RECIEVED_MAIL` / `ALREADY_RECIEVED_NOTICE` (mail system,
`PostBox`). Serialization is Newtonsoft (Cloud Save `GetAs<T>`), so **public field names are the
JSON keys**.

`PlayerSaveData` fields: `UnlockData` (`List<List<bool>>` per main/sub stage *unlock* flags from
`SettingDataManager.SaveClearData`, not clear flags), `DeckPresets` (`Dictionary<int, List<int>>`),
`DeckNames`, `ActiveDeckIndex`, `TileGridData` (`TileDataSnapshot`: 5×5 `int` building ids,
`TileStatus`, repair turns, `long` cooldown `DateTime.ToBinary`), `OwnedCardData` (`List<int>`),
`CardCounts` (`Dictionary<int,int>`), `OwnedArtifacts` / `EquippedArtifacts` (**JSON strings** made
by `SaveArtifactData` with `TypeNameHandling.Auto`), `PlayerLevel`, `PlayerExp`.

Economy currencies ↔ `ResourceType`: `GOLD`↔Gold, `WOOD`↔Wood, `IRON`↔Iron, `MAGICSTONE`↔MagicStone,
`BM`↔Bm, `TICKET`↔Ticket. `Food` is local and battle-only; `EXP` has no currency and is never in
`_resources` (EXP lives in `CurExp`). Not persisted at all: food/supply, synergy and building-effect
values (recomputed), destiny/challenge, `SelectedStageIdx`, `OwnedActiveAfData`/`EquippedActiveAfData`.

## Common changes

- **To add a saved field:** 1. add a public field to `PlayerSaveData`; 2. fill it in
  `InternalSaveDataToCloudAsync`; 3. restore it in `LoadDataFromCloundAsync` with a null/default
  guard (old saves won't have it); 4. if it must reset on a StartScene reload, clear it explicitly
  (reload does not recreate the manager).
- **To add a currency:** 1. create it in the Economy dashboard; 2. add the id to `Constants`, a
  `ResourceType` value (append — see Gotchas), and both switches in `BackendManager.EconomyEnumToId`
  / `EconomyIdToEnum`; 3. also both currency switches in `PostContent` if mail can grant it; 4. a text
  field in `ResourceUI`. Do step 2 **before** the dashboard currency reaches players:
  `InternalLoadEconomyData` throws on an unknown currency id, which breaks the whole load.
- **To change a currency amount:** call `await AddResource(type, delta)` (server + local). Use
  `ChangeResourceOnlyLocal` only when the server already changed it (Cloud Code result).
- **To add a unit rarity list:** `CardGenerate` only indexes `PlayerUnitSO.allianceCommon/Rare/Epic`;
  add the new list there or `UnLockUnit` will reject its ids.

## Gotchas

- **A schema change can wipe progress.** If `PLAYER_DATA` fails to deserialize, the catch in
  `LoadDataFromCloundAsync` loads the new-player state and leaves `IsTutorialCompleted` false; the
  tutorial replays, and the first save after it (`UITutorialDeck`) overwrites the server data. A
  `NullReferenceException` *after* `IsTutorialCompleted = true` leaves partial state that the next
  save writes back. Never rename, retype, or remove a `PlayerSaveData` field (or a `TileDataSnapshot`
  field) without a migration; renaming silently drops the old value.
- Artifacts are stored with `$type` names: renaming/moving `ArtifactData` subclasses
  (`PassiveArtifactData`, `ActiveArtifactData`, `ActiveArtifactLevelData`) fails
  `LoadArtifactData`, which resets owned and equipped artifacts to empty (and the next save persists that).
- Enums are serialized as ints: reordering `TileStatus` corrupts saved grids; `ResourceType` values
  are also stored as ints in SO data (e.g. building costs). Append only.
- Currency changes are optimistic: the local value changes before the server call. A non-422 failure
  (or a user cancel) leaves local ≠ server until the next reload. The 99999 cap is client-side only.
- `GetResourceAmount` returns -1 before Economy loads, and for Food before `ResetFood`; `AddResource(Food)`
  before `ResetFood` only logs a warning.
- `ownedCount` is a field on the `PlayerUnitSO` row objects, mutated at runtime. In the Editor the
  value appears to persist in memory across Play sessions until the SO is reloaded.
- StartScene reload (`CheckInterentAsync`) reruns the load on the live manager without clearing
  `OwnedCardData` or `DeckPresets`; `ConvertIntToDeck` only writes non-`-1` slots.
- Gacha: the server deducts tickets and writes pity, but the unit is unlocked client-side and saved
  afterwards — if the app dies in between, the unit is lost.
- `UpdateAllSynergyEffects` calls `DetectAllSynergies` twice and publishes `SynergyDataUpdatedEvent`
  twice (once via `UpdateAllBuildingEffects`).
- Initial unit ids 100001–100010 are hard-coded twice in `LoadDataFromCloundAsync`.
- Dead code: `LoadPityCounts`, `ApplyDefeatPenalties`, `OnUnitCountChanged` (no subscribers),
  `OwnedActiveAfData`/`EquippedActiveAfData` ("어떤 경우로 사용되는 지 애매해서 살려둬용" = unclear
  use, kept), `BackendManager.writeLocks` (written, never read), `ResourceManager.cs`, the commented
  `CheckLevelUp`.
- `Constants.PLAYER_DATA_KEY` and the other save keys are `static string`, not `const` (mutable,
  unusable in `switch`).
- Suspected issue: `UpdateClearedStagesFromServer`, when a main stage's first sub-stage is locked,
  marks `(i, 8)` uncleared. Main stages have 9 sub-stages (`MainStageSO.subStageCount`), so the last
  unlocked stage is `(i, 9)`: after a reload sub-stage 8 reads as uncleared and 9 as cleared.
- Suspected issue: `BackendManager.InternalLoadDataAsync` sets `NormalPity = 0` in the branch where the
  *pickup* pity key is missing (harmless today, since the default is 0). If a banner id is missing,
  a `null` key goes into the Cloud Save key set.
- Suspected issue: a duplicate `PlayerDataManager` would throw in `OnEnable`, because the channel fields
  are only set when `Instance == this`. Today it is only auto-created, never placed in a scene.
