# Battle

Parent: [`../architecture.md`](../architecture.md). The battle scene: the player walks a commander
along one lane, spends supply (food) to summon deck units from the player HQ, and wins by destroying
the enemy HQ while enemies stream from their HQ and five timed waves.

Terms: **Player** = the commander the user steers (`Player`). **Hero (용사)** = the random
reinforcement `Hero_Unit1..5` spawned when the top timer ends. **Hero synergy** = deck units whose
`synergyType` has `UnitSynergyType.Hero`. The code mixes these words; keep them apart.

## Where it lives

| Path | Role |
|---|---|
| `Assets/Scripts/Scenes/SceneLoader/SceneLoaderBattle.cs` | Scene bootstrap: opens HUD UIs, instantiates `Resources/Map/Map{m}_{s}` |
| `Assets/Scripts/Manager/GameManager.cs` | Persistent: `IsBattleStarted`, food tick, artifact-select pause, `ShowResultUI`, `ClearStage`, analytics, editor cheat keys |
| `Assets/Scripts/Manager/UnitManager.cs` | Scene-owned: player/enemy `BaseCharacter` lists, `FindClosestTarget`, hero stat boost, minimap C# events |
| `Assets/Scripts/Base/Character/` | `BaseCharacter`/`BaseUnit`/`BaseHQ`, `BaseController`/`BaseUnitController`, `KnockbackHandler`, `BuffController` + `BuffEffect`/`BuffTimer`, `BaseCharacterHpBar` |
| `Assets/Scripts/Base/IDamageable.cs`, `Assets/Scripts/Base/IAttackable.cs` | Damage/heal/dead interface (used everywhere); `IAttackable` is implemented but never consumed |
| `Assets/Scripts/Player/Player.cs`, `Assets/Scripts/Player/PlayerController.cs` | Commander stats, mana, EXP/level, aura; movement clamp, targeting, attack |
| `Assets/Scripts/Player/PlayerHQ/` | `PlayerHQ` (summon, legendary counter), `PlayerHQSkill` (auto skills), `BaseHQSkill` + `HQSkill1`–`HQSkill5` |
| `Assets/Scripts/Player/PlayerUnit/` | `PlayerUnit` (stats) + 5 controllers; `SummonedUnitController` (golem, derives from `PlayerUnit`); `GolemAIController` (unused) |
| `Assets/Scripts/Enemy/` | `EnemyHQ`, `EnemyUnit` + 5 controllers, `EnemyWaveSystem` (wave data: [stages.md](stages.md)) |
| `Assets/Scripts/Base/Framework/CameraController.cs` | Follow/auto-follow, wave shake, destiny rain FX, **spawns the hero** |
| `Assets/Scripts/CombatAmount/`, `Assets/Scripts/FX/`, `Assets/Scripts/Animation/` | Pooled damage/heal numbers; pooled particle FX (release on disable); `AnimationData` hash singleton, `AttackStateBehaviour` (sets `BaseUnit.IsAttackAnimPlaying`), eye helpers |
| `Assets/Scripts/UI/BattleScene/` | HUD: `UISpawnUnitSlot`/`UIPlayerUnitSpawnPanel`, `SupplyUI`, `UIMana`, `UIJoyStickVer2`, `UISwipeAreaVer2`, `UIWaveWarning`, `StageAnnouncerUI`, `UIUnitexplanationPopup`; `TopUI/`: `UITimer` (hero timer), `UITimeBar`, `UIMiniMap`, `UIHQSkills`/`HQSkillsCooldown`, `UIDeckSynergyForBattleScene` |
| `Assets/Scripts/UI/HeroCinematic/`, `Assets/Scripts/UI/UIHpBarContainer.cs`, `Assets/Scripts/UI/UIHpbar.cs`, `Assets/Scripts/UI/Pause/UIGiveUpPanel.cs` | Hero cut-scenes; screen-space HP bars (HQs only); "give up" → defeat |

