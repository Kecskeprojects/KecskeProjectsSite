# Improvement notes

Findings from code-quality investigations, kept so the work can be picked up later. Nothing here has been applied yet unless marked done. Security items with their own write-up are tracked in `SECURITY_RECOMMENDATIONS.md`; this file only adds new findings and points there.

## 1. Bugs and security findings (found and fixed 2026-11-15)

Found while writing `CLAUDE.md`. **All done** (not committed at the time of writing); kept here as a record.

### Security (done)

- **Upload file name not sanitized (path traversal).** `HandleFileReadAsync` wrote to `Path.Combine(fullPath, fileName)` with the client's `fileName`, so `..\..\x.txt` or an absolute name (`C:\x.txt`) could write anywhere the app pool can. Now resolved through `FileTools.GetFilePathInDirectory`, which accepts only a plain file name that stays directly inside the target folder (400 otherwise). The folder check in `GetFullPathIfValid` now uses `FileTools.IsSameOrInsideDirectory` instead of searching the relative path for `..`, so it also catches a path on another drive and no longer rejects folder names such as `a..b`. Checked against the real code with traversal, drive-letter, sibling-prefix (`Videos` / `Videos2`) and dotted-folder cases.
- **Exception messages sent to the client.** `ErrorLoggingFilterAttribute` returned any exception's message. Now only `ApiException` (`Communication/Internal/ApiException.cs`) messages and status codes reach the client; everything else is logged and returns a generic 500. The filter also logged `Response.StatusCode` (always 200 at that point); it now logs the real one.
- **Wrong status codes for file errors.** No access to a category is now 403, a missing or escaping path 404, an existing file on a new upload 409, a bad path or file name 400, and `GetSingle` returns 404 for a missing file. Because file-access denials are now 403, the frontend no longer redirects to the login page on 403 (`BaseService`); see `SECURITY_RECOMMENDATIONS.md` section 9.
- **Hidden and system files were downloadable** through `GetSingle` if their hash was known, although the listing skipped them. `GetFileRoute` now skips them too.
- **Open items in `SECURITY_RECOMMENDATIONS.md`:** CSP in `frontend-web.config` (1F), frontend display of error messages (7), clearing the user context on 401 (9), frontend input validation (3), password policy (8), CSRF decision (4).

### Bugs (done)

- **Registration rejected names that are substrings of existing ones** (`Contains` instead of equality in `AccountService.RegisterAsync`). The redundant second password hash was removed.
- **Firewall IP matching by substring.** Expiring `1.2.3.4` also removed `11.2.3.45`. `FirewallTools` now compares the address part before `/` exactly (Windows reports `1.2.3.4/255.255.255.255`) and rebuilds the list instead of string-replacing `*`.
- **Notifications lost the last queued item** (`slice(1, length - 1)`), and two notifications raised before a re-render overwrote each other. `Notification` now uses functional state updates. The `alert` fallback in `LogTools` printed `[object Object]`; it now shows the message.
- **Wrong logger categories** in `FileController`, `FileDirectoryController`, `SecurityController` and `FirewallApiService`.
- **`AccountResource.Roles` exposed EF `Role` entities** and was always empty. It is now a `List<string>` of role names (Mapster mapping in `MappingConfiguration`), and `GetUsersAsync` loads the roles, ordered by user name.
- **Dots removed from folder names.** `relativePath.Replace(".", "")` turned a folder `v1.2` into `v12`, breaking its links. `FileTools.GetClientSubPath` now only handles the `.` that `Path.GetRelativePath` returns for the folder itself, and uses `/` separators.
- **Misleading error message** for an empty path in `FileStorageTargetPathDetails`.

## 2. Build, tooling and linting (investigated 2026-11-15)

### Backend can't be built with the .NET CLI

`Backend.csproj` uses a `COMReference` to `NetFwTypeLib`, and `ResolveComReference` only runs on Visual Studio's .NET Framework MSBuild (`dotnet build` fails with MSB4803). This blocks CI and `dotnet run`. Options:
- Use late binding: the code already creates the objects with `Type.GetTypeFromProgID`, so the rule could be handled through `dynamic` without the interop types.
- Define the few needed COM interfaces (`INetFwPolicy2`, `INetFwRules`, `INetFwRule`) by hand with `[ComImport]`, the usual way of avoiding a `COMReference`.
- Call `netsh advfirewall firewall` or PowerShell's `Set-NetFirewallRule` as a process instead.

The second option keeps type safety. After that, `.github/workflows/` (currently empty) could get a build for the backend and `npm run build` + `npm run lint` for the frontend.

### Analyzers

The Discord bot's linting findings apply here too:
- There is no root `.editorconfig` and no `Directory.Build.props`. Both C# projects build with 0 warnings at the default analysis level; a trial with `-p:AnalysisLevel=latest-recommended -p:EnforceCodeStyleInBuild=true` hasn't been run yet.
- Recommended setup: `Directory.Build.props` with `<AnalysisLevel>latest-recommended</AnalysisLevel>` and `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`, a root `.editorconfig` that silences the unwanted rules (most culture rules), `dotnet format`, and `TreatWarningsAsErrors` only once at zero warnings. Nullable is already enabled in both projects.
- **Gotcha:** `DatabaseORM/.editorconfig` has `root = true`, so a future repo-root `.editorconfig` would not apply inside `DatabaseORM/`. Move `root = true` to the root file. Its `[/KecskeDatabaseContext.cs]` section also misses the file, which is at `Context/KecskeDatabaseContext.cs`.
- `DatabaseORM/GlobalSuppressions.cs` targets `~T:DatabaseORM.KecskeDatabaseContext` for IDE0290, but the class is `DatabaseORM.Context.KecskeDatabaseContext`, so the suppression doesn't apply.

