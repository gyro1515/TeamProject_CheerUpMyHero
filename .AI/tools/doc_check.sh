#!/usr/bin/env bash
# Docs-and-tree check — one shared, read-only implementation for Claude Code, Codex, and CI.
# Parent: .AI/flow.md §2
#
#   doc_check.sh                        check the working tree; exit 0 clean, 1 findings
#   doc_check.sh --baseline <file>      record today's findings before you edit (always exit 0)
#   doc_check.sh --against <file>       fail only on findings that are not in the baseline
#
# Exit 2 means BLOCKED (no usable python3, or a bad argument). Paths are relative to the repo root.
# Checks: C1 tree paths exist; C2 each node's Parent names a router whose tree lists it; C3 no
# orphan shared or family node; C4 AGENTS.md repeats PROJECT.md's first-action paragraph; C5 repo
# paths and relative links in the docs (Docs/AI/ and its subfolders) exist; S Claude skill stubs
# match .agents/skills both ways. Routers: the roots, flow.md, skills, and any node PROJECT.md's
# tree marks `[router]`.
# The rules it enforces live in Docs/AI/PROJECT.md → "Keeping the tree honest".
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd -P)"
cd "$ROOT" || { echo "BLOCKED: cannot enter $ROOT"; exit 2; }

# A python3 that actually runs: the Windows Store alias and the macOS stub exist but fail.
PY=""
for c in python3 python; do
  if command -v "$c" >/dev/null 2>&1 &&
     "$c" -c 'import sys; sys.exit(0 if sys.version_info >= (3, 7) else 1)' >/dev/null 2>&1; then
    PY="$c"; break
  fi
done
[ -n "$PY" ] || { echo "BLOCKED: no usable python3 (doc_check.sh needs Python 3.7+)"; exit 2; }

exec "$PY" - "$@" <<'PY'
# -*- coding: utf-8 -*-
import glob, io, os, re, subprocess, sys

# usage: doc_check [--baseline FILE | --against FILE] [ROOT]
args, mode, base = sys.argv[1:], None, None
if args and args[0] in ("--baseline", "--against"):
    if len(args) < 2:
        print("BLOCKED: %s needs a file path" % args[0]); sys.exit(2)
    mode, base = args[0], os.path.abspath(args[1]); args = args[2:]
os.chdir(args[0] if args else ".")
findings = []

def read(p):
    with io.open(p, encoding="utf-8", errors="replace") as f:
        return f.read()

def tracked(pattern):
    """Files git would commit: indexed or new-but-not-ignored, and still on disk."""
    out = subprocess.run(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard",
                          "--", pattern], capture_output=True, text=True).stdout
    return sorted({l for l in out.split("\0")
                   if l and (os.path.isfile(l) or os.path.islink(l))})

def globn(pattern):
    return sorted(norm(p) for p in glob.glob(pattern, recursive=True))

# Only the AI workflow's own machine-local rules excuse a missing path (not e.g. `*.log`).
LOCAL = ("/.AI/", "/.claude/", "/.codex/", "/.cate/")

def ignored(path):
    out = subprocess.run(["git", "check-ignore", "-v", "--no-index", path],
                         capture_output=True, text=True).stdout
    rule = out.split("\t")[0].split(":", 2)[-1] if out else ""
    return rule.startswith(LOCAL)

def norm(path):
    return os.path.normpath(path).replace(os.sep, "/")

PLACEHOLDER = re.compile(r"<[^>]*>")
IGNORE_RULES = [l.strip() for l in read(".gitignore").splitlines()
                if l.strip().startswith(("/.AI/", "/.claude/", "/.codex/", "/.cate/"))] \
    if os.path.isfile(".gitignore") else []

def exists(path):
    """True if the path (with <placeholder> and * as wildcards) matches, or git ignores it."""
    if path.endswith("/") and not PLACEHOLDER.search(path) and not os.path.isdir(path):
        # a machine-local directory: some .gitignore rule covers it or a file under it
        return any(r.lstrip("/").startswith(path) or path.startswith(r.lstrip("/"))
                   for r in IGNORE_RULES)
    path = path.rstrip("/") or path
    if PLACEHOLDER.search(path) or "*" in path:
        if glob.glob(PLACEHOLDER.sub("*", path)):
            return True
        return ignored(PLACEHOLDER.sub("x", path).replace("*", "x"))
    return os.path.exists(path) or os.path.islink(path) or ignored(path)


