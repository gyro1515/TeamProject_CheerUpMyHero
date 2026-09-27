# Deck and units

Parent: [`../architecture.md`](../architecture.md). The player's unit roster (owned unit cards), the
5 deck presets of 8 slots the player edits before a battle, the deck-unit synergy badges, and the
static unit table that battle spawns from.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/DB/Data/Unit/BaseUnitData.cs` | Row class for every player unit, plus the enums `Rarity`, `UnitType`, `UnitAttackType`, `UnitSynergyType` (`[Flags]`), `UnitClass`. Also holds the runtime fields `ownedCount`/`curLevel` (see Gotchas). |
| `Assets/Scripts/DB/Data/Unit/HeroData.cs` | `BaseUnitData` + hero cinematic strings/sprites (sheet `hero_unit`). |
| `Assets/Scripts/DB/Data/Unit/PlayerData.cs`, `Assets/Scripts/DB/Data/Unit/PlayerSO.cs` | The commander (`Player`) per-level stats; `PlayerSO.SetData` keys by `level`, not `idNumber`. Read by `UIPlayerStatPopup` and `Player`. |
| `Assets/Scripts/DB/SO/PlayerUnitSO.cs` | SO for `Assets/Excel/PlayerUnitSO.xlsx` → `Assets/Resources/DB/PlayerUnitSO.asset`. Loads card sprites at runtime in `SetData`. |
| `Assets/Scripts/DB/SO/SynergyEffectSO.cs`, `Assets/Scripts/DB/Data/Synergy/SynergyData.cs` | Deck-synergy thresholds and texts (`SynergyGrade` Bronze/Gold/Prism). |
| `Assets/Scripts/Manager/PlayerDataManager.cs` (region `Deck`, `저장 관련`) | `DeckData`, `DeckPresets`, `ActiveDeckIndex`, `OwnedCardData`, `UnLockUnit`, `AppliedDeckUnitSynergies`, deck save/load converters. |
| `Assets/Scripts/UI/CardDeckSystem/DeckPresetController.cs` | Root of the deck screen (`Resources/Prefabs/UI/DeckPresetController`). Tabs, slots, validation, auto-fill, reset, rename, "complete" → battle. |
| `Assets/Scripts/UI/CardDeckSystem/` (rest) | `DeckTabController` (5 tabs), `DeckUnitSlot`, `DeckNameEditPanel`, `ConfirmationPopup` (empty-slot confirm), `UIDeckSynergy` (+ `SynergyIconSlot`, `UISynegyToolTipPanel`), card widgets `UIUnitSynergeIconArea`/`UICardSynergyIcon`/`UISynergyIconPressHandler`/`UICardSynergyExpanationPopup`/`UIRarityIconArea`, `UIUnitCardDetailPanel`, `UIPlayerStatPopup`, `UIDisplayStage` (selected stage label). `BmResourcePanel.cs` declares `ResourcePanelUI` (currency bar used in `Assets/Prefabs/UI/GachUI.prefab`, not deck logic). |
| `Assets/Scripts/UI/UnitCardSelect/` | Unit picker popup `UIUnitCardSelect` + `CardFilter` (filter/sort/search), `UIUnitCardSlot` (grid cell), `UIUnitCardInScroll` (full card view, reused by the deck hold-preview and battle `UIUnitexplanationPopup`), `SearchPanelInUnitCard`, `UIDropDownInUnitCard`, `UnitSelectBtnInUnitCard`. Dead: `InfiniteScroll`, `UIUnitCardDetailPopup`, `FilterBtnInUnitCard`. |
| `Assets/Scripts/UI/UnitUpgrade/UnitUpgradeService.cs` | Card-merge upgrade service. **No callers** (dead). |
| `Assets/Scripts/UI/MainScreen/DeckSelectPopup.cs` | Fade popup inside `MainScreenUI`; no code opens it (vestigial). |
| `Assets/Scripts/UI/SelectCard(Old)/` | Legacy picker. Only referenced by `Assets/Prefabs/UI/UISelectCard.prefab`, which only `Assets/Z_ForTest/ZZGyeongpyoTest/GyeongpyoTestScene.unity` uses. No code references. |
| `Assets/Scripts/Manager/UnitRenderManager.cs`, `Assets/Scripts/Base/Framework/UnitTextureHandler.cs` | Battle-scene live unit portraits (RenderTexture per deck unit) for spawn buttons. |
| `Assets/Scripts/Base/Framework/CaptureModelSystem/SpriteCapture.cs` | Editor-only tool (button in `Assets/Editor/SpriteCaptureEditor.cs`, scene `Assets/Scenes/Z_CaptureSpriteScene.unity`) that writes PNGs to `Assets/Resources/UnitIcon` — the source of `unitIconSprite`. |

## How it works

**Unit data model** (`BaseUnitData`, key `idNumber` from `MonoData`):
- `rarity` (`common`, `rare`, `epic`, `Legendary`), `unitType` (Tanker/Dealer/Healer — the "role"
  filter), `unitClass` (Normal/Hero/Boss), `synergyType` (bitmask: factions Kingdom/Empire, classes
  Mage/Cleric/Berserker/Archer/Hero, elements Frost/Burn/Poison; a unit can carry several).
- Stats: `health`, `atkPower`, `attackRate`, `attackDelayTime`, `attackRange`, `cognizanceRange`,
  `moveSpeed`, `hitBack`, `attackType`, `isRangedAttack`, `healAmount`, `maxTargetCount`,
  `spawnCooldown`, `cost` (food per spawn). `poolType` links the row to the pooled prefab
  `Resources/Prefabs/ObjPooling/<PoolType>`.
- Sheets in `PlayerUnitSO`: `hero_unit` (5 rows, 150xxx, `Legendary`, `UnitClass.Hero`),
  `hiller_unit` (5 rows duplicating ids from other sheets; its `SetData` loop is commented out, so
  ignored), `allianceCommon` (100xxx/105xxx), `allianceRare` (110xxx/115xxx), `allianceEpic`
  (120xxx/125xxx), `spawnedUnit` (180001 golem, summoned by an artifact). Id bands are observed in
  the data, not enforced.
- `PlayerUnitSO.SetData` registers every row under **both** `(int)poolType` and `idNumber` in one
  dictionary (comment: pool-type count stays below the minimum id). `PlayerUnit.SetDataFromExcelData`
  looks its data up by `(int)poolType` parsed from the GameObject name; deck/UI code looks up by id.
- For common/rare/epic rows `SetData` assigns `unitBGSprite` (`SetBgFromSynergy`: first faction,
  else first class, else first element slice from `Resources/Synergy/Synergy_Background1|2`),
  `unitIconSprite` (`Resources/UnitIcon/<poolType>`), `gachaHeroSprite` (`Resources/GachaHero/<poolType>`).
  Hero rows only get `firstWaveSprite`/`spawnSprite` from their path columns.

**Ownership (roster).** `PlayerDataManager.CardGenerate(ids)` builds the private `AllCardData`
from `allianceCommon`+`allianceRare`+`allianceEpic` only (heroes and golem are never ownable), then
puts each id into `OwnedCardData` (id → the shared SO row) with `ownedCount = 1`. Called from
`LoadDataFromCloundAsync`: new player (no save) or corrupt save → ids 100001–100010; otherwise
the saved `OwnedCardData` list, then `CardCounts` overwrites `ownedCount`. Gacha calls
`UnLockUnit(id)`: new id → add with count 1; duplicate → `ownedCount++`. Both invoke the C# event
`OnUnitCountChanged` (no subscribers today).

**Deck presets.** `DeckData` = `DeckName` + two parallel 8-length lists: `UnitIds` (-1 = empty,
this is what is saved) and `BaseUnitDatas` (null = empty, this is what everything reads).
`PlayerDataManager.LoadDecks` (in `Awake`) creates keys 1..5 named "덱 1".."덱 5".
`ActiveDeckIndex` (default 1) is the selected preset and is what battle uses.

**Deck screen flow** (`DeckPresetController`, a `BaseUI` living in the main scene):
- Opened by `FadeManager.SwitchGameObjects` from `MainScreenUI.OnDeckSelectButtonClick` (when
  `GameManager.IsStageAndDestinySelected`), `UIMenu`, `UIDestinyRoullette` (after applying destiny),
  `UIArtifact` (back button), and `SceneLoaderMain` with `LoadMain.DeckPresetController` (retry after
  defeat). `GoToMainScene` returns to `UIMenu` or `MainScreenUI` by `UIManager.fromUI`.
- `Start` → `SelectDeck(ActiveDeckIndex)`; tab click → `SelectDeck(i)` sets `ActiveDeckIndex`
  immediately → `UpdateUnitSlotsUI` → `UpdateCompleteButtonState`, `ValidateDeckAndUpdateUI`,
  `UpdateSynergyUI`.
- Slot tap → `UIUnitCardSelect.OpenUI()` + `SetDeckSlotNum`; picking a card calls
  `DeckPresetController.OnUnitSelected(slot, id)` (via `UIManager.GetUI<DeckPresetController>()`),
  which writes both lists. Slot hold → `UIUnitCardInScroll.UpdateCardDataByData` preview.
- **Limits** (`ValidateDeckAndUpdateUI`): commons unlimited; rare/epic capped by
  `PlayerDataManager.RareUnitSlots`/`EpicUnitSlots`, which `UpdateAllBuildingEffects` computes from
  buildings (see the territory doc). Over-cap slots get an overlay, the complete button is disabled
  and `LaterUpdatePopup.Show` explains. Re-run on `SynergyDataUpdatedEvent`. No duplicates: the
  picker hides units already in the active deck.
- `OnAutoFormClicked`: fills empty slots with shuffled owned units not in the deck, epic → rare →
  common while caps allow. `OnResetClicked` clears both lists. Neither saves.
- Rename (`EnterEditMode`/`OnConfirmNameChange`) saves immediately.
- `OnCompleteClicked` → if any slot is -1 and `PlayerPrefs` `DontAskAgain_EmptyDeck` ≠ 1, shows
  `ConfirmationPopup` (its toggle sets that pref) → `CompleteFormationDirect`:
  `await PlayerDataManager.SaveDataToCloudAsync()` then `SceneLoader.StartLoadScene(BattleScene)`.
- Destiny/challenge: `UIRandomAndChallenge` (a child component) opens the destiny/challenge popups
  when `PlayerDataManager.currentDestiny`/`activeChallenges` are set; covered by the stage-modifier doc.

**Picker** (`UIUnitCardSelect` + `CardFilter`, a `BasePopUpUI` nested inside the
`DeckPresetController` prefab, not loaded by name). `OnEnable` → `CardFilter.UpdateUsable()`
(owned ids minus active-deck ids; ids whose rarity cap is reached go into `greyCardSet`) →
`FilterAndSort()` (LINQ: role filter, `unitName.Contains(search)`, sort by rarity/cost/health/
atkPower/spawnCooldown, then `idNumber`, asc/desc) → `OnFilterUpdated` → `RefreshGrid`, which
reuses/instantiates `UIUnitCardSlot` from a serialized prefab (`Assets/Prefabs/UI/DeckPreset/UIUnitCardSlot.prefab`).
This is a plain grid; it is **not** an infinite scroll. Rarity counters show `n/max`.

**Deck-unit synergy.** `UIDeckSynergy.CheckDeckUnitSynergy(deck)` counts, per flag, the deck units
carrying it; thresholds come from `DataManager.SynergyEffectData.GetData(type*1000 + grade)`
(`requiredUnitCount`; e.g. Kingdom 2/4/6, Berserker/Archer 3/5 with no Prism). `UpdateSynergyUI`
clears and refills `PlayerDataManager.AppliedDeckUnitSynergies` (type → highest met grade) and
shows badge icons (sprites `Resources/Synergy/<Type>`, slice index = grade). Tapping a badge opens
`UISynegyToolTipPanel`; pressing a card's synergy icon publishes `UISynergyIconPressedEvent`
(subscribers: `UICardSynergyExpanationPopup`, `UISynergyExplanationForGuide`).
**The grade has no gameplay effect in code**: the only readers of `AppliedDeckUnitSynergies` are
`UIDeckSynergyForBattleScene` (battle badges) and the two writers. Per-unit flags do matter:
Hero flag (no spawn-count upgrade in `PlayerHQ.SpawnUnit`; 75% cooldown refund on death via
`HeroUnitDeadEvent`), Kingdom/Empire (destiny/challenge nation conditions in `Modifiercalculator`,
`DestinyChallengeCalculator`), Burn/Frost/Poison/Archer/Mage (attack SFX only). The `Synergy*`
stat bonuses applied in `PlayerUnit.SetStatMultiplier` are the **territory** building synergies,
not deck synergies.

**What battle reads.** Everything reads `DeckPresets[ActiveDeckIndex].BaseUnitDatas` (never
`UnitIds`): `UIPlayerUnitSpawnPanel.Awake` (one `UISpawnUnitSlot` per slot, null = disabled),
`PlayerHQ.SetUnitDataFromCardDatd` (rarity per pool type; spawn-count upgrade every 8 commons /
4 rares), `UnitRenderManager.Awake` (one `UnitTextureHandler` per unit: pooled unit + camera on
layer "Animation" into a 512² `RenderTexture`, parked at y = -50; `UISpawnUnitSlot` fetches it
through the static `GetUnitTextureHandlerByPoolType`, which also auto-creates the scene-scoped
manager), `Modifiercalculator`/`DestinyChallengeCalculator` (deck composition). Heroes are not in
the deck: `UITimer` picks a random `hero_unit` row per battle. Tutorial: `UIPlayerUnitSpawnPanel`
forces slots 0–3 to 100001–100004 and recomputes `AppliedDeckUnitSynergies` itself.

**Upgrade** (`UnitUpgradeService`, unused): `CanUpgradeCard` needs `ownedCount >= 3`
(`UpgradeRequiredCount`, same for every unit); `UpgradeCard` subtracts 3, calls
`CardCountChanged`, saves. It raises no level/stat and charges no currency (TODO comments).

## Data and persistence
- Static: `Assets/Excel/PlayerUnitSO.xlsx` → `PlayerUnitSO` → `DataManager.PlayerUnitData`;
  `Assets/Excel/SynergyEffectSO.xlsx` (sheet `Sheet1`) → `SynergyEffectSO` (key `type*1000+grade`)
  → `DataManager.SynergyEffectData`; `Assets/Excel/PlayerSO.xlsx` → `PlayerSO` → `DataManager.PlayerData`.
  `DataManager.SynergyIconSprites` and `UnitTypeIconSprites` (`Resources/Icon/Position`) are
  preloaded in `DataManager.Awake`.
- Player state, all inside the single Cloud Save key `Constants.PLAYER_DATA_KEY` ("PLAYER_DATA") as
  `PlayerSaveData` (details in [player-data.md](player-data.md)): `DeckPresets`
  (`Dictionary<int, List<int>>` from `UnitIds`), `DeckNames`, `ActiveDeckIndex`, `OwnedCardData`
  (id list), `CardCounts` (id → `ownedCount`). **`curLevel` is not saved.** Load rebuilds
  `BaseUnitDatas` from ids in `ConvertIntToDeck`.
- Saved only through `PlayerDataManager.SaveDataToCloudAsync` (no-op until
  `GameManager.IsTutorialCompleted`): deck complete, deck rename, app pause, and other systems'
  saves. Slot edits, auto-fill, reset, tab switch and gacha unlocks are memory-only until then.
- `PlayerPrefs`: `DontAskAgain_EmptyDeck`.

## Common changes
- **Add a unit:** 1. Add a `PoolType` value and a prefab `Resources/Prefabs/ObjPooling/<PoolType>`
  (architecture.md). 2. Add a row to the right `PlayerUnitSO.xlsx` sheet (id unique across all
  sheets and larger than the `PoolType` count) and reimport. 3. Add `Resources/UnitIcon/<PoolType>`
  (capture with `SpriteCapture`) and `Resources/GachaHero/<PoolType>`. 4. It becomes ownable only if
  it is in `allianceCommon/Rare/Epic` and is granted (gacha or the starter list in
  `LoadDataFromCloundAsync`).
- **Add a synergy type:** add a `UnitSynergyType` bit, rows for each grade in `SynergyEffectSO.xlsx`,
  a sliced sprite `Resources/Synergy/<Type>` (one slice per grade, Bronze first), `SynergyIcon`
  values plus both maps in `UIDeckSynergy` and in `UIDeckSynergyForBattleScene`, and the background
  slice in `Synergy_Background1`/`2` if `SetBgFromSynergy` should pick it. Implement any gameplay
  effect yourself — none exists.
- **Change deck count/size:** `LoadDecks` (5) and `DeckData` ctor (8) plus the tab list in the
  prefab and `UIPlayerUnitSpawnPanel.spawnUnitSlotList`/`DeckPresetController.unitSlots` (indexed
  in lockstep with the 8 slots). Old saves with fewer entries load fine (`ConvertIntToDeck` bounds).
- **Change a deck from code:** write both `UnitIds[i]` and `BaseUnitDatas[i]`, then call
  `SaveDataToCloudAsync` if it must persist.

## Gotchas
- **Runtime state in static rows.** `BaseUnitData.ownedCount`/`curLevel` (inherited by
  `HeroData` and `PlayerData`) and the sprites assigned in `PlayerUnitSO.SetData` are serialized
  fields of SO rows. `OwnedCardData` values *are* the `PlayerUnitSO` row objects, so gacha/upgrade
  mutate the SO in memory; in the Editor this survives leaving Play mode and could be written to
  `PlayerUnitSO.asset` if the asset is saved. `Assets/Resources/DB/PlayerUnitSO.asset` already
  stores `ownedCount: 0`/`curLevel: 0` on all 52 rows; `Assets/Resources/DB/PlayerSO.asset` has
  neither field on its 30 rows, so the next reimport/reserialize of `PlayerSO` adds both lines to
  every row as a no-op diff. `curLevel` is unused by any code (artifact code uses its own
  `ActiveArtifactData.curLevel`).
- `DataManager.PlayerUnitData.Values`/`Count` contain every row twice (pool-type and id keys).
  Iterate the SO lists instead (as `UIGuide` does).
- `UnitIds` and `BaseUnitDatas` can diverge. Suspected issue: the tutorial writes only
  `BaseUnitDatas` (`UIPlayerUnitSpawnPanel.Awake`, `DeckPresetController.Awake`), so after the
  tutorial the active deck may display units while `UnitIds` (saved, used by `CardFilter`,
  auto-fill and the empty-slot check) still hold -1.
- Suspected issue: in `UpdateUnitSlotsUI`, `ValidateDeckAndUpdateUI` runs after
  `UpdateCompleteButtonState` and overwrites `completeButton.interactable`, so the "empty deck"
  disable never sticks; a fully empty deck can start a battle after the confirm popup.
- `AppliedDeckUnitSynergies` is a side effect of deck UI refreshes, with the counting logic
  duplicated in `UIDeckSynergy` and `UIPlayerUnitSpawnPanel`. `OnEnable` does not refresh it.
- `UIDeckSynergy.Init` maps sprite slice `i` to `_iconMap[(type, i)]`; a sprite sheet with more
  slices than the map (e.g. a 3rd slice for Berserker/Archer) throws `KeyNotFoundException`.
- `CardFilter.UpdateUsable` indexes `OwnedCardData[id]` for every deck id: an unowned id in the
  deck throws. `UIUnitCardSelect.RefreshGrid` uses `return` (not `continue`) on an unowned id.
- `UICardSynergyExpanationPopup` subscribes in `Init` (from `UIUnitCardSelect.Awake`) and
  unsubscribes in `OnDestroy` (breaks rule C5); since the guide publishes the same
  `UISynergyIconPressedEvent`, it appears to react to guide presses too.
- `hero_unit` rows get no `unitBGSprite`/`unitIconSprite`; don't show heroes with card widgets.
- `UIUnitCardDetailPopup.Show` returns when `_display` is set (inverted check) — dead anyway.
- `InfiniteScroll` is only in the orphan `Assets/Prefabs/UI/UIUnitCardSelect.prefab` (nothing
  references it; the live picker is embedded in `DeckPresetController.prefab`), and its `InitRef`
  is never called.
- `BmResourcePanel.cs` declares class `ResourcePanelUI`: file/class name mismatch.
