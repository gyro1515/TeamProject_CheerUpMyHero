# App shell (start, main menu, settings, audio, input, tutorial, common UI)

Parent: [`../architecture.md`](../architecture.md). Everything around the gameplay: the title/login
screen, the first-launch story and tutorial, the main-menu hub, the settings/pause popup, BGM/SFX,
the back button, and the shared popups and widgets other screens reuse.

## Where it lives
| Path | Role |
|---|---|
| `Assets/Scripts/Scenes/SceneLoader/SceneLoaderStart.cs` | StartScene bootstrap: opens `StartUI`, shows the loading overlay, awaits backend init |
| `Assets/Scripts/UI/Start/` | `StartUI` (title → login → story), `SecondStartGroup` (login buttons), `LoginConfirmPopup` ("coming soon" OK popup, reused elsewhere), `StoryScrollController`, `BlinkingText` (file `BlinkText.cs`), `GoogleMobileAdsConsentController` (fake consent) |
| `Assets/Scripts/Scenes/SceneLoader/SceneLoaderMain.cs` | MainScene bootstrap: picks the first visible panel from `GameManager.LoadMain` |
| `Assets/Scripts/UI/Guide/` | `UIMenu` (the hub), `UIGuide` (unit/artifact/synergy encyclopedia) + its two explanation popups, cosmetic `MenuBackgroundChanger`/`MenuRotatingBanner`/`ToggleUIController` |
| `Assets/Scripts/UI/MainScreen/MainScreenUI.cs` | "Wisdom"/territory screen root (its Building/LordManor/Diplomacy children are other docs) |
| `Assets/Scripts/UI/MainScreen/` (small files) | `LaterUpdatePopup` (OK popup with message), `PopupBackground` (tap-outside closes a `BaseUI`), `LoadingText` + `AutoRotate` (on `UI_Loading`) |
| `Assets/Scripts/UI/Pause/` | `UIPause` (gear/pause + battle speed), `UISettingMenu`, sub-panels (FPS, exit, game option, control layout, give-up) |
| `Assets/Scripts/UI/Tutorial/` | `UITutorialBase` paged panels: `UITutorialMain`, `UITutorialDeck`, `UITutorialBattle`, `UITutorialHero` (file `UITurorialHero.cs`) |
| `Assets/Scripts/Manager/AudioManager.cs`, `Assets/Scripts/DB/Data/AudioData.cs`, `Assets/Scripts/UI/AudioSettingUI.cs` | Audio singleton, clip table SO, volume sliders |
| `Assets/Scripts/Manager/SettingDataManager.cs` | Control-layout pref, battle speed, wave-warning option (also stage-unlock cache, see Gotchas) |
| `Assets/Scripts/Manager/InputManager.cs`, `Assets/PlayerInput/` | Back button only (`PlayerInput.cs` is the generated wrapper) |
| `Assets/Scripts/Manager/AdManager.cs` | Google Mobile Ads init + consent (reward flow: see the ads/backend doc) |
| `Assets/GlobalScripts/FadeManager.cs` | Screen fade + `SwitchGameObjects` panel swap + `CanvasGroup` fade helpers |
| `Assets/Scripts/UI/SystemPopup.cs`, `Assets/Scripts/UI/ErrorPopUP.cs` | Awaitable system dialog; fatal init error |
| `Assets/Scripts/UI/UIAdvancedButton.cs`, `Assets/Scripts/UI/UISafeArea.cs` | Short-click/hold button events; notch safe-area anchors |
| `Assets/Scripts/Base/Framework/ScrambleText.cs`, `Assets/Scripts/Base/Framework/PriorityQueue.cs`, `Assets/Scripts/Base/Framework/ReadOnlyAttribute/` | Text scramble effect (hero cut-scene), binary-heap PQ (splash targeting, HQ skill), inspector `[ReadOnly]` |
| Resources | `Assets/Resources/Prefabs/UI/` (`StartUI`, `UIMenu`, `MainScreenUI`, `UIPause`, `UITutorial*`, `SystemPopup`, `ErrorPopUp`, `UI_Loading`), `Assets/Resources/Sound/GameMixer.mixer`, `Assets/Resources/Sound/SoundData.asset` |

