---
name: pre-push
description: "Trigger: git push, pushing changes, pushing commits, subir cambios, antes de pushear. Verify staged commits for unwanted files before pushing, and ask for confirmation if suspicious patterns are detected."
license: Apache-2.0
metadata:
  author: gentleman-programming
  version: "1.0"
---

# Skill: pre-push

## Activation Contract

Use this skill BEFORE executing `git push` (or any variant like `git push origin`). The orchestrator MUST load this skill when the user requests to push, or when it is about to push as part of a workflow.

Do NOT use this skill for local commits, merges, or other git operations — only pushes.

## Hard Rules

1. BEFORE calling `git push`, inspect what will be pushed. Run: `git log origin/{branch}..HEAD --name-status --oneline`
2. Parse the output for files that match ANY of these patterns:

   ```
   docs/
   .pi/
   .agents/skills/disenador/
   openspec/
   node_modules/
   scripts/
   release-*.txt
   AGENTS.md
   tasks.md
   design.md
   appsettings.Development.json
   ```

3. **Exclude deletions**: if the line starts with `D` (deleted), it is SAFE — the file is being REMOVED from tracking. Do NOT flag it.
4. **Flag additions and modifications** (lines starting with `A`, `M`, or no status prefix).
5. If any flagged files are found:
   - Show the user the list of suspicious files
   - ASK explicitly: "Detected files that should not be tracked. Want to continue anyway?"
   - If user says no (or equivalent), ABORT the push and suggest running `git rm --cached` on those files first
   - If user says yes, proceed with the push
6. If NO flagged files are found, proceed with the push without asking.
7. Only after passing this check, execute `git push`.

## Decision Gates

| Situation | Action |
|-----------|--------|
| Files detected in push | Show list, ASK user, proceed only if confirmed |
| Only deletions of unwanted files | Safe — proceed without asking |
| No unwanted files found | Proceed without asking |
| User declines to push | Abort, suggest `git rm --cached <files>` + `.gitignore` update |

## Execution Steps

1. Determine the current branch: `git branch --show-current`
2. Determine what will be pushed: `git log origin/{branch}..HEAD --name-status --oneline`
3. Parse output for unwanted patterns (excluding deletions)
4. If flagged files exist → show user + ask confirmation
5. If confirmed or no flags → execute `git push`

## Output Contract

Return:
- Whether the check passed or flagged files
- The list of flagged files (if any)
- Whether the push proceeded or was aborted
