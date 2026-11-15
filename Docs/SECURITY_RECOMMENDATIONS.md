# Frontend Security Recommendations - Implementation Notes

## 1. Security Headers Implementation (CRITICAL)
**Status**: DONE (except CSP, see F)  
**Priority**: CRITICAL  
**Location**: Backend/Middleware/SecurityHeadersMiddleware.cs

### Overview
Add HTTP security headers to protect against common web vulnerabilities. These headers are part of the response from the backend to the frontend.

### Headers to Implement & Explanation

#### A. **X-Content-Type-Options: nosniff**
- **What it does**: Prevents browsers from MIME-type sniffing (guessing file types)
- **Why it matters**: Stops attackers from serving malicious content disguised as safe files
- **Example attack prevented**: A `.txt` file containing malicious JavaScript being interpreted as a script

#### B. **X-Frame-Options: DENY**
- **What it does**: Prevents the page from being embedded in iframes on other domains
- **Why it matters**: Stops clickjacking attacks (tricking users into clicking hidden elements)
- **Alternative**: `SAMEORIGIN` if you need iframes on your own domain

#### C. **X-XSS-Protection: 0**
- **What it does**: Explicitly disables the legacy XSS filter of older browsers
- **Why `0` and not `1; mode=block`**: The filter is deprecated, and the filter itself could be abused to leak data from a page. OWASP recommends either omitting the header or setting it to `0`
- **Note**: Modern browsers ignore this header, CSP is the real protection against XSS

#### D. **Referrer-Policy: strict-origin-when-cross-origin**
- **What it does**: Controls what referrer information is sent when navigating to other sites
- **Why it matters**: Prevents leaking sensitive URL information
- **How it works**: 
  - Same-site requests: Full URL is sent
  - Cross-site requests: Only origin is sent (no path/query params)

#### E. **Permissions-Policy: geolocation=(), microphone=(), camera=()**
- **What it does**: Disables specific browser features that could be exploited
- **Why it matters**: Prevents malicious scripts from accessing sensitive hardware
- **Recommendation**: Add other dangerous features: `payment=(), usb=(), accelerometer=(), gyroscope=()`

#### F. **Content-Security-Policy (CSP)**
- **Status**: TODO - intentionally **not** set by the backend. The backend only returns JSON and files, a CSP only has an effect on the HTML document that loads the scripts. It belongs on the frontend, which IIS serves, so it should go into `Build/Deployment/Config-Production/frontend-web.config` (see Implementation below)
- **What it does**: Whitelist policy for what content can be loaded and executed
- **Why it matters**: Most powerful defense against XSS attacks
- **Current recommendation**:
  ```
  default-src 'self'; 
  script-src 'self' cdn.jsdelivr.net; 
  style-src 'self' cdn.jsdelivr.net 'unsafe-inline'; 
  img-src 'self' data:; 
  font-src 'self'
  ```
- **Explanation of directives**:
  - `default-src 'self'` - Default: only allow same-origin resources
  - `script-src 'self' cdn.jsdelivr.net` - Allow scripts only from your domain and Bootstrap CDN
  - `style-src` - Allow CSS from your domain, CDN, and inline styles (needed for Bootstrap)
  - `img-src 'self' data:` - Images from your domain + data URLs (for embedded images)
  - `font-src 'self'` - Fonts only from your domain

### Implementation
Headers A-E are added to every backend response by `Backend/Middleware/SecurityHeadersMiddleware.cs`, registered in `Program.cs` as the first middleware (before `UseCors`):
```csharp
_ = app.UseMiddleware<SecurityHeadersMiddleware>();
```

The CSP (F) still needs to be added to the frontend's IIS config, `frontend-web.config`. The `connect-src` must include the backend's domain, otherwise API calls are blocked:
```xml
<system.webServer>
	<httpProtocol>
		<customHeaders>
			<add name="Content-Security-Policy" value="default-src 'self'; script-src 'self' cdn.jsdelivr.net; style-src 'self' cdn.jsdelivr.net 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' https://BACKEND-DOMAIN" />
		</customHeaders>
	</httpProtocol>
</system.webServer>
```

