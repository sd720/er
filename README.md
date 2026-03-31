# Item Processor App

A full-stack web application built with **.NET 8 MVC + SQL Server** for managing items and tracking parent-child item processing trees.

---

## Features

- 🔐 Secure login with BCrypt password hashing
- 📦 Item management (Add, Edit, Delete, Search)
- ⚗️ Process items with parent-child output relationships
- 🌳 Recursive tree view of processed items
- ✅ Full input validation and error handling

---

## Tech Stack

- **Backend**: .NET 8 MVC, C#, Entity Framework Core
- **Database**: SQL Server Express
- **Auth**: Session-based with BCrypt hashing
- **Frontend**: Razor Views, Vanilla CSS, Vanilla JS

---

## Steps to Run

### Prerequisites

| Tool | Download |
|------|----------|
| .NET 8 SDK | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server Express | https://www.microsoft.com/sql-server/sql-server-downloads |
| SSMS (optional) | https://aka.ms/ssmsfullsetup |

### 1 – Set Up the Database

Run `database_script.sql` against your SQL Server instance:

```bash
# Using sqlcmd (adjust path as needed):
"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE" -S .\SQLEXPRESS -E -C -i database_script.sql
```

Or open it in SSMS and press **F5**.

### 2 – Configure Connection String

Open `ItemProcessorApp/appsettings.json` and update:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=ItemProcessorDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

Change `.\SQLEXPRESS` to your SQL Server instance name if different.

### 3 – Run the Application

```bash
cd ItemProcessorApp
dotnet run --urls "http://localhost:5000"
```

Open your browser at: **http://localhost:5000**

### 4 – Login

| Field | Value |
|-------|-------|
| Email | `admin@test.com` |
| Password | `Admin@123` |

---

## Project Structure

```
├── database_script.sql          ← Run first in SQL Server
├── test_cases.md                ← 49 test cases (all roles)
├── selenium_tests.py            ← Python + Selenium automation
└── ItemProcessorApp/
    ├── Controllers/             ← AuthController, ItemsController, ProcessController
    ├── Models/                  ← User, Item, ProcessedItem
    ├── ViewModels/              ← LoginViewModel, ProcessItemViewModel
    ├── Data/                    ← AppDbContext (EF Core)
    ├── Filters/                 ← AuthFilter (session auth)
    ├── Views/                   ← Razor views
    └── wwwroot/                 ← CSS & JS assets
```

---

## Screens

| Screen | URL |
|--------|-----|
| Login | `/Auth/Login` |
| Items List + Search | `/Items` |
| Add Item | `/Items/Create` |
| Edit Item | `/Items/Edit/{id}` |
| Delete Item | `/Items/Delete/{id}` |
| Process Item | `/Process/Create` |
| Tree View | `/Process/Tree` |

---

## Running Selenium Tests (Optional)

```bash
pip install selenium pytest webdriver-manager
pytest selenium_tests.py -v
```

> Requires Chrome and the app running on http://localhost:5000
