# ClubHub — Uni Club Event Manager

31927/32998 .NET Applications Development — Assignment 2 (Group Project).

A WPF desktop app that helps university clubs manage members, events, RSVPs, check-in and budgets, with attendance prediction (ML.NET).
Full plan, roles and timeline: [`docs/PROJECT_PLAN.md`](docs/PROJECT_PLAN.md). Task checklist: [`docs/CHECKLIST.md`](docs/CHECKLIST.md).

## Requirements

**Windows (to run and debug the app)**
- Visual Studio 2022 **17.12 or later**, with the **.NET desktop development** workload
- .NET 9 SDK (installed with VS 2022 17.12+)

**macOS (to write code, build and run tests only)**
- .NET 9 or .NET 10 SDK
- WPF builds on macOS but **cannot run** there — run the app on Windows.

## Run it (Windows)

1. Clone the repo and open `ClubHub.sln` in Visual Studio 2022.
2. Copy `ClubHub.Wpf/appsettings.example.json` to `ClubHub.Wpf/appsettings.json` and paste in the
   database connection string (ask the team — it has the password, so it is **not** in Git).
3. Right-click **ClubHub.Wpf** → **Set as Startup Project**.
4. Press **F5**.

The app uses a shared **cloud PostgreSQL** database, so it needs internet. On first run it creates
the tables and seeds starter data. Everyone on the team shares the same data.

> **Changed a model class?** Tell the team first, then drop all tables in the cloud database —
> the app re-creates and re-seeds them on the next run. (We use `EnsureCreated()`, not migrations.)

## Run the tests

- Visual Studio: **Test → Run All Tests**
- Terminal (Windows or macOS): `dotnet test`

## Mac → Windows VM workflow

1. Write code on the Mac. Check it compiles: `dotnet build` (and `dotnet test`).
2. Commit and push your branch.
3. On the Windows VM: `git pull`, then run and debug in Visual Studio.

> **Mac build error `CS2001 ... .g.cs could not be found` or `... .baml cannot be found`?** This is a WPF build quirk on macOS (it does not happen in Visual Studio on Windows). Run `dotnet build` a second time; if it still fails, delete `ClubHub.Wpf/obj` and build twice.

## Solution structure

| Project | What goes in it |
|---|---|
| `ClubHub.Core` | Models, enums, interfaces, services, extension methods. No UI and no EF code. |
| `ClubHub.Data` | `ClubHubDbContext` (EF Core + PostgreSQL; tests use SQLite), `Repository<T>`, `DbSeeder` |
| `ClubHub.ML` | Attendance prediction (placeholder for now → ML.NET model) |
| `ClubHub.Wpf` | WPF app: `Views/` (XAML), `ViewModels/` (MVVM with CommunityToolkit.Mvvm) |
| `ClubHub.Tests` | NUnit tests |

### Adding a new screen

1. Add `ViewModels/XxxViewModel.cs` that inherits `PageViewModel`.
2. Add `Views/XxxView.xaml` (a `UserControl`).
3. Map them with a `DataTemplate` in `App.xaml`.
4. Add the view model to the `Pages` list in `MainViewModel`.

## Git rules

- `main` must always build. Work on a branch (`feature/waitlist`, `feature/dashboard`, …) and merge with a pull request.
- Never commit `bin/`, `obj/`, `.vs/` or `*.db` files (already in `.gitignore`).

## Team

| Member | Name | Area |
|---|---|---|
| A | _fill in_ | Data layer, members, tests |
| B | _fill in_ | Events, clash checking, RSVP/waitlist, check-in |
| C | _fill in_ | Budget, dashboard, ML.NET |