## How it works

**First launch (StartScene).** `AudioManager`, `FadeManager`, `SceneLoader` self-create
`BeforeSceneLoad`. `SceneLoaderStart.Awake`: `OpenUI<StartUI>()` → `UIManager.ShowLoading()` (overlay
`UI_Loading` blocks touches and back) → `BackendManager.CheckInterentAsync()` (a no-op on first launch;
on a later return to StartScene it re-checks network and reloads player data) →
`EnsureInstanceAndInitializedAsync()`. That first `BackendManager.Instance` access starts
`InitializeAndLoginAsync` (network retry popup → UGS init → **automatic anonymous sign-in**, which
calls `GetUI<StartUI>().SetPlayerId` → Analytics → Economy config → `AdManager.InitializeAsync` (not on
WebGL) → `PlayerDataManager.InitializeResourcesAsync` + `LoadDataFromCloundAsync` → `HideLoading`). Any
exception opens `ErrorPopUP` (quit only).

`StartUI`: tap anywhere (`clickToMove`) → `SecondStartGroup`. **The login buttons are cosmetic**:
Guest calls `StartUI.OnLoginSuccess`; Google/Apple show `LoginConfirmPopup` whose OK also calls
`OnLoginSuccess`. `OnLoginSuccess`: tutorial complete → `StartLoadScene(MainScene)`; else fade to
`StoryScrollController` (auto-scroll `scrollDuration`, hold = `fastScrollMultiplier`, skip/finish →
`MainScene` if complete, else `BattleScene`).

**Tutorial gating.** Flag: `GameManager.IsTutorialCompleted` (static wrapper over the instance field;
`GameManager.Awake` sets it false). Set true only in (1) `PlayerDataManager.LoadDataFromCloundAsync`
when a Cloud Save `PlayerSaveData` exists ("saved data exists ⇒ tutorial done"), and (2)
`UITutorialDeck.OnSkipButtonClicked`, which then calls `SaveDataToCloudAsync().Forget()`.
`PlayerDataManager.SaveDataToCloudAsync` returns early while the flag is false, so the player snapshot
is not saved during the tutorial (Economy currency grants still are; see Gotchas). Flow: story → BattleScene with `SelectedStageIdx` (-1,-1), which
`SceneLoaderBattle` turns into 1-1 → `UIPause.Start` opens `UITutorialBattle` → `UITimer` opens
`UITutorialHero` near the first wave → victory → `RewardPanelUI.OnReturnToMainButton` sets
`LoadMain.TutorialInWisdom` → MainScene opens `MainScreenUI`, whose `Start` opens `UITutorialMain` →
adviser/deck button sets `IsStageAndDestinySelected` and stage (0,1) → `DeckPresetController` (clears
the active preset) opens `UITutorialDeck` → closing it completes the tutorial.

Every read of the flag (grep `IsTutorialCompleted`):
| Where | Effect while tutorial incomplete |
|---|---|
| `StartUI.OnLoginSuccess`, `StoryScrollController.OnSkipClicked` | show story; go to BattleScene |
| `UIPause.Start` | open `UITutorialBattle` (BattleScene only) |
| `UITimer` (3 checks) | hero timer fixed at `tutorialTotalTime` 40 s (the comment says 15), open `UITutorialHero` at 30 s left, skip first-wave hero line |
| `UIPlayerUnitSpawnPanel.Awake` | fixed deck 100001–100004 |
| `UIActiveAfPanel.Awake` | registers 5 tutorial artifacts into `PlayerDataManager` |
| `EnemyHQ.Dead` | skip artifact select, show result directly |
| `GameManager.ShowResultUI` | no defeat resource penalty |
| `PlayerDataManager` (`OnBattleEnded`; `SaveDataToCloudAsync`) | no tile damage; no save |
| `RewardPanelUI` (3 checks) | hide penalty text and extra buttons; set `LoadMain.TutorialInWisdom` |
| `MainScreenUI` (Awake/Start/deck button/back) | hide back button, open `UITutorialMain`, force stage (0,1), block back |
| `DeckPresetController` (Awake/OnEnable) | empty the preset, open `UITutorialDeck` |
| `UIRandomAndChallenge` | "no destiny/challenge" popups |

