# Stages, waves and stage modifiers

Parent: [`../architecture.md`](../architecture.md). The player picks a stage (main chapter × sub
stage), gets a random "destiny" (운명) and can opt into "challenges" (도전) for a reward bonus, then
fights a map whose enemy waves come from a table; clearing unlocks the next stage.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/UI/StageSelect/UIStageSelect.cs`, `Assets/Scripts/UI/StageSelect/UISelecStageSlot.cs` | Stage list UI (one flat scroll of all sub stages) and its slot |
| `Assets/Scripts/UI/StageSelect/UIStageSelect(Old).cs`, `Assets/Scripts/UI/StageSelect/UIStageSlot(Old).cs` | Dead: 100% commented out (old two-level main/sub picker) |
| `Assets/Scripts/UI/StageModifier/` | Destiny screen (`UIDestinyRoullette` in `UIDestinyRoullete.cs`), destiny result popup, challenge popup + element; Model/ViewModel classes |
| `Assets/Scripts/Modifiercalculator.cs` | Static calculator every combat stat consumer calls for destiny + challenge bonuses and the reward multiplier |
| `Assets/Scripts/DestinyChallenge/DestinyChallengeCalculator.cs` | Dead: per-unit cache calculator, never invoked |
| `Assets/Scripts/UI/CardDeckSystem/UIRandomAndChallenge.cs`, `Assets/Scripts/UI/CardDeckSystem/UIDeckDestinyPopup.cs`, `Assets/Scripts/UI/CardDeckSystem/UIDeckChallengePopup.cs`, `Assets/Scripts/UI/CardDeckSystem/UIDeckChallengeElement.cs`, `Assets/Scripts/UI/CardDeckSystem/UIDisplayStage.cs` | Read-only views on the deck screen (`DeckPresetController` prefab): active destiny, active challenges, "m-s" label |
| `Assets/Scripts/UI/CardDeckSystem/Popup/UIChallengePopupForDeck.cs` | Dead: no prefab uses it (copy of `UIChallengePopup`) |
| `Assets/Scripts/DB/Data/` (`MainStageData`, `SubStageData`, `StageWaveData`, `StageRewardData`, `EnemyData`), `Assets/Scripts/DB/Data/StageModifier/` | Row classes |
| `Assets/Scripts/DB/SO/` (`MainStageSO`, `SubStageSO`, `StageWaveSO`, `StageRewardSO`, `StageModifierSO`, `EnemyUnitSO`, `EnemySO`) | Table SOs; enums in `Assets/Scripts/DB/Data/DataEnums.cs` |
| `Assets/Scripts/Manager/SettingDataManager.cs` | Owns the runtime stage/unlock table `MainStageData` (not PlayerDataManager) |
| `Assets/Scripts/Enemy/EnemyWaveSystem.cs`, `Assets/Scripts/Enemy/EnemyHQ.cs` | Wave loading/spawning; regular spawn loop and defense wave |
| `Assets/Scripts/Scenes/SceneLoader/SceneLoaderBattle.cs` | Instantiates `Resources/Map/Map{main+1}_{sub+1}` |
| `Assets/Resources/Map/` | 45 map prefabs `Map1_1`…`Map5_9` (+ `TestMap`); each holds an `EnemyHQ` + `EnemyWaveSystem` instance |

## How it works

**Id scheme** (mixed bases, the main trap):
- Runtime selection: `PlayerDataManager.SelectedStageIdx` = `(mainStageIdx, subStageIdx)`, **0-based**,
  default `(-1,-1)`. `SettingDataManager.MainStageData[main].subStages[sub]` is 0-based too.
- Table ids are 1-based: `MainStageSO` 1..9; `SubStageSO` and `StageRewardSO` use `main*1000+sub`
  (1001..9009). `SettingDataManager.LoadStageDataFromSO` builds the sub id by string concat
  `"{main}00{sub}"`, `GameManager.ShowResultUI` computes the reward id arithmetically, the analytics
  `stageId` is string concat again. Identical only while sub ≤ 9.
- `PlayerDataManager.clearedStages` (`IsStageCleared`, `MarkLocalStageClear`) is keyed **1-based**
  `(main, sub)`; callers pass `idx + 1`.
- 9 subs per main is hard-coded in several places: `GameManager.OpenSelectArtifactUI` (`subIdx + 1 == 9`
  opens the artifact pick with a bonus flag), `AudioManager.SelectBGM` (`subIdx == 8` → boss BGM),
  `PlayerDataManager.UpdateClearedStagesFromServer` (`(i, 8)`), unused `DestinyModel.GetFortuneProbability`.

**Selection → battle** (all in MainScene, UI swapped with `FadeManager.SwitchGameObjects`):
1. `MainScreenUI.OnDeckSelectButtonClick` / `UIMenu.OnBattleBtnClicked`: if
   `GameManager.IsStageAndDestinySelected` go to `DeckPresetController`, else `UIStageSelect`.
   Tutorial forces `SelectedStageIdx = (0, 1)` (stage 1-2) and the flag true, with no destiny.
2. `UIStageSelect.Awake` → `CreateAllStageSlots` from `SettingDataManager.MainStageData`; a slot is
   clickable iff `main.isUnlocked && sub.isUnlocked`. `OnEnable` scrolls to the last unlocked stage.
   Click → `MoveToBattle(main, sub)` (misnamed: it only sets `SelectedStageIdx` and opens `UIDestinyRoullette`).
3. `UIDestinyRoullette.OnEnable` → intro panel → `DestinyRoulleteViewModel.OnIntroSequenceFinished`
   draws `DestinyModel.GetRandomDestiny(DestinyType.Misfortune)` (always misfortune; the fortune/misfortune
   wheel is commented out as "폐기된 룰렛" = discarded roulette) → `UIDestinyEffectPopup.OpenPanel`
   (icon `Resources.Load<Sprite>(iconSpritePath)`, e.g. `Destiny_Images/...`). "도전하기" opens the nested
   `UIChallengePopup`. "결정하기" (`OnConfirmButtonClicked`): `DestinyModel.ApplyDestiny` →
   `PlayerDataManager.currentDestiny`; `UIChallengePopup.ApplyChanges` → `ChallengeModel.ApplyChallenges`
   replaces `PlayerDataManager.activeChallenges` (`Dictionary<challengeId, level>`); sets
   `IsStageAndDestinySelected = true`; opens `DeckPresetController`.
4. `DeckPresetController.CompleteFormationDirect` saves to cloud, then `SceneLoader` → BattleScene.
5. `SceneLoaderBattle.Awake`: `(-1,-1)` becomes `(0,0)`; loads and instantiates the map prefab.
   `Player` calls `GameManager.StartBattle`, which records `isClearedButTryAgain` (analytics only).

**MVVM-ish pattern in StageModifier**: plain C# Model (`DestinyModel`, `ChallengeModel`: table
lookups + writes to `PlayerDataManager`) ← ViewModel (`DestinyRoulleteViewModel`,
`ChallengePopupViewModel`: temp state, raises C# `event Action<…>` such as `OnResultSet`,
`OnConfirmStateChanged`, `OnRewardTextChanged`) ← View MonoBehaviour (`UIDestinyRoullette`,
`UIChallengePopup`) that `new`s both in `Awake`, subscribes to VM events and forwards clicks. No
EventManager, no unsubscription (VM lives as long as the view). Comments are Korean; the large
commented regions are the removed wheel.

**Waves** (`EnemyWaveSystem`, on the map's enemy HQ):
- `Awake` → `SetWaveData`: `StageWaveSO.GetStageWaveDataList(mainIdx)` (sheet per main), keeps rows with
  `stage - 1 == subIdx`, opens a new `EnemyWave` whenever `wave` increases (rows must be sorted and
  gap-free), and adds `unitCount` entries of `(poolType, spawnProbability / 100)`. **`spawnProbability`
  is used as the enemy stat multiplier**, not a probability.
- `Start` publishes `TimeSyncEvent { waveTime, maxWaveCount = 5 }` (`UITimer` sets hero timer =
  waveTime × 5) and runs `WaveTimeRoutine`: 5 waves, every `waveTime` (90 s serialized); warning
  `warningBeforeWaveTime` (15 s) earlier via `UIWaveWarning.OpenUI` + `OnWarningDisplayed`; then
  `EnemyHQ.SetSpawnEnemyActive(false)`, `WaveRoutine` spawns each entry with
  `ObjectPoolManager.Get(poolType)` at `EnemyHQ.GetRandomSpawnPos()` + `EnemyUnit.SetStatMultiplier`
  every 0.5 s, then re-enables the HQ loop. Publishes `StartWaveEvent { waveIdx }` (1-based).
- Defense wave: the `EnemyHQ.CurHp` setter at ≤70% calls `SpawnDefenseWave` once: replays wave
  index 2 (if ≥3 waves), `PoolType.FXEnemyHQDefense`, hit-back on all player units.
- The regular HQ roster is the serialized `EnemyHQ.enemyUnits` list, overridden per map prefab (not
  table data). Enemy stats come from `DataManager.EnemyUnitData` (`EnemyUnitSO`, keyed by `(int)PoolType`).

**Stage modifiers at battle time** (`Modifiercalculator`):
- `GetMultiplier(target, stat, unitData)` returns a **bonus fraction** (Σ% / 100, e.g. 0.2), not 1.2.
  Sum = `currentDestiny.modifiers` matching (target, stat) whose condition passes + for each active
  challenge `valuePerLevel × level` matching (target, stat). `Percentage` and `Absolute` both add the raw
  value as percent; `Set` adds 0. Cached per (target, stat) in a static dictionary until
  `Modifiercalculator.EndBattle()` (called from `GameManager.ShowResultUI`); conditional modifiers skip
  the cache, and both condition checks (`CheckIsDiffrentNation`, `CheckSameNationCount`) always return false.
- Consumers (the only targets ever queried are `PlayerUnit`, `EnemyUnit`, `Player`):
  `PlayerUnit.SetStatMultiplier` / `EnemyUnit.SetStatMultiplier` / `Player.SetStatMultiplier`
  (MaxHp, AtkPower added to `statMultiplier`; MoveSpeed × (1+bonus)); `UISpawnUnitSlot.InitSpawnUnitSlot`
  (PlayerUnit SpawnCost, SpawnCooldown); `EnemyHQ.SetUnitCoolTime` (EnemyUnit SpawnCooldown).
- Special-cased by hard-coded destiny id: `09010003` → `UITimer.SetTimer` hero timer 300 s;
  `09020001` → `CameraController` rain FX.
- Reward: `Modifiercalculator.GetRewardMultiplier()` = 1 + Σ(`pointPerLevel` × level) × 3 / 100.

**Clear, unlock, rewards**:
- `EnemyHQ.Dead` → (post-tutorial) `GameManager.OpenSelectArtifactUI`, and always `GameManager.ClearStage`:
  if not yet cleared, `MarkLocalStageClear(main+1, sub+1)` (publishes `ClearedStagesUpdatedEvent`,
  which `MainScreenBuildingController` subscribes to; stage 1-2 first clear also grants 10 `Ticket`, and
  `BuildingTile` unlocks the diplomacy tile on `IsStageCleared(1, 2)`); then sets the next sub's
  `isUnlocked` (rolling over also sets the next main's `isUnlocked`); `SaveDataToCloudAsync`;
  `IsStageAndDestinySelected = false`.
- `GameManager.ShowResultUI(true)`: `RewardData.GetData((main+1)*1000 + sub+1)`; Gold, MagicStone, EXP =
  base × challenge multiplier; Wood/Iron = base + territory production × (1 + (building% + challenge%)/100);
  plus the building magic-stone roll. **Same reward on first clear and on repeats** (only the 1-2 ticket
  is first-clear-only). Stage 2-9 (`isTestEndStage`) opens `UITestEndPopup` instead of `RewardPanelUI`.
- `RewardPanelUI`: Next → `SelectedStageIdx` = next stage, `LoadMain.UIDestinyRoullette`, MainScene
  (`SceneLoaderMain` opens the destiny screen directly); Retry → reload BattleScene (destiny and
  challenges kept); Reform deck / Return → `activeChallenges.Clear()`. `currentDestiny` is never cleared.

## Data and persistence
- `MainStageSO.xlsx` / `SubStageSO.xlsx` → `MainStageData` (`displayName`, `isUnlocked`, `subStageCount`;
  `subStages` is empty in the asset and filled at runtime) / `SubStageData` (`displayName` "1-1",
  `isUnlocked`). The table's `isUnlocked` is the fresh-player default (only main 1 and 1-1 are true).
- `StageRewardSO.xlsx` → `StageRewardData` (`rewardGold/Wood/Iron/MagicStone/EXP`).
- `StageWaveSO.xlsx` → `StageWaveSO` fields `stageWaveData1..3` (ids `main×10000000 + n`).
- `StageModifierSO.xlsx` → one dictionary keyed by `idNumber`: sheet `DestinyEffects`
  (`StageDestinyData`, 901xxxx fortune / 902xxxx misfortune, `iconSpritePath`), `DestinyEffectModifier`
  (`StageDestinyModifier` rows carrying the **parent destiny's** id; `SetData` appends them to that
  destiny's `modifiers`), `ChallengeEffects` (`StageChallengeData`, 903xxxx, `maxLevel`, `valuePerLevel`,
  `pointPerLevel`, `modifierSpecialEffect`).
- Player state: only `PlayerSaveData.UnlockData` (`List<List<bool>>`, from
  `SettingDataManager.SaveClearData`) inside the Cloud Save key `PLAYER_DATA`. It holds **unlock** flags,
  not clear flags. Load: `PlayerDataManager.LoadDataFromCloundAsync` → `SettingDataManager.LoadClearData`
  + `UpdateClearedStagesFromServer` (cleared = unlocked minus the last unlocked stage). Saved on stage
  clear, result screen and deck completion; skipped until the tutorial is completed.
- Session-only (lost on restart): `SelectedStageIdx`, `currentDestiny`, `activeChallenges`,
  `GameManager.IsStageAndDestinySelected`, `GameManager.LoadMain`.

## Common changes
- **Make a new main stage playable** (e.g. 4): 1. `MainStageSO`/`SubStageSO`/`StageRewardSO` rows (ids
  must stay contiguous from 1; `LoadStageDataFromSO` loops `GetData(1..N)`). 2. Map prefabs
  `Resources/Map/Map4_1..9` with `EnemyHQ` (`enemyUnits` roster) + `EnemyWaveSystem`. 3. In `StageWaveSO`
  add `public List<StageWaveData> stageWaveData4;`, a `case 3` in `GetStageWaveDataList`, and a loop in
  `SetData`; the sheet already exists in the xlsx; reimport. 4. `AudioManager.SelectBGM` has cases for mains 0–4 only.
- **Add a destiny**: a `DestinyEffects` row + one `DestinyEffectModifier` row per effect with the same
  id, and an icon under `Assets/Resources/Destiny_Images/`. It only works if every modifier's target is
  `PlayerUnit`/`EnemyUnit`/`Player` and the stat is one the consumers above query; anything else needs
  new consumer code. A fortune (901xxxx) is never drawn today.
- **Add a challenge**: a `ChallengeEffects` row; `UIChallengePopup.CreateElements` builds its element
  automatically. Same target/stat limits; `pointPerLevel` drives the reward bonus.
- **Consume a modifier in new code**: `x * (1f + Modifiercalculator.GetMultiplier(target, stat, unitData))`
  — remember it returns the fraction.
- **Edit waves**: rows in the main's sheet with `stage` = sub number (1-based), `wave` 1..5 sorted and
  contiguous, `poolType` = enemy `PoolType` name, `spawnProbability` = stat % (80 → 0.8×).

## Gotchas
- **Stages 4-x and later fall back to stage 1**: `StageWaveSO` only declares `stageWaveData1..3`, so the
  importer drops the xlsx sheets 4 and 5; `SetWaveData` then loads main 1's waves **and overwrites
  `SelectedStageIdx` to `(0, 0)`**, so reward, unlock and analytics apply to 1-1. Mains 6–9 exist in the
  tables but have no map prefab (`Instantiate(null)`). Clearing 9-9 throws in `ClearStage` (index past the
  last main; caught and logged, no save).
- **Most destiny/challenge data is inert**: modifiers targeting `Hero`, `KnightUnit`, `SameNation`,
  `DifferentNation`, `System` or stats `Timer`/`MaxSpawnCount`/`AuraRange` are never queried. Of the 7
  misfortunes only 9020001, 9020002, 9020003 do anything. Challenges 9030009 (`DisableDeckSlot`) and
  9030010 (`DisableTerritory`) have no implementation anywhere yet still give 4 points/level of reward bonus.
- The 3%-per-point rule is duplicated: `Modifiercalculator.RewardBonusPerPoint`,
  `ChallengeModel.RewardBonusPerPoint`, `UIDeckChallengePopup.CalculateBonus` (literal `3`),
  `DestinyChallengeCalculator.rewardBonusPerPoint`. Change all together.
- `DestinyChallengeCalculator` is dead (the call in `GameManager.StartBattle` is commented out;
  `PlayerDataManager.SetDestinyChallengeCache`/`GetDestinyChallengeBonus` have no callers). If revived:
  `EvaluateSameNationCount` loops on `_deckList.Count > 0` (index overrun), `AddBonusToCache` adds
  SpawnCooldown into `SpawnCostBonusPercent`, `CalculateRewardMultiplier` sums `valuePerLevel` instead of
  `pointPerLevel`, `ApplyChallengeBonus` doesn't null-check.
- Suspected issue: `Modifiercalculator.CheckConditionType` logs `unitData.unitName` inside the
  `unitData == null` branch (NullReferenceException).
- Suspected issue: the `ChallengePopup` object is inactive in `Assets/Resources/Prefabs/UI/UIDestinyRoullette.prefab`,
  so `UIChallengePopup.Awake` (which creates the VM) runs only on first open, and `ApplyChanges` is
  `_viewModel?.`. If the player never opens it, `activeChallenges` is left untouched, and "Next stage"
  doesn't clear it, so the previous stage's challenges carry over silently.
- Suspected issue: `EnemyWaveSystem.WaveRoutine` `yield break`s when the wave index has no data
  *after* the HQ loop was paused, so a stage with fewer than 5 waves stops regular spawning for good.
- Suspected issue: `UpdateClearedStagesFromServer` marks `(i, 8)` not cleared when a main's first sub is
  locked; with 9 subs the last unlocked stage is `(i, 9)`, so 1-9 reads as cleared and 1-8 as uncleared.
- Suspected issue: `GameManager.ShowResultUI` returns early when the reward row is missing, skipping the
  result panel and the save. Its `AdditionalIronProduction` uses `*` where wood uses `+`.
- `SettingDataManager.MainStageData` holds the SO's own row objects; unlock flags, `subStages` and
  destiny `modifiers` (in `StageModifierSO.SetData`) are mutated on the ScriptableObject in memory. In the
  Editor this appears able to leak between play sessions. `StageModifierSO.SetData` uses `DB[id]`, which
  throws if a modifier row has no destiny row.
- Name traps: `UIDestinyRoullette` lives in `UIDestinyRoullete.cs` (prefab binds by GUID, `UIManager` by
  class name — rename neither alone); `Modifiercalculator`, `UISelecStageSlot`, `DestinyRoulleteViewModel`.
  `ESelecStageSlotType` and `GameMode`/`DestinyModel.GetFortuneProbability`/`GetSpecificDestiny` are unused.
- `EnemySO`/`EnemyData` (`DataManager.EnemyData`) is legacy with no callers; enemy stats are `EnemyUnitSO`.
- `Assets/Excel/StageWaveSO_Old.xlsx` is legacy: no `StageWaveSO_Old` class exists, so the importer
  (matches xlsx file name to SO class) ignores it. Old sheets `stageWaveData`/`stageWaveData2` with the
  EnemyUnit2–5 roster. Don't edit it expecting an effect.
- `EnemyWave.unitList` is a tuple list, which Unity can't serialize, so the "확인용" (for inspection)
  `WaveData` inspector field shows nothing.