# ---- router trees -------------------------------------------------------
GLYPHS_PRE = re.compile(r"^[\s│├└─]*")
ROUTERS = ["Docs/AI/PROJECT.md", "CLAUDE.md", "AGENTS.md", ".AI/flow.md"] + globn(".agents/skills/*/SKILL.md")
# Any other node the full tree marks `[router]` (e.g. Docs/AI/architecture.md) is a router too.
# Only lines inside PROJECT.md's tree fence(s) count, never prose.
for _fence in re.findall(r"```\n(.*?)```", read("Docs/AI/PROJECT.md"), re.S):
    if "├─" not in _fence and "└─" not in _fence:
        continue
    for _line in _fence.splitlines():
        _tok = GLYPHS_PRE.sub("", _line).split(" ")[0]
        if "[router]" in _line and _tok.endswith(".md") and os.path.isfile(_tok) and _tok not in ROUTERS:
            ROUTERS.append(_tok)
GLYPHS = re.compile(r"^[\s│├└─]*")
PATHLIKE = re.compile(r"(/|\.(md|sh|conf|json|py|txt)$)")

def tree_entries(router):
    """Repo paths named in the router's tree fence(s): first token of each line."""
    text, entries = read(router), []
    for fence in re.findall(r"```\n(.*?)```", text, re.S):
        if "├─" not in fence and "└─" not in fence:
            continue
        for line in fence.splitlines():
            tok = GLYPHS.sub("", line).split(" ")[0] if line.strip() else ""
            if not tok or tok.startswith("§") or not PATHLIKE.search(tok):
                continue
            if "/" not in tok:                      # bare name: next to the router, else root
                near = norm(os.path.join(os.path.dirname(router), tok))
                tok = near if exists(near) else tok
            entries.append(tok)
    return entries

TREES = {r: tree_entries(r) for r in ROUTERS}

FAMILY_ROOTS = ("CLAUDE.md", "AGENTS.md")

def listed(node, router):
    if router not in FAMILY_ROOTS:              # shared trees name each node explicitly
        return node in TREES[router]
    for e in TREES[router]:
        pat = PLACEHOLDER.sub("*", e)
        if glob.fnmatch.fnmatch(node, pat) or glob.fnmatch.fnmatch(node, pat + "/*"):
            return True
    return False

# C1: every tree path exists (or is git-ignored, i.e. machine-local)
for r, entries in TREES.items():
    for e in entries:
        if r not in FAMILY_ROOTS and (PLACEHOLDER.search(e) or "*" in e):
            findings.append("C1 %s: wildcard entry %s (shared trees list nodes explicitly)" % (r, e))
        elif not exists(e):
            findings.append("C1 %s: tree names missing path %s" % (r, e))

# C2: each Parent: names a router whose tree lists this node
PARENT = re.compile(r"^(?:#\s*|<!--\s*)?Parent:\s*(.*)$", re.M)
SHARED = tracked("Docs/AI/*") + tracked(".AI/*") + tracked(".agents/*") + tracked(".github/*")
for node in SHARED + tracked(".claude/*") + tracked(".codex/*"):
    if node == "Docs/AI/PROJECT.md" or node.endswith(".json") or os.path.islink(node):
        continue
    m = PARENT.search(read(node))
    if not m:
        findings.append("C2 %s: no Parent line" % node)
        continue
    link = re.search(r"\]\(([^)]+)\)", m.group(1))
    raw = link.group(1) if link else m.group(1).split()[0].strip("`").rstrip(".,;:")
    parent = norm(os.path.join(os.path.dirname(node), raw)) if link else norm(raw)
    if parent not in TREES:
        findings.append("C2 %s: parent %s is not a router" % (node, parent))
    elif not listed(node, parent):
        findings.append("C2 %s: parent %s does not list it in its tree" % (node, parent))

