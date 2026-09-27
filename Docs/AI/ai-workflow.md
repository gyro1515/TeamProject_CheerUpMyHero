# AI 워크플로우 셋업 (팀원용)

Parent: [`PROJECT.md`](PROJECT.md). 이 문서는 사람이 읽는 설명서입니다. AI 에이전트가 따를
규칙 원본은 [`.AI/flow.md`](../../.AI/flow.md)이고, 이 문서와 어긋나면 그쪽이 맞습니다.

Claude Code와 Codex는 둘 다 같은 지시문, 같은 스킬, 같은 스크립트를 씁니다. 그래서 어느 쪽에서
작업해도 절차와 결과가 같습니다. 무엇이 강제이고 무엇이 자유인지는 §8에 표로 정리했습니다.

## 1. 구성 한눈에 (라우터 트리)

모델별 루트 파일은 얇게 두고, 두 모델이 공유하는 내용은 모두 공통 루트 `Docs/AI/PROJECT.md`
아래에 트리로 모았습니다. 라우터(`[router]`) 문서마다 자기 하위 트리를 보여 주고, 각 문서는
첫머리의 `Parent:` 줄로 부모를 가리킵니다. 그래서 AI는 작업에 필요한 가지만 따라 내려가 읽습니다.

```
CLAUDE.md          Claude 루트: 공통 루트를 import + Claude 전용(스킬 스텁, 훅, 리뷰어 에이전트)
AGENTS.md          Codex 루트: "공통 루트 먼저 읽기" + Codex 전용(훅 설치·신뢰, 샌드박스 권한)
└─ Docs/AI/PROJECT.md  [router]      공통 루트: 첫 작업 절차, 프로젝트 요약, 전체 트리
   ├─ .AI/flow.md  [router]           작업 규칙: 세션 폴더, 변경 흐름·게이트, 위험 영역, 협업
   │  ├─ .AI/tools/session.sh         세션 작업 폴더 도구
   │  ├─ .AI/tools/doc_check.sh       문서·트리 검사기                    ← 아래 §7
   │  └─ .agents/skills/cross-review/SKILL.md  [router]   교차검증 스킬 (권장, 요청 시 실행)
   │     ├─ .AI/reviewer.md           리뷰어 공용 페르소나
   │     ├─ .AI/cross-review.conf     리뷰어 모델·추론강도 팀 기본값   ← 아래 §3
   │     └─ .AI/tools/cross_review.sh 라운드 실행기
   ├─ Docs/AI/development.md          빌드·컴파일 게이트·테스트·커밋·코드 스타일
   ├─ Docs/AI/architecture.md  [router]   기존 구조(공통 인프라) + 게임 시스템별 문서로 라우팅
   │  ├─ Docs/AI/systems/app-shell.md        시작 씬·튜토리얼·메인 메뉴·설정·오디오·입력·공용 팝업
   │  ├─ Docs/AI/systems/player-data.md      세이브 구조·로드 순서·저장 시점·재화
   │  ├─ Docs/AI/systems/territory.md        영지 5×5: 건물·비용·수리·이동·건물 시너지
   │  ├─ Docs/AI/systems/deck-and-units.md   유닛 카드·덱 프리셋·유닛 시너지
   │  ├─ Docs/AI/systems/artifacts.md        유물: 인벤토리·장착·합성/강화·스탯·액티브 스킬
   │  ├─ Docs/AI/systems/gacha-and-rewards.md 가챠·천장·우편·광고 보상·지급 중복/유실 위험
   │  ├─ Docs/AI/systems/stages.md           스테이지 id·해금·웨이브·운명/도전 수정자
   │  └─ Docs/AI/systems/battle.md           전투: 시작 순서·식량/소환·지휘관·유닛 AI·데미지·전투 종료
   ├─ Docs/AI/module-rules.md         새 코드 규칙(소유·통신·EventManager·프리팹)
   ├─ Docs/AI/ai-workflow.md          이 문서
   ├─ .github/workflows/doc-check.yml CI: PR과 main/Develop push마다 문서 검사   ← 아래 §7
   └─ .github/pull_request_template.md PR 체크리스트
```

모델마다 다르게 적어야 하는 지시는 `CLAUDE.md`나 `AGENTS.md`에만 넣고, 두 모델이 똑같이 따라야
하는 내용은 공통 트리에 넣습니다. 스킬 원본은 `.agents/skills/`에 있고, Claude 쪽
`.claude/skills/<이름>/SKILL.md`는 원본과 같은 머리말(frontmatter)에 "원본을 읽고 따르라"는 짧은 안내만
담은 스텁 파일입니다. 스킬 내용은 원본에서만 고칩니다. 둘이 어긋나면 문서 검사(§7)가 실패합니다.

## 2. 클론 후 한 번만 할 일

