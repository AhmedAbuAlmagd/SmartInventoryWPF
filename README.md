# Smart Inventory - POS Desktop System

A high-performance, professional Point of Sale (POS) application built with WPF and .NET 8. Designed for modern retail and warehouse environments with a focus on speed, reliability, and high-contrast accessibility.

## 🎨 Modern UI Design
The system features a custom-crafted UI following modern design principles:
- **Palette**: Soft warm tones (Cream #faf8f4, Beige #f0ede6) with Sage Green (#4e6e4e) accents.
- **Accessibility**: 100% text visibility with deep charcoal on light backgrounds.
- **Layout**: 12px border radii, professional card-based containers, and high-contrast data tables.
- **Kiosk Ready**: Full-screen startup with a split-view modern login interface.

## 🚀 Key Features
- **Point of Sale**: 
    - Real-time cart management with +/- quantity controls.
    - Automatic **Tax (14%)** and **Discount** calculations.
    - Smart **Amount Paid** synchronization for rapid checkout.
    - Barcode scanner support for instant product lookup.
- **Product Management**: 
    - Full CRUD (Create, Read, Update, Delete) with modern modals.
    - Real-time stock tracking and inventory status indicators.
    - Searchable dropdowns and high-speed debounce search.
- **Invoice & History**: 
    - Detailed transaction logs with formatted financial data.
    - Re-open past invoices to view detailed sale items.
    - Centered, high-contrast table presentation.
- **Security & Roles**: 
    - Role-based access control (**Cashier** vs **Manager**).
    - Encrypted password storage using BCrypt.
    - SQL Injection protection via EF Core.

## 🛠 Tech Stack
- **Frontend**: WPF (Windows Presentation Foundation) on .NET 8.
- **Backend**: C# with a robust Layered Architecture.
- **Persistence**: Entity Framework Core with SQL Server.
- **Logging**: Professional audit trails and error tracking using **Serilog**.
- **Dependency Injection**: Full integration via `Microsoft.Extensions.DependencyInjection`.

## 🏗 Project Architecture
The system follows a clean, 4-layer architecture:
- **Presentation**: WPF Views, ViewModels, and XAML Styles.
- **Application**: Business services, DTOs, and interface definitions.
- **Domain**: Core entities, enums, and domain logic.
- **Infrastructure**: Data access (EF Core), Repository implementations, and external services (Logging/Printing).

## 🔧 Setup & Installation

1. **Prerequisites**:
   - .NET 8.0 SDK
   - Visual Studio 2022 (with .NET Desktop Development workload)
   - SQL Server (LocalDB or Express)

2. **Database Setup**:
   - Update the connection string in `App.xaml.cs` or `appsettings.json` (if applicable).
   - The default configuration uses `Server=.;Database=POSDesktopDb;Trusted_Connection=True;TrustServerCertificate=True;`.
   - Run the following command in the Package Manager Console to apply migrations:
     ```bash
     Update-Database -Project src/POSDesktopSystem.Infrastructure
     ```

3. **Running the App**:
   - Open `POSDesktopSystem.sln`.
   - Set `POSDesktopSystem.Presentation` as the Startup Project.
   - Build and Run (F5).

## 🔑 Seeded Credentials

The system comes with pre-seeded users for testing:

- **Manager**: `manager` / `Manager@123`
- **Cashier**: `cashier` / `Cashier@123`

## 📝 Logging & Diagnostics
Logs are automatically generated for all critical business actions and system errors.
- **File Location**: `%AppData%/Roaming/POSDesktopSystem/logs/`
- **Audit Logs**: Sales completions, stock changes, and login attempts are recorded for manager review.

---
*Built as part of the Smart Inventory Suite.*