# C3: orphans — shared nodes in PROJECT's tree, family nodes in their root's tree
for node in SHARED:
    if node != "Docs/AI/PROJECT.md" and not listed(node, "Docs/AI/PROJECT.md"):
        findings.append("C3 %s: missing from Docs/AI/PROJECT.md tree" % node)
for prefix, root in ((".claude/", "CLAUDE.md"), (".codex/", "AGENTS.md")):
    for node in tracked(prefix + "*"):
        if not listed(node, root):
            findings.append("C3 %s: missing from %s tree" % (node, root))

# C4: AGENTS.md repeats PROJECT.md's first-action paragraph word for word
def first_action(text):
    sec = re.search(r"## First action of any repository work\n(.*?)(?=\n## )", text, re.S)
    paras = [p for p in sec.group(1).strip().split("\n\n") if not p.startswith("(")] if sec else []
    return " ".join(paras[-1].split()) if paras else None
a, p = first_action(read("AGENTS.md")), first_action(read("Docs/AI/PROJECT.md"))
if not a or a != p:
    findings.append("C4 AGENTS.md: first-action paragraph differs from Docs/AI/PROJECT.md")

# C5: backticked repo paths in the routed docs exist
PREFIX = re.compile(r"^(Assets|Docs|Packages|ProjectSettings|\.AI|\.agents|\.claude|\.codex|\.github)/")
for doc in sorted(set(globn("Docs/AI/**/*.md") + ROUTERS + [".AI/reviewer.md", "README.md"] +
                      globn(".claude/skills/*/SKILL.md") + globn(".github/*.md"))):
    for span in re.findall(r"`([^`\n]+)`", read(doc)):
        tok = span.split()[0].rstrip(".,;:)")
        if PREFIX.match(tok) and not exists(tok):
            findings.append("C5 %s: `%s` does not exist" % (doc, tok))
    for target in re.findall(r"\]\(([^)\s]+)\)", read(doc)):
        target = target.split("#")[0]
        if target and not re.match(r"^[a-z]+:", target) and \
                not exists(norm(os.path.join(os.path.dirname(doc), target))):
            findings.append("C5 %s: link target %s does not exist" % (doc, target))

# Skill stubs: .claude/skills/<n>/SKILL.md frontmatter == .agents/skills/<n>/SKILL.md
def frontmatter(p):
    m = re.match(r"---\n(.*?)\n---\n", read(p), re.S)
    return m.group(1) if m else None
for src in globn(".agents/skills/*/SKILL.md"):
    stub = src.replace(".agents/", ".claude/", 1)
    if os.path.islink(os.path.dirname(stub)):
        continue                                   # symlink layout: nothing to compare
    if not os.path.isfile(stub):
        findings.append("S %s: no Claude stub at %s" % (src, stub))
    elif frontmatter(stub) != frontmatter(src):
        findings.append("S %s: frontmatter differs from %s" % (stub, src))

for stub in globn(".claude/skills/*/SKILL.md"):   # reverse: a Claude-only skill breaks parity
    if not os.path.isfile(stub.replace(".claude/", ".agents/", 1)):
        findings.append("S %s: no canonical skill in .agents/skills/" % stub)

if mode == "--baseline":                   # record findings present before your edit
    os.makedirs(os.path.dirname(base), exist_ok=True)
    with io.open(base, "w", encoding="utf-8") as f:
        f.write("".join(x + "\n" for x in findings))
    print("doc_check: baseline of %d finding(s) written to %s" % (len(findings), base))
    sys.exit(0)
known = set()
if mode == "--against":
    if not os.path.isfile(base):
        print("BLOCKED: baseline %s not found" % base); sys.exit(2)
    known = set(io.open(base, encoding="utf-8").read().splitlines())
new = [x for x in findings if x not in known]
for x in findings:
    print(("pre-existing  " if x in known else "") + x)
print("doc_check: %d new, %d pre-existing finding(s)" % (len(new), len(findings) - len(new)))
sys.exit(1 if new else 0)
PY
