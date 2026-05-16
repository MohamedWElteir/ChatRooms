---
name: git
description: Git workflow automation — commits, PRs, branch management, and CI bypass. Use when the user says commit, push, create PR, branch, merge, rebase, bypass CI, or any git operation.
---

# Git Workflows

## Quick start

```bash
# Create feature branch
git checkout -b feature/my-feature master

# Stage and commit
git add <files>
git commit -m "feat: add widget"

# Push and create PR
git push -u origin feature/my-feature
gh pr create --fill
```

---

## 1. Commit workflow

### Conventional commits

```
type(scope): description
```

| Type | When |
|------|------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code change with no behavior change |
| `test` | Adding/fixing tests |
| `docs` | Documentation |
| `chore` | Maintenance, deps, tooling |
| `style` | Formatting, whitespace |

Scope is optional — use the bounded context name (e.g. `fix(room): correct capacity guard`).

Breaking changes: add `!` before the colon (`feat!: redesign auth`).

### Before committing

Always verify:

1. `git status` — understand what changed
2. `git diff` — review unstaged changes; `git diff --cached` — review staged
3. Check recent commits for style: `git log --oneline -5`
4. Build + test locally if feasible

### Staging rules

- Never stage `.env`, `credentials.json`, `secrets`
- Warn user if they try to commit these
- Use `git add -A` or list specific files

---

## 2. Branch management

### Naming

| Prefix | Use |
|--------|-----|
| `feature/` | New features |
| `fix/` | Bug fixes |
| `chore/` | Maintenance |
| `refactor/` | Refactoring |

### Syncing with master

```bash
git fetch origin
git rebase origin/master
# or: git merge origin/master
```

Prefer rebase for feature branches to keep history linear.

---

## 3. PR workflow

### Creating a PR

```bash
gh pr create --title "type(scope): concise title" --body @'
## Summary
- <bullet points of changes>

## Testing
- [ ] Tests pass
- [ ] Manually verified
'@
```

### PR description format

```
## Summary
[1-3 bullet points of what changed and why]

## Testing
[Checklist or description of how tested]
```

### CI bypass

If CI fails but the change is low-risk or urgent:

1. Go to GitHub → Actions → **CI Bypass** workflow
2. Click **Run workflow**
3. Enter PR number and reason
4. Only users with access to the `ci-bypass` environment can trigger it

The bypass creates a passing check run and comments on the PR.

---

## 4. CI workflow

The `.github/workflows/ci.yml` runs on every push and PR:
1. `dotnet restore`
2. `dotnet build --no-restore --warningsaserrors`
3. `dotnet format --verify-no-changes`
4. `dotnet test --no-build`

Branch protection should require the `CI / test` check to pass before merging. Admins can bypass via GitHub UI or the CI Bypass workflow.