Artifact UIs in that folder (`UIActiveAFSlot`, `UIActiveAfPanel`, `UIAfExpanationPopup`,
`UIRandomArtifactSlot`, `UIStageClearArtifactSelect`): [artifacts.md](artifacts.md). HUD root:
`Resources/Prefabs/UI/UITest.prefab`, nesting `BottonBG(DefaultVer)250930` and `TopUIContainer`.
Legacy (old prefabs only): `UIJoyStick`, `UIJoyStickHandle`, `UIMoveButton`, `UISwipeArea`,
`UISwipeIndex`; `Assets/Scripts/Scenes/BattleScene.cs` (dead `SceneLoadManager` path).

## How it works
**Start sequence.**
1. `BattleScene.unity` holds only the camera (`CameraController`), `SceneLoaderBattle`, and FX.
   `SceneLoaderBattle.Awake`: `GetUI<UITest>()`, `GetUI<UIPause>()`, `GetUI<UIHpBarContainer>()`,
   activates `UnitManager`, forces stage (0,0) if `SelectedStageIdx` is (-1,-1), then
   `Instantiate(Resources.Load("Map/Map{m+1}_{s+1}"))`. The HUD must come first:
   `UIPlayerUnitSpawnPanel.Awake` overwrites the tutorial deck before `PlayerHQ.Awake` reads it.
2. Every map prefab nests `Player.prefab`, `PlayerHQ.prefab`, `EnemyHQ.prefab` and overrides HQ
   `MaxHp`, `EnemyHQ.enemyUnits`, `spawnInterval`, and `EnemyWaveSystem.waveTime` (90 s).
3. Awake: `PlayerHQ` sets `GameManager.PlayerHQ`, reads the active deck (rarity, spawn counters).
   `EnemyHQ` sets `GameManager.enemyHQ` and starts every type's spawn cooldown
   (`spawnCooldown × (1 + mod SpawnCooldown)`). `Player.Awake` sets `GameManager.Player`, joins
   `UnitManager`, and calls **`GameManager.StartBattle()`** (`ResetFood`, `IsBattleStarted = true`,
   `StartTime`). Each `BaseUnit.Awake` runs `SetDataFromExcelData` + `SetStatMultiplier(1f)`.
4. Start: both HQs publish `BaseHQ.SpawnHQEvent` → `UnitManager` lists them (HQs are targets) and
   `UIHpBarContainer` makes their bars. `EnemyHQ` starts `SpawnUnitRoutine`. `EnemyWaveSystem`
   publishes `TimeSyncEvent` (→ `UITimer`, `UITimeBar`) and starts `WaveTimeRoutine`. Modifiers,
   artifacts and synergy are not applied here; each unit reads them in `SetStatMultiplier`.

**Supply (food).** `GameManager.Update` calls `PlayerDataManager.AddFoodOverTime(Time.deltaTime)`
while `IsBattleStarted`: gain/s = `baseFoodGainBySupplyLevel[SupplyLevel-1] × (1 + farm%/100)`
([territory.md](territory.md)). `MaxFood` is a draining reservoir, not a cap: gained food is
subtracted from it; it starts at `CalculatedMaxFood`. `SupplyUI` → `UpgradeSupplyLevel` pays
`supplyUpgradeCosts[level-1]` from both `CurrentFood` and `MaxFood` (9 levels).

**Summoning.** `UIPlayerUnitSpawnPanel.Awake` inits 8 `UISpawnUnitSlot`s from
`DeckPresets[ActiveDeckIndex]`. Cost = `floor(cost × (1 + mod SpawnCost))`; cooldown =
`spawnCooldown × (mod SpawnCooldown + 1 − TotalUnitCooldownReduction/100)`. Short tap →
`OnSpawnUnit` (HQ alive, not cooling, food ≥ cost) → `AddResource(Food, -cost)` →
`PlayerHQ.SpawnUnit(poolType, slot)`: pool `Get` at `GetRandomSpawnPos()` (random y in
`minY..maxY`). For non-epic, non-hero-synergy units `UnitSpawnCnt` counts spawns; every Nth
(`upgradeCntByRarity` 8 common / 4 rare) is "legendary" → `SetStatMultiplier(statMultiplier = 1.2)`;
the slot outline lights one spawn early. A released hero-synergy unit publishes `HeroUnitDeadEvent`
→ its slot refunds 75% of the remaining cooldown. Hold → `SpawnUnitSlotStartHoldEvent` → popup.