### Testing CSP
- Use browser DevTools Console to see CSP violations
- Start with Report-Only mode during development: `Content-Security-Policy-Report-Only`
- Once confirmed working, switch to enforcement mode

---

## 2. CORS Configuration Restriction (HIGH)
**Status**: DONE  
**Priority**: HIGH  
**Location**: Backend/Program.cs (`ConfigureCors`)

### Previous Issue
```csharp
.AllowAnyHeader()
.AllowAnyMethod()
```
This was too permissive and increased attack surface.

### Implementation
```csharp
_ = app.UseCors(policy => ConfigureCors(policy, frontendDomains));

private static void ConfigureCors(CorsPolicyBuilder policy, string[] frontendDomains)
{
	_ = policy
	.WithOrigins(frontendDomains)
	.AllowCredentials()
	.WithMethods("GET", "POST", "PUT", "DELETE") // Only necessary HTTP methods
	.WithHeaders("Content-Type", "Authorization") // Only expected headers
	.WithExposedHeaders("Content-Disposition") // If returning files
	.SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache preflight for 10 minutes
}
```

### Notes
- Remove any methods your API doesn't use (e.g., if no `PATCH`, don't include it)
- `Content-Type` and `Authorization` headers are typically all that's needed
- Add only headers your API actually requires
- The `SetPreflightMaxAge` reduces redundant CORS preflight requests

---

## 3. Frontend Input Validation (HIGH)
**Status**: TODO  
**Priority**: HIGH  
**Location**: Frontend/src/pages/Login.tsx, Frontend/src/components (any form components)

### Areas to Validate
- **Email format**: Use regex or HTML5 pattern attribute
- **Password strength**: Min 12 chars, uppercase, lowercase, numbers, special chars
- **File uploads**: Check file size before upload (use `getFileSizeLimit()` throughout)
- **String length limits**: Username, file names, etc.

### Implementation Notes
- Use HTML5 `required`, `pattern`, `maxlength` attributes as first pass
- Add JavaScript validation for better UX (show errors before submit)
- Backend validation is also critical (never trust frontend alone)
- Consider adding a password strength meter component for UX feedback

### Files to Review
- Login form component
- Register form component
- File upload components
- Any other user input forms

---

## 4. CSRF Protection Implementation (RESEARCH NEEDED)
**Status**: DEFERRED - NEEDS PROPER IMPLEMENTATION  
**Priority**: HIGH  
**Location**: Backend API design / Frontend request handling

### Current Situation
Your application uses a REST API architecture with cookie-based authentication. CSRF protection for APIs is different from traditional form-based applications.

### Two Approaches for API CSRF Protection

#### Approach A: Double-Submit Cookie Pattern (Recommended for APIs)
This is the standard for modern REST APIs:
- Backend generates random CSRF token
- Frontend reads token from response header or dedicated endpoint
- Frontend includes token in custom header (e.g., `X-CSRF-Token`) for state-changing requests
- Backend middleware validates token matches cookie

**Pros**: Works well with APIs, no special attributes needed
**Cons**: Requires token generation and validation middleware

#### Approach B: Rely on SameSite Cookie (Already Implemented ✓)
Your current `SameSite=Strict` already provides strong protection:
- Cookies are never sent in cross-site requests
- CSRF attacks require cookies to be sent automatically
- With Strict mode, this is impossible

**Pros**: Zero additional code, already deployed
**Cons**: Older browsers don't support SameSite (rare in 2024)

### Current Status
Your architecture already has **Approach B** fully implemented:
```csharp
options.Cookie.SameSite = 
	environment == EnvironmentConstants.Production
		? SameSiteMode.Strict
		: SameSiteMode.None;
```

### Recommendation
**Either:**
1. Keep current SameSite=Strict protection (sufficient for most applications)
2. Implement double-submit cookie pattern for defense-in-depth (additional complexity)

**Do NOT use**: `[ValidateAntiForgery]` attribute - it's designed for form-based MVC apps, not REST APIs

### Implementation If Needed
If you want to add double-submit cookie protection later, see section on implementing a custom CSRF middleware pattern in your API. This would involve:
- CSRF token generation middleware
- Token validation middleware for POST/PUT/DELETE
- Frontend modification to read and send tokens