1. **Codex를 쓴다면** 프로젝트 훅을 설치하고 신뢰합니다.
   - `.AI/tools/session.sh install-codex-hook`를 실행합니다. Codex 샌드박스는 `.codex/`에 쓸 수
     없으니, Codex 안에서는 권한 상승(escalation)을 승인하거나 일반 터미널에서 직접 실행하세요.
     설치 전에는 `session.sh current`·`new`·`bind`가 설치 안내를 한 줄 출력합니다.
   - Codex TUI에서 `/hooks`로 훅을 신뢰합니다.
   - 설치 명령은 우리 항목을 목록 맨 앞에 넣습니다. 그래서 이미 신뢰해 둔 다른 도구의 항목도
     순번이 바뀌어 `/hooks` 재신뢰를 한 번 더 요청받을 수 있습니다.
   - `.codex/hooks.json`은 **머신 로컬 파일**이라 git에서 제외됩니다. Cate 같은 도구가 이 파일에
     자기 훅을 절대경로로 병합하기 때문입니다. 설치 명령은 병합 방식이라 여러 번 실행해도 안전하고,
     실행 시점에 파일에 있던 다른 도구의 항목은 그대로 둡니다. 다만 Cate가 **바로 그 순간** 같은
     파일을 쓰면 그 변경이 사라질 수 있습니다. 그러니 Cate가 훅을 설정하는 중에는 실행하지 마세요.
     Cate 항목이 없어졌다면 Cate에서 훅 연결을 다시 하면 됩니다.
2. **Claude Code**는 할 일이 없습니다. 훅이 추적 파일인 `.claude/settings.json`에 들어 있습니다.
   개인 설정은 `.claude/settings.local.json`에 두세요. 이 파일은 git에서 제외됩니다.
3. 훅이 동작하지 않아도 작업은 할 수 있습니다. `.AI/tools/session.sh current`가 같은 정보를
   알려 줍니다.
4. **Windows**: 도구는 bash 스크립트라서 Git Bash나 WSL에서 실행합니다. 심링크는 더 쓰지 않으므로
   심링크 설정이나 개발자 모드는 필요 없습니다. 기존 클론에서 두 가지 문제가 생길 수 있습니다.
   - 예전에 `.claude/skills/cross-review`를 손으로 폴더로 바꿔 두었다면 pull이
     "untracked working tree files would be overwritten"으로 멈춥니다. 그 폴더를 지우고 다시
     pull하세요.
   - `core.autocrlf=true`로 받은 클론은 스크립트가 CRLF로 남아 Git Bash에서 실패할 수 있습니다.
     `rm .AI/tools/*.sh && git checkout -- .AI/tools/`로 한 번 다시 받으세요. 새 클론은
     `.gitattributes`가 LF로 받게 합니다.

## 3. 교차검증 모델·추론강도 설정 (요금제별)

`cross-review` 스킬은 Claude 리뷰어와 Codex 리뷰어를 둘 다 띄웁니다. 기본값은 비싼 모델과
`high` 추론이고, 관점 하나마다 family별로 리뷰어가 1명씩 붙습니다. 그래서 관점이 2개면 리뷰어가
4명 실행됩니다. 요금제가 낮다면 **팀 기본값 파일은 그대로 두고**, 개인 오버라이드 파일을 만드세요.

```
# .AI/cross-review.local.conf   (git 제외 — 본인 머신에만 적용)
CLAUDE_MODEL=claude-sonnet-5
CLAUDE_EFFORT=medium
CODEX_EFFORT=medium
MAX_PERSPECTIVES=1
```

| 키 | 의미 | 값 |
|---|---|---|
| `CLAUDE_MODEL` | `claude -p --model`에 전달 | 전체 ID 또는 별칭(`sonnet` 등) |
| `CLAUDE_EFFORT` | `claude --effort` | `low` / `medium` / `high` / `xhigh` / `max` |
| `CODEX_MODEL` | `codex exec -m`에 전달 | 계정에서 쓸 수 있는 Codex 모델 |
| `CODEX_EFFORT` | Codex `model_reasoning_effort` | `minimal` / `low` / `medium` / `high` / `xhigh` |
| `MAX_PERSPECTIVES` | 라운드당 관점 수 상한 (비우면 상한 없음) | 숫자 |
| `REVIEW_POLICY` | 교차검증을 언제 돌릴지 | `recommended`(기본: AI가 제안, 동의하면 실행) / `required`(AI가 묻지 않고 실행) |

- 파일은 `KEY=VALUE` 형식으로 한 줄에 하나씩 씁니다. 따옴표와 공백은 쓰지 않고, `#`부터는
  주석입니다. 파일은 파싱만 하고 셸로 실행하지 않습니다. 모르는 키나 잘못된 값이 있으면 라운드가
  시작 전에 멈춥니다.
