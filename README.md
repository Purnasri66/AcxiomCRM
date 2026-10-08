# AcxiomCRM - Enterprise Customer Relationship Management

A complete, production-grade web application built with **ASP.NET Core 8 MVC (Razor Views)**, a co-located **REST API with JWT authentication**, **Entity Framework Core 8 Code First**, and **ASP.NET Core Identity**.

---

## 1. Quick Setup & Run Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server LocalDB (`(localdb)\mssqllocaldb`)

### Steps
1. Navigate to the project directory:
   ```bash
   cd AcxiomCRM
   ```
2. Apply Entity Framework Core database migrations:
   ```bash
   dotnet ef database update
   ```
   *(Note: The database automatically seeds realistic CRM data when launched if empty).*
3. Run the application:
   ```bash
   dotnet run
   ```
4. Access the web interface at `https://localhost:5001` or `http://localhost:5000` (or the port displayed in terminal output).
5. Access Swagger API documentation at `/swagger`.

---

## 2. Seeded Accounts & Roles

| Role | Email | Password | Scope & Responsibilities |
|---|---|---|---|
| **Admin** | `admin@acxiomcrm.com` | `Admin@12345` | Unrestricted full access, user/role management, security admin, audit logs with diff viewer, all reports. |
| **Manager** | `manager@acxiomcrm.com` | `Manager@12345` | All CRM records, team pipeline forecast, follow-ups, management reports, read-only limited audit logs (no security admin). |
| **SalesExecutive** | `sales1@acxiomcrm.com` | `Sales@12345` | Own assigned customers, leads, opportunities, follow-ups, activities. Modifying URL IDs returns 403/404. |
| **SalesExecutive** | `sales2@acxiomcrm.com` | `Sales@12345` | Second sales executive to verify cross-user resource isolation. |

---

## 3. Assumptions Made

1. **Manager Team Scope**: Because the specification defines no distinct sales team organizational hierarchy, Manager "team" scope gives visibility to all CRM records and management reports across the enterprise, while strictly denying access to user/role credentials and security administration.
2. **FollowUp Opportunity Association**: FollowUp includes a nullable `OpportunityId` foreign key in addition to `CustomerId` and `LeadId` to facilitate direct deal-level follow-up scheduling.

---

## 4. Text Architecture Diagram

```
+-----------------------------------------------------------------------------------+
|                                  BROWSER CLIENT                                   |
|  - Bootstrap 5 + Bootstrap Icons + Chart.js + jQuery Validation + Dark/Light Theme|
|  - Keyboard Shortcut Palette (Ctrl+K) + Drag-and-Drop Pipeline Kanban Board       |
+-----------------------------------------------------------------------------------+
                                   |           |
            Cookie Auth (MVC)      |           |  Bearer JWT (/api/*)
                                   v           v
+-----------------------------------------------------------------------------------+
|                                ASP.NET CORE 8 HOST                                |
|  +---------------------------+       +-----------------------------------------+  |
|  |     MVC Controllers       |       |       REST API Controllers (/api)       |  |
|  | - AccountController       |       | - AuthController (Rate-limited)         |  |
|  | - DashboardController     |       | - CustomersController                   |  |
|  | - CustomerController      |       | - LeadsController                       |  |
|  | - LeadController          |       | - OpportunitiesController               |  |
|  | - OpportunityController   |       | - FollowUpsController                   |  |
|  | - ActivitiesController    |       | - ReportsController                     |  |
|  | - AuditController         |       +-----------------------------------------+  |
|  | - ReportsController       |                                                    |
|  | - UsersController         |                                                    |
|  +---------------------------+                                                    |
|                                                                                   |
|  MIDDLEWARE & SECURITY:                                                           |
|  - GlobalExceptionMiddleware (ProblemDetails for API, Friendly View for MVC)      |
|  - AutoValidateAntiforgeryToken (Global Filter)                                   |
|  - AspNetCoreRateLimit (Login Rate Limiter)                                       |
|  - ResourceOwnerAuthorizationHandler (Strict Data Scope Enforcement)              |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                                 SERVICES LAYER                                    |
|  - CustomerService           - LeadService & Workflow Engine                      |
|  - OpportunityService        - DealHealthService (0-100 Algorithm)                |
|  - FollowUpService           - LeadScoringService (ExpectedValue + Source)        |
|  - ActivityService           - AuditService (Diff Serialization & Logging)        |
|  - DashboardService          - ReportService (CSV Export Generator)               |
|  - UserService               - NotificationService (Overdue/Upcoming Alerts)      |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                               REPOSITORIES LAYER                                  |
|  - Generic IRepository<T> & Repository<T>                                         |
|  - ICustomerRepository, ILeadRepository, IOpportunityRepository                   |
|  - IFollowUpRepository, IActivityRepository, IAuditLogRepository                 |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                       ENTITY FRAMEWORK CORE 8 (CODE FIRST)                         |
|  - ApplicationDbContext                                                           |
|  - Fluent API Configurations (Soft Delete Query Filters, Indexes, Foreign Keys)   |
|  - ApplicationUser : IdentityUser, IdentityRole                                   |
|  - Customers, Leads, Opportunities, FollowUps, Activities, AuditLogs               |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                             SQL SERVER LOCALDB DATABASE                           |
+-----------------------------------------------------------------------------------+
```

---

## 5. Signature Differentiators

