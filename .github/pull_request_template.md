## Summary

<!-- What changed and why, in one to three sentences. Describe behaviour, not files: "Bookings now hold their seats for 15 minutes", not "Edited BookingService.cs". -->

Closes #<!-- issue number -->

## How to verify

<!-- What a reviewer should run or look at to see it working: a test name, a request in CustomerJourney.http, a SQL query. Delete this section if CI covers everything. -->

## Definition of Done

<!-- Tick every line that is true. A line that does not apply (no model change, no API change): tick it and add "n/a". Do not merge with an unticked line. -->

- [ ] The PR builds and all tests pass in CI.
- [ ] New behaviour has tests, including failure cases.
- [ ] No secrets or personal details are committed.
- [ ] README and `docs/` match the new behaviour.
- [ ] If the model changed: one migration, `Up()` and `Down()` reviewed, and applied to a local database.
- [ ] If API behaviour changed: `CustomerJourney.http` and `docs/CustomerJourney.md` are updated, and the journey is re-run on a clean database.
- [ ] Reviewed against the PR review checklist.

## Notes for the reviewer

<!-- Optional: anything to look at first, decisions made, known limitations, follow-up issues. -->