**Enemy spawning.** `EnemyHQ.SpawnUnitRoutine` spawns the first `enemyUnits` entry whose cooldown
ended, then waits `spawnInterval` (else retries next frame). `EnemyWaveSystem.WaveTimeRoutine` runs 5
waves, one per `waveTime`: `UIWaveWarning` `warningBeforeWaveTime` early, then
`SetSpawnEnemyActive(false)`, `WaveRoutine` (one unit per 0.5 s, each `SetStatMultiplier(m)`),
`StartWaveEvent{waveIdx = 1..5}`, and `SetSpawnEnemyActive(true)` when the list is spawned. At ≤70%
HP `EnemyHQ.CurHp` fires `SpawnDefenseWave` once (needs ≥3 waves): wave index 2, `FXEnemyHQDefense`,
`StartHitBack()` on every player-side unit; the HQ loop keeps running.

**Commander.** `UIJoyStickVer2` sets `Player.MoveDir` to ±x (sign only). `PlayerController.FixedUpdate`
moves by `FinMoveSpeed`, clamps x between the HQ positions cached in `Start`, and raises static
`PlayerController.OnPlayerAction` while moving. `TargetingRoutine` (0.1 s) + `AttackRoutine`
coroutine: attacks only while standing still, animation-timed, single target, `FinAttackPower`.
Mana starts at `MaxMana` (15, serialized), +1 per `PlayerData.manaRecoveryTime`; active artifacts
spend it. Every 0.1 s `UpdateAuraBuffs` calls `PlayerUnit.ApplyAuraBuff(auraAtkBonus)` on units
within `AuraRange` (x distance). On victory `Player.CurExp += EXP`; `PlayerLevelUP` loads the next
`PlayerSO` row and writes `PlayerDataManager.PlayerLevel`/`CurExp` (leftover EXP is dropped).

**HQ skills (automatic, no input).** `PlayerHQSkill.HQSkillRoutine` waits until `UIHQSkills.Start`
registers the cooldown icons, then every 0.1 s: `FindTarget` = nearest live enemy ahead within
`hQSkills[0].DetectRange`; for i = 4..0, if in that skill's range and `!IsCoolTime` →
`Get(PoolType.HQSkill{i+1})`, `ActivateSkill` (DOTween arc, 1 s) → `BaseHQSkill.AttackRange`: up to
`maxTargetCount` lowest-x enemies within `attackRange/2` take `atkPower` (serialized on
`Resources/Prefabs/ObjPooling/HQSkill{n}.prefab`). `HQSkillsCooldown.Update` clears `IsCoolTime`.

**Units and AI.** `BaseCharacter` → `BaseUnit` → `PlayerUnit` (→ `SummonedUnitController`),
`EnemyUnit`, `Player`; `BaseCharacter` → `BaseHQ` → `PlayerHQ`, `EnemyHQ`. `BaseController` →
`BaseUnitController` → `{Player,Enemy}{UnitController, MeleeSplashController,
RangedSplashController, HealerUnitController, HealerSplashController}` (+ unused `GolemAIController`);
`PlayerController` derives from `BaseController` directly.
- Unit prefabs carry **no controller**. `PlayerUnit`/`EnemyUnit.SetDataFromExcelData` parses
  `gameObject.name` as `PoolType`, loads `DataManager.PlayerUnitData`/`EnemyUnitData.GetData((int)poolType)`,
  and `AddComponent`s by `attackType`: Target → `*UnitController`, Area → `*RangedSplashController`,
  PierceArea → `*MeleeSplashController`; healers: Target → `*HealerUnitController`, else `*HealerSplashController`.
- Loop: `TargetingRoutine` (0.1 s) sets `TargetUnit` and `MoveDir` (zero when a target exists, else
  right/left). `Update` → `AttackUpdate`: timer ≥ `FinAttackRate`, live target, not attacking →
  trigger, stop targeting, `AtkAnimRoutine`: wait for `IsAttackAnimPlaying`, play at
  `StartAttackTime / attackDelayTime` speed until `StartAttackNormalizedTime`, hit, then resume targeting.
