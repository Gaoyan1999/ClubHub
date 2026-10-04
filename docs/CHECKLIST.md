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
- [x] Project references and NuGet packages (EF Core SQLite, ML.NET, CommunityToolkit.Mvvm, NUnit)
- [x] `.gitignore` / `.gitattributes`
- [x] Main window with side menu and page navigation (MVVM)
- [x] App runs on Windows and shows the Members page with seed data

## 2. Core — models, enums, interfaces (A)

- [x] Enums: `MemberRole`, `EventType`, `RsvpStatus`, `EntryType`
- [x] Models: `Club`, `Member`, `Room`, `Rsvp`, `BudgetEntry`
- [x] Abstract `Event` + `Workshop`, `Social`, `Competition` with `EstimateCost()` override
- [x] `Member` constructor overloads
- [x] Interfaces: `IRepository<T>`, `IClashChecker`, `IAttendancePredictor`, `IExporter<T>`
- [x] Extension method: `DateTime.Overlaps(...)`
- [ ] Input validation rules on models/services (required fields, email format, end > start, capacity > 0, amount > 0)

## 3. Data layer (A)

- [x] `ClubHubDbContext` with TPH mapping for events
- [x] Generic `Repository<T>`
- [x] `DbSeeder` with starter club, members, rooms, events
- [ ] Seed RSVPs and budget entries for upcoming events
- [ ] Seed ~300 synthetic past events with RSVPs + check-ins (ML training data)
- [ ] CSV export: `CsvExporter : IExporter<T>` for members and attendance
- [ ] Error handling around database calls (friendly message, no crash)

## 4. Screen 1 — Members (A)

- [x] Member list in a DataGrid
- [ ] Search box (filter by name, student ID, email)
- [ ] Role filter dropdown
- [ ] Add member dialog (modal) with validation
- [ ] Edit member dialog
- [ ] Delete with confirmation
- [ ] Right-click menu (edit / delete)
- [ ] Export members to CSV button
- [ ] Duplicate student ID check

## 5. Screen 2 — Events & Calendar (B)

- [ ] Event list for the selected club
- [ ] Month calendar view showing event days
- [ ] Create/edit event form: title, type, room, start/end date + time, capacity, price, type-specific fields
- [ ] Validation: end after start, capacity ≤ room capacity, price ≥ 0
- [x] `ClashChecker` — room clash logic
- [ ] Room clash warning blocks saving
- [ ] Member clash warning (member RSVPed to two overlapping events)
- [ ] Delete event with confirmation

## 6. Screen 3 — Event Detail & Check-in (B)

- [ ] Event summary (time, room, type, price, capacity bar)
- [ ] RSVP a member (Going or Waitlisted when full)
- [ ] `WaitlistService` — cancel promotes first waitlisted member (`Queue<Rsvp>`)
- [ ] `MemberPromoted` C# event → notification in UI
- [ ] Tabs: Going / Waitlist / Cancelled
- [ ] Check-in checkboxes + search
- [ ] Predicted attendance shown ("48 RSVPs → about 35 expected")
- [ ] Rain warning for outdoor events (optional, weather API)

## 7. Screen 4 — Budget (C)

- [ ] Income/cost entries per event in a DataGrid
- [ ] Add/edit/delete entry with validation
- [ ] Auto ticket income = check-ins × ticket price
- [ ] Estimated cost (`EstimateCost()`) vs actual cost
- [ ] Bar chart: income vs cost per event
- [ ] Club balance total

## 8. Screen 5 — Dashboard (C)

- [ ] Add LiveCharts2 package
- [ ] `StatsService` with LINQ + lambda queries
- [ ] Stat cards: members, upcoming events, attendance rate, no-show rate
- [ ] Pie chart: events by type
- [ ] Line chart: attendance or money over time
- [ ] Date range slider/filter

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

- [x] Clash checker tests (3)
- [x] Seeder tests (2)
- [ ] Waitlist tests: full event → waitlisted; cancel → promoted in order
- [ ] Budget tests: totals, estimated cost per event type (polymorphism)
- [ ] Stats tests: attendance rate, no-show rate
- [ ] Validation tests: bad email, end before start, negative amount
- [ ] All tests pass in VS 2022 Test Explorer

## 11. Rubric check (All)

- [ ] Polymorphism used for a real purpose (`EstimateCost()` shown in Budget screen)
- [ ] 2+ interfaces in real use
- [ ] Generics / generic collections in real use
- [ ] LINQ + lambda in real use
- [ ] NUnit tests present and passing
- [ ] Enums, properties, extension methods, delegates/events in use
- [ ] 4+ distinct screens with their own job
- [ ] 6+ UI element categories (buttons, grids, dropdowns, date pickers, calendar, checkboxes, tabs, charts, slider, context menu, modal, progress bar)
- [ ] Every screen resizes cleanly (small window and maximised)
- [ ] Error handling: no crash on bad input, missing DB, ML or API failure
- [ ] Input validation on every form
- [ ] Code quality: consistent formatting, clear names, short helpful comments
- [ ] Bonus: WPF ✅ · EF Core ✅ · ML.NET model working · (optional) weather API

## 12. Integration and polish (Tue 13 – Wed 14 Oct)

- [ ] All feature branches merged; `main` builds with no errors
- [ ] Fresh DB test: delete `clubhub.db`, run, everything seeds and works
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
- [ ] Zip: solution + all projects (no `bin/`, `obj/`, `.vs/`, `*.db`) + `Report.pdf` + `README.txt`
- [ ] Unzip on a clean Windows machine → open in **VS 2022** → build → run → run tests
- [ ] Team leader submits on Canvas (aim: midday Fri 16 Oct)
- [ ] Download the submission from Canvas and test it again
- [ ] Practise the demo once; each member can explain their own code (32998: Q&A)