**Note**: This was deferred due to architectural complexity with ensuring it works properly with your API design. The current SameSite=Strict protection is sufficient and industry-standard for modern APIs.

---

## 5. CDN Dependency Security (MEDIUM)
**Status**: PARTIAL MITIGATION  
**Priority**: MEDIUM  
**Location**: Frontend/index.html

### What We Can Do Without Hosting Locally

#### A. Strengthen Subresource Integrity (Already Done ✓)
- Keep the `integrity` attributes on both Bootstrap CSS and JS
- Regularly verify the SRI hashes match the official Bootstrap releases
- When updating Bootstrap version, update SRI hashes from official source

#### B. Add CORS Attribute
```html
<link
  href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css"
  rel="stylesheet"
  integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB"
  crossorigin="anonymous"
/>
```
The `crossorigin="anonymous"` is already present ✓

#### C. Use Trusted CDN Provider
- jsdelivr.com is a highly reputable CDN
- Consider alternatives like cdnjs.cloudflare.com or unpkg.com
- All major CDNs have security protections in place

#### D. Implement CSP to Restrict Script Sources
- Planned in Section 1F, **not yet in effect** until the CSP is added to `frontend-web.config`
- CSP directive `script-src 'self' cdn.jsdelivr.net` explicitly allows only this CDN

### Limitations Without Hosting
- You cannot prevent a compromised CDN from serving malicious content
- SRI only verifies the file *currently* being served matches the hash
- Hosting locally would eliminate CDN dependency risk entirely

### Recommendation
Keep current approach. The risk is minimal with SRI + trusted CDN. Revisit if:
- Your risk profile changes (e.g., handling highly sensitive data)
- You have specific compliance requirements
- The CDN provider has a security incident

---

## 6. Rate Limiting & Brute Force Protection (MEDIUM)
**Status**: DONE on the backend, frontend part optional  
**Priority**: MEDIUM  
**Location**: Backend/Program.cs (`ConfigureRateLimiter`), Backend/Constants/RateLimitPolicies.cs, Backend/Controllers/AccountController.cs

### Frontend Implementation (Optional)

#### Client-Side Rate Limiting
```typescript
// In Login component
const [loginAttempts, setLoginAttempts] = useState(0);
const [cooldownSeconds, setCooldownSeconds] = useState(0);

const handleLogin = async (credentials: FormData) => {
  if (cooldownSeconds > 0) {
	LogTools.setErrorNotification(`Too many attempts. Try again in ${cooldownSeconds}s`);
	return;
  }

  try {
	const response = await AccountService.Login(credentials);
	setLoginAttempts(0); // Reset on success
	// Handle success
  } catch (error) {
	const newAttempts = loginAttempts + 1;
	setLoginAttempts(newAttempts);

	// Exponential backoff: 2^attempts * 1 second
	if (newAttempts >= 3) {
	  const backoffSeconds = Math.pow(2, newAttempts - 3);
	  setCooldownSeconds(backoffSeconds);

	  // Countdown timer
	  const timer = setInterval(() => {
		setCooldownSeconds(s => {
		  if (s <= 1) clearInterval(timer);
		  return Math.max(0, s - 1);
		});
	  }, 1000);
	}
  }
};
```

### Backend Implementation (Critical, Implemented)
Uses ASP.NET Core's built-in rate limiter (`Microsoft.AspNetCore.RateLimiting`), no custom middleware:
- **Policy**: `RateLimitPolicies.Authentication`, a fixed window of 5 requests per minute, tracked separately for each client IP address
- **Applied by default** to the whole `AccountController` with `[EnableRateLimiting(RateLimitPolicies.Authentication)]`, so Login, Register and ResetPassword are limited
- **Opted out** with `[DisableRateLimiting]` on actions that are called during normal use (GetLoggedInAccount runs on every page load) or need a login anyway (Logout, GetUsers, GenerateNewSecretKey)
- **Rejected requests** get `429 Too Many Requests`, a `Retry-After` header and the usual `GenericResponse` JSON body, so the frontend displays the error message
- **Pipeline order**: `app.UseRateLimiter()` must come after `app.UseCors(...)`, otherwise the 429 response has no CORS headers and the browser hides it from the frontend
- To limit another controller, add the attribute to it, or add a new policy in `ConfigureRateLimiter`