- Targeting: `UnitManager.FindClosestTarget` scans the opposite list by x only, forward only, within
  `CognizanceRange` (not `AttackRange`), nearest wins. Ranged splash hits up to `maxTargetCount`
  within `AttackRange/2` of the target; melee splash hits within `AttackRange` of self on both sides.
- Healers act only while an enemy target exists; they heal `healAmount` (+`FXHealEffect`) to an ally
  under 80% HP (player healers: commander first, then Tanker > Healer > Dealer), else attack.

**Damage, heal, knockback, death.** `IDamageable.TakeDamage` → `BaseUnitController.TakeDamage`
(ignored while `IsInvincible`) → `BaseController.TakeDamage` (ignored if dead): `CurHp -= dmg`, hit
FX + number (player side `FXPlayerUnitHit`+`DealAmountEnemy`, enemy side `FXEnemyUnitHit`+
`DealAmountPlayer`). `TakeHeal` adds HP + `HealAmount`. HQs implement `IDamageable` themselves (no
FX; heal logs an error). `BaseUnit.CurHp`: `hitbackHp = MaxHp / hitBack`; crossing a threshold (and
always at 0 HP) → `OnHitBack` → `KnockbackHandler.HitBackRoutine` (~1 s arc, invincible, controller
coroutines stopped); any other HP change → small `OnKnockBack` push. `OnDead` removes the unit from
`UnitManager` and sets `IsDead`; the lethal hit-back ends in `UnitController.SetDead()` →
`ReleaseSelf()`. `BaseHQ.Dead` plays a sound and `Destroy`s the HQ.

**Stat pipeline** (`SetStatMultiplier`, then `InitFinStats`; attacks read `Fin*`; `AttackRate` is the
attack interval in seconds, so bigger = slower; "fraction" = percent/100):

| Factor | Where it multiplies |
|---|---|
| Table base (`UnitData`) | `PlayerUnitSO` / `EnemyUnitSO` / `PlayerSO` row |
| Destiny + challenge (fraction) | HP/ATK: added in `(mod + statMultiplier + artifact)`; MoveSpeed `× (1 + mod)` |
| `statMultiplier` | same bracket; AttackRate `/ m` (player units) but `× m` (enemies, Player) |
| Artifacts (`ArtifactStatCalculator`, fraction) | same bracket (player units HP/ATK; Player also speed, aura) |
| Territory synergy (player units only) | `× (1 + SynergyAllUnitHealthBonus/AttackBonus /100)`; AttackRate `× (1 − cd%/100)` |
| Hero arrived (`isSpawnHero`, Normal class, new spawns) | `× 2` HP/ATK, AttackRate `/ 2`, scale clamp 0.8–1.2 |
| Buffs (`BuffController.ApplyBuff`, active artifacts only) | `Fin = base × (1 + Σfraction) + Σadd` (`BaseUnit.SetBuffStatPercent/Add`) |
| Aura | `AtkPower ×= 1 + auraAtkBonus/100` (see Gotchas) |

`statMultiplier`: 1, 1.2 (legendary), 2 (hero boost), or the wave value. Destiny/challenge values come
from `Modifiercalculator.GetMultiplier` (cached per battle). No card-level factor exists
(`UnitUpgradeService.UpgradeCard` changes nothing in battle).

**Hero timer.** `UITimer.Awake` picks a random `PlayerUnitSO.hero_unit` row. Timer = `waveTime × 5`
(450 s), 300 s for destiny `9010003`, 40 s in the tutorial. Wave 1 → first-wave cut-scene
(tutorial: `UITutorialHero` at 30 s left); last 3 s → pre-spawn speech; 0 → spawn cut-scene +
`HeroSpawnEvent`. Subscribers:
`CameraController.SpawnHero` (pool `Get` at the camera target x), `UnitManager.ApplyHeroSummonStatBoost`
(existing Normal units `SetStatMultiplier(2f)`), `PlayerHQ` (`isSpawnHero`). No time-out defeat.

