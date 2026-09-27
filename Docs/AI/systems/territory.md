# Territory (buildings, tiles, building synergies)

Parent: [`../architecture.md`](../architecture.md). The main-screen 5x5 territory grid where the
player builds, upgrades, moves, repairs and destroys Farms, Lumber Mills, Mines and Barracks, whose
effects and placement synergies change battle food, unit spawn cooldown, unit stats, deck rarity
slots, stage-clear resource rewards and upgrade costs.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/UI/MainScreen/Building/Synergy/TileDataHandler.cs` | Plain C# class owned by `PlayerDataManager._TileDataHandler`. Holds the grids, detects synergies, sums building effects, damage/repair turns, snapshot for save. Also declares `TileDataSnapshot`. (Lives in `Synergy/` despite covering all tile state.) |
| `Assets/Scripts/UI/MainScreen/Building/Synergy/BuildingSynergySystem.cs` | `BuildingSynergyType` enum and `DetectedSynergy` (type + tile positions). No logic. |
| `Assets/Scripts/Manager/PlayerDataManager.cs` | Regions "영지 시너지 보너스", "시너지 로직", "Building", "Food", "Building Effects": synergy effect values, aggregated bonuses, build list, invested-cost/refund, destroy, food. Declares `TileStatus` and `SynergyDataUpdatedEvent`. |
| `Assets/Scripts/UI/MainScreen/Building/MainScreenBuildingController.cs` | Scene-side controller (own static `Instance`, not a `SingletonMono`). Creates the 25 tiles, routes clicks, and runs build/upgrade/repair/ad-repair/destroy/drag-move. |
| `Assets/Scripts/UI/MainScreen/Building/BuildingTile.cs` | One tile view. Declares `TileType` (Normal/Special). Click and drag handlers forward to the controller. Visuals: sprite, level stars, damage/repair tint, cooldown timer. |
| `Assets/Scripts/UI/MainScreen/Building/ConstructionSelectPanel.cs`, `Assets/Scripts/UI/MainScreen/Building/BuildingSelectItem.cs` | Popup listing the buildable level-0 blueprints. Picking one opens the upgrade panel in construction mode. |
| `Assets/Scripts/UI/MainScreen/Building/ConstructionUpgradePanel.cs` | Popup with modes Construction / Upgrade / Repair: shows cost and effects, action button, and a "destroy" button that becomes "광고 수리" (ad repair) in repair mode. |
| `Assets/Scripts/UI/MainScreen/Building/DestoryConfirmPopup.cs` | Class `DestroyConfirmPopup` (file name misspelled): shows the 50% refund and confirms. |
| `Assets/Scripts/UI/MainScreen/Building/AdCooldownPopup.cs` | "Watch an ad to move during cooldown?" popup. |
| `Assets/Scripts/UI/MainScreen/Building/Synergy/BuildingSynergyPanel.cs`, `Assets/Scripts/UI/MainScreen/Building/Synergy/SynergyInfoItem.cs` | Side list of per-type effect totals and active synergies. Items are pooled (`PoolType.SynergyInfoItem`, `Assets/Resources/Prefabs/ObjPooling/SynergyInfoItem.prefab`). Synergy titles/descriptions are hard-coded in `GetSynergyUIData`. |
| `Assets/Scripts/UI/MainScreen/Building/Synergy/SynergyOuitlineManager.cs` | Class `SynergyOutlineManager` (file name differs). Draws a colored rectangle around each active synergy (blue adjacency, green line, red block). |
| `Assets/Scripts/UI/MainScreen/Building/GridStateChangedEvent.cs` | One line with no trailing newline (so `wc -l` says 0): `public struct GridStateChangedEvent { }`. It is a real, used event. |
| `Assets/Scripts/UI/MainScreen/LordManorPanel.cs` | Special tile (4,0): buttons that switch the main screen to `UIArtifact` / `UIArtifactUpgrade`. |
| `Assets/Scripts/UI/MainScreen/DiplomacyPanel.cs` | Special tile (4,1): "Adventurer guild" switches to `GachaUIPanel`; the other three buttons show a "coming later" popup. |
| `Assets/Scripts/DB/Data/BuildingUpgradeData.cs`, `Assets/Scripts/DB/SO/BuildingUpgradeSO.cs` | Static table row (`BuildingType`, `BuildingEffectType`, `Cost`, `BuildingEffect` enums/classes are declared here) and its SO. |
| `Assets/Scripts/DB/Data/BuildingInstanceData.cs` | Dead: never referenced. |

All territory UI lives inside `Assets/Resources/Prefabs/UI/MainScreenUI.prefab` (`MainScreenUI`).
The panels and popups are `[SerializeField]` references on the controller, opened with their own
`OpenUI()`, not through `UIManager.OpenUI<T>()`. `MainScreenUIOld.prefab` also references these
scripts but no class loads it.

## How it works

**Grid model.** `TileDataHandler` holds four parallel `[5,5]` arrays indexed `[x, y]`:
`BuildingGridData` (a reference to the current level's `BuildingUpgradeData` row, or null),
`TileStatusGrid` (`TileStatus.Normal/Damaged/Repairing`), `TileRepairTurnsGrid`, and
`CooldownEndTimeGrid` (UTC `DateTime`). Tiles are created row by row (y outer), so x is the column
and y the row. The 4x4 block `x<4 && y<4` holds normal build tiles. The right column `x==4` and
the bottom row `y==4` are `TileType.Special`: (4,0) Lord Manor → `LordManorPanel`; (4,1) Diplomacy →
`DiplomacyPanel`, interactable only once `PlayerDataManager.IsStageCleared(1, 2)` is true (re-checked
on `ClearedStagesUpdatedEvent`); (4,2) military → shows `loginconfirmPopup` as a "coming later" popup.
The other special tiles are gray and not clickable.

**Building types and levels.** `BuildingType` is Farm/LumberMill/Mine/Barracks. Each type has 10
rows in `BuildingUpgradeSO` (ids 101–110, 201–210, 301–310, 401–410). Level 0 (x01) is the
construction blueprint, and `PlayerDataManager.GetBuildableList()` returns the `level == 0` rows.
Levels 1–9 are placed buildings. `nextLevel` links the chain, and level 9 has `nextLevel = -1`.
A row's `costs` are the price of moving *from* that row to `nextLevel`, so the level-0 row holds
the build price (Gold 100 in the current asset), and the level-9 row has no costs. Costs use Gold,
Wood, Iron and MagicStone. Effects per type in the asset: Farm `MaximumFood` and
`IncreaseFoodGainSpeed`; LumberMill `BaseWoodProduction` and `AdditionalWoodProduction`; Mine
`BaseIronProduction`, `AdditionalIronProduction`, `MagicStoneFindChance` and
`MagicStoneProduction`; Barracks `UnitCoolDown`, `CanSummonRareUnits` and `CanSummonEpicUnits`.
Level stars on the tile: 1–4 one star, 5–8 two, 9 three.

**Click routing** (`MainScreenBuildingController.HandleTileClick`). On a normal tile: an empty
Normal tile opens `ConstructionSelectPanel`; a Normal tile with a building opens
`ConstructionUpgradePanel.InitializeForUpgrade`; a Damaged tile with a building opens
`InitializeForRepair`; a Damaged empty tile or a Repairing tile shows `laterUpdatePopup` with the
remaining turns. While any popup is open the synergy panel is hidden, and `DeselectTile()` shows it
again.

**Build** (`BuildBuildingOnTile(tile, level0Id)`): checks every cost, then calls
`PlayerDataManager.AddResource(type, -amount)` once per cost (each one is an Economy call). It sets
`BuildingGridData` to the level-1 row, publishes `GridStateChangedEvent`, plays `buildingSE`, and
calls `SaveDataToCloudAsync()`. Build costs get **no** synergy discount.

**Upgrade** (`UpgradeBuildingOnTile`): the price is `current.costs`, with Wood/Iron/MagicStone reduced
by `SynergyWoodCostReduction` / `SynergyIronCostReduction` / `SynergyMagicStoneCostReduction` %
(`CeilToInt`). It deducts, swaps in the `nextLevel` row, publishes `GridStateChangedEvent`, and
saves. The same discount math is duplicated in `ConstructionUpgradePanel.UpdateCostText` for display.

**Destroy**: `InitiateDestruction` computes the refunds and opens `DestroyConfirmPopup`; then
`ConfirmDestruction` → `PlayerDataManager.DestroyBuildingAt(x,y)`. The refund is
`FloorToInt(50%)` of `CalculateTotalInvestedCost` (the level-0 row's costs, plus the costs of every
row from level 1 up to the current level minus one, at list price with no synergy discount). It
refunds through `AddResource`, clears the tile and its cooldown, and saves. It does **not** publish
`GridStateChangedEvent`; the controller calls `UpdateAllSynergyEffects()` and
`synergyPanel.UpdateDisplay()` directly. You can't destroy a damaged building (repair mode reuses
the destroy button for the ad).

**Damage and repair (defeat penalty).** `PlayerDataManager.OnBattleEnded(BattleEndedEvent)` runs on
every battle end (`GameManager.ShowResultUI` publishes it). It calls
`TileDataHandler.AdvanceRepairTurn()`, and on a defeat after the tutorial also calls
`DamageRandomTile()`. `DamageRandomTile` picks a random Normal tile in the 4x4 block, sets it to
`Damaged` with 3 turns, and publishes `GridStateChangedEvent`. A damaged empty tile ("황폐화", wasted)
counts down one turn per battle and returns to Normal on its own. A damaged building ("반파", half
destroyed) never counts down until it is repaired:
- `RepairBuildingOnTile` costs 50% (`CeilToInt`) of the costs of the row whose `nextLevel` is the
  current row, which means the price that was paid to reach this level. It sets `Repairing`, and
  the existing turn counter then counts down per battle.
- `ConstructionUpgradePanel.OnAdRepairButtonClicked` → `AdManager.ShowRewardedAd(callback)` →
  `RepairBuildingWithAd` sets Normal and 0 turns immediately.
Both call `UpdateAllBuildingEffects()` and save. `GameManager.ShowResultUI` saves after the battle.
A non-Normal tile is ignored by `CalculateTotalBuildingEffects` and by synergy detection
(`GetBuildingTypeAt` returns None).

**Move / swap with cooldown (the ad skip).** Drag starts only from a Normal tile with a building
(`BuildingTile.OnBeginDrag`). The drop target must be a normal tile in Normal status
(`HandleDrop`). If neither tile is on cooldown, `PerformMoveOrSwap(dest, true)` calls
`StartCooldownForBuildingAt` for the moved building(s): cooldown = `level * 3` minutes, stored in
`CooldownEndTimeGrid`. Then it calls `MoveBuildingData` or `SwapBuildingData` (the cooldowns travel
with the buildings, and both publish `GridStateChangedEvent`), recomputes the synergies, refreshes
the tiles, and saves with `.Forget()`. If either tile is on cooldown, `AdCooldownPopup` opens →
`ConfirmAdAndMove` → `AdManager.ShowRewardedAd` → on success `ReduceCooldownForBuildingAt(…, 30)`
on the source (and the destination if it has a building), then `PerformMoveOrSwap(dest, false)`.
The maximum cooldown is 27 min (level 9), so 30 min is effectively a full skip. The cooldown only
blocks moving. It does not block upgrading, repairing or destroying. The ad flow is client-only and
doesn't use `BackendManager.WatchAdAndGetReward`.

**Synergy detection** (`TileDataHandler.DetectAllSynergies`). This covers only the 4x4 block with
Normal tiles. Each tile is used by at most one synergy, and the priority order is:
1. Line: 4 of the same type across a full row, then a full column → `Farm_Line`,
   `LumberMill_Line`, `Mine_Line` or `Barracks_Line`.
2. 2x2 block (scanned y then x): all four the same type → `Specialized_Block`; all four types
   different → `Balanced_Block`.
3. Adjacency pairs (scan y then x, right neighbour first, then down): an unordered pair of
   *different* types → one of 6 (`Farm_Barracks`, `Barracks_Mine`, `Barracks_LumberMill`,
   `Mine_LumberMill`, `Farm_Mine`, `Farm_LumberMill`). A same-type pair gives nothing.

**Synergy effects** (`PlayerDataManager.ApplySynergyEffect`; the numbers are hard-coded, with no
table). They stack per instance:

| Synergy | Effect (field) | Consumed by |
|---|---|---|
| Farm_Barracks | +2.5 `SynergyUnitCooldownReduction`, −2.5 `SynergyFoodProductionBonus` | spawn cooldown; battle food rate |
| Barracks_Mine | +1.5 `SynergyAllUnitAttackBonus` (%) | `PlayerUnit.SetStatMultiplier` |
| Barracks_LumberMill | +1.5 `SynergyAllUnitHealthBonus` (%) | `PlayerUnit.SetStatMultiplier` |
| Mine_LumberMill | +2.5 tile efficiency on both tiles | see efficiency below |
| Farm_Mine, Farm_LumberMill | +2.5 tile efficiency on the Farm tile only | see efficiency below |
| Farm_Line | +5 `SynergyMaxFoodBonus`, +2.5 `SynergyFoodProductionBonus` | battle max food / rate |
| LumberMill_Line | +5 `SynergyWoodCostReduction` | upgrade cost |
| Mine_Line | +5 `SynergyIronCostReduction`, +2.5 `SynergyMagicStoneCostReduction` | upgrade cost |
| Barracks_Line | +10 `SynergyUnitAttackCooldownReduction` | `PlayerUnit` `AttackRate` |
| Specialized_Block | +10 `SynergyBlockBonusPercent` (global, not per block) | building max-food bonus multiplier and food rate only |
| Balanced_Block | +5 tile efficiency on all 4 tiles | see efficiency below |

"Tile efficiency" (`TileEfficiencyBonuses[(x,y)]`, %) is applied per building effect. A flat
effect is multiplied by `1 + b/100` (`MaximumFood`, `Base*Production`, `MagicStoneProduction`). A
percent effect has `b` *added* to it (`IncreaseFoodGainSpeed`, `UnitCoolDown`,
`AdditionalWoodProduction`).

**Aggregation** (`PlayerDataManager.UpdateAllSynergyEffects` → `ResetSynergyBonuses`, detect,
apply, `UpdateAllBuildingEffects`, then publish `SynergyDataUpdatedEvent`).
`TileDataHandler.CalculateTotalBuildingEffects` sums the Farm `MaximumFood` / food % and the Barracks
`UnitCoolDown` over Normal tiles. Rare and epic slots come **only from the highest-level Barracks**
(they don't stack). The results are:
- `CalculatedMaxFood = Ceil((20000 + Ceil(farmMaxFood * (1 + Block/100))) * (1 + SynergyMaxFoodBonus/100))`.
- The private `currentFarmGainPercent` = the buildings' food % + `SynergyFoodProductionBonus` +
  `SynergyBlockBonusPercent`.
- `TotalUnitCooldownReduction` = the Barracks % + `SynergyUnitCooldownReduction`.
- `RareUnitSlots` and `EpicUnitSlots`.
It runs on each `GridStateChangedEvent` (the subscription is in `PlayerDataManager.OnEnable`), and
`MainScreenBuildingController.Co_InitializeMainScreen` forces it once when the scene opens.

**Where the outputs are consumed.**
- `GameManager.Update` → `AddFoodOverTime`. The battle food rate is
  `baseFoodGainBySupplyLevel[SupplyLevel-1] * (1 + currentFarmGainPercent/100)`.
- `GameManager.StartBattle` → `ResetFood()`. `MaxFood` starts at `CalculatedMaxFood` and is a
  shrinking budget: every food gained, and every supply upgrade, is taken out of it.
- `SupplyUI` shows the food and the supply level.
- `UISpawnUnitSlot` reads `TotalUnitCooldownReduction` for the spawn cooldown.
- `PlayerUnit.SetStatMultiplier` reads the three unit-stat synergies.
- `DeckPresetController`, `UIUnitCardSelect` and `CardFilter` cap rare/epic cards per deck with
  `RareUnitSlots` / `EpicUnitSlots`. The first two re-check on `SynergyDataUpdatedEvent`.
- Stage-clear rewards: on a victory, `GameManager.ShowResultUI` reads the building production
  effects straight from `BuildingGridData` over the 4x4 block, plus `TileEfficiencyBonuses`:
  - wood = table reward + `Ceil(baseWood * (1 + (addWood% + challenge%)/100))`, and the same for
    iron;
  - magic stone: a `MagicStoneFindChance` % roll adds `Random(min..max)` of the summed
    `MagicStoneProduction`.
  There is no idle or timed production outside battles.
- `SynergyOutlineManager` and `BuildingSynergyPanel` subscribe to or read `ActiveSynergies` for
  display.

## Data and persistence
- **Static**: `Assets/Excel/BuildingUpgradeSO.xlsx` → `Assets/Resources/DB/BuildingUpgradeSO.asset`
  (sheet `BuildingUpgrade`), read through `DataManager.Instance.BuildingUpgradeData`. This SO must
  be imported by hand: see the `BuildingUpgradeSO` exception in
  [architecture.md → Static game data](../architecture.md#static-game-data-excel--scriptableobject).
  `buildingSprite` is a `Sprite` column, so the sprites live in the SO asset. There is no table for
  building synergies. `SynergyData` and `SynergyEffectSO` are the **deck/unit** synergy table
  (`UnitSynergyType`), not this system.
- **Player state**: `PlayerSaveData.TileGridData` (`TileDataSnapshot`: `int[,]` building ids with 0
  for empty, `TileStatus[,]`, repair turns, `long[,]` cooldowns via `DateTime.ToBinary`) inside the
  Cloud Save key `Constants.PLAYER_DATA_KEY` ("PLAYER_DATA"). `TileDataHandler.GetSnapshot` builds
  it in `PlayerDataManager.InternalSaveDataToCloudAsync`, and `RestoreFromSnapshot` restores it in
  `LoadDataFromCloundAsync` (which publishes `GridStateChangedEvent` → recompute). The synergy and
  effect values are derived and never saved. Food and `SupplyLevel` are local to a battle and never
  saved. Resources are Economy currencies spent through `AddResource`.
- **When saved**: after every build, upgrade, repair, ad repair, destroy and move, on app pause, and
  at the end of `ShowResultUI`. `SaveDataToCloudAsync` is a no-op until `GameManager.IsTutorialCompleted`.
- `GameManager` also sends the snapshot as JSON in the stage-result Analytics event.

## Common changes
- **To tune building stats or costs:** edit `BuildingUpgradeSO.xlsx`, then import it into the SO by
  hand (see the architecture link). Keep the chain rules: exactly one level-0 row per type,
  `nextLevel` links, `-1` on the last row, and `costs` on the row you upgrade *from*.
- **To add a building effect:**
  1. Add a `BuildingEffectType` value **at the end** (the asset stores it as an int).
  2. Consume it: for a food or Barracks effect in `TileDataHandler.CalculateTotalBuildingEffects`
     plus a new `out` and a `PlayerDataManager` property; for a stage reward in the
     `GameManager.ShowResultUI` loop.
  3. Add labels to both `EffectNames` dictionaries (`ConstructionUpgradePanel`,
     `BuildingSynergyPanel`), and to both `GetEffectValueString` if it is a percent.
- **To add or retune a building synergy:**
  1. Add a `BuildingSynergyType` value.
  2. Detect it in `TileDataHandler` (the `Get*SynergyType` mappers or a new pass; order = priority).
  3. Apply it in `PlayerDataManager.ApplySynergyEffect`, and reset any new field in
     `ResetSynergyBonuses`.
  4. Add a title, icons and text in `BuildingSynergyPanel.GetSynergyUIData`, a color in
     `SynergyOutlineManager.GetColorForSynergyType`, and the icon layout in
     `SynergyInfoItem.Initialize`. The UI text is not generated from the numbers, so keep them in
     sync by hand.
- **To add a building type:** add it to `BuildingType` (at the end), add 10 table rows, and then
  update everything that hard-codes the four types: the line and adjacency mappers, the
  `Balanced_Block` superset check, `ApplySynergyEffect`, `GetSynergyUIData`, and
  `CalculateTotalBuildingEffects` (which checks the type per effect).
- **To change the grid size:** `5`/`4` are literals in `TileDataHandler`, `TileDataSnapshot`,
  `MainScreenBuildingController`, `BuildingTile.Initialize`, `SynergyOutlineManager` and the
  `GameManager` reward loop. Old saves hold `[5,5]` arrays.

## Gotchas
- The special-tile coordinates are literals in several places: (4,0), (4,1) and (4,2) in
  `HandleTileClick` / `BuildingTile.Initialize`, and `x == 4 || y == 4` in `DamageRandomTile`.
- `GridStateChangedEvent` works as a "please recompute" request, published by UI
  (`MainScreenBuildingController`) and by `TileDataHandler`, and handled by `PlayerDataManager`.
  Several paths also call `UpdateAllSynergyEffects()` directly after a publish, so the recompute runs
  twice (move/swap). `UpdateAllSynergyEffects` itself calls `DetectAllSynergies()` twice and
  publishes `SynergyDataUpdatedEvent` twice (once inside `UpdateAllBuildingEffects`).
- `ResetSynergyBonuses` calls `ActiveSynergies?.Clear()` on the old list, so any list reference you
  cached is emptied.
- `MainScreenBuildingController.Awake` subscribes to `ClearedStagesUpdatedEvent` and never
  unsubscribes, so the handler outlives the scene (it only logs a warning).
- `LoginConfirmPopup` is reused as the (4,2) "coming later" popup, and `DiplomacyPanel.laterUpdatePopup`
  is also a `LoginConfirmPopup`. Its OK button calls `StartUI.OnLoginSuccess()` if a `StartUI` is wired.
- Build, upgrade, repair and destroy call `AddResource` once per cost with no rollback. A failure
  partway leaves a partial charge or refund. `AddResource` changes the local value first.
- `RestoreFromSnapshot(null)` (a save from before the tiles existed) returns early without
  publishing, which leaves an empty grid.
- File/class name mismatches: `SynergyOuitlineManager.cs` → `SynergyOutlineManager`,
  `DestoryConfirmPopup.cs` → `DestroyConfirmPopup`. Unity normally requires a MonoBehaviour's file
  name to match its class. Check in the Editor before you rename anything, and keep the `.meta` GUIDs.
- Dead code: `BuildingInstanceData`, `ModifierSpecialEffect.DisableTerritory` (never read), the
  controller's `LaterUpdatePopup()`, and the unused local in `Update()`. `Update()` refreshes the
  cooldown UI of all 25 tiles every frame.
- Suspected issue: in `GameManager.ShowResultUI`, `AdditionalIronProduction` does
  `effectValueMin * additiveBonusPercent`, where wood uses `+`. The additional iron bonus is 0 unless
  the tile has an efficiency bonus.
- Suspected issue: the stage-reward loop doesn't check `TileStatusGrid`, so Damaged and Repairing
  buildings still produce wood, iron and magic stone. `BuildingSynergyPanel` also counts them in its
  totals.
- Suspected issue: `RepairBuildingWithAd` makes a tile Normal but calls only
  `UpdateAllBuildingEffects()`, not `UpdateAllSynergyEffects()`. The synergies and tile efficiency
  stay stale until the next grid event or scene load. `RepairBuildingWithAd` is `async void`.
- Suspected issue: `ConfirmAdAndMove` fires `SaveDataToCloudAsync()` right away, before the ad
  finishes (the move saves again later). The callback wrapper `AdManager.ShowRewardedAd` skips the
  WebGL bypass and the init check that `ShowRewardedAdAsync` has, so on WebGL the ad move and ad
  repair appear never to succeed.
- Suspected issue: the text doesn't match the code. `Specialized_Block` says "블록 내 건물 효율 +2.5%"
  (efficiency +2.5% inside the block), but the code adds a global +10% that affects only food. The
  RepairBuildingOnTile log prints the tile status where it says "remaining turns".
