# PR Review Checklist

Use this for every pull request in this repo, including your own. A review asks one question: **is this change correct, safe, and something I'd be happy to maintain?** It is not a style contest.

Work through the checklist in order. The first items matter most; readability comes last.

## 1. Get the branch and check it builds

```bash
gh pr checkout <number>                 # or: git switch <branch>
git diff --stat master...HEAD           # what changed, and how much
git log --oneline master..HEAD          # one meaningful commit per slice? messages about code/behaviour, no day or task numbers?
dotnet build BusBookingSystem.slnx
dotnet test BusBookingSystem.Tests      # add --filter "Category!=SqlServer" if no SQL Server
```

## 2. The checklist

1. **Intent.** Understand what the change is for before reading any code.
    - Read the PR title and description. What problem does it claim to solve?
    - Is it one coherent change, or several unrelated ones bundled together?
    - Does the size match the description?
2. **Correctness.** Does it do what it says?
    - Trace the happy path, then the edge cases: null, zero, empty, the maximum, two requests at the same time.
    - On every early `return`, check nothing is left behind (an open transaction, a half-saved change).
    - Check what disappeared. Removed lines matter as much as added ones.
3. **Security.**
    - Are secrets, passwords or connection strings with credentials being added?
    - Can a user reach something they shouldn't (`[Authorize]`, ownership checks)?
    - Do error responses leak internals such as exception messages or stack traces?
4. **Data and migrations.** These are hard to undo.
    - Does every model change come with a migration?
    - Do both `Up()` and `Down()` look right? Reviewers often skip `Down()`.
    - Could it break existing data, such as a new `NOT NULL` column with no default?
5. **Tests.**
    - Is there a test for the new behaviour, especially the failure cases?
    - Would the test actually fail if the code were wrong?
    - Did it run? (Step 1.)
6. **Project rules.** Check the change against `CLAUDE.md`: no repositories, `Result<T>` instead of throwing, thin controllers, explicit DTO projection, `is null`, `Async` suffixes.
7. **Readability.** Keep this light and leave it for last.
    - Look for unclear names, dead code, and comments that say *what* instead of *why*.
    - Prefix small points with "nit:" so the author knows they're optional.

## 3. Hot spots in this codebase

When a PR touches one of these, slow down.

| Where | What to check |
| --- | --- |
| `API/appsettings*.json` | No real secret. `JwtSettings:Secret` stays `""`; it comes from user secrets or `JwtSettings__Secret`. |
| `API/GlobalExceptionHandler.cs` | Still returns a generic 500 and never echoes `exception.Message`. |
| Controllers | Thin: call the service, `result.ToApiResponse()`, no `AppDbContext`, `[Authorize]` where needed. Every status has a `ProducesResponseType`. |
| Services with `BeginTransactionAsync` | `RollbackAsync()` before every early `return` and in every `catch`. `DbUpdateConcurrencyException` maps to 409, not 500. |
| Money | `decimal`, never `int` or `double` (casting to `int` truncates 3500.50 to 3500). |
| DTOs | No entities, navigation graphs, audit fields or password hashes exposed. |
| `Infrastructure/Migrations` | One migration per model change; existing migrations not hand-edited; `Down()` restores the previous state. |
| `Tests/` | Transactions, concurrency and check constraints are tested in `SqlServer/`, not InMemory. |
| `docs/` | Docs match the code; `PROGRESS.md` updated. |

## 4. Check before you comment

If something looks wrong, check the history before writing it up: `git log -p -- <file>` or the earlier migrations. Example from PR #1: a migration's `Down()` re-created a constraint named `greaterThanZero` with the SQL `>= 0`. It looked like a bug, but an earlier migration had already changed it to `>= 0` and kept the misleading name, so `Down()` was correct.

## 5. Leaving the review on GitHub

1. Open the **Files changed** tab.
2. Hover over a line, click **+**, and write your comment.
3. Click **Review changes** and pick one:
    - **Comment** for questions only.
    - **Approve** when it's ready to merge.
    - **Request changes** when something must be fixed first.

Good comments are specific, polite, and suggest a fix. For example: *"If `SaveChangesAsync` throws here, is the transaction rolled back? Maybe add a test for it."*

Merge with **Create a merge commit** (or **Rebase and merge**). Avoid **Squash and merge**: it collapses the PR into one commit, and this project keeps one commit per slice.

Reviewing your own PR? Read the diff as if a stranger wrote it.