**Battle end.** Victory: `EnemyHQ.Dead` → tutorial done: `OpenSelectArtifactUI` (`IsBattleStarted =
false`, timeScale 0; sub-stage 9 picks passive then active) → `UIStageClearArtifactSelect` →
`ShowResultUI(true)`; tutorial: `ShowResultUI(true)` directly. It always also runs `ClearStage()`
(`MarkLocalStageClear` on first clear, unlock the next stage in `SettingDataManager.MainStageData`,
save, `IsStageAndDestinySelected = false`). Defeat: `PlayerHQ.Dead`, the `Player.OnDead` lambda, and
`UIGiveUpPanel` Yes → `ShowResultUI(false)`. `ShowResultUI`: clear destiny cache, publish
`BattleEndedEvent`, `Modifiercalculator.EndBattle()`, record `StageResultEvent` straight to
`AnalyticsService`, timeScale 0. Victory rewards: `StageRewardSO` key `(m+1)*1000+s+1`, building
production (4×4 tiles), `× GetRewardMultiplier()`; open `RewardPanelUI` (`UITestEndPopup` on 2-9),
then `AddResource` ×4. Defeat (after tutorial): `ApplyResourcePenalty` (5% each). Then
`SaveDataToCloudAsync`. `RewardPanelUI` restores timeScale and loads the next scene.

**Camera/HUD.** `CameraController.LateUpdate` (only while `IsBattleStarted`) follows the Player, or
after 3 s without `OnPlayerAction` the right-most `PlayerUnitList` entry; 2 s shake on
`StartWaveEvent`. `UIMiniMap` uses `UnitManager.onUnitSpawn/onUnitDeSpawn` + pooled `UIMinimapIcon`.
- Events published: `SpawnHQEvent` (HQs), `TimeSyncEvent`/`StartWaveEvent` (`EnemyWaveSystem`),
  `HeroSpawnEvent` (`UITimer`), `HeroUnitDeadEvent`, `BattleEndedEvent` (`GameManager`;
  `PlayerDataManager` repairs/damages tiles), `FinishTutorialDeckSettingEvent`, slot hold events.
- Pools: `Allies_Unit*`, `EnemyUnit*`, `Hero_Unit*`, `Allies_UnitGolem`, `HQSkill1–5`,
  `FxHQSkill1–5Effect`, `FXPlayerUnitHit`, `FXEnemyUnitHit`, `FXHealEffect`, `FXEnemyHQDefense`,
  `DealAmountPlayer`, `DealAmountEnemy`, `HealAmount`, `UIMinimapIcon`.
- Time scale: 0 in `OpenSelectArtifactUI`, `ShowResultUI`, pause, tutorials; 1–3 from `UIPause`
  speed (`SettingDataManager.SavedSpeed`); 1 from `UIGiveUpPanel`/`RewardPanelUI`. Camera and damage
  numbers use unscaled time.

## Data and persistence
- Tables: `PlayerUnitSO`, `EnemyUnitSO`, `PlayerSO` (class in `Assets/Scripts/DB/Data/Unit/PlayerSO.cs`),
  `StageRewardSO`, `StageWaveSO` ([stages.md](stages.md)), `ArtifactSO` (golem reads id `08010005`).
  Unit dictionaries are keyed by `(int)PoolType` (player units also by `idNumber`).
- Prefab tuning (not tables): HQ `MaxHp`, `minY/maxY`, `spawnInterval`, `EnemyHQ.enemyUnits`,
  `waveTime` per map prefab; `PlayerHQ.statMultiplier`/`upgradeCntByRarity`; HQ skill stats;
  `Player.MaxMana`; `KnockbackHandler` scales per unit prefab.
- Battle-only state (food, supply level, buffs, HQ and unit state) is never saved. The player
  snapshot can still be written mid-battle: `PlayerDataManager.OnApplicationPause(true)` calls
  `SaveDataToCloudAsync` whenever the app goes to the background (after the tutorial). At the end: rewards and
  penalties through `AddResource` (Economy), `PlayerLevel`/`CurExp` and stage unlocks via
  `SaveDataToCloudAsync` ([player-data.md](player-data.md)).

## Common changes
- **Add a unit**: add the `PoolType` (see architecture.md; values are stored as ints in
  `Resources/DB/*UnitSO.asset` and map-prefab `enemyUnits`, so inserting mid-enum renumbers them).
  Prefab in `Resources/Prefabs/ObjPooling/` named exactly like the enum, with `PlayerUnit`/`EnemyUnit`,
  `KnockbackHandler`, `BuffController`, `BasePoolable`, an Animator whose attack state has
  `AttackStateBehaviour`, and no controller. Add the xlsx row (`hitBack` ≥ 1, `maxTargetCount` for
  splash, `healAmount` for healers). Enemies also go into a map `enemyUnits` list or the wave table.