**Tutorial panels.** `UITutorialBase` pages through a serialized `tutorialSteps` GameObject list
(prefab-authored: Main 13, Deck 21, Battle 9, Hero 1 pages); last "next", skip, and back all call
`OnSkipButtonClicked` → `CloseUI`. `UITutorialBattle`/`UITutorialHero` set `Time.timeScale = 0` in
**`Awake`** and publish `TutorialSkipEvent` (struct in `UITutorialBattle.cs`) on close; `UIPause`
restores its speed on it. Replay: settings "tutorial retry" opens Main/Deck/Battle by
`UISettingMenu.uiState` (0/1/2).

**Main menu (MainScene).** All panels are `UIManager` prefabs created active. `SceneLoaderMain.Awake`
creates them and closes all but one, chosen by `GameManager.LoadMain`: `None` → `UIMenu`;
`DeckPresetController` (after defeat "redeploy"); `UIDestinyRoullette` (after victory "next"); `TutorialInWisdom`
→ `MainScreenUI`. It resets `LoadMain` to `None`. `UIMenu` buttons: Wisdom → `MainScreenUI`; Battle →
`DeckPresetController` if `GameManager.IsStageAndDestinySelected` else `UIStageSelect`; Gacha →
`GachaUIPanel` (fires Cloud Code `WakeUpServer` unawaited); Enforce → `UIArtifactUpgrade`; Guide →
`UIGuide`; Post/Notice → `PostBox` popups; Store/Allies → `LoginConfirmPopup` ("later update").
Navigation is always `UIManager.Instance.fromUI = FromUI.X; FadeManager.Instance.SwitchGameObjects(from, to)`
(fade out, `SetActive(false/true)`, fade in; ignored while `isFadeOut`). Targets read `fromUI` to
return (`UIStageSelect`, `DeckPresetController`, `GachaUIPanel`, `UIArtifactUpgrade`).

**Back button.** `InputManager` enables action map `ReturnBtn`/`Return` (binding `*/{Back}`: Android
back, Escape) and publishes `InputManager.BackButtonPressedEvent`; `UIManager` calls `OnBackPressed`
on the top of its `IBackButtonHandler` stack (ignored while loading; empty stack does nothing). Pushers
include every `BasePopUpUI` (for example `UIStageSelect`, `GachaUIPanel`, `DeckNameEditPanel`), `UIArtifact`,
`UIArtifactUpgrade`, `UIMenu` (back = open settings), `MainScreenUI` (back = `UIMenu`), `UIGuide`,
`DeckPresetController`, `UITutorialBase`, and `UIPause` only in BattleScene (back = pause). Push in
`OnEnable`, pop in `OnDisable`.