### Notes
- Frontend rate limiting is UX improvement only; backend enforcement is required
- The limiter counts every request, not only failed ones, and there are no growing delays. This is normal for login endpoints
- Counters are kept in memory and reset when the app restarts. That is fine for a single server; a database or cache (Redis) would only be needed with multiple servers
- Not implemented (possible later): account lockout after M failures, CAPTCHA after repeated failures, logging of rejected requests

---

## 7. Error Message Sanitization (HIGH)
**Status**: TODO  
**Priority**: HIGH  
**Location**: Frontend/src/tools/LogTools.ts, Frontend/src/services/BaseService.ts

### Problem
Unsanitized error messages from backend could contain malicious content or be exploited via error-based XSS.

### Solution Without Extra Libraries
Use `textContent` instead of `innerHTML` - this treats content as plain text:

#### Current Approach (Potentially Unsafe)
```typescript
// If error message is displayed directly to DOM
element.innerHTML = errorMessage; // ❌ Vulnerable to XSS
```

#### Safe Approach
```typescript
// Use textContent instead
element.textContent = errorMessage; // ✅ Safe - treats as text only

// OR when displaying in React
<div>{errorMessage}</div>
// ✅ Safe - React automatically escapes content in JSX
```

#### Additional Validation
Create a simple sanitizer for known dangerous patterns:
```typescript
export function sanitizeErrorMessage(message: string): string {
  // Remove any script tags or event handlers
  return message
	.replace(/<script[^>]*>.*?<\/script>/gi, '')
	.replace(/on\w+\s*=/gi, '') // Remove onclick, onerror, etc.
	.substring(0, 500); // Limit message length
}
```

### Implementation Notes
- React JSX already escapes content by default (good!)
- Only use `innerHTML` if you trust the source and validate thoroughly
- Apply sanitization when displaying *any* user-controlled content
- Test with error messages containing: `<script>`, `<img onerror=`, quotes, etc.

### Code Location
- **BaseService.ts**: `ErrorHandling()` method - sanitize error response
- **LogTools.ts**: `setErrorNotification()` method - ensure safe display

---

## 8. Secure Password Policy Enforcement (HIGH)
**Status**: TODO  
**Priority**: HIGH  
**Location**: Frontend/src/pages/Register.tsx, Backend/Controllers/AccountController.cs

### Frontend Validation

#### Password Requirements
Create a password validator:
```typescript
export function validatePassword(password: string): {
  isValid: boolean;
  errors: string[];
} {
  const errors: string[] = [];

  if (password.length < 12) errors.push("Password must be at least 12 characters");
  if (!/[A-Z]/.test(password)) errors.push("Must contain uppercase letter");
  if (!/[a-z]/.test(password)) errors.push("Must contain lowercase letter");
  if (!/\d/.test(password)) errors.push("Must contain number");
  if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) errors.push("Must contain special character");

  return {
	isValid: errors.length === 0,
	errors
  };
}
```

#### React Component Implementation
```typescript
// In Register component
const [password, setPassword] = useState("");
const [passwordValidation, setPasswordValidation] = useState({ isValid: false, errors: [] });

const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  const pwd = e.target.value;
  setPassword(pwd);
  setPasswordValidation(validatePassword(pwd));
};

return (
  <>
	<input 
	  type="password" 
	  value={password}
	  onChange={handlePasswordChange}
	/>
	{!passwordValidation.isValid && (
	  <div className="text-danger">
		{passwordValidation.errors.map((err, i) => (
		  <div key={i}>✗ {err}</div>
		))}
	  </div>
	)}
  </>
);
```

### Backend Validation (CRITICAL)
Never trust frontend validation alone:

