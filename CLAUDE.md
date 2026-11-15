# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. General workflow rules (no implicit branches or worktrees, the 300-line limit for this file) are in the owner's personal `~/.claude/CLAUDE.md`.

## Overview

Kecske Projects (`kecske-projects.hu`) is a personal site for sharing media and files. It is a React SPA plus an ASP.NET Core Web API on SQL Server, both hosted on IIS on a Windows server. Features:

- Cookie login with roles. New accounts can register but can't log in until `Account.IsRegistrationApproved` is set (there is no UI for that yet).
- Browsing, streaming/downloading and chunked uploading of files in role-restricted directories under one base folder on the server.
- An admin-only page that whitelists an IP address for RDP by editing **Windows Firewall** rules; the whitelist expires after a configured time.

## Solution layout

- `Backend/`: ASP.NET Core Web API (`net11.0`, nullable on, Windows-only because of the firewall code).
- `DatabaseORM/`: class library with the EF Core context, scaffolded models, a generic repository/service layer and Mapster mappings. The backend references it.
- `Database/`: SSDT `.sqlproj`, the **source of truth for the schema**. It builds and publishes only from Visual Studio.
- `Frontend/`: React 19 + TypeScript + Vite. `Frontend/UI-Test/` is a static HTML/JS design mock-up, not part of the app.
- `Docs/`: `SECURITY_RECOMMENDATIONS.md` (security review with status per item) and `improvement-notes.md` (findings, ideas, code TODOs).
- `Build/`: **a separate private git repository**, ignored by this one. It holds the deployment scripts and the real config files (see Configuration). Never copy its contents into this repository.

There is no test project. `.github/workflows/` exists but is empty, so there is no CI.

## Commands

The backend has a `COMReference` to `NetFwTypeLib` (Windows Firewall), which the .NET SDK's MSBuild can't resolve. **`dotnet build` / `dotnet run` fail on `Backend.csproj` with MSB4803**; build it with Visual Studio's MSBuild:

```bash
"/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" Backend/Backend.csproj -restore -nologo -v:m
```

`DatabaseORM` alone builds with `dotnet build DatabaseORM/DatabaseORM.csproj`. Run the backend from Visual Studio (`https` profile: `https://localhost:7129`). Both projects build with 0 warnings; keep it that way.

Frontend (run inside `Frontend/`):

```bash
npm run dev
```

```bash
npm run build
```

```bash
npm run lint
```

`build` runs `tsc -b` then `vite build`. `lint` currently reports 52 problems (see `Docs/improvement-notes.md`); don't add new ones. `Frontend/.vscode/` is a stale Create React App leftover (`npm start`, port 3000).

## Configuration

`appsettings*.json` and `.env*` are git-ignored. `Build/Deployment/copy-dev-config.cmd` copies the development versions into place (`Backend/appsettings.json`, `Frontend/.env`, the SSDT publish profiles and `update-scaffold.txt`).

Backend keys (constants in `DatabaseORM/Constants/ConfigurationKeys.cs`):

| Key | Use |
|---|---|
| `ConnectionStrings:DatabaseConnection` | SQL Server; the app throws at startup if missing |
| `Environment` | `Development` or `Production`. **This, not `ASPNETCORE_ENVIRONMENT`, decides** via `IConfiguration.IsProduction()`. A missing value counts as Production (fail safe) |
| `FrontendDomains` | String array, the CORS origins |
| `BaseFileDirectory` | Root folder of the shared files |
| `RDPAccessExpirationMinutes` | How long a whitelisted IP stays in the firewall rule |
| `LogPath` | Log folder, relative to the app folder if not absolute (default `log`) |
| `Logging:FileLogger:MinLogLevel`, `EventId` | File logger settings (`EventId` 0 = all) |

Production vs development: Production gets strict `SameSite` cookies, no Swagger, no EF sensitive-data logging, a 5-minute firewall cleanup interval (1 minute in dev), and firewall rule names without the `[DEBUG] ` prefix. In development Swagger UI is served with the OpenAPI document at `/openapi/v1.json`.

Frontend `.env` keys: `VITE_BACKEND_URL` (the backend base URL **including `/api`**, because the frontend calls routes like `/Account/Login`) and `VITE_MAX_FILE_UPLOAD_SIZE_IN_GB` (the upload **chunk** size, see Files).

## Deployment

`Build/Deployment/Deploy-All.cmd` runs three scripts: `copy-db-patches.cmd` (copies `Database/Patches`), `backend-deploy.cmd` (VS MSBuild publish with `FolderProfile.pubxml`, then the production `appsettings.json` and `backend-web.config`), and `frontend-deploy.cmd` (`npm run build` with `.env.production`, then `frontend-web.config`). The output lands in `Build/Backend`, `Build/Frontend` and `Build/Database`. Site-wide HTTP headers for the frontend (e.g. the planned CSP) belong in `frontend-web.config`.

