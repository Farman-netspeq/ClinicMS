# Self-Review Notes — ClinicMS (Milestones A–H)

Definition-of-Done checklist (Section 10) applied per module before senior review.

## Module A — Departments
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired
- [x] Server+client validation
- [x] Business rules tested manually
- [x] Audit logging (if applies)

**Tricky:** None — straightforward CRUD, no BRs attached.

## Module B — Doctors & Schedules
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired
- [x] Server+client validation
- [x] Business rules tested manually
- [x] Audit logging (if applies)

**Tricky:** Doctor creation also provisions an ApplicationUser in the Doctor role — kept both operations inside one service call so a failure doesn't leave an orphaned login.

## Module C — Patients
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired
- [x] Server+client validation
- [x] Business rules tested manually (BR1, BR7)
- [x] Audit logging (if applies)

**Tricky:** Soft-deleted (inactive) patients were leaking into the booking dropdown — fixed by adding an `IsActive` predicate to the lookup query, kept separate from the admin-facing "show inactive" filter.

## Module D — Appointment Booking
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired (cascading dropdowns)
- [x] Server+client validation
- [x] Business rules tested manually (BR1–BR3)
- [x] Audit logging (if applies)

**Tricky:** Overlap detection (BR2) — used a half-open interval check (`startA < endB && startB < endA`) against all non-cancelled appointments for the doctor/date; slot alignment (BR3) validated against `SlotDurationMinutes` server-side, not trusted from the client-selected slot.

## Module E — Appointment Lifecycle
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired (status actions refresh grid)
- [x] Server+client validation
- [x] Business rules tested manually (BR4, BR8, BR10)
- [x] Audit logging (BR9)

**Tricky:** Valid-transition enforcement (BR4) — modeled as a small dictionary of allowed (from-status, to-status) pairs checked before any write, so illegal jumps fail fast with a clear message instead of silent corruption.

## Module F — Medical Records & Prescriptions
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired + isolated custom JS for prescription item rows
- [x] Server+client validation
- [x] Business rules tested manually (BR5)
- [x] Audit logging (if applies)

**Tricky:** One-record/one-prescription-per-appointment (BR5) — enforced with a check-before-insert in the service (not just a unique DB constraint) so the user gets a clear message instead of a raw SQL error; ownership check (assigned doctor or Admin) applied before the completed-status check.

## Module G — Billing
- [x] Entity + EF config + migration applied
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired + isolated custom JS for invoice item rows
- [x] Server+client validation
- [x] Business rules tested manually (BR6)
- [x] Audit logging (BR9)

**Tricky:** Immutability after payment (BR6) — added a guard at the top of every update/add-line method that short-circuits with a failure result if `Status == Paid`, rather than relying on UI to hide the buttons.

## Module H — Dashboard & Audit
- [x] Entity + EF config + migration applied (audit log)
- [x] DTOs: Request/Response/ListItem/Filter
- [x] Interface in Application; impl in Infrastructure
- [x] Registered in DI
- [x] API controller returns ApiResponseDto<T>, correct status codes
- [x] Role checks match matrix
- [x] Web controller uses IHttpService
- [x] CustomAjax.js wired
- [x] Server+client validation
- [x] Business rules tested manually (BR9, BR10)
- [x] Audit logging (BR9)

**Tricky:** Role-scoped dashboard tiles — same tile queries reused across roles, scoped by injecting the current user's id/role into the filter rather than branching the query logic per role.