```csharp
// In AccountController or validation service
private bool ValidatePassword(string password)
{
	const int MinLength = 12;
	const int MaxLength = 128;

	if (string.IsNullOrWhiteSpace(password)) return false;
	if (password.Length < MinLength || password.Length > MaxLength) return false;
	if (!password.Any(char.IsUpper)) return false;
	if (!password.Any(char.IsLower)) return false;
	if (!password.Any(char.IsDigit)) return false;
	if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;

	return true;
}
```

### Optional Enhancements
- **Password strength meter**: Visual feedback (weak → fair → good → strong)
- **Common password check**: Reject passwords from "Top 10000 worst passwords" list
- **Breach database check**: Check against haveibeenpwned.com API (optional)

---

## 9. Session Expiration & Logout Handling (HIGH)
**Status**: PARTIAL - 401 handling done, 403 handling and user context clearing TODO  
**Priority**: HIGH  
**Location**: Frontend/src/services/BaseService.ts, Frontend/src/pages/Login.tsx, Backend/Program.cs

### Current Behavior
- **401** redirects to `/login?expired=true`. The login page shows "Your session has expired" and removes the flag from the URL, so a refresh doesn't show it again
- **403** still redirects to `/login` (without the flag), which logs the user out of the page. This is the remaining TODO below
- Users who never logged in don't see the "expired" message: `LoginChecker` redirects them on the frontend before any API call
- **Note**: ResetPassword returns 401 for a wrong key. It is not called from the frontend yet; when that page is built, add its endpoint to the exclusions in `BaseService`, otherwise a wrong key redirects to the login page with the "expired" message

### Required Changes

#### Backend Status Codes (Clarification)
- **401 Unauthorized**: User is not authenticated (no valid session/token)
- **403 Forbidden**: User is authenticated but lacks permission for the resource
- **Action**: Only 401 should trigger logout/redirect

#### Frontend Implementation - BaseService.ts
```typescript
static ErrorHandling(route: string, errorData: any): void {
	// ... existing error logging code ...

	const status = errorData?.response?.status;

	// 401 = Not authenticated, redirect to login
	if (status === 401 && !route.startsWith(Constants.GetUserStateEndpoint)) {
		// Pass flag to show "session expired" message (implemented)
		window.location.replace(`${Constants.LoginRoute}?expired=true`);
		return;
	}

	// 403 = Forbidden, show error but DON'T logout (TODO)
	if (status === 403) {
		LogTools.setErrorNotification("You don't have permission to access this resource.");
		return; // Don't logout
	}
}
```

#### Frontend Implementation - Login.tsx (Implemented)
```typescript
const [searchParams, setSearchParams] = useSearchParams();

useEffect(() => {
	if (searchParams.get("expired") === "true") {
		LogTools.setInfoNotification("Your session has expired. Please log in again.");

		// Remove the flag so a page refresh doesn't show the message again
		setSearchParams(
			(params) => {
				params.delete("expired");
				return params;
			},
			{ replace: true },
		);
	}
}, [searchParams, setSearchParams]);
```
The URL flag is enough, `sessionStorage` is not needed.

#### Clear User Context on 401
In `App.tsx`, modify the error handling to clear user state:
```typescript
.catch((error) => {
	if (error?.response?.status === 401) {
		setUser(null); // Clear user context
		// Redirect is handled by BaseService
	}
	LogTools.setErrorNotification(error?.message ?? error);
});
```

### Backend - Session Configuration (Review)
Your current config in Program.cs is good:
```csharp
options.ExpireTimeSpan = TimeSpan.FromDays(1);
options.SlidingExpiration = true; // Session extends on each request
```

### Testing
1. Set session timeout to shorter duration during testing (e.g., 5 minutes)
2. Log in, wait for session to expire
3. Try to access protected resource
4. Verify you see "session expired" message and redirect occurs
5. Verify 403 errors don't cause logout

---

## 10. CSP & Inline Styles (INFORMATIONAL)
**Status**: NO ACTION NEEDED  
**Priority**: LOW  
**Location**: CSP configuration (Section 1)

### Your Situation
Bootstrap CSS is sourced from CDN and contains inline styles. You cannot modify this without hosting Bootstrap locally.

**Note**: No CSP is in effect yet. This section applies once the CSP from Section 1F is added to `frontend-web.config`.

