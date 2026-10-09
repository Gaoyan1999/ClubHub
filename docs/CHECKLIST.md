# ClubHub — Project Checklist

> Tick `[x]` when a task is finished **and** merged into `main`.
> Owner tags: **(A)** data/members/tests · **(B)** events/RSVP/check-in · **(C)** budget/dashboard/ML · **(All)** everyone.
> Details for each task: [`PROJECT_PLAN.md`](PROJECT_PLAN.md). Due **Fri 16 Oct 2026, 11:59 pm**.

---

## 0. Admin and setup

- [ ] (All) Confirm team members, roles A/B/C, and team leader
- [ ] (All) Submit project registration form — https://forms.office.com/r/deQr7rTGXt
- [ ] (All) Check the idea with the tutor at the lab
- [ ] (All) Find out the demo date and time
- [ ] (All) Every member can clone, build and run the app on Windows
- [x] README with build/run steps and Mac → VM workflow

## 1. Solution skeleton

- [x] Solution with 5 projects: Core, Data, ML, Wpf, Tests (.NET 9)
- [x] Project references and NuGet packages (EF Core PostgreSQL + SQLite for tests, ML.NET, CommunityToolkit.Mvvm, NUnit)
- [x] `.gitignore` / `.gitattributes`
- [x] Main window with side menu and page navigation (MVVM)
- [x] (B) Club dropdown in side menu; selected club shared with every screen (F1)
- [x] App runs on Windows and shows the Members page with seed data

## 2. Core — models, enums, interfaces (A)

- [x] Enums: `MemberRole`, `EventType`, `RsvpStatus`, `EntryType`
- [x] Models: `Club`, `Member`, `Room`, `Rsvp`, `BudgetEntry`
- [x] Abstract `Event` + `Workshop`, `Social`, `Competition` with `EstimateCost()` override
- [x] `Member` constructor overloads
- [x] Interfaces: `IRepository<T>`, `IClashChecker`, `IAttendancePredictor`, `IExporter<T>`
- [x] Extension method: `DateTime.Overlaps(...)`
- [x] Input validation rules on models/services (required fields, email format, end > start, capacity > 0, amount > 0)

## 3. Data layer (A)

- [x] `ClubHubDbContext` with TPH mapping for events
- [x] Move to cloud PostgreSQL (external database bonus); connection string in gitignored `appsettings.json`
- [x] Generic `Repository<T>`
- [x] `Repository.Find` takes an `Expression` so filters run as SQL `WHERE`; failed saves are undone
- [x] `DbSeeder` with starter club, members, rooms, events
- [x] Seed RSVPs and budget entries for upcoming events (incl. a full event with a waitlist)
- [ ] Seed ~300 synthetic past events with RSVPs + check-ins (ML training data)
- [x] CSV export: `CsvExporter<T> : IExporter<T>` for members and attendance
- [x] Error handling around database calls (friendly message, no crash)

## 4. Screen 1 — Members (A)

- [x] Member list in a DataGrid
- [x] Search box (filter by name, student ID, email)
- [x] Role filter dropdown
- [x] Add member dialog (modal) with validation
- [x] Edit member dialog
- [x] Delete with confirmation
- [x] Right-click menu (edit / delete)
- [x] Export members to CSV button
- [x] Duplicate student ID check

## 5. Screen 2 — Events & Calendar (B)

- [x] Event list for the selected club
- [x] Month calendar view showing event days
- [x] Create/edit event form: title, type, room, start/end date + time, capacity, price, type-specific fields
- [x] Validation: end after start, capacity ≤ room capacity, price ≥ 0
- [x] `ClashChecker` — room clash logic
- [x] Room clash warning blocks saving
- [x] Member clash warning (member RSVPed to two overlapping events)
- [x] Delete event with confirmation

## 6. Screen 3 — Event Detail & Check-in (B)

- [x] Event summary (time, room, type, price, capacity bar)
- [x] RSVP a member (Going or Waitlisted when full)
- [x] `WaitlistService` — cancel promotes first waitlisted member (`Queue<Rsvp>`)
- [x] `MemberPromoted` C# event → notification in UI
- [x] Tabs: Going / Waitlist / Cancelled
- [x] Check-in checkboxes + search
- [x] Predicted attendance shown ("48 RSVPs → about 35 expected") — placeholder predictor until the ML model is done
- [ ] Rain warning for outdoor events (optional, weather API)

## 7. Screen 4 — Budget (C)