- 오버라이드할 키만 적으면 나머지는 `.AI/cross-review.conf` 값을 씁니다.
- 판정마다 실제로 실행한 설정이 `<task>/<family>-config-r<n>-p<k>.txt`에 남습니다. 라운드 요약은
  `config-r<n>.txt`에 쌓이고, 결과표의 MODEL/EFFORT 열에도 표시됩니다. 실행한 모델이 설정과
  다르면 그 판정은 무효입니다. Codex는 배너로, Claude는 `modelUsage`로 확인합니다. Claude의
  effort는 요청값만 기록하고, 실제로 적용됐는지는 검증하지 않습니다.
- 팀 전체의 기본값을 바꾸려면 `.AI/cross-review.conf`를 수정해서 커밋합니다.
- 교차검증을 항상 받고 싶다면 개인 파일에 `REVIEW_POLICY=required` 한 줄을 넣으세요. AI는 세션마다
  `.AI/tools/cross_review.sh --policy`로 이 값을 확인합니다.
- 한쪽 CLI만 설치되어 있어도 `--only claude` 또는 `--only codex`로 한쪽 리뷰를 돌릴 수 있습니다.
- 두 family 중 한쪽 CLI가 없거나 한도를 넘으면 라운드는 `ONE-SIDED`로 끝나고 수렴으로 치지
  않습니다. 교차검증은 **양쪽이 모두 있어야** 합의가 됩니다. 한쪽 CLI만 쓸 수 있는 사람은 동의하면
  한쪽 리뷰(`--only claude` 또는 `--only codex`)를 받을 수 있지만, 보고서에는 `ONE-SIDED`로 적히고
  합의로 치지 않습니다.

## 4. 레포에 남는 것 / 로컬에만 남는 것

| 레포(추적) | 로컬(git 제외) |
|---|---|
| `CLAUDE.md`, `AGENTS.md`, `Docs/AI/*` | `.AI/sessions/` — 세션별 작업 기록(STATE.md, 리뷰 판정, 로그) |
| `.AI/flow.md`, `.AI/reviewer.md`, `.AI/cross-review.conf`, `.AI/tools/*` | `.AI/cross-review.local.conf` — 개인 모델 설정 |
| `.agents/skills/*`, `.claude/skills/*`(스텁), `.claude/agents/*` | `.claude/settings.local.json` — 개인 Claude 설정 (Cate 훅 포함) |
| `.claude/settings.json` | `.codex/hooks.json` — `install-codex-hook`로 생성 |
| `.github/workflows/doc-check.yml`, `.github/pull_request_template.md` | `.cate/` — Cate 앱 작업공간 상태 |
| `README.md` 맨 위 안내, `.gitignore`, `.gitattributes` | `.claude/worktrees/` — Claude Code 워크트리 |

## 5. 세션 작업 폴더

AI가 레포 파일을 고치는 작업을 시작하면 `.AI/sessions/<YYMMDD>-<slug>/STATE.md`가 만들어집니다.
여기에는 요청 원문, 작업 목록, 결정 사항("(내 판단)" 표시), 진행 로그가 기록됩니다. 컨텍스트가
압축되거나 세션을 재개해도 AI는 이 파일부터 읽고 이어서 작업합니다. 이전 작업을 이어가려면
"`<세션 이름>` 이어서 해줘"라고 말하면 됩니다. 목록은 `.AI/tools/session.sh status`로 볼 수 있습니다.

## 6. Claude ↔ Codex를 함께 쓸 때 (Cate)

두 터미널을 Cate로 연결했다면, 한쪽 AI가 다른 쪽에 작업이나 검토를 요청할 수 있습니다.
요청과 답은 세션 폴더의 파일로 주고받고, 터미널에는 그 파일을 가리키는 한 줄만 입력합니다.
자세한 규칙은 `.AI/flow.md` §4에 있습니다.

## 7. 문서·트리 동기화와 CI 문서 검사

코드나 문서를 바꾸면 그 때문에 틀려진 문서도 같이 고쳐야 합니다. AI와 사람이 같은 기준으로
작업하도록 세 가지 장치를 두었습니다.

- **AI 작업 절차**: AI는 `.AI/flow.md` §2에 따라 수정 전에 문서 검사 기준선을 기록하고, 수정 후
  문서를 동기화한 다음 기준선과 비교합니다. 이번 변경이 새로 만든 결함만 실패로 치고, 원래
  있던 결함은 보고서에 "pre-existing"으로 수정 제안과 함께 적습니다. 고칠지는 사용자가 정합니다.