- **Add an attack pattern**: new `BaseUnitController` subclass (implement `HitBackActive`, copy the
  targeting/`AttackUpdate` pattern), new `UnitAttackType`, and a case in both `SetDataFromExcelData`.
- **Add a stat factor**: edit `PlayerUnit`/`EnemyUnit`/`Player.SetStatMultiplier` before
  `InitFinStats()`; for a timed effect use `BuffController.ApplyBuff(new BuffSource, effects, duration)`.
- **Add an HQ skill**: class + prefab + `PoolType.HQSkill6` + `idxToPoolType` entry + `hQSkills` on
  `PlayerHQ.prefab` (index order = strength order).

## Gotchas
- Deck-unit synergies (Frost, Burn, Poison, Berserker…) have **no combat effect**: only attack SFX
  (`BaseUnitController.PlayUnitAttackEffectSound`) and the badges read them. Territory synergy does apply.
- Suspected issue: the aura changes `AtkPower`, never `FinAttackPower`, so it changes no damage until a
  buff recomputes `Fin*` (and then the boost outlives the aura).
- Suspected issue: `SetStatMultiplier` → `InitFinStats` resets buff multipliers; a buff active then (e.g.
  at the hero boost) later subtracts in `ResetEffect`, leaving stats below base. It also refills HP.
- Hero boost on existing units (`statMultiplier` 2, inside the bracket) ≠ the new-spawn bonus (`× 2` after it).
- The wave multiplier is the `spawnProbability` column /100; enemy AttackRate `× m` makes stronger wave
  enemies attack **slower**.
- Every heal knocks the target back (`BaseUnit.CurHp` can't tell heals from damage). Death runs through
  the hit-back coroutine, so a unit with `hitBack` ≤ 0 would never be released.
- Suspected issues: enemy splash min-heaps replace on `priority < Peek()` (wrong targets past
  `maxTargetCount`); `EnemyUnit.SetDataFromExcelData`'s default case adds `PlayerHealerUnitController`;
  `SummonedUnitController.SetStatMultiplier` never calls `InitFinStats` (golem `Fin*` stay 0).
- Heal priority reads the serialized `BaseUnit.UnitType`, not `UnitData.unitType`. Legacy test prefabs
  `PlayerUnit*` still carry controllers; live `Allies_*`/`EnemyUnit*`/`Hero_*` prefabs must not.
- `PlayerController` is a plain `BaseController`: no invincibility, no hit-back trigger, no attack SFX;
  `AttackRoutine` caches `FinAttackRate` once in `OnEnable`.
- Suspected issues in `ShowResultUI`: a missing `StageRewardSO` row returns early (no panel, no save,
  timeScale stays 0); `AdditionalIronProduction` multiplies where wood adds; no re-entry guard (two
  defeat sources → double penalty). `IsBattleStarted` is never cleared on defeat or tutorial victory.
- `UIGiveUpPanel` No sets timeScale 1, dropping ×2/×3. `UITimer` reopens the pre-spawn speech every
  frame in the last 3 s. A finished defense wave restarts the HQ loop even during a timed wave.
- HQ skill cooldowns live in UI (`HQSkillsCooldown`); `FindTarget` only uses skill 0's range.
- The tutorial overwrites `DeckPresets[ActiveDeckIndex].BaseUnitDatas` in place.
- `baseFoodGainBySupplyLevel[0]` (84) exceeds levels 2–4; appears to be a test value.
- `EnemyWaveSystem.Update` key 3 is not editor-guarded. Editor cheats (`GameManager.Update`): 1/2 speed,
  C/V/B/H kill or halve HQs/Player.
- Dead code: `BaseHQ.Dead`'s `CancelInvoke("SpawnUnit")`, `SpawnUnitEvent`, `PlayerLevelUpEvent`,
  `BuffType`/`DebuffType`, `GolemAIController`, `SceneLoaderBattle.map`, `UITest.UpdateLayout`.
