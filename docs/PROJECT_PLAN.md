# ClubHub — Uni Club Event Manager — Project Plan

> 31927/32998 .NET Applications Development — Assignment 2 (Group Project)
> Due: **Friday 16 October 2026, 11:59 pm** (team leader submits one zip).
> Plan written: Sunday 4 October 2026.
> Read with: `Assignment2_Specification.md` (marking guide) and `Assignment2_Canvas_Page.md` (report format).

"ClubHub" is a working name — change it if you like.

---

## 1. The idea in one paragraph

University clubs and societies run events (workshops, socials, competitions) using spreadsheets and group chats. RSVPs, waitlists, check-in, clashing room bookings and money are all tracked by hand. ClubHub is a Windows desktop app for club admins that puts all of this in one place. It checks for clashes, runs a waitlist automatically, tracks the budget for each event, and uses machine learning to **predict how many people will actually turn up**, so clubs stop over-ordering food for no-shows.

---

## 2. Tech stack

| Part | Choice | Why |
|---|---|---|
| Framework | .NET 9, Visual Studio 2022 | Required by spec — code must compile in VS 2022 in the lab |
| UI | **WPF** | +2 bonus (instead of Windows Forms) |
| MVVM helper | CommunityToolkit.Mvvm | Less boilerplate for view models and commands |
| Database | **EF Core + SQLite** | Part of the +4 bonus; single `.db` file, nothing to install |
| Machine learning | **ML.NET** (regression) | Part of the +4 bonus — attendance prediction (LLM APIs do **not** count) |
| External API (optional) | Open-Meteo weather (free, no key) | Rain warning for outdoor events |
| Charts | LiveCharts2 (`LiveChartsCore.SkiaSharpView.WPF`) | Dashboard charts |
| Tests | **NUnit** | Required |

> **Windows only:** WPF needs Windows. Anyone on a Mac must build and test on a Windows PC, a lab machine, or a VM (e.g. Parallels). Always check the solution builds in **VS 2022** before pushing — if it does not compile in the demo, we get **zero**.

---

## 3. Solution structure

```
ClubHub.sln
├── ClubHub.Core/        Models, enums, interfaces, services (no UI, no EF code)
│   ├── Models/          Club, Member, Event (+ subclasses), Rsvp, BudgetEntry, Room
│   ├── Enums/           MemberRole, EventType, RsvpStatus, EntryType
│   ├── Interfaces/      IRepository<T>, IClashChecker, IAttendancePredictor, IExporter
│   ├── Services/        ClashChecker, WaitlistService, BudgetService, StatsService
│   └── Extensions/      DateTime / collection extension methods
├── ClubHub.Data/        ClubHubDbContext, Repository<T>, seed data, migrations
├── ClubHub.ML/          AttendancePredictor (ML.NET), training data builder
├── ClubHub.Wpf/         Views (XAML), ViewModels, navigation, App.xaml
└── ClubHub.Tests/       NUnit tests
```

**Rule:** `Core` does not reference `Data`, `ML` or `Wpf`. Everything talks through interfaces. This gives us "high cohesion, low coupling" for the Code Requirement marks.

---

## 4. Data model

| Class | Key fields |
|---|---|
| `Club` | Id, Name, Description, LogoPath, Members, Events |
| `Member` | Id, StudentId, FirstName, LastName, Email, Role (`MemberRole`), JoinedDate |
| `Room` | Id, Name, Capacity, IsOutdoor |
| `Event` (**abstract**) | Id, ClubId, Title, Start, End, RoomId, Capacity, TicketPrice, Rsvps, BudgetEntries |
| `Workshop : Event` | MaterialsCostPerHead |
| `Social : Event` | CateringCostPerHead |
| `Competition : Event` | PrizePool, TeamSize |
| `Rsvp` | Id, EventId, MemberId, Status (`RsvpStatus`), CreatedAt, CheckedIn |
| `BudgetEntry` | Id, EventId, Type (`EntryType`: Income/Cost), Amount, Description, Date |

Enums: `MemberRole { Member, Treasurer, Secretary, President }`, `EventType { Workshop, Social, Competition }`, `RsvpStatus { Going, Waitlisted, Cancelled }`, `EntryType { Income, Cost }`.

EF Core maps the `Event` subclasses with **TPH** (one table + a discriminator column).

---

## 5. Features and the logic behind them

### 5.1 Clash checking
- Two time ranges overlap when `a.Start < b.End && b.Start < a.End`.
- **Room clash:** same room, overlapping times → block saving, show a warning dialog.
- **Member clash:** a member RSVPs to two overlapping events → warn, allow if confirmed.
- Lives in `ClashChecker : IClashChecker`. Overlap test is an extension method `DateRange.Overlaps(...)`.