### Frontend lint (`npm run lint`: 51 errors, 1 warning)

- 23 × `no-undef` in `UI-Test/multimedia-repo-ui/js/app.js`. The config only gives `.ts`/`.tsx` files browser globals. Add `UI-Test` to `globalIgnores`.
- 26 × `@typescript-eslint/no-explicit-any`, mostly in `BackendServiceTools`, `ConvertTools`, `LogTools`, `BaseService` and the models' constructors. Replace with `unknown` plus narrowing, or typed response interfaces.
- `react-hooks/immutability` in `Notification.tsx`: overwriting `LogTools.setNotification` during render. A small notification store (or a context with an `addNotification` function, or `useSyncExternalStore`) would replace the monkey patch.
- `react-hooks/set-state-in-effect` in `DirectoryTest.tsx` and `react-hooks/exhaustive-deps` in `App.tsx`.

### Other tooling

- **Duplicate folder casing:** git tracks both `Frontend/src/Services/` (`AccountService.ts`, `BaseService.ts`) and `Frontend/src/services/`. On a case-sensitive file system (Linux CI) the `./services/...` imports of the first two would break. Fix with `git mv` to the lowercase folder.
- `Frontend/.vscode/launch.json` and `tasks.json` still start Create React App (`npm start`, port 3000). Update them for Vite (`npm run dev`, port 5173) or delete them.
- `Backend/.config/dotnet-tools.json` pins `dotnet-ef` 10.0.0 while EF Core is 11.0.0. Update it, or remove it if scaffolding stays in the Package Manager Console.

## 3. Test project (not started)

There are no tests. Pieces that can be tested now without changes, in order of value:
- **Path handling** (`FileStorageTargetPathDetails`, `FileTools.GetFullPathIfValid`, `GetPathRelativeToCategoryDirectory`): security-relevant, pure, and easy to test with a temp folder. Include the traversal cases from section 1.
- `StringTools.ValidateIPv4`, `EncryptionTools`, `ClaimsPrincipalTools` (claims round trip), the Mapster mappings.
- `FirewallTools.AddIpAddress` / `RemoveExpiredIpAddresses` against a fake `INetFwRule` (it's an interface), which would have caught the substring bug.
- DB services with a mocked or real repository. As in the bot: don't use EF InMemory or SQLite, because queries rely on the case-insensitive collation; use SQL Server LocalDB if repository tests are wanted.
- Frontend: Vitest (fits Vite) for `BackendServiceTools`, `FileTools.getBlobChunksByLimit` and `UserData.hasRoles`.

Blockers for the rest: `DateTime.UtcNow` in `PermittedIpAddressService` and `AccountService` (inject `TimeProvider`), and the static COM calls in `FirewallTools` (put them behind an interface, which also helps section 2).

## 4. Backend structure (not started)

- **`FileStorageService` cleanup** (also a code TODO): `GetFilesInDirectory` and `GetDirectoriesInDirectory` still repeat the same listing loop (the sub-path part moved to `FileTools.GetClientSubPath`). Split it into path resolution, listing and upload.
- **`GenericRepository` TODOs:** memory caching, pagination for `GetListAsync`, concurrency handling (e.g. a `rowversion` column), and EF connection resiliency (`EnableRetryOnFailure`). `CheckQueryString` builds the SQL string in development and throws the result away; log it at debug level or remove it.
- **`FirewallRuleExpirationWatcher`** uses a `Timer` callback with sync-over-async (`GetAwaiter().GetResult()`) and a plain `bool` guard. A `BackgroundService` with `PeriodicTimer` is simpler and naturally non-overlapping.
- **IPv6:** the whitelist is IPv4-only (`varchar(15)`, `ValidateIPv4`). `IPAddress.TryParse` plus a wider column would allow IPv6 if the server gets IPv6 clients.
- **Generic `BaseService` idea from the bot** is already in place here as `GenericService<T>`, and the DB layer is already its own library (`DatabaseORM`), so those bot sections don't apply.

## 5. Unfinished frontend features

The backend has endpoints without a frontend yet:
- Register page (`AccountService.Register` exists, no page), reset password page (`ResetPassword`; add its endpoint to the 401 exclusions in `BaseService`, see security doc section 9), and an admin user list (`GetUsers`, `GenerateNewSecretKey`) with a way to approve registrations, which currently needs a manual DB update.
- Category list (`FileDirectory/GetCategories`) for navigation.
- `DirectoryTest` is the file browser in test form: uploads go to a hard-coded `testFolder` (also in `Home`). `Frontend/UI-Test/multimedia-repo-ui/` is a design mock-up for the real page (dark theme, drag-and-drop upload, media player).
- `index.html` references `/img/logo192.png`, which doesn't exist in `public/img` (only `favicon.ico`); see the TODO below.

## Code TODOs

Collected 2026-11-15:

- `Backend/ApiServices/FileStorageService.cs:18`: clean up for readability and maintainability (split into methods, more logging and error handling). See section 4.
- `DatabaseORM/Repository/GenericRepository.cs:13`: memory caching.
- `DatabaseORM/Repository/GenericRepository.cs:14`: pagination for `GetListAsync`.
- `DatabaseORM/Repository/GenericRepository.cs:15`: concurrency conflict handling (the next line links connection resiliency).
- `Frontend/index.html:4`: replace the images (Open Graph image and icons).
- `Frontend/src/components/input/InputBase.tsx:7`: frontend input validation (email, password strength, file size, length limits); same as security doc section 3.
- `Frontend/src/components/window/EditWindowBase.tsx:56`: `hasCloseFunctionality` and the close behavior itself are not implemented.