### Why `'unsafe-inline'` is Risky
- Allows any inline CSS or styles to execute
- Doesn't prevent malicious style injection attacks
- However, CSS alone cannot execute JavaScript

### Why It's Acceptable Here
- CSS can only affect presentation, not execute code
- CSP is primarily for script protection
- The real risk would be if you allowed `'unsafe-inline'` for scripts

### Planned CSP Setting
```
style-src 'self' cdn.jsdelivr.net 'unsafe-inline'
```
This is a reasonable compromise for Bootstrap. The `'unsafe-inline'` for styles is much less dangerous than for scripts.

### No Action Needed
Keep as-is. Bootstrap's inline styles don't present the same risk as inline JavaScript.

---

## 11. Centralized Security Middleware (OPTIONAL)
**Status**: DONE  
**Priority**: LOW  
**Location**: Backend/Middleware/SecurityHeadersMiddleware.cs

### Why Create It?
- Consolidates all security headers in one place
- Easier to audit and maintain
- Can be reused across projects
- Follows separation of concerns pattern

### Implementation
`Backend/Middleware/SecurityHeadersMiddleware.cs` adds the headers from Section 1 (A-E) to every response. It intentionally does not add a CSP, see Section 1F.

Registered in `Program.cs` as the first middleware, before `UseCors`:
```csharp
_ = app.UseMiddleware<SecurityHeadersMiddleware>();
```

### Benefits vs. Inline Version
| Aspect | Inline | Middleware Class |
|--------|--------|------------------|
| Readability | ❌ Clutters Program.cs | ✓ Clean, focused |
| Reusability | ❌ Copy-paste | ✓ Reusable class |
| Testability | ❌ Hard to unit test | ✓ Easy to unit test |
| Maintainability | ❌ Mixed concerns | ✓ Single responsibility |

---

## Summary of TODO Items

### CRITICAL - Do First
- [x] Section 1: Add security headers middleware
- [ ] Section 1F: Add the CSP to `frontend-web.config`
- [x] Section 2: Restrict CORS to specific methods/headers  
- [ ] Section 4: Implement CSRF protection (anti-forgery tokens)
- [ ] Section 7: Add error message sanitization
- [x] Section 9: 401 handling and session expiration messaging
- [ ] Section 9: 403 should show an error without logging out, clear user context on 401

### HIGH - Do Soon
- [ ] Section 3: Add frontend input validation (email, password, file size)
- [ ] Section 8: Implement password policy enforcement (12+ chars, mixed case, numbers, symbols)
- [x] Section 6: Add rate limiting on login attempts (backend is critical)

### MEDIUM - Do When Convenient
- [ ] Section 5: Update CSP hash verification process for CDN
- [x] Section 11: (Optional) Extract security headers to middleware class

### COMPLETED ✓
- Security headers middleware (Section 1 A-E, Section 11)
- CORS restricted to used methods and headers (Section 2)
- Backend rate limiting on AccountController with built-in ASP.NET Core rate limiter (Section 6)
- 401 redirects to login with "session expired" message (Section 9)
- Session configuration (HttpOnly, Secure, SameSite)
- Credentials handling (withCredentials in production)
- Debug logging (already disabled in prod)
- SRI implementation on Bootstrap resources
- Page title update

---

## Testing Checklist

- [ ] Test each header is present (use browser DevTools → Network → Response Headers)
- [ ] Test CSP violations show in console (try inline script, should be blocked) - once the CSP is added to `frontend-web.config`
- [ ] Test CSRF token validation works on form submission
- [ ] Test password validation shows all requirements
- [ ] Test 401 redirects to login with "expired" message
- [ ] Test 403 shows error without logout
- [ ] Test the 6th login attempt within a minute returns 429 and the frontend shows the "try again in N seconds" message
- [ ] Test GetLoggedInAccount and Logout are not rate limited (reload the page more than 5 times in a minute)
- [ ] Test error messages don't contain JavaScript/HTML tags
- [ ] Test CORS: requests with extra headers should fail
- [ ] Test CORS: preflight OPTIONS requests succeed from your frontend domain