- **CI (`doc-check`)**: 모든 PR과, `main`·`Develop`에 push된 커밋마다(로컬 병합 후 push 포함) GitHub Actions가
  `.AI/tools/doc_check.sh`를 돌립니다. 로컬 설정은 필요 없습니다. push한 뒤에는 GitHub의 커밋 옆
  ✓/✗나 Actions 탭에서 결과를 확인하세요. 실패 메일은 GitHub 알림 설정에서 Actions 알림을 켠
  경우에만 옵니다. ✗가 이전부터 있던 결함 때문일 수도 있으니 로그를 읽어 보고, 오래된 결함은
  빨리 고쳐 두세요. 계속 빨간 CI는 아무것도 알려 주지 못합니다. 필수 체크는 아니라서 병합을
  막지는 않습니다.
- **PR 템플릿**: PR을 열면 "틀려진 문서를 고쳤나요?"와 "CI 결과를 확인했나요?"가 체크리스트로
  나옵니다. 템플릿은 `main`에 들어간 뒤부터 보입니다.

로컬에서 직접 돌리려면 bash와 python3가 필요합니다: `.AI/tools/doc_check.sh`. 이 검사는 트리에
적힌 경로가 실제로 있는지, 각 문서의 `Parent:`가 부모 트리와 맞는지, 트리에 빠진 문서가 없는지,
`AGENTS.md`의 첫 작업 문단이 `PROJECT.md`와 같은지, 문서에 적힌 레포 경로·링크가 살아 있는지,
Claude 스킬 스텁이 원본과 맞는지를 봅니다. 파일 이름의 대소문자는 macOS·Windows에서는 구분되지
않으므로 CI(Linux) 결과가 기준입니다. 문서의 설명 내용이 코드와 맞는지는 기계로 볼 수 없어서,
AI 절차와 리뷰어가 맡습니다. 새 하위 라우터가 필요하면 `PROJECT.md` 전체 트리에서 그 문서에
`[router]`를 붙이면 검사기가 부모로 인정합니다(`architecture.md`가 그 예).

시스템 문서(`Docs/AI/systems/`)의 "Gotchas"에는 조사 중 발견한 **의심 버그**(`Suspected issue:`)가
적혀 있습니다. 고치지 않고 기록만 한 것이니, 그 시스템을 수정할 때 참고하세요.

## 8. 무엇이 강제이고 무엇이 자유인가

각자의 작업 방식은 존중합니다. CI가 모든 사람에게 검사하는 것은 **문서·라우터 트리의 구조**
하나뿐이고, 그것도 병합이나 push를 막지 않습니다. AI는 그 밖에 자기 작업에 대해 컴파일 게이트를
돌립니다(아래 표).

| 층 | 대상 | 적용 대상 | 강제 수준 |
|---|---|---|---|
| 문서·트리 구조 | `Docs/AI/`, `.AI/`, `.agents/`, `.claude/`, `.codex/`, `.github/`의 트리, `Parent:`, 고아 문서, 문서에 적힌 레포 경로·링크, 스킬 스텁 | 모든 사람 (CI) | CI `doc-check`가 ✓/✗만 표시. 필수 체크가 아니어서 막지 않음 |
| AI 작업 절차 | 세션 폴더와 `STATE.md`, 문서 기준선·동기화, C# 변경 후 Unity 배치 컴파일 게이트, 한국어 보고 (`.AI/flow.md`) | Claude·Codex로 작업할 때만 | AI가 스스로 따름. 사람에게는 적용 안 됨 |
| 교차검증 | `cross-review` 스킬 | AI 작업 | **권장**. AI가 제안하고, 사용자가 요청하거나 동의할 때만 실행. 개인 설정 `REVIEW_POLICY=required`면 묻지 않고 실행 (§3) |
| 코드 규칙 | `module-rules.md`(소유·통신·EventManager·프리팹), `development.md`(스타일·커밋·브랜치) | AI가 코드를 쓸 때 참고 | 기계 검사 없음. 기존 코드는 예외 목록에 두고 고치지 않음 |
| 개인 설정 | 리뷰어 모델·추론강도, `REVIEW_POLICY`, `settings.local.json` | 각자 | 자유 (git 제외 파일) |

- 사람이 코드를 바꾸다 CI에 걸리는 경우는 하나뿐입니다. 문서에 적힌 코드 경로(예:
  `Assets/GlobalScripts/SceneLoader.cs`)를 옮기거나 지우면 ✗가 뜹니다. 이때 해당 문서의 경로를
  고치면 됩니다. 클래스·메서드 이름 변경은 기계로 잡지 못하니, 관련 문서가 있으면 같이 고쳐 주세요.
- 브랜치 이름, 커밋 메시지, 코딩 스타일, 작업 도구(GUI·CLI·AI 여부)는 검사하지 않습니다.
- CI를 병합 필수 체크로 바꾸는 것은 레포 설정(브랜치 보호)에서 소유자가 정합니다.
