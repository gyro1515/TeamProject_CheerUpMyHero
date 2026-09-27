# Artifacts

Parent: [`../architecture.md`](../architecture.md). Artifacts (유물) are equippable relics. Passive ones add a percentage stat bonus to the player or to your units, active ones give a mana-cost skill button in battle. You get them from stage-clear picks, merge passives, and level up actives with resources.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/DB/Data/Artifact/` | Row classes: `ArtifactData` (base: `name`, `description`, `artifactType`, `iconSpritePath`), `PassiveArtifactData`, `ActiveArtifactData`, `ActiveArtifactLevelData` |
| `Assets/Scripts/DB/SO/ArtifactSO.cs` | SO with 3 sheets. `SetData` merges them into one dictionary, and attaches level rows to their active |
| `Assets/Excel/ArtifactSO.xlsx` → `Assets/Resources/DB/ArtifactSO.asset` | Design table (sheets `activeArtifacts`, `passiveArtifacts`, `activeArtifactLevels`) |
| `Assets/Scripts/Manager/PlayerDataManager.cs` (region `유물 관련`, `SaveArtifactData`/`LoadArtifactData`) | Owns the player's `OwnedArtifacts` and `EquippedArtifacts` (8 slots), and the change events |
| `Assets/Scripts/UI/Artifact/ArtifactService.cs` | Add, remove, equip, unequip, sort, and auto-equip rules over `PlayerDataManager` |
| `Assets/Scripts/UI/Artifact/ArtifactStatCalculator.cs` | Read-only sum of equipped passive bonuses, used by battle units |
| `Assets/Scripts/UI/Artifact/ArtifactRewardGenerator.cs` | Random stage-clear candidates (chapter grade weights) |
| `Assets/Scripts/UI/Artifact/UIArtifact.cs` + `ArtifactUIPresenter.cs` + `UIArtifactEquipPanel/EquipSlot/InventoryPanel/InventorySlot/StatPanel/StatPlayerPage/StatUnitPage.cs`, `ViewportSizeNotifier.cs`, `UIArtifactUpMoney.cs` | Lobby equip screen (MVP). Prefab `Assets/Resources/Prefabs/UI/UIArtifact.prefab` |
| `Assets/Scripts/UI/Artifact/UIArtifactUpgrade/` | Upgrade screen (MVP): `UIArtifactUpgrade` (view), `UIArtifactUpgradePresenter`, `ArtifactUpgradeService`, `ArtifactUpgradeViewModels`, sub-views. Prefab `Assets/Resources/Prefabs/UI/UIArtifactUpgrade.prefab` |
| `Assets/Scripts/UI/Artifact/Active/ActiveSkillEffect.cs` and the five `Skill_*.cs` beside it | **Live**: the 5 active skill effects |
| `Assets/Scripts/UI/Artifact/Active/` (all other files) | **Dead prototype**: `UISelectActiveArtifact` and its panels, which use the temporary `ActiveAfData` class. Nothing opens `Assets/Resources/Prefabs/UI/UISelectActiveArtifact.prefab` |
| `Assets/Scripts/UI/BattleScene/SwipeArea/UIActiveAfPanel.cs`, `UIActiveAFSlot.cs`, `UIAfExpanationPopup.cs` | Battle HUD: 8 slots mirroring `EquippedArtifacts`, skill use, and the hold-to-describe popup. The panel sits in the bottom-bar prefabs in `Assets/Prefabs/UI/BattleScene/`, inside `Assets/Resources/Prefabs/UI/UITest.prefab` (the battle HUD, despite the name) |
| `Assets/Scripts/UI/BattleScene/UIStageClearArtifactSelect.cs`, `UIRandomArtifactSlot.cs` | Stage-clear pick screen. Prefab `Assets/Resources/Prefabs/UI/UIStageClearArtifactSelect.prefab` |

## How it works

**Types and ids.** `ArtifactType` is `Active`/`Passive` (in `Assets/Scripts/DB/Data/DataEnums.cs`). Code dispatches with `is PassiveArtifactData` / `is ActiveArtifactData`.
- Passive: `effectTarget`, `statType`, `grade` (`PassiveArtifactGrade` Common..Legendary), `value` (a percent: 5 means +5%). The ids are `8020XXXG`, where the last digit is grade+1 (80200011..80200015 is one family). The table has 40 rows: 8 families × 5 grades. They cover `Player`×{MaxHp, AtkPower, MoveSpeed, AuraRange} and `MeleeUnit`/`RangedUnit`×{MaxHp, AtkPower}.
- Active: ids 8010001..8010005, `cost` (mana), `type` (a display string), `levelData` (9 `ActiveArtifactLevelData` rows), `curLevel` (a 0-based index into `levelData`; the UI shows `curLevel + 1`). A level row holds `coolTime`, the effect fields (`damageBonusPercent`, `attackBonusPercent`, `attackSpeedBonusPercent`, `healPercent`, `summonHealth`, `summonDuration`, `summonPoolType`), and the cost of upgrading *from* that level (`goldCost`/`woodCost`/`ironCost`/`magicStoneCost`). The max-level row has -1 costs.

**Ownership and equip (`PlayerDataManager`).**
- `OwnedArtifacts` is a flat `List<ArtifactData>`, where duplicates are separate entries. `EquippedArtifacts` is a fixed 8-slot list (`ArtifactSlotCount = 8`) holding nulls, and any mix of active and passive is allowed. `EnsureEquippedSlotsInitialized` restores it to 8 slots.
- Mutators: `AddOwnedArtifact`, `RemoveOwnedArtifact` (by reference), `SetOwnedArtifacts`, `SetEquippedArtifact`, `ClearEquippedSlot`, `ClearAllEquippedSlots`, `SetEquippedArtifacts`. Each raises the plain C# events `OnArtifactOwnedChanged` / `OnArtifactEquippedChanged`. No `EventManager` channel carries artifact ownership or equip changes (the battle slots publish only the hold events `AfSlotStartHoldEvent`/`AfSlotReleaseHoldEvent`).
- Equipping does not remove from owned, and nothing checks that an equipped artifact is still owned.
- `ArtifactService.AutoEquipArtifacts(type)` clears every slot, then fills them with the primary type first. Passives go by grade desc, then id. Actives go by current `coolTime` asc. `SortOwnedArtifacts` puts actives first, then passives by grade desc, then name, and **replaces** the list instance.

**Passive merge (`ArtifactUpgradeService`).** `CanUpgradePassive(id)` needs ≥3 owned with that id and a next grade. `GetNextPassiveArtifact` finds the row with the same `effectTarget`+`statType` and grade+1. `UpgradePassive` removes 3 (`ArtifactService.RemoveArtifactsByIdNumber`), adds the result, then `SaveDataToCloudAsync`. There is no currency cost.

**Active upgrade.** `CanUpgradeActive` checks `curLevel < levelData.Count-1` and that `GetResourceAmount` covers `GetActiveUpgradeCost`, which reads the current level row. `UpgradeActive` calls `PlayerDataManager.AddResource(type, -cost)` once per resource (sequential awaits), then `artifact.curLevel++`, then `SaveDataToCloudAsync`.

**Stat aggregation.**
- `ArtifactStatCalculator.GetPassiveStatBonus(target, stat)` sums `value` over the equipped passives that match both fields, and `…Percent` divides by 100.
- `DetermineUnitEffectTarget` classifies a unit as `RangedUnit` if `cognizanceRange >= 2`, and otherwise as `MeleeUnit`.
- Only the `Player`/`MeleeUnit`/`RangedUnit` targets are ever queried.
- Consumers: `BaseUnit.Awake` creates `_artifactStatCalculateor` for every unit, enemies included (enemies never use it). `Player.SetStatMultiplier` calls `GetPlayerArtifactBonus` for HP, Atk, MoveSpeed, and AuraRange. `PlayerUnit.SetStatMultiplier` calls `GetUnitArtifactBonus` for HP and Atk.
- The bonus is **added inside the multiplier**: `base * (modifier + statMultiplier + bonusPct)`, and then synergy multiplies it for units. It is read when stats are set (spawn or `OnEnable`), not live.

**Battle actives (`UIActiveAfPanel` → `UIActiveAFSlot`).**
- `UIActiveAfPanel.Awake`: while `!GameManager.IsTutorialCompleted`, `EnsureTutorialArtifactsRegistered` adds all 5 actives to owned and equips them into slots 0–4 when those slots are empty. These are saved later with everything else, so every player starts with all 5. Then, in every case, it calls `InitAfSlot(EquippedArtifacts[i])` for its 8 slots.
- Active slot: it caches `levelData[curLevel]`, uses `cooldown = coolTime` and `manaCost = cost`, and creates the effect with `CreateSkillEffectInstance(idNumber)`, a hardcoded id→`Skill_*` switch.
- A passive slot shows its icon and can't be used.
- Use happens on `UIAdvancedButton.onShortClick` → `OnUseActiveAF`. It returns early if the skill is on cooldown or `Player.CurMana < manaCost`. Otherwise it subtracts the mana, starts the cooldown (the `Update` fill uses scaled `Time.deltaTime`), triggers `PlayerController.TestForUseActiveArtifact` (attack animation), plays a hardcoded per-id sound, and calls `skillEffectInstance.Execute(levelData)`.
- Mana: `Player.MaxMana` defaults to 15, and battle starts at full. `PlayerController.ManaRecoveryRoutine` adds +1 every `PlayerData.manaRecoveryTime`. A slot greys itself through `Player.OnCurManaChanged`. There is no cooldown at battle start.
- Hold → `AfSlotStartHoldEvent(ArtifactData)` / `AfSlotReleaseHoldEvent` on `EventManager`. They are declared in `UIActiveAFSlot.cs`, and `UIAfExpanationPopup` subscribes, toggled by `UITest.OnEnable/OnDisable`.

**Skills (`ActiveSkillEffect.Execute(ActiveArtifactLevelData)`).**
| id | Class | Effect (hardcoded parts in *italics*) | Pool FX |
|---|---|---|---|
| 8010001 | `Skill_IceSpiritBreath` | `Player.FinAttackPower × damageBonusPercent/100` to non-HQ enemies within *10* units ahead, plus a `BuffController.ApplyBuff(BuffSource.Skill_IceSpiritBreath, …)` of *move speed −15% and attack interval (`IntegratedBuffType.AttackRate`) +15%, 15 s*, blue tint | `FXActiveAf1` |
| 8010002 | `Skill_ThunderJudgment` | Same damage formula to all non-HQ enemies, *5 hits, 1 s apart* (coroutine hosted on `PlayerDataManager`) | `FXActiveAf2` |
| 8010003 | `Skill_KingMarch` | Ally buff `BuffSource.Skill_KingMarch`: AttackPower +`attackBonusPercent`%, attack interval *fixed −50%* (the line using `attackSpeedBonusPercent` is commented out), *10 s* | `FXActiveAf3` |
| 8010004 | `Skill_GoddessBlessing` | After *3 s*, heals every ally `MaxHp × healPercent/100` through `BaseController.TakeHeal` | `FXActiveAf4` |
| 8010005 | `Skill_GiantCoffin` | Gets `PoolType.Allies_UnitGolem` at player x+*1*, clamped between the HQs. The golem's HP and lifetime come from `SummonedUnitController.SetStatMultiplier`, which reads `DataManager.ArtifactData.GetData(8010005)` (see Gotchas) | `FXActiveAf5` |

**Stage-clear pick.**
- After the tutorial, `EnemyHQ.Dead` → `GameManager.OpenSelectArtifactUI()` (sets `Time.timeScale = 0`) → `UIStageClearArtifactSelect.OpenSelectUI(ArtifactType.Passive[, isSub9])`, where `isSub9` is `subIdx + 1 == 9`.
- `RandomCreate` requests 3 distinct passives with `GetRandomPassiveArtifacts(3, mainStageIdx)`. For each attempt it rolls a grade from `_chapterProbabilities[min(chapter,4)]` (Common/Rare/Epic/Unique/Legendary %: ch0 89/9.5/1.5/0/0, ch1 69.5/26/3.5/1/0, ch2 49.5/41.5/7/1.5/0.5, ch3 34.5/50/9/5/1.5, ch4 25/52.5/12.5/7.5/2.5), then picks a uniform row of that grade. After 300 attempts it gives up and may return fewer.
- `OnSelectButtonClicked` → `ArtifactService.AddArtifact(id)`. On an X-9 stage it then offers 2 random distinct actives (`GetRandomActiveArtifacts(2)`, uniform over all 5, owned or not). Otherwise it fades out → `GameManager.ShowResultUI(true)`, whose final `SaveDataToCloudAsync` persists the pick.
- Reroll = `BackendManager.WatchAdAndGetReward()` and then a new roll. It is unlimited, and `isRerolled` is written but never read.

**MVP pattern (follow this for new artifact UI).**
1. The **view** is a `BaseUI`/`BasePopUpUI` MonoBehaviour. It only renders and reports. It exposes `public event Action…` intents (`OnRequestEquip`, `OnRequestUpgrade`, …) raised from button listeners, and public render methods that take view-model structs (`RefreshX(vm)`, `OpenX(vm)`). New child views should not touch `PlayerDataManager` or the services (existing exception: `UIArtifactUpMoney.Awake` reads the gold amount); the **root** view (`UIArtifact`, `UIArtifactUpgrade`) is the one exception, because it wires the dependencies in step 2.
2. The root view builds the plain-C# **services** and the **presenter** in `Start`: `new ArtifactService(PlayerDataManager.Instance)`, `new ArtifactUpgradeService(data, service)`, `new …Presenter(data, services, views…)`. It calls `_presenter.InitialDisplay()` on open or re-enable, and `_presenter.Dispose()` in `OnDestroy`. Sub-views reach the presenter either as constructor args (`ArtifactUIPresenter`) or through getters on the root view (`UIArtifactUpgrade.GetPassivePopup()`).
3. The **presenter** subscribes with method references in its constructor, and unsubscribes in `Dispose`. It subscribes to the view intents and to `PlayerDataManager.OnArtifactOwnedChanged/OnArtifactEquippedChanged`. It calls the services for every mutation, and builds **view models**: structs holding display-ready strings, `Resources.Load<Sprite>(iconSpritePath)`, and grade colors (`GetGradeColor`). Examples are `InventorySlotViewModel`, `EquipSlotViewModel`, `StatPanelViewModel`, and the ones in `ArtifactUpgradeViewModels.cs`. Changes to the owned or equipped lists re-render through the events. Anything else (for example a `curLevel` change) needs an explicit refresh call in the presenter.
4. The **services** hold the rules (validation, costs, sort, merge) and should write only through `PlayerDataManager` methods (existing exception: `ArtifactUpgradeService.UpgradeActive` increments `curLevel` on the row directly; see Gotchas). Async ones return `UniTask<bool>` and save.

## Data and persistence
- Static: `ArtifactSO` (`[ExcelAsset]`, auto-imported) → `DataManager.ArtifactData` (a `DataBase<ArtifactData, ArtifactSO>` keyed by `idNumber`). `ArtifactRewardGenerator` instead calls `Resources.Load` on the same SO, and `UIGuide` reads `DataManager.ArtifactData.SO`. Both read the SO lists directly.
- Player state: `PlayerSaveData.OwnedArtifacts` and `PlayerSaveData.EquippedArtifacts` are **JSON strings** inside the Cloud Save key `PLAYER_DATA` (`Constants.PLAYER_DATA_KEY`). `SaveArtifactData` serializes **the whole row objects** with Newtonsoft `TypeNameHandling.Auto`, so the JSON holds `$type` (for example `ActiveArtifactData, Assembly-CSharp`), `name`, `description`, `value`, the full `levelData` list, and `curLevel`. `LoadArtifactData` deserializes the two strings separately, and on failure resets both to empty.
- Saves: `SaveDataToCloudAsync` runs after a passive merge, after an active upgrade, at the end of `ShowResultUI`, after `ClearStage`, and on app pause. It is skipped until the tutorial is done. Equip changes on `UIArtifact` are **not** saved on their own. They persist with the next save from anything.
- Resources: upgrade costs use `ResourceType.Gold/Wood/Iron/MagicStone` through `PlayerDataManager.AddResource`. The analytics `StageResultEvent.stageUsedArtifat_String` is `SaveArtifactData(EquippedArtifacts)`.

## Common changes
- **To add a passive:** add 5 rows (one per grade) to `passiveArtifacts` in `Assets/Excel/ArtifactSO.xlsx` with the same `effectTarget`+`statType`, because the merge chain finds the next grade by those two fields. Use a new `effectTarget`/`statType` pair only if `ArtifactStatCalculator` and a battle consumer read it, and add a `StatBarViewModel` to `StatPanelViewModel`/`ArtifactUIPresenter.HandleEquippedArtifactChanged` (plus a stat page) if it should show in the equip screen.
- **To add an active:** 1. Add an `activeArtifacts` row and its level rows (same `idNumber`, ascending `level`, -1 costs on the last). 2. Write `Skill_X : ActiveSkillEffect`. 3. Add the id to `UIActiveAFSlot.CreateSkillEffectInstance`, and a sound to the switch in `OnUseActiveAF`. 4. For FX, add a `PoolType` and a prefab in `Resources/Prefabs/ObjPooling/`. 5. For a timed buff, add a `BuffSource` value in `Assets/Scripts/Base/Character/BuffController.cs`. New actives automatically enter the X-9 reward pool.
- **To change reward odds:** edit `_chapterProbabilities` in the `ArtifactRewardGenerator` constructor. Each row must sum to 100, in `PassiveArtifactGrade` order.
- **To add artifact UI:** follow the MVP steps above. Put the rules in a service, never in the view.

## Gotchas
- **Reference identity is the source of several bugs.** Artifacts added in-session are the *same* SO row instances as in `DataManager` (and duplicates share one object). After a reload, every owned and equipped entry is a separate deserialized object, and owned is not the same object as equipped. Several paths compare by reference: `EquippedArtifacts.Contains`, `==` in `HandleUnEquipRequest`, `RemoveOwnedArtifact`, and `UIActiveAfPanel`'s `OwnedArtifacts.Contains`. Consequences:
  - Suspected issue: after a reload, the inventory's `IsEquipped` is always false. Unequip from the inventory does nothing, and the same artifact can be equipped in several slots, where its passive bonus stacks.
  - Suspected issue: before a reload, `UpgradeActive`'s `curLevel++` mutates the shared `ArtifactSO` row. That upgrades every copy and the static data, and in the Editor the change survives leaving Play mode. After a reload, it upgrades only the owned copy, while the equipped copy (which battle reads) keeps the old level until re-equipped.
  - Suspected issue: the golem reads `curLevel` from the **static** row (`DataManager.ArtifactData.GetData(8010005)`), so after a reload it always summons at level 1 stats.
  - Suspected issue: a passive merge removes owned copies but leaves equipped ones in place, because equip isn't tied to ownership.
- Suspected issue (Editor): `ArtifactSO.SetData` does `activeAf.levelData.Add(row)` on the SO's own lists. If the `DataBase` is built again while the SO stays in memory (a second Play session without an asset reload), the levels duplicate. The serialized asset has `levelData: []`, so don't save the asset from Play mode.
- The save carries a full copy of the static table (`value`, `description`, `levelData`, costs). A later xlsx change does **not** reach artifacts that are already owned. Renaming `ActiveArtifactData`/`PassiveArtifactData` breaks the `$type` of existing saves.
- `UpgradeActive` spends each resource with a separate `AddResource` await before `curLevel++`. A failure midway loses resources with no level gain.
- The upgrade screen's buttons are misnamed. `OnRequestAutoEquip` randomly picks one mergeable passive into the 3 material slots, and `OnRequestUnequipAll` only clears that selection.
- `ArtifactUIPresenter` hooks the slot clicks through `UIArtifactEquipPanel.OnslotsInitialize`, which fires in the panel's `Start`. It appears to depend on `UIArtifact.Start` (where the presenter is built) running first, because no script execution order is set.
- `UIArtifactInventorySlot.Init` adds `onClick` again on every refresh, so the listeners pile up.
- `UIArtifactStatUnitPage.cs` declares class `UIArtifactUnitStatPage`, a file/class name mismatch that Unity normally rejects for MonoBehaviours. It is on `UIArtifact.prefab`, so verify it in the Editor before renaming either one.
- Duplicated logic: `ArtifactUIPresenter.CreateStatBarViewModel` re-implements `GetPassiveStatBonus`. `GetGradeColor` is copied in both presenters, and `UIRandomArtifactSlot` has serialized copies of the colors. `ArtifactService` and `PlayerDataManager` each define `ArtifactSlotCount = 8`, `UIArtifactEquipPanel` hardcodes 8, and battle has 8 slots in the prefab.
- Hardcoded ids 8010001–8010005 appear in `UIActiveAFSlot` (twice), `UIActiveAfPanel.TutorialActiveArtifactIds`, and `SummonedUnitController`.
- Dead or unused code: `ActiveAfData`, `PlayerDataManager.OwnedActiveAfData/EquippedActiveAfData`, the `Active/` prototype UI, `ArtifactStatCalculator.GetPassiveArtifactValue`, `ArtifactSO.GetList`, `ActiveArtifactLevelData.summonPoolType` (its use is commented out), `CreatePassiveEffectText`, the `_cashText`/`_ticketText` of `UIArtifactUpMoney`, and the level rows' own `name`/`description`. `CreateActiveEffectText` prints an empty "계수 :" (coefficient) line. `PassiveSlotViewModel.BorderColor` is never set.
- `ArtifactRewardGenerator`'s "max attempts exceeded" log is unreachable (`attempts > maxAttempts` after a `<` loop). `OpenSelectUI(type)` (the non-X-9 overload) doesn't call `OpenUI()`, while the X-9 overload does.
- `UIArtifact.OnBackPressed` always goes to `DeckPresetController`, even when it was opened from `LordManorPanel`.
- In the data, the level-7 cooldown of 8010002 (76) is higher than level 6 (70).