1. **Customer 360 View**: Chronological unified timeline aggregating Activities, Follow-Ups, and Audit events with state change diffs.
2. **Deal Health Score (0-100)**: Evaluates opportunity probability, days until expected close, days since last customer activity, and overdue follow-up counts. Color-coded badge with detailed explanation tooltip.
3. **Lead Score & Priority Engine**: Automatically calculates numerical lead score (0-100) and priority category (High/Medium/Low) based on deal size, channel source, and status.
4. **Interactive Kanban Pipeline Board**: Drag-and-drop HTML5 Kanban board with AJAX updates, anti-forgery token verification, server authorization, and audit logging.
5. **Lead Workflow Engine**: Enforces valid status transitions (`New` -> `Contacted` -> `Qualified` -> `Converted` / `Unqualified` / `Lost`).
6. **One-Click Lead Conversion**: Atomically converts Qualified leads into Customers with email duplicate prevention and optional Opportunity generation within a single database transaction.
7. **Audit Diff Viewer**: Field-by-field before-and-after comparison table rendered from JSON-serialized entity states, with sensitive fields redacted.
8. **Global Quick Search (Ctrl+K)**: Client modal searching across Customers, Leads, and Opportunities with real-time server querying scoped strictly by role.
9. **Navbar Notifications Bell**: Live badges displaying overdue and upcoming scheduled follow-ups.
10. **Theme Switcher**: Dark and Light theme toggle persisted in `localStorage`.
11. **Rich Seed Data**: Automatically populates 3 roles, 4 users, 30 customers, 40 leads, 40 opportunities, 60 follow-ups, and 80 activities spread over historical months.

---

## 6. Requirements Traceability Matrix

| Requirement | Implementation File / Class / Method |
|---|---|
| **Tech Stack: .NET 8 MVC + REST API** | `AcxiomCRM/Program.cs`, `AcxiomCRM.csproj` |
| **SQL Server LocalDB & EF Core Migrations** | `Data/ApplicationDbContext.cs`, `Migrations/` |
| **ASP.NET Core Identity & Password Policy** | `Program.cs`, `Models/Entities/ApplicationUser.cs` |
| **JWT Bearer Authentication** | `Program.cs`, `Controllers/Api/AuthController.cs` |
| **Rate Limiting on /api/auth/login** | `Program.cs`, `AspNetCoreRateLimit` |
| **Thin Controllers & ViewModels/DTOs** | `Controllers/`, `Models/ViewModels/`, `Models/Dtos/` |
| **Customer 360 Page & Unified Timeline** | `Controllers/CustomerController.cs` (`Details`), `Views/Customer/Details.cshtml` |
| **Deal Health Score (0-100)** | `Services/DealHealthService.cs`, `Services/IDealHealthService.cs` |
| **Lead Score & Priority** | `Services/LeadScoringService.cs` |
| **Kanban Pipeline Board & AJAX Stage Change** | `Controllers/OpportunityController.cs` (`Kanban`, `UpdateStage`), `Views/Opportunity/Kanban.cshtml` |
| **Lead Status Workflow Engine** | `Services/LeadService.cs` (`ChangeStatusAsync`, `TransitionMap`) |
| **One-Click Lead Conversion** | `Services/LeadService.cs` (`ConvertAsync`), `Controllers/LeadController.cs` (`Convert`) |
| **Audit Diff Viewer** | `Controllers/AuditController.cs` (`ComputeDiff`), `Views/Audit/Index.cshtml` |
| **Global Quick-Search (Ctrl+K)** | `Controllers/SearchController.cs`, `wwwroot/js/site.js`, `Views/Shared/_Layout.cshtml` |
| **Notifications Bell** | `Services/NotificationService.cs`, `Views/Shared/_Layout.cshtml` |
| **Dark/Light Theme Toggle** | `wwwroot/js/site.js`, `wwwroot/css/site.css` |
| **Rich Seed Data** | `Data/DbSeeder.cs` |
| **Role-based Scoping (Admin/Manager/Sales)** | `Security/ResourceOwnerAuthorizationHandler.cs`, `Services/*Service.cs` |
| **Validation: Indian Phone Regex** | `Validation/IndianPhoneAttribute.cs` |
| **Validation: FutureOrToday** | `Validation/FutureOrTodayAttribute.cs` |
| **Validation: ValidLeadStatus & ActivityType**| `Validation/ValidLeadStatusAttribute.cs`, `Validation/ValidActivityTypeAttribute.cs` |
| **Opportunity Amount > 0 & Probability 0-100**| `Services/OpportunityService.cs`, `Models/ViewModels/OpportunityViewModels.cs` |
| **Follow-Up Date Validation** | `Validation/FutureOrTodayAttribute.cs`, `Services/FollowUpService.cs` |
| **Global Exception Middleware** | `Middleware/GlobalExceptionMiddleware.cs` |
| **Security Headers & Anti-Forgery** | `Program.cs`, `Views/Shared/_Layout.cshtml` |
| **Audit Service & Log Tracking** | `Services/AuditService.cs`, `Models/Entities/AuditLog.cs` |
| **User & Role Administration** | `Controllers/UsersController.cs`, `Services/UserService.cs`, `Views/Users/` |
| **CSV Export & Reports** | `Controllers/ReportsController.cs`, `Services/ReportService.cs`, `Views/Reports/` |
| **xUnit Tests** | `AcxiomCRM.Tests/` |