**Settings / pause.** There is no shared settings prefab: a `UIPause` + `UISettingMenu` subtree is
copied into `StartUI`, `UIMenu`, `MainScreenUI`, `DeckPresetController`, `UIArtifact`, and `UIPause`
prefabs. `UIPause._pauseButton` (the gear) sets `GameManager.IsPaused`, `Time.timeScale = 0`, opens the
settings `BasePopUpUI`; closing it runs `UISettingMenu.OnDisable` → `IsPaused = false` +
`OnResumeButton` → `UIPause.ApplySpeed(CurrentSpeed)`. Speed button cycles x1→x2→x3 (blocked while
settings open); applied only when `SceneManager.GetActiveScene().buildIndex == 2` (BattleScene),
else x1. Wave warning resets to x1 if `SettingDataManager.IsSpeedChangedInWaring`. Buttons active per
copy: give-up only in `UIPause`; exit (quit app) in StartUI/MainScreenUI/UIMenu; game option
(`UIGameSettingPanel`) in UIMenu/UIPause; FPS (30/60/120 → `Application.targetFrameRate`) and sound
in all. `UIGiveUpPanel` yes → `GameManager.ShowResultUI(false)`.

**Audio.** `AudioManager` loads `Resources/Sound/GameMixer` (groups `BGM`, `SFX`; exposed params
`MasterVolume`/`BGMVolume`/`SFXVolume`). BGM is automatic: on `SceneManager.sceneLoaded` it calls
static `PlayBGM(SceneState, 0.3f)` → `SelectBGM` (`StartScene` → `startBGM`, `MainScene` → `mainBGM`,
`BattleScene` → `BossStageBGM` when sub-stage index 8, else `chapter{main+1}BGM`; `EmptyScene` → null,
so BGM stops during every transition and restarts). SFX API used by callers:
`AudioManager.PlayOneShot(clip, vol)`, `PlayRandomOneShot(clips, vol)`,
`PlayOneShotByCameraDistance(clip, transform, vol)` (×`attenuation` 0.2 beyond 7.5 world-x units
from `Camera.main`), `PlayRandomOneShotByCameraDistance`. Clips come from `DataManager.AudioData`
(`Resources.Load<AudioData>("Sound/SoundData")`) or a serialized `AudioClip`; `BasePopUpUI` plays its
`openSound`/`closeSound` at 0.3.

**Ads init.** `AdManager.InternalInitializeAsync`: `GoogleMobileAdsConsentController.GatherConsent`
(fake, always succeeds, added at runtime) → `MobileAds.Initialize` → preload a rewarded ad. No banner
or interstitial exists. Ad unit is forced to Google's test ID (`#if true`).

## Data and persistence
- **PlayerPrefs (all device-local keys in the project):** `Master_VOL`, `BGM_VOL`, `SFX_VOL` (float
  0–1; constants duplicated in `AudioManager` and `AudioSettingUI`; written on slider change, `Save()`
  in `AudioSettingUI.OnDestroy`), `ControlPanelLayoutType` (int 0/1, `SettingDataManager`),
  `DontAskAgain_EmptyDeck` (deck system).
- **Not persisted (memory only):** `SettingDataManager.SavedSpeed`, `IsSpeedChangedInWaring`, FPS.
- **Tutorial:** no own key; implied by the existence of Cloud Save `PLAYER_DATA_KEY`.
- **Static:** `AudioData` SO (hand-authored, not Excel), `SoundData.asset`.

## Common changes
- **Play a new SFX:** add `public AudioClip` to `AudioData`, assign it in `SoundData.asset`, call
  `AudioManager.PlayOneShot(DataManager.AudioData.x)` (or `...ByCameraDistance` for in-world sounds).
- **Change a settings button/panel:** edit all six prefab copies (see Settings), and keep
  `uiState` right per copy.
- **Show a dialog:** `bool ok = await UIManager.Instance.GetUI<SystemPopup>().ShowMessageAsync(title, msg, PopupStyle.ConfirmOnly | RetryOrCancel | RetryOrQuit);`
  For a fixed "coming soon" message, serialize a `LoginConfirmPopup` and call `Show()`; for a custom
  one-button message, `LaterUpdatePopup.Show(message)`. New popups: derive from `BasePopUpUI`.
- **Add a menu destination:** add the button in `UIMenu`, cache `GetUI<T>()` in `Start`, set
  `fromUI`, `SwitchGameObjects`; add `GetUI<T>().CloseUI()` in `SceneLoaderMain.Awake`; make the target's
  back handler read `fromUI`.
