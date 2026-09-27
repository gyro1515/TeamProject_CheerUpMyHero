# AI 워크플로우 셋업 (팀원용)

Parent: [`PROJECT.md`](PROJECT.md). 이 문서는 사람이 읽는 설명서입니다. AI 에이전트가 따를
규칙 원본은 [`.AI/flow.md`](../../.AI/flow.md)이고, 이 문서와 어긋나면 그쪽이 맞습니다.

Claude Code와 Codex는 둘 다 같은 지시문, 같은 스킬, 같은 스크립트를 씁니다. 그래서 어느 쪽에서
작업해도 절차와 결과가 같습니다.

## 1. 구성 한눈에

```
AGENTS.md                         공용 지시문 원본 (Codex는 자동으로 읽고, Claude는 CLAUDE.md가 import)
CLAUDE.md                         @AGENTS.md + Claude 전용 메모
.AI/flow.md                       작업 규칙: 세션 폴더, 변경 흐름·게이트, 위험 영역, Claude↔Codex 협업
.AI/reviewer.md                   교차검증 리뷰어 공용 페르소나
.AI/cross-review.conf             교차검증 리뷰어 모델·추론강도 팀 기본값      ← 아래 §3
.AI/tools/session.sh              세션 작업 폴더 도구
.AI/tools/cross_review.sh         교차검증 라운드 실행기
.agents/skills/cross-review/      교차검증 스킬 원본 (Codex 탐색 경로)
.claude/skills/cross-review       → 위 폴더를 가리키는 심링크 (Claude 탐색 경로)
.claude/agents/reviewer.md        Claude 쪽 읽기 전용 리뷰어 (Read/Grep/Glob만)
.claude/settings.json             Claude SessionStart 훅
Docs/AI/                          프로젝트 문서 트리 (PROJECT.md가 입구)
```

## 2. 클론 후 한 번만 할 일

1. **Codex를 쓴다면** 프로젝트 훅을 설치하고 신뢰합니다.
   - `.AI/tools/session.sh install-codex-hook`를 실행합니다.
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
4. **Windows**: 도구는 bash 스크립트이고, Claude 스킬 경로는 심링크입니다. 그래서 Git Bash나
   WSL에서 `git clone -c core.symlinks=true <url>`로 클론해야 하고, 심링크를 만들 수 있게
   개발자 모드나 관리자 권한도 필요합니다. 이 설정이 없으면
   `.claude/skills/cross-review`가 일반 텍스트 파일로 체크아웃되어, Claude 쪽에서만 스킬이
   보이지 않습니다.

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

- 파일은 `KEY=VALUE` 형식으로 한 줄에 하나씩 씁니다. 따옴표와 공백은 쓰지 않고, `#`부터는
  주석입니다. 파일은 파싱만 하고 셸로 실행하지 않습니다. 모르는 키나 잘못된 값이 있으면 라운드가
  시작 전에 멈춥니다.
- 오버라이드할 키만 적으면 나머지는 `.AI/cross-review.conf` 값을 씁니다.
- 판정마다 실제로 실행한 설정이 `<task>/<family>-config-r<n>-p<k>.txt`에 남습니다. 라운드 요약은
  `config-r<n>.txt`에 쌓이고, 결과표의 MODEL/EFFORT 열에도 표시됩니다. 실행한 모델이 설정과
  다르면 그 판정은 무효입니다. Codex는 배너로, Claude는 `modelUsage`로 확인합니다. Claude의
  effort는 요청값만 기록하고, 실제로 적용됐는지는 검증하지 않습니다.
- 팀 전체의 기본값을 바꾸려면 `.AI/cross-review.conf`를 수정해서 커밋합니다.
- 두 family 중 한쪽 CLI가 없거나 한도를 넘으면 라운드는 `ONE-SIDED`로 끝나고 수렴으로 치지
  않습니다. 교차검증은 **양쪽이 모두 있어야** 의미가 있습니다.

## 4. 레포에 남는 것 / 로컬에만 남는 것

| 레포(추적) | 로컬(git 제외) |
|---|---|
| `AGENTS.md`, `CLAUDE.md`, `Docs/AI/*` | `.AI/sessions/` — 세션별 작업 기록(STATE.md, 리뷰 판정, 로그) |
| `.AI/flow.md`, `.AI/reviewer.md`, `.AI/cross-review.conf`, `.AI/tools/*` | `.AI/cross-review.local.conf` — 개인 모델 설정 |
| `.agents/skills/*`, `.claude/skills/*`(심링크), `.claude/agents/*` | `.claude/settings.local.json` — 개인 Claude 설정 (Cate 훅 포함) |
| `.claude/settings.json` | `.codex/hooks.json` — `install-codex-hook`로 생성 |
| | `.cate/` — Cate 앱 작업공간 상태 |

## 5. 세션 작업 폴더

AI가 레포 파일을 고치는 작업을 시작하면 `.AI/sessions/<YYMMDD>-<slug>/STATE.md`가 만들어집니다.
여기에는 요청 원문, 작업 목록, 결정 사항("(내 판단)" 표시), 진행 로그가 기록됩니다. 컨텍스트가
압축되거나 세션을 재개해도 AI는 이 파일부터 읽고 이어서 작업합니다. 이전 작업을 이어가려면
"`<세션 이름>` 이어서 해줘"라고 말하면 됩니다. 목록은 `.AI/tools/session.sh status`로 볼 수 있습니다.

## 6. Claude ↔ Codex를 함께 쓸 때 (Cate)

두 터미널을 Cate로 연결했다면, 한쪽 AI가 다른 쪽에 작업이나 검토를 요청할 수 있습니다.
요청과 답은 세션 폴더의 파일로 주고받고, 터미널에는 그 파일을 가리키는 한 줄만 입력합니다.
자세한 규칙은 `.AI/flow.md` §4에 있습니다.