### 5.2 RSVP and waitlist
- If `Going` count < capacity → status `Going`, else `Waitlisted`.
- On cancel, the waitlist is rebuilt as a `Queue<Rsvp>` ordered by `CreatedAt`, and the first person is moved to `Going`.
- `WaitlistService` raises a C# event `MemberPromoted` (delegate) → UI shows a notification.

### 5.3 Check-in
- On the event day, tick `CheckedIn` for each attendee. Search box filters the list.

### 5.4 Budget
- Income/cost entries per event. Ticket income auto-calculated from check-ins × price.
- **Polymorphism:** `Event.EstimateCost()` is `abstract`; each subclass overrides it (materials, catering, prize pool). Budget screen shows *estimated vs actual*.

### 5.5 Dashboard
- Attendance rate, no-show rate, income vs cost per month, most popular event type.
- All stats via **LINQ with lambdas**, e.g.
  `events.Where(e => e.End < DateTime.Now).GroupBy(e => e.Type).Select(g => new { g.Key, Rate = g.Average(e => e.AttendanceRate()) })`

### 5.6 Attendance prediction (ML.NET)
- Regression model (e.g. FastTree). Features: event type, day of week, start hour, RSVP count, ticket price, is outdoor. Label: actual attendees.
- Training data: past events in the seed database. We will **generate realistic synthetic history** (~300 past events) — say so clearly in the report.
- Shown on the event detail screen: "48 RSVPs → about 35 expected to attend".
- Behind `IAttendancePredictor`, so tests can use a simple fake predictor.

### 5.7 Weather warning (optional, only if time allows)
- For outdoor rooms, call Open-Meteo for the event date; show a rain icon + warning.
- Must fail safely (no internet → hide the warning, no crash).

### 5.8 Export
- Export member list / attendance to CSV via `IExporter` (`CsvExporter`).

---

## 6. Screens (rubric: 4+ distinct, resizable, responsive)

| # | Screen | Main job | UI elements |
|---|---|---|---|
| 1 | **Members** | List/search/add/edit club members | DataGrid, search box, dropdown (role), buttons, right-click menu (edit/delete), modal dialog |
| 2 | **Events & Calendar** | Create/edit events, month view, clash warnings | Calendar, date/time pickers, dropdowns (type, room), numeric inputs, warning dialog |
| 3 | **Event Detail & Check-in** | RSVPs, waitlist, check-in, ML prediction | Tabs (Going / Waitlist), checkboxes, progress bar (capacity), image/icon, toast notification |
| 4 | **Budget** | Income/cost entries, estimate vs actual | DataGrid, form inputs, bar chart |
| 5 | **Dashboard** | Club-wide stats | Pie chart, line chart, stat cards, date-range slider |

Navigation: a left side menu in a main window; selecting a club is shared across all screens (counts for "communication between multiple interfaces").

**Responsive rules:** use `Grid` with `*` sizing, no fixed widths/heights on main layouts, set `MinWidth`/`MinHeight` on the window, test at small and maximised sizes.

**UI element categories used (rubric needs 6+):** buttons, data grids, dropdowns, date pickers, calendar, checkboxes, tabs, charts, sliders, context menus, modal dialogs, progress bars, images. ✅

---

## 7. Rubric checklist — where each item lives

| Rubric item | Where | Owner |
|---|---|---|
| Polymorphism | `Event.EstimateCost()` overridden in `Workshop`/`Social`/`Competition`; constructor overloads on `Member` | A |
| 2+ interfaces | `IRepository<T>`, `IClashChecker`, `IAttendancePredictor`, `IExporter` | A, B, C |
| NUnit tests | `ClubHub.Tests` — clash, waitlist, budget, stats | A (+ everyone tests own service) |
| Anonymous method / LINQ + lambda | `StatsService`, search filters | C |
| Generics / generic collections | `IRepository<T>`, `Repository<T>`, `Queue<Rsvp>`, `Dictionary<EventType, …>` | A, B |
| Enums, properties, extension methods | `Enums/`, `Extensions/` | A |
| Delegates / events | `WaitlistService.MemberPromoted` | B |
| File/database read & write, EF | EF Core SQLite + CSV export | A |
| Error handling | try/catch around DB, ML and API calls; friendly error dialogs | everyone |
| Input validation | required fields, email format, end > start, capacity > 0, amounts > 0 | everyone |
| Code quality | consistent formatting, short helpful comments, clear names | everyone |
| Bonus: WPF (+2) | `ClubHub.Wpf` | — |
| Bonus: EF + ML.NET / API (+4) | `ClubHub.Data`, `ClubHub.ML`, weather | A, C |