- **Add a tutorial panel:** subclass `UITutorialBase`, prefab `Resources/Prefabs/UI/<Class>` with
  `tutorialSteps`, open it behind an `IsTutorialCompleted` check.

## Gotchas
- Quitting mid-tutorial loses the `PLAYER_DATA` progress (units, deck, grid, unlocks: nothing is
  snapshotted until `UITutorialDeck` closes), but **not currencies**: the tutorial victory's stage
  rewards go through `PlayerDataManager.AddResource` straight to Economy. A replayed tutorial grants
  them again. If the one fire-and-forget save
  at the end fails, the next launch replays the tutorial.
- `SignInAnonymouslyAsync` calls `GetUI<StartUI>()`: pressing Play in another scene instantiates the
  title UI there. On a later StartScene visit the guest id text is never set.
- Suspected issue: `SystemPopup` keeps one `_tcs`; a second `ShowMessageAsync` before the first
  answer orphans the first awaiter. It is a `BaseUI`, so not on the back stack.
- Suspected issue: `ErrorPopUP` prefab is named `ErrorPopUp` (case differs from the class used by
  `OpenUI<ErrorPopUP>()`); works only if `Resources.Load` stays case-insensitive. `ShowErrorPopUp` is unused.
- Suspected issue: `UITestEndPopup` has no prefab anywhere, yet `GameManager` calls
  `GetUI<UITestEndPopup>().OpenUI()` on clearing stage 2-9 → null reference instead of rewards.
- `UIManager.PopUI` pops the top regardless of who calls; `BasePopUpUI.CloseUI` pops only after its
  fade, so overlapping open/close can pop the wrong handler.
- `UITutorialBattle`/`Hero` pause time only in `Awake`: a replay via settings does not pause.
- `UIPause` subscribes `TutorialSkipEvent` in `Awake`, unsubscribes in `OnDisable` (C5 exception);
  it also adds a lambda to `EnemyWaveSystem.OnWarningDisplayed` it never removes. Speed uses a build
  index (2), not `SceneState`.
- Control layout: saved and previewed by `UIControlSettingPanel`, but `UITest.UpdateLayout` is never
  called, so the setting has no in-game effect. `UISettingMenu` has no button field that opens it.
- `AudioManager`: `InitSFXPool` creates the list only when `sfxPoolSize <= 0` (always true for the
  runtime-created instance), so `PlaySFX`/`SFXSource` are dead; `PlaySFXByResource` ignores `path`;
  instance `PlayBGM(AudioClip)`, `StopBGM`, `Get*VolumeLinear` are unused; `masterVolume`/`bgmVolume`/
  `sfxVolume` fields stay 1 (real volume is the mixer). `AudioSettingUI.OnBackPressed` is empty.
  Unused `AudioData` clips: `levelUpSE`, `buttonTouchSE`, `optionOpenSE`, `optionQuitSE`, `heroBuffSE`.
- `UIAdvancedButton` reads legacy `Input.mousePosition`; it relies on Active Input Handling = Both.
- Dead/legacy: `MainScene.cs` and `SceneLoadManager` (only `Z_ForTest` uses it), `ResourceManager.cs`
  (fully commented), `OutlineExample`, `SceneLoaderStart.nextScene`, `MainScreenUI.OpenPanel`,
  `GoogleMobileAdsConsentController.ShowPrivacyOptionsForm`, orphan prefabs
  `Assets/Prefabs/UI/UIPause_NoGiveUp.prefab`, `Assets/Prefabs/UI/BattleScene/GameSettingPanel.prefab`,
  `Assets/Prefabs/UI/UISafeAreaPanel.prefab`, `Assets/Prefabs/SoundSetting.prefab` (test scene only).
- `SystemPopup`, `ErrorPopUp`, tutorial prefabs are saved active: `GetUI<T>()` alone shows them.