- [x] Income/cost entries per event in a DataGrid
- [x] Add/edit/delete entry with validation
- [x] Auto ticket income = check-ins × ticket price
- [x] Estimated cost (`EstimateCost()`) vs actual cost
- [ ] Bar chart: income vs cost per event
- [ ] Club balance total

## 8. Screen 5 — Dashboard (C)

- [x] Add LiveCharts2 package
- [x] `StatsService` with LINQ + lambda queries
- [x] Stat cards: members, upcoming events, attendance rate, no-show rate
- [x] Pie chart: events by type
- [x] Line chart: attendance or money over time
- [x] Date range slider/filter

## 9. Machine learning and external API (C)

- [x] Placeholder predictor so the app runs end to end
- [ ] Build training data from past events
- [ ] Train ML.NET regression model (features: type, day, hour, RSVPs, price, outdoor)
- [ ] Save/load the trained model file
- [ ] `MlAttendancePredictor : IAttendancePredictor` replaces the placeholder
- [ ] Record model accuracy (e.g. R², MAE) for the report
- [ ] (Optional) Open-Meteo weather service, fails safely with no internet
- [ ] Decide: weather API in or out (by Mon 12 Oct)

## 10. Tests — NUnit (All)

- [x] Clash checker tests (room + time clashes, 6)
- [x] Seeder tests (3)
- [x] Member validator, CSV exporter and repository tests (include, cascade delete)
- [x] Event tests: `EstimateCost()` polymorphism, counts; event validator tests
- [x] Waitlist tests: full event → waitlisted; cancel → promoted in order
- [x] Budget tests: totals, estimated cost per event type (polymorphism)
- [x] Stats tests: attendance rate, no-show rate
- [x] Validation tests: bad email, duplicate student ID, end before start, capacity, negative amounts
- [ ] All tests pass in VS 2022 Test Explorer (58 pass with `dotnet test` on macOS)

## 11. Rubric check (All)

- [x] Polymorphism used for a real purpose (`EstimateCost()` shown in Budget screen)
- [x] 2+ interfaces in real use
- [x] Generics / generic collections in real use
- [x] LINQ + lambda in real use
- [x] NUnit tests present and passing
- [x] Enums, properties, extension methods, delegates/events in use
- [x] 4+ distinct screens with their own job
- [x] 6+ UI element categories (buttons, grids, dropdowns, date pickers, calendar, checkboxes, tabs, charts, slider, context menu, modal, progress bar)
- [ ] Every screen resizes cleanly (small window and maximised)
- [ ] Error handling: no crash on bad input, missing DB, ML or API failure
- [ ] Input validation on every form
- [ ] Code quality: consistent formatting, clear names, short helpful comments
- [ ] Bonus: WPF ✅ · EF Core ✅ · ML.NET model working · (optional) weather API

## 12. Integration and polish (Tue 13 – Wed 14 Oct)

- [ ] All feature branches merged; `main` builds with no errors
- [ ] Fresh DB test: drop all tables in the cloud DB, run, everything seeds and works
- [ ] Full click-through of every screen on Windows
- [ ] Remove unused code, TODOs and placeholder text
- [ ] Consistent look (fonts, colours, spacing) across screens
- [ ] **Feature freeze** end of Tue 13 Oct

## 13. Report — PDF, 1500–2000 words (All)

- [ ] Introduction and Summary — purpose, motivation, key features (≥ 250 words) (A)
- [ ] Development approach — WPF, MVVM, EF Core, ML.NET, NUnit, Git, usage instructions (B)
- [ ] Flowchart — screens, services, data, ML (C)
- [ ] Role of team members — paragraph each + summary table (≥ 500 words) (All)
- [ ] Acknowledgments — including how AI tools were used (A)
- [ ] References — APA 7th (All)
- [ ] Screenshots of each screen with captions
- [ ] Word count 1500–2000 checked
- [ ] Exported to PDF

## 14. Submission (Thu 15 – Fri 16 Oct)

- [ ] `README.txt` in zip root: how to build, run, run tests; team members
- [ ] Zip: solution + all projects (no `bin/`, `obj/`, `.vs/`, `*.db`) + `Report.pdf` + `README.txt` + real `ClubHub.Wpf/appsettings.json` (tutor needs it to run)
- [ ] Unzip on a clean Windows machine → open in **VS 2022** → build → run → run tests
- [ ] Team leader submits on Canvas (aim: midday Fri 16 Oct)
- [ ] Download the submission from Canvas and test it again
- [ ] Practise the demo once; each member can explain their own code (32998: Q&A)