## Backend architecture

**Pipeline order in `Program.cs` matters:** `SecurityHeadersMiddleware` → `UseCors` → `UseRateLimiter` (must come after CORS, or 429 responses lack CORS headers and the browser hides them) → HTTPS redirection → authentication → authorization → controllers. `builder.Logging.ClearProviders()` removes the console logger; all logging goes to the file logger.

**Controllers:**
- Each has `[ApiController]`, `[ErrorLoggingFilter]` and `[Route("api/[controller]/[action]")]`, and derives from `ApiControllerBase<TService>`, which provides `service`, `Logger`, `LoggedInAccount` and the result helpers.
- Return through `ContentResult(data)`, `MessageResult(text)` or `ErrorResult(status, text)`. Every response body is a `GenericResponse` (`{ error, message, content }`, camelCase), which the frontend reads as `ResponseObject`.
- `ErrorLoggingFilter` handles unhandled exceptions. **Only an `ApiException` (`Communication/Internal/`) or a DatabaseORM `ServiceException` reaches the client**, with its message and a status code (the `ApiException`'s own, or the one `GetClientStatusCode` maps the `ServiceException` to). Any other exception is logged and becomes a 500 with a generic message, so internal details don't leak. In the backend, throw `ApiException` for errors the user should read (400/403/404/409). Services throw `ServiceException`s, so actions use service results directly without checking them.
- Controllers: `Account` (login, logout, register, reset password, users, secret key), `File` (list, download, upload), `FileDirectory` (accessible categories), `Security` (firewall whitelist).

**Authentication:** cookie auth (1 day, sliding, `HttpOnly`, `Secure`). `CustomCookieAuthenticationEvents` returns JSON 401/403 instead of redirecting. Roles are `ClaimTypes.Role` claims built in `ClaimsPrincipalTools`, so use `[Authorize(Roles = "Admin")]`. Resources expose roles as names (`List<string>`), never as `Role` entities. Passwords and reset keys are hashed with ASP.NET Identity's `PasswordHasher` and stored as `varbinary`. Registration returns a secret key (a GUID) that is the only way to reset the password.

**Rate limiting:** policy `RateLimitPolicies.Authentication` (5 requests/minute per IP) is applied to the whole `AccountController`. Actions used in normal browsing or behind a login opt out with `[DisableRateLimiting]`. Rejections return 429 with `Retry-After` and a `GenericResponse`.

**Files (`FileStorageService`, `FileTools`, `FileStorageTargetPathDetails`):**
- Paths are `{category}/{sub/folders}`. The first segment must match a `FileDirectory.RelativePath` row, and the account needs one of its roles (a directory with no roles is open to every logged-in user). The resolved path must stay inside the category folder (`FileTools.IsSameOrInsideDirectory`); no access gives 403, a missing or escaping path 404.
- An uploaded file's name comes from the client: always resolve it with `FileTools.GetFilePathInDirectory`, which rejects anything but a plain file name.
- `SubPath` values sent to the frontend use `/` separators (`FileTools.GetClientSubPath`).
- Files are identified by the MD5 of their lower-cased full path (`EncryptionTools`); `GetSingle/{path}/{hash}` serves them with range support, so videos stream.
- Hidden and system files and folders are skipped, in listings and downloads.
- Uploads are streamed with `MultipartReader` (`[DisableFormValueModelBinding]`). The frontend slices files into chunks of `VITE_MAX_FILE_UPLOAD_SIZE_IN_GB`. The first chunk is sent with `isNewFile=true` (fails if the file exists), and later chunks are **appended** to the file. The `fileName` form field must come before the file part.

**Firewall (`FirewallApiService`, `FirewallTools`, `FirewallRuleExpirationWatcher`):** whitelisted IPs are stored in `PermittedIpAddress` with an expiry and added to the inbound rules "RDP Whitelist TCP" / "RDP Whitelist UDP" (port 3389) through the `HNetCfg` COM API; the rules are created if missing. The watcher removes expired IPs; an empty rule falls back to `127.0.0.1`. **Running the backend locally edits this PC's real firewall** (with `[DEBUG] ` rule names) and needs admin rights.

**Logging:** `FileLogger` puts entries into a static `BlockingCollection` (`FileLoggerLogStorage`), and `FileLoggerBackgroundService` appends them to `Backend-Log-yyyy-MM-dd.txt` (UTC). The project suppresses CA2254 / CA1873, so interpolated log messages are accepted.

**Windows-only code** is marked `[SupportedOSPlatform("windows")]`; add it to new types that touch the firewall.

## DatabaseORM

- `DatabaseStartup.ConfigureDatabaseServices` registers the DbContext, `GenericRepository<>` and the concrete services. **Register every new service there.** Services are scoped.
- Services derive from the abstract `GenericService<TEntity>`, which only holds the `repository` and a `MapList` helper; services query through the repository. They return plain values (null or an empty list when nothing matches). For outcomes the user should read they throw a `ServiceException` subclass from `Communication/Exceptions/` (`NotFoundException` 404, `ConflictException` 409, `InvalidSecretException` 400; mapped in the backend's `ErrorLoggingFilterAttribute`, where an unmapped subclass becomes 400), documented with `<exception>` comments. Database errors stay ordinary exceptions and become a 500.
- Controllers get `*Resource` types (`Communication/Resource/`) through `MapList`, which uses Mapster. Add a `TypeAdapterConfig<,>.NewConfig().Compile()` line in `Mapping/MappingProfiles/MappingConfiguration.RegisterMappings` for every new mapping.
- The context sets the `SQL_Latin1_General_CP1_CI_AS` collation, so string comparisons in queries are case-insensitive. EF can't translate `StringComparison` overloads (CA1862 is suppressed for that reason).
- The many-to-many navigations (`Account.Roles`, `FileDirectory.Roles`, `Role.Accounts`, `Role.FileDirectories`) are hand-written in the `*_Extended.cs` partial files and `KecskeDatabaseContext_Extended.cs`, because the join tables have a `CreatedOnUtc` column and the scaffolder doesn't create skip navigations for them.

## Database changes

The schema is **database-first**:
1. Edit `Database/Tables/*.sql` (the `Triggers/` keep `ModifiedOnUtc` up to date; add one for each new table that has that column).
2. Publish with SSDT (profiles `Local_Debug` / `Local_Release`, copied from `Build`). Keep deployed patch scripts in `Database/Patches/Deployed/`. The Debug post-deployment script inserts test data from `Database/TestInserts/` (git-ignored).
3. Re-scaffold with the `Scaffold-DbContext` command in `update-scaffold.txt` (Package Manager Console). It overwrites `Model/` and `Context/KecskeDatabaseContext.cs` (`-f`), so put customizations in `*_Extended.cs` partials, never in the generated files.
4. Update the matching resource, Mapster mapping and service.

## Frontend architecture

- React 19 with the **React Compiler** (babel plugin in `vite.config.ts`), so don't add `useMemo` / `useCallback` by hand. React Router 7, axios, react-icons.
- **Bootstrap 5.3.8 comes from jsDelivr** in `index.html` with SRI hashes. When changing the version, update both `integrity` hashes from the official release.
- `services/*Service.ts` are static classes on top of `BaseService`, which sends every request `withCredentials`, builds query strings and path segments through `BackendServiceTools` (URI-encoded), and handles errors: a 401 redirects to `/login?expired=true`, while a 403 only shows the error and keeps the user logged in.
- `models/` are classes that copy the camelCase JSON into PascalCase properties in their constructor (e.g. `UserData`); `ConvertTools.ConvertListToType` builds them from lists.
- Auth state is the `UserContext` (`utilities/Contexts.ts`), loaded once in `App.tsx` from `GetLoggedInAccount`. Route guards: `LoginChecker` (logged in), `RoleChecker` (route needs a role), `ComponentRoleChecker` (hides a component).
- Notifications: the `Notification` route component **overwrites `LogTools.setNotification` on every render**. Call `LogTools.setErrorNotification` / `setSuccessNotification` / `setInfoNotification`; `LogTools.DebugLog` logs only outside production.
- Forms are described as data: an `IWindowDescription` (e.g. `LoginWindowDescription.ts`) rendered by `EditWindowBase`, with inputs from `components/input/InputDefinitions/`.
- Pages: `Login`, `Home`, `DirectoryTest` (the file browser at `/files/*`, still a test page that uploads to `testFolder`) and `Security` (admin).

## Gotchas

- **Git tracks `Frontend/src/Services/` and `Frontend/src/services/` as two folders** (on disk it is one, `services`, because Windows and `core.ignorecase` are case-insensitive). Imports use `./services/`. Moving `AccountService.ts` and `BaseService.ts` with `git mv` to the lowercase path fixes it.
- `DatabaseORM/.editorconfig` has `root = true` and marks `/KecskeDatabaseContext.cs` as generated, but the file is at `Context/KecskeDatabaseContext.cs`, so that section doesn't match.
- The local `dotnet-ef` tool (`Backend/.config/dotnet-tools.json`) is pinned to 10.0.0 while EF Core is 11.0.0. Scaffolding is done with the PMC command anyway.
- The backend's IP handling is IPv4-only (`StringTools.ValidateIPv4`, `varchar(15)` column).