---

## 8. Team split (team of 3 — adjust if 2)

| Member | Name | Main area | Screens |
|---|---|---|---|
| **A** | _fill in_ | Solution setup, Core models/enums/interfaces, EF Core + seed data, repositories, NUnit test project, CSV export | Members |
| **B** | _fill in_ | Events, clash checking, RSVP + waitlist, check-in, main window + navigation | Events & Calendar, Event Detail |
| **C** | _fill in_ | Budget, stats (LINQ), charts, ML.NET prediction, weather API (optional) | Budget, Dashboard |

**Team of 2:** A takes A + half of C (budget); B takes B + the other half of C (dashboard, ML). Drop the weather API.

Each person: builds their own screens, writes NUnit tests for their own services, writes their own paragraph in "Role of team members" in the report.

---

## 9. Timeline

| Dates | Phase | Goal |
|---|---|---|
| **Sun 4 – Mon 5 Oct** | 0. Setup | Agree plan + roles. Submit registration form. Check idea with tutor at next lab. A creates repo + solution skeleton + all models/interfaces so B and C can start. |
| **Tue 6 – Thu 8 Oct** | 1. Foundations | DbContext, migrations, seed data (incl. ~300 synthetic past events). Each screen built with real data binding. Main window navigation working. |
| **Fri 9 – Mon 12 Oct** | 2. Features | Clash check, waitlist, check-in, budget, dashboard charts, ML model trained and shown. NUnit tests written alongside. |
| **Tue 13 Oct** | 3. Integration | Merge everything, fix bugs, validation + error handling pass, resizing check on every screen. **Feature freeze at end of day.** |
| **Wed 14 Oct** | 4. Polish + report | Bug fixes only. Draft report, take screenshots, draw flowchart, write README. |
| **Thu 15 Oct** | 5. Pack + test | Final report PDF. Build zip, unzip on a **clean Windows machine**, open in VS 2022, build, run, run tests. |
| **Fri 16 Oct** | 6. Submit | Team leader submits by **midday** (deadline 11:59 pm). Download the submission from Canvas and test it again. |

The demo is in our lab session — check the date with the tutor and practise the demo once before.

---

## 10. Working together

- **Git:** one GitHub repo. `main` must always build. Each person works on a feature branch (`feature/waitlist`, `feature/dashboard`, …) and merges by pull request; someone else glances at it before merging.
- **Never** commit `bin/`, `obj/`, `.vs/` (use the standard VS `.gitignore`). Commit the seed logic and migrations, **not** the `.db` file — the app creates and seeds it on first run.
- **Commit messages:** short, what changed (e.g. `add waitlist promotion`).
- **Naming:** PascalCase for classes/methods/properties, `_camelCase` for private fields, interfaces start with `I`. One class per file.
- **Comments:** short, only where the *why* is not obvious. XML doc comments (`///`) on public service methods.
- **Check-ins:** quick message in the group chat every day: done / doing / blocked.

---

## 11. Report plan (PDF, 1500–2000 words, 4 marks)

Follows the Canvas report format:

| Section | Min words | Writer |
|---|---|---|
| 1. Introduction and Summary (purpose, motivation, key features) | 250 | A |
| 2. Development approach (WPF, MVVM, EF Core, ML.NET, NUnit, Git; usage instructions summary) | — | B |
| 3. Flowchart (how screens, services, data and ML connect) | — | C |
| 4. Role of team members (each person's paragraph + summary table) | 500 | everyone |
| 5. Acknowledgments (incl. how AI tools were used, if used) | — | A |
| 6. References (APA 7th — libraries, Open-Meteo, any tutorials) | — | everyone adds as they go |

Keep a running list of every library, tutorial and AI tool used — we need it for sections 5 and 6.

---

## 12. Zip contents (final submission)

```
ClubHub_Group<N>.zip
├── ClubHub.sln + all project folders (source only, no bin/obj)
├── Report.pdf
└── README.txt   — how to open, build, run, run tests; test login/data notes; team members
```

---

## 13. Open decisions (agree at first meeting)

- [ ] Team size, names, and who is A / B / C
- [ ] Team leader (submits on Canvas, registered on the form)
- [ ] Final app name
- [ ] Weather API in or out (decide by Mon 12 Oct based on progress)
- [ ] Who has a Windows machine for builds; when we meet to merge on Tue 13 Oct
