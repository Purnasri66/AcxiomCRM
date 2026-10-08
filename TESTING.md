# AcxiomCRM - Manual Acceptance Test Walkthrough

Follow these 14 verification steps to validate all business requirements and security scopes.

---

### Step 1: Protected Pages Block Unauthenticated Users
1. Open browser in incognito mode.
2. Navigate to `https://localhost:5001/Dashboard` or `https://localhost:5001/Customer`.
3. **Expected Result**: User is automatically redirected to `/Account/Login?ReturnUrl=%2FDashboard`.

---

### Step 2: Register & Login Flow
1. Navigate to `/Account/Register`.
2. Register a new user: `newuser@acxiomcrm.com` with password `Password@123`.
3. **Expected Result**: User is registered with role `SalesExecutive`, logged in, and lands on `/Dashboard`.
4. Sign out and sign in with `admin@acxiomcrm.com` / `Admin@12345`.
5. **Expected Result**: Successfully redirected to the full Admin Dashboard.

---

### Step 3: Client-Side Validation on Customer Creation
1. Go to **Customers** -> **Add Customer** (`/Customer/Create`).
2. Enter an invalid phone number: `12345` (does not match Indian 10-digit mobile `^[6-9]\d{9}$`).
3. Enter an invalid email: `invalid-email`.
4. Click **Save Customer**.
5. **Expected Result**: Client-side validation stops form submission. Inline validation messages display:
   - "Enter a valid phone number."
   - "Enter a valid email address."

---

### Step 4: Server-Side Validation Bypass Prevention
1. Using Postman, curl, or browser dev tools, submit a POST request to `/Customer/Create` with invalid phone `12345`.
2. **Expected Result**: Request is rejected server-side with `400 Bad Request` or validation error display; record is not inserted into database.

---

### Step 5: Opportunity Amount Validation
1. Navigate to **Opportunities** -> **Add Opportunity** (`/Opportunity/Create`).
2. Enter Amount: `-500` or `0`.
3. Click **Save Opportunity**.
4. **Expected Result**: Rejected with message: *"Opportunity Amount must be greater than 0."*

---

### Step 6: Opportunity Probability Validation
1. On `/Opportunity/Create`, enter Probability: `120`.
2. Click **Save Opportunity**.
3. **Expected Result**: Rejected with message: *"Probability must be between 0 and 100."*

---

### Step 7: Expected Close Date Validation
1. On `/Opportunity/Create`, select an Expected Close Date in the past (e.g., yesterday).
2. Click **Save Opportunity**.
3. **Expected Result**: Rejected with message: *"Expected Close Date cannot be in the past."*

---

### Step 8: Follow-up Date Validation
1. Navigate to **Follow-Ups** -> **Schedule Follow-Up** (`/FollowUps/Create`).
2. Pick a date earlier than today.
3. Click **Schedule**.
4. **Expected Result**: Form is rejected with: *"Follow-up date cannot be earlier than today."*

---

### Step 9: SalesExecutive Scope Isolation
1. Sign in as `sales1@acxiomcrm.com` / `Sales@12345`.
2. Observe customer list: Only customers created by `sales1` are shown.
3. Attempt to access a customer owned by `sales2` by changing the ID in the URL to `/Customer/Details/2`.
4. **Expected Result**: Server returns `404 Not Found` or `403 Forbidden`. Cross-tenant data leak is prevented.

---

### Step 10: Manager Scope Verification
1. Sign in as `manager@acxiomcrm.com` / `Manager@12345`.
2. Navigate to **Dashboard**, **Opportunities**, and **Reports**.
3. **Expected Result**: Manager sees all company records, team pipeline, and management reports.
4. Attempt to navigate to `/Users`.
5. **Expected Result**: Redirected to `/Account/AccessDenied` because Manager does not have user management privileges.

---

### Step 11: Admin Privileges & Audit Trail
1. Sign in as `admin@acxiomcrm.com` / `Admin@12345`.
2. Access **User Management** (`/Users`) and **Audit Logs** (`/Audit`).
3. **Expected Result**: Admin sees all accounts, lockout status, unlock buttons, and full audit logs with the diff viewer.

---

### Step 12: Audit Logging on CRM State Changes
1. As Admin, edit a Customer or update a Lead status.
2. Navigate to **Audit Logs** (`/Audit`).
3. **Expected Result**: A new audit record appears with Action `Update` or `StatusChange`. Clicking **View Diff** reveals the field-by-field diff between OldValue and NewValue.

---

### Step 13: REST API Authentication & Endpoints
1. Send POST request to `/api/auth/login`:
   ```json
   { "email": "admin@acxiomcrm.com", "password": "Admin@12345" }
   ```
2. **Expected Result**: Returns `200 OK` with JWT token.
3. Send GET request to `/api/customers` with header `Authorization: Bearer <token>`.
4. **Expected Result**: Returns `200 OK` with JSON array of customers.
5. Send GET request to `/api/customers` without authorization header.
6. **Expected Result**: Returns `401 Unauthorized`.

---

### Step 14: Dashboard Charts & Scoped Metrics
1. Log in as `sales1@acxiomcrm.com`.
2. Inspect the Dashboard charts and cards: metrics represent only `sales1`'s assigned records.
3. Log in as `admin@acxiomcrm.com`.
4. **Expected Result**: Dashboard shows enterprise-wide aggregates, security stat cards (total users, failed logins, lockout counts), and top deals at risk.
