# POS Desktop System — WPF Implementation Prompt for AI Agent

> **Read every section before writing a single line of code.**
> **Violations of the architecture, structure, or constraints will require a full rewrite.**

---

## Project Overview

Build a WPF Desktop POS (Point of Sale) system in C# .NET 8.
The app manages products, sales, invoices, and receipt printing.
It has two roles: Manager and Cashier.

**Technology stack — fixed, no substitutions:**
- WPF (.NET 8)
- C# 12
- SQL Server (LocalDB for dev)
- Entity Framework Core 8
- MVVM pattern (no code-behind logic — ever)

---

## Non-Negotiable Architecture Rules

These are hard constraints. Violating any of them is not acceptable.

```
RULE 1 — MVVM is strict.
  Views: XAML only. Zero business logic in .xaml.cs files.
  The only code allowed in .xaml.cs is:
    - InitializeComponent()
    - Constructor that receives ViewModel via DI
  Everything else — commands, data, logic — lives in the ViewModel.

RULE 2 — No layer imports upward.
  Domain has zero dependencies on anything.
  Application depends only on Domain.
  Infrastructure depends on Domain and Application.
  WPF (Presentation) depends on Application only — never on Infrastructure directly.
  Infrastructure is wired up only in the DI composition root (App.xaml.cs).

RULE 3 — One ViewModel per View. One View per ViewModel.
  No shared ViewModels across multiple windows.
  No God ViewModel that handles multiple concerns.

RULE 4 — No static classes except constants and extension methods.
  No static service locators. No static data holders.
  All dependencies injected via constructor.

RULE 5 — Repository abstractions live in Application layer.
  EF Core DbContext and concrete repositories live in Infrastructure.
  No EF Core namespace imports anywhere in Application or Domain.

RULE 6 — No direct MessageBox calls in ViewModels.
  Use a IDialogService abstraction injected into the ViewModel.
  The WPF implementation lives in the Presentation layer.

RULE 7 — Every public method that touches the database is async.
  Use async/await throughout. No .Result or .Wait() calls ever.

RULE 8 — No magic strings.
  Navigation keys, role names, config keys — all in constants files.

RULE 9 — Folder structure must match exactly what is specified below.
  Do not create extra folders. Do not merge folders.
  Do not rename anything without a documented reason.

RULE 10 — Every ViewModel inherits BaseViewModel.
  Every command uses RelayCommand or AsyncRelayCommand.
  No inline ICommand implementations.
```

---

## Solution Structure

```
POSDesktopSystem/
├── POSDesktopSystem.sln
│
├── src/
│   ├── POSDesktopSystem.Domain/            ← Class Library
│   ├── POSDesktopSystem.Application/       ← Class Library
│   ├── POSDesktopSystem.Infrastructure/    ← Class Library
│   └── POSDesktopSystem.Presentation/      ← WPF Application (.NET 8-windows)
│
└── tests/
    └── POSDesktopSystem.Tests/             ← xUnit Test Project
```

**Project references:**
```
Domain        ← no references
Application   ← Domain
Infrastructure← Domain + Application
Presentation  ← Application only  (NOT Infrastructure)
Tests         ← Application + Domain
```

Infrastructure is only referenced in `Presentation/App.xaml.cs` for DI wiring.

---

## Detailed Folder Structure

### Domain Project
```
POSDesktopSystem.Domain/
├── Entities/
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Product.cs
│   ├── Invoice.cs
│   ├── InvoiceItem.cs
│   └── Payment.cs
├── Enums/
│   ├── UserRole.cs          # Manager, Cashier
│   ├── PaymentMethod.cs     # Cash, Card
│   └── InvoiceStatus.cs     # Open, Paid, Cancelled
└── Interfaces/
    ├── Repositories/
    │   ├── IRepository.cs
    │   ├── IProductRepository.cs
    │   ├── IInvoiceRepository.cs
    │   ├── IInvoiceItemRepository.cs
    │   └── IUserRepository.cs
    └── IUnitOfWork.cs
```

### Application Project
```
POSDesktopSystem.Application/
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequestDto.cs
│   │   └── LoginResultDto.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductDto.cs
│   │   └── UpdateProductDto.cs
│   ├── Invoices/
│   │   ├── InvoiceDto.cs
│   │   ├── InvoiceItemDto.cs
│   │   └── CreateInvoiceDto.cs
│   └── Common/
│       └── PagedResultDto.cs
├── Exceptions/
│   ├── AppException.cs
│   ├── NotFoundException.cs
│   ├── ValidationException.cs
│   ├── UnauthorizedException.cs
│   └── InsufficientStockException.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IProductService.cs
│   ├── IInvoiceService.cs
│   ├── ISessionService.cs    # holds current logged-in user in memory
│   └── IReceiptService.cs    # receipt printing abstraction
└── Services/
    ├── AuthService.cs
    ├── ProductService.cs
    ├── InvoiceService.cs
    └── SessionService.cs
```

### Infrastructure Project
```
POSDesktopSystem.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/           # EF migrations auto-generated here
├── Repositories/
│   ├── Repository.cs          # generic base
│   ├── ProductRepository.cs
│   ├── InvoiceRepository.cs
│   ├── InvoiceItemRepository.cs
│   └── UserRepository.cs
├── UnitOfWork.cs
├── Logging/
│   └── FileLogger.cs          # simple file logger, implements ILogger<T>
└── Printing/
    └── ReceiptPrinter.cs       # implements IReceiptService
```

### Presentation Project (WPF)
```
POSDesktopSystem.Presentation/
├── App.xaml
├── App.xaml.cs                 # DI composition root ONLY
├── Constants/
│   ├── AppConstants.cs         # role names, config keys
│   └── NavigationKeys.cs       # window/view navigation string keys
├── Infrastructure/
│   ├── DependencyInjection.cs  # extension method: services.AddPresentation()
│   ├── DialogService.cs        # implements IDialogService
│   └── NavigationService.cs    # implements INavigationService
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── InvoiceStatusConverter.cs
│   └── PaymentMethodConverter.cs
├── Behaviors/
│   └── FocusOnLoadBehavior.cs
├── Styles/
│   ├── Colors.xaml
│   ├── Typography.xaml
│   ├── Buttons.xaml
│   ├── Inputs.xaml
│   ├── DataGrid.xaml
│   └── App.xaml.merged         # merges all above into one ResourceDictionary
├── ViewModels/
│   ├── Base/
│   │   ├── BaseViewModel.cs    # INotifyPropertyChanged + helper methods
│   │   └── RelayCommand.cs     # ICommand implementation
│   ├── Auth/
│   │   └── LoginViewModel.cs
│   ├── Products/
│   │   ├── ProductsViewModel.cs
│   │   └── ProductFormViewModel.cs
│   ├── POS/
│   │   └── PosViewModel.cs
│   ├── Invoices/
│   │   ├── InvoicesViewModel.cs
│   │   └── InvoiceDetailViewModel.cs
│   └── Shell/
│       └── ShellViewModel.cs   # main window nav state
└── Views/
    ├── Auth/
    │   └── LoginWindow.xaml (+.xaml.cs)
    ├── Products/
    │   ├── ProductsView.xaml (+.xaml.cs)
    │   └── ProductFormDialog.xaml (+.xaml.cs)
    ├── POS/
    │   └── PosView.xaml (+.xaml.cs)
    ├── Invoices/
    │   ├── InvoicesView.xaml (+.xaml.cs)
    │   └── InvoiceDetailView.xaml (+.xaml.cs)
    └── Shell/
        └── MainWindow.xaml (+.xaml.cs)
```

### Tests Project
```
POSDesktopSystem.Tests/
├── Services/
│   ├── AuthServiceTests.cs
│   ├── ProductServiceTests.cs
│   └── InvoiceServiceTests.cs
└── ViewModels/
    └── PosViewModelTests.cs
```

---

## Domain Layer — Full Specification

### BaseEntity
```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### Entities

```csharp
// User.cs
public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}

// Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;      // unique
    public string? Description { get; set; }
    public decimal Price { get; set; }                   // selling price
    public decimal CostPrice { get; set; }               // purchase cost
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
}

// Invoice.cs
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = default!;  // auto-generated: INV-YYYYMMDD-XXXX
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    public decimal SubTotal { get; set; }     // sum of all items before tax/discount
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }      // stored as decimal e.g. 0.14 for 14%
    public decimal TaxAmount { get; set; }    // calculated: (SubTotal - DiscountAmount) * TaxRate
    public decimal TotalAmount { get; set; }  // SubTotal - Discount + Tax

    public int CashierId { get; set; }
    public User Cashier { get; set; } = default!;

    public ICollection<InvoiceItem> Items { get; set; } = [];
    public Payment? Payment { get; set; }
}

// InvoiceItem.cs
public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string ProductName { get; set; } = default!;  // snapshot at time of sale
    public decimal UnitPrice { get; set; }               // snapshot at time of sale
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }               // UnitPrice * Quantity
}

// Payment.cs
public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public PaymentMethod Method { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }       // AmountPaid - Invoice.TotalAmount (cash only)
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
```

### Enums
```csharp
public enum UserRole      { Manager, Cashier }
public enum PaymentMethod { Cash, Card }
public enum InvoiceStatus { Open, Paid, Cancelled }
```

### Repository Interfaces
```csharp
// IRepository.cs — generic base
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// IProductRepository.cs
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
    Task<IEnumerable<Product>> SearchAsync(string term);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search);
}

// IInvoiceRepository.cs
public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(int id);       // includes Items + Payment + Cashier
    Task<IEnumerable<Invoice>> GetTodaysInvoicesAsync();
    Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, InvoiceStatus? status);
    Task<string> GenerateInvoiceNumberAsync();        // INV-YYYYMMDD-XXXX format
}

// IUserRepository.cs
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}

// IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IInvoiceRepository Invoices { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
```

---

## Application Layer — Full Specification

### DTOs

```csharp
// LoginRequestDto.cs
public class LoginRequestDto
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

// LoginResultDto.cs
public class LoginResultDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = default!;
    public UserRole Role { get; set; }
}

// ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}

// CreateProductDto.cs
public class CreateProductDto
{
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
}

// UpdateProductDto.cs — same fields + IsActive
public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; }
}

// InvoiceDto.cs
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public string Status { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CashierName { get; set; } = default!;
    public List<InvoiceItemDto> Items { get; set; } = [];
    public string? PaymentMethod { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? Change { get; set; }
}

// InvoiceItemDto.cs
public class InvoiceItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

// CreateInvoiceDto.cs
public class CreateInvoiceDto
{
    public int CashierId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
}

// PagedResultDto.cs
public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

### Service Interfaces

```csharp
// IAuthService.cs
public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto dto);
}

// ISessionService.cs
// Holds the currently logged-in user in memory for the app lifetime.
// Singleton — injected wherever the current user is needed.
public interface ISessionService
{
    LoginResultDto? CurrentUser { get; }
    bool IsLoggedIn { get; }
    bool IsManager { get; }
    void SetUser(LoginResultDto user);
    void Clear();
}

// IProductService.cs
public interface IProductService
{
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto?> GetByBarcodeAsync(string barcode);
    Task<PagedResultDto<ProductDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<IEnumerable<ProductDto>> SearchAsync(string term);   // for POS quick search
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);                                 // soft delete
}

// IInvoiceService.cs
public interface IInvoiceService
{
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<PagedResultDto<InvoiceDto>> GetAllAsync(int page, int pageSize, InvoiceStatus? status);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceDto> CancelAsync(int id);
    Task<IEnumerable<InvoiceDto>> GetTodaysInvoicesAsync();
}

// IReceiptService.cs
public interface IReceiptService
{
    void PrintReceipt(InvoiceDto invoice);
    // Sends to default printer. On failure: throws AppException — never crashes silently.
}

// IDialogService.cs  ← lives in Application, implemented in Presentation
public interface IDialogService
{
    bool Confirm(string title, string message);
    void ShowError(string title, string message);
    void ShowInfo(string title, string message);
    T? ShowDialog<T>(object viewModel) where T : class;  // opens a dialog, returns result
}

// INavigationService.cs  ← lives in Application, implemented in Presentation
public interface INavigationService
{
    void NavigateTo(string viewKey);           // uses NavigationKeys constants
    void GoBack();
    string CurrentView { get; }
}
```

### Service Implementations — Logic Detail

```csharp
// AuthService.cs
// LoginAsync:
//   1. GetByUsernameAsync → throw UnauthorizedException("Invalid credentials") if null
//   2. Check IsActive → throw UnauthorizedException("Account is disabled") if false
//   3. BCrypt.Verify(dto.Password, user.PasswordHash) → throw UnauthorizedException if false
//   4. Map to LoginResultDto and return

// SessionService.cs (Singleton)
// Holds CurrentUser in a private field.
// IsManager: CurrentUser?.Role == UserRole.Manager
// SetUser: sets the field
// Clear: sets field to null (called on logout)

// ProductService.cs
// GetByBarcodeAsync:
//   1. repo.GetByBarcodeAsync → return null if not found (not an exception — POS uses this)
// CreateAsync:
//   1. BarcodeExistsAsync → throw ConflictException("Barcode already exists") if true
//   2. StockQuantity must be >= 0 → throw ValidationException if negative
//   3. Price and CostPrice must be > 0 → throw ValidationException if not
//   4. Map, Add, SaveChanges, return dto
// UpdateAsync:
//   1. GetByIdAsync → throw NotFoundException if null
//   2. Apply all fields from dto
//   3. Set UpdatedAt = DateTime.UtcNow
//   4. Update, SaveChanges, return dto
// DeleteAsync:
//   1. GetByIdAsync → throw NotFoundException if null
//   2. entity.IsActive = false  ← soft delete, never hard delete products
//   3. Update, SaveChanges

// InvoiceService.cs
// CreateAsync:
//   1. Validate Items list is not empty → throw ValidationException
//   2. For each item: GetByIdAsync product, check IsActive, check StockQuantity >= item.Quantity
//      → throw InsufficientStockException if not enough stock
//   3. Calculate:
//        SubTotal      = sum(item.UnitPrice * item.Quantity)
//        DiscountAmount= dto.DiscountAmount (validate: 0 <= discount <= SubTotal)
//        TaxAmount     = (SubTotal - DiscountAmount) * dto.TaxRate
//        TotalAmount   = SubTotal - DiscountAmount + TaxAmount
//   4. Validate AmountPaid >= TotalAmount for Cash payments
//      (Card payments: AmountPaid == TotalAmount, Change == 0)
//   5. GenerateInvoiceNumberAsync
//   6. Snapshot product name and price into InvoiceItem
//   7. Deduct StockQuantity for each product
//   8. Create Invoice + InvoiceItems + Payment in one SaveChanges call
//   9. Return full InvoiceDto with all details
// CancelAsync:
//   1. GetWithDetailsAsync → NotFoundException if null
//   2. Status must be Open → throw ValidationException("Only open invoices can be cancelled")
//   3. Restore stock quantities for each item
//   4. Set Status = Cancelled, UpdatedAt = UtcNow
//   5. SaveChanges
```

---

## Infrastructure Layer — Full Specification

### AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode).IsUnique();
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => p.IsActive);   // global soft delete filter

        // Invoice
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber).IsUnique();
        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxRate).HasPrecision(5, 4);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Status).HasConversion<string>();

        // InvoiceItem
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.LineTotal).HasPrecision(18, 2);

        // Payment
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithOne(i => i.Payment)
            .HasForeignKey<Payment>(p => p.InvoiceId);
        modelBuilder.Entity<Payment>()
            .Property(p => p.AmountPaid).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>()
            .Property(p => p.Change).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>()
            .Property(p => p.Method).HasConversion<string>();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .Property(u => u.Role).HasConversion<string>();

        // Seed: manager account
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
            Role = UserRole.Manager,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
```

### Repository Implementations

```csharp
// Repository.cs — generic base, same pattern as Task 1
// ProductRepository.cs
//   GetByBarcodeAsync: FirstOrDefaultAsync(p => p.Barcode == barcode)
//   BarcodeExistsAsync: AnyAsync with IgnoreQueryFilters (barcode globally unique)
//   SearchAsync: Where name or barcode contains term, take 20 results for POS dropdown
//   GetPagedAsync: same paged pattern as Task 1 backend

// InvoiceRepository.cs
//   GetWithDetailsAsync:
//     Include(i => i.Items).ThenInclude(ii => ii.Product)
//     Include(i => i.Payment)
//     Include(i => i.Cashier)
//   GetTodaysInvoicesAsync: Where InvoiceDate.Date == DateTime.Today
//   GenerateInvoiceNumberAsync:
//     Format: "INV-{YYYYMMDD}-{sequence:D4}"
//     Sequence = count of today's invoices + 1
//     Example: INV-20250523-0001
```

### Logging

```csharp
// FileLogger.cs
// Writes to: %AppData%\POSDesktopSystem\logs\log-YYYY-MM-DD.txt
// Format per line: [TIMESTAMP] [LEVEL] [CATEGORY] MESSAGE
// Exception: also log ex.Message + ex.StackTrace
// Implement ILogger<T> so it integrates with Microsoft.Extensions.Logging
// Rotate files by date — one file per day
// Never throw from the logger itself — wrap all IO in try/catch
```

### Receipt Printing

```csharp
// ReceiptPrinter.cs — implements IReceiptService
// Uses System.Drawing.Printing.PrintDocument
// Receipt layout (80mm thermal printer width = 576 units at 96dpi):
//
//   ================================
//        SMART POS SYSTEM
//   ================================
//   Invoice: INV-20250523-0001
//   Date: 23/05/2025 08:39 PM
//   Cashier: john
//   --------------------------------
//   Item Name         Qty    Price
//   --------------------------------
//   [each item line]
//   --------------------------------
//   Subtotal:              EGP XX.XX
//   Discount:             -EGP XX.XX
//   Tax (14%):             EGP XX.XX
//   TOTAL:                 EGP XX.XX
//   ================================
//   Payment: Cash
//   Paid:                  EGP XX.XX
//   Change:                EGP XX.XX
//   ================================
//        Thank you for your visit!
//   ================================
//
// PrintReceipt sends to the default printer.
// On PrinterNotFound or any Exception: throw AppException("Printing failed: " + ex.Message)
// Do NOT show MessageBox here — throw and let the ViewModel handle it via IDialogService
```

---

## Presentation Layer — Full Specification

### App.xaml.cs — Composition Root

```csharp
// App.xaml.cs is the ONLY place Infrastructure is referenced.
// Build the DI container here, show LoginWindow, nothing else.

public partial class App : Application
{
    private IServiceProvider _services = default!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Infrastructure
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(GetConnectionString()));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddSingleton<ISessionService, SessionService>();

        // Presentation Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IReceiptService, ReceiptPrinter>();

        // Logging
        services.AddLogging(builder => builder.AddProvider(new FileLoggerProvider()));

        // ViewModels — all Transient
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ProductFormViewModel>();
        services.AddTransient<PosViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();

        _services = services.BuildServiceProvider();

        // Auto-migrate on startup
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Show login
        var loginWindow = new LoginWindow(_services.GetRequiredService<LoginViewModel>());
        loginWindow.Show();
    }

    private static string GetConnectionString() =>
        "Server=(localdb)\\mssqllocaldb;Database=POSDesktopDb;Trusted_Connection=True;";
}
```

### BaseViewModel

```csharp
// ViewModels/Base/BaseViewModel.cs
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    // Convenience: notify multiple properties at once
    protected void OnPropertiesChanged(params string[] names)
    {
        foreach (var name in names) OnPropertyChanged(name);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    protected void ClearError() => ErrorMessage = null;
}
```

### RelayCommand

```csharp
// ViewModels/Base/RelayCommand.cs
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

// AsyncRelayCommand — for all async operations
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter)
    {
        _isExecuting = true;
        CommandManager.InvalidateRequerySuggested();
        try   { await _execute(parameter); }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
```

### ViewModel Specifications

```csharp
// LoginViewModel.cs
// Properties: Username (string), Password (string), IsLoading (bool), ErrorMessage (string?)
// Commands:
//   LoginCommand (AsyncRelayCommand)
//     CanExecute: !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsBusy
//     Execute:
//       1. IsBusy = true, ClearError()
//       2. await _authService.LoginAsync(...)
//       3. _sessionService.SetUser(result)
//       4. _navigationService.NavigateTo(NavigationKeys.Shell)
//          ← opens MainWindow, closes LoginWindow
//       5. On UnauthorizedException: ErrorMessage = ex.Message
//       6. On any other exception: log it, ErrorMessage = "An unexpected error occurred."
//       7. finally: IsBusy = false

// ShellViewModel.cs
// Properties:
//   CurrentViewModel (BaseViewModel) — bound to ContentControl in MainWindow
//   CurrentViewTitle (string)
//   IsManagerRole (bool) — from _sessionService.IsManager
//   CurrentUsername (string) — from _sessionService.CurrentUser.Username
// Commands:
//   NavigateToPosCommand
//   NavigateToProductsCommand  (CanExecute: IsManagerRole)
//   NavigateToInvoicesCommand
//   LogoutCommand
//     Execute: _sessionService.Clear(), open LoginWindow, close MainWindow

// ProductsViewModel.cs
// Properties:
//   Products (ObservableCollection<ProductDto>)
//   SelectedProduct (ProductDto?)
//   SearchTerm (string) — when set, re-loads with 400ms debounce (use DispatcherTimer)
//   TotalCount (int)
//   CurrentPage (int)
//   PageSize (int) = 15
// Commands:
//   LoadCommand (AsyncRelayCommand) — loads paged products
//   AddCommand — opens ProductFormDialog with new empty form
//   EditCommand (AsyncRelayCommand) — opens ProductFormDialog with selected product loaded
//     CanExecute: SelectedProduct != null
//   DeleteCommand (AsyncRelayCommand)
//     CanExecute: SelectedProduct != null
//     Execute:
//       1. _dialogService.Confirm("Delete Product", "Are you sure?") → return if false
//       2. _productService.DeleteAsync(SelectedProduct.Id)
//       3. Reload
//   NextPageCommand, PreviousPageCommand

// ProductFormViewModel.cs
// Mode determined by whether an existing ProductDto is passed in constructor
// Properties: Name, Barcode, Description, Price, CostPrice, StockQuantity, Category, IsActive
// Commands:
//   SaveCommand (AsyncRelayCommand)
//     CanExecute: all required fields filled, Price > 0, CostPrice >= 0, StockQty >= 0
//     Execute:
//       1. Validate — if invalid, set ErrorMessage and return
//       2. CreateAsync or UpdateAsync depending on mode
//       3. On ConflictException (duplicate barcode): ErrorMessage = "Barcode already exists"
//       4. On success: CloseRequested?.Invoke() — ViewModel raises event, Dialog handles close
//   CancelCommand — raises CloseRequested

// PosViewModel.cs  ← most complex ViewModel
// Properties:
//   BarcodeInput (string) — bound to barcode text box, triggers AddByBarcode on Enter
//   SearchTerm (string) — product search for manual add
//   SearchResults (ObservableCollection<ProductDto>)
//   CartItems (ObservableCollection<CartItemViewModel>)
//   SubTotal (decimal) — computed from CartItems
//   DiscountAmount (decimal)
//   TaxRate (decimal) = 0.14m  (14% default, editable by manager)
//   TaxAmount (decimal) — computed: (SubTotal - DiscountAmount) * TaxRate
//   TotalAmount (decimal) — computed: SubTotal - DiscountAmount + TaxAmount
//   SelectedPaymentMethod (PaymentMethod)
//   AmountPaid (decimal)
//   Change (decimal) — computed: AmountPaid - TotalAmount (only for Cash)
//   IsDiscountEditable (bool) — from _sessionService.IsManager (only managers set discount)
//   IsTaxEditable (bool) — from _sessionService.IsManager
//   CanCheckout (bool) — CartItems.Count > 0 && AmountPaid >= TotalAmount
// Commands:
//   AddByBarcodeCommand (AsyncRelayCommand)
//     Execute:
//       1. GetByBarcodeAsync(BarcodeInput)
//       2. If null: ErrorMessage = $"Product with barcode '{BarcodeInput}' not found"
//       3. If found: AddToCart(product), clear BarcodeInput
//   SearchProductsCommand (AsyncRelayCommand)
//   AddFromSearchCommand — adds selected search result to cart
//   RemoveCartItemCommand — removes item from CartItems
//   IncrementQuantityCommand (CartItemViewModel param)
//   DecrementQuantityCommand (CartItemViewModel param)
//   CheckoutCommand (AsyncRelayCommand)
//     CanExecute: CanCheckout
//     Execute:
//       1. Build CreateInvoiceDto from current cart state
//       2. _invoiceService.CreateAsync(dto)
//       3. On success: _receiptService.PrintReceipt(result)
//       4. Clear cart, reset all fields
//       5. On InsufficientStockException: ErrorMessage = ex.Message
//       6. On AppException (printing failed): 
//            _dialogService.ShowError("Print Failed", ex.Message)
//            — invoice was still saved, just print failed
//   ClearCartCommand — _dialogService.Confirm before clearing

// CartItemViewModel.cs  ← simple sub-ViewModel for each cart row
// Properties: ProductId, ProductName, Barcode, UnitPrice, Quantity, LineTotal
// LineTotal = UnitPrice * Quantity (auto-updated when Quantity changes)

// InvoicesViewModel.cs
// Properties:
//   Invoices (ObservableCollection<InvoiceDto>)
//   SelectedInvoice (InvoiceDto?)
//   StatusFilter (InvoiceStatus?) — null = all
//   CurrentPage, PageSize = 20, TotalCount
// Commands:
//   LoadCommand
//   ViewDetailCommand — opens InvoiceDetailView with selected invoice
//   CancelInvoiceCommand (AsyncRelayCommand)
//     CanExecute: SelectedInvoice?.Status == "Open" && IsManagerRole
//     Execute: Confirm → CancelAsync → reload
//   ReprintCommand
//     CanExecute: SelectedInvoice?.Status == "Paid"
//     Execute: GetByIdAsync → PrintReceipt

// InvoiceDetailViewModel.cs
// Properties: Invoice (InvoiceDto) — loaded on init
// Read-only display ViewModel — no edit commands
// Commands: PrintCommand, CloseCommand
```

---

## Navigation Pattern

```csharp
// NavigationService.cs
// MainWindow has a ContentControl bound to ShellViewModel.CurrentViewModel
// NavigateTo(string key) resolves the ViewModel from DI and sets it on ShellViewModel

// NavigationKeys.cs (constants)
public static class NavigationKeys
{
    public const string Shell     = "Shell";
    public const string Pos       = "Pos";
    public const string Products  = "Products";
    public const string Invoices  = "Invoices";
}

// LoginWindow → on success → show MainWindow → close LoginWindow
// MainWindow uses ContentControl, not separate windows for internal views
// ProductFormDialog and InvoiceDetailView are separate Dialogs (Window.ShowDialog())
// All other navigation is ContentControl swap inside MainWindow
```

---

## XAML / UI Specification

### MainWindow Layout
```xml
<!-- Shell: DockPanel -->
<!-- Top: Topbar (app name, username, role, logout button) -->
<!-- Left: NavPanel (vertical StackPanel of nav buttons) -->
<!-- Center: ContentControl bound to ShellViewModel.CurrentViewModel -->
<!-- DataTemplates in App.xaml map ViewModel types to View UserControls -->
```

### DataTemplates (in App.xaml ResourceDictionary)
```xml
<DataTemplate DataType="{x:Type vm:PosViewModel}">
    <views:PosView />
</DataTemplate>
<DataTemplate DataType="{x:Type vm:ProductsViewModel}">
    <views:ProductsView />
</DataTemplate>
<DataTemplate DataType="{x:Type vm:InvoicesViewModel}">
    <views:InvoicesView />
</DataTemplate>
<!-- etc. for each ViewModel/View pair -->
```

### POS View Layout
```
Top row:   Barcode input box (auto-focus) + "Add" button
           Product search box + search results dropdown (Popup or ListBox)
           
Left panel (60% width): Cart DataGrid
  Columns: Product | Barcode | Unit Price | Qty (editable) | Line Total | Remove button
  
Right panel (40% width): Order summary card
  SubTotal:       EGP XX.XX
  Discount:       EGP [editable if Manager]  
  Tax (XX%):      EGP XX.XX  (rate editable if Manager)
  ─────────────────────────
  TOTAL:          EGP XX.XX  (large, bold)
  
  Payment Method: Radio buttons (Cash / Card)
  Amount Paid:    [input, only enabled for Cash — Card auto-fills TotalAmount]
  Change:         EGP XX.XX  (only visible for Cash)
  
  [Checkout] button — full width, primary color
  [Clear Cart] button — secondary style
```

### Products View Layout
```
Top row:  Search TextBox (with 400ms debounce) + [Add Product] button (Manager only)
DataGrid: Name | Barcode | Category | Price | Stock | Status | [Edit] [Delete]
          Edit/Delete column visible only when IsManager
Bottom:   Pagination controls (Previous | Page X of Y | Next)
```

### Invoices View Layout
```
Top row: Status filter ComboBox (All / Open / Paid / Cancelled)
DataGrid: Invoice# | Date | Cashier | SubTotal | Discount | Tax | Total | Status | [View] [Cancel] [Reprint]
          Cancel visible only for Open invoices AND Manager role
          Reprint visible only for Paid invoices
Bottom: Pagination
```

---

## Style System (XAML Resource Dictionaries)

```xml
<!-- Colors.xaml — define all colors as SolidColorBrush resources -->
<SolidColorBrush x:Key="PrimaryBrush"       Color="#2D6A4F"/>
<SolidColorBrush x:Key="PrimaryDarkBrush"   Color="#1B4332"/>
<SolidColorBrush x:Key="AccentBrush"        Color="#40916C"/>
<SolidColorBrush x:Key="BackgroundBrush"    Color="#F7F8FA"/>
<SolidColorBrush x:Key="SurfaceBrush"       Color="#FFFFFF"/>
<SolidColorBrush x:Key="BorderBrush"        Color="#E4E7EC"/>
<SolidColorBrush x:Key="TextPrimaryBrush"   Color="#111827"/>
<SolidColorBrush x:Key="TextSecondaryBrush" Color="#6B7280"/>
<SolidColorBrush x:Key="TextMutedBrush"     Color="#9CA3AF"/>
<SolidColorBrush x:Key="ErrorBrush"         Color="#C53030"/>
<SolidColorBrush x:Key="SuccessBrush"       Color="#276749"/>
<SolidColorBrush x:Key="WarningBrush"       Color="#B7791F"/>
<SolidColorBrush x:Key="NavBgBrush"         Color="#16213E"/>
<SolidColorBrush x:Key="NavTextBrush"       Color="#A8B2C8"/>
<SolidColorBrush x:Key="NavActiveBgBrush"   Color="#1E2D4A"/>
<SolidColorBrush x:Key="NavActiveTextBrush" Color="#FFFFFF"/>

<!-- Typography.xaml -->
<!-- Define TextBlock styles: HeadingStyle, SubheadingStyle, BodyStyle, MutedStyle, MonoStyle -->
<!-- Font family: Segoe UI for all text, Consolas for barcodes/numbers -->

<!-- Buttons.xaml -->
<!-- PrimaryButtonStyle: green bg, white text, 8px radius, hover darken -->
<!-- SecondaryButtonStyle: white bg, border, green text, hover light green bg -->
<!-- DangerButtonStyle: red bg, white text -->
<!-- IconButtonStyle: transparent, icon only, hover light bg -->

<!-- Inputs.xaml -->
<!-- TextBoxStyle: border, rounded, focus highlight in primary color -->
<!-- ComboBoxStyle: matching style -->

<!-- DataGrid.xaml -->
<!-- Clean DataGrid: no vertical gridlines, horizontal only, row hover bg -->
```

---

## NuGet Packages

### Domain — none

### Application
```
BCrypt.Net-Next (4.x)
```

### Infrastructure
```
Microsoft.EntityFrameworkCore.SqlServer (8.x)
Microsoft.EntityFrameworkCore.Tools (8.x)
Microsoft.EntityFrameworkCore.Design (8.x)
BCrypt.Net-Next (4.x)
Microsoft.Extensions.Logging.Abstractions (8.x)
```

### Presentation
```
Microsoft.Extensions.DependencyInjection (8.x)
Microsoft.EntityFrameworkCore.SqlServer (8.x)  ← for DI wiring only
```

### Tests
```
xunit (2.x)
xunit.runner.visualstudio (2.x)
Microsoft.NET.Test.Sdk
Moq (4.x)
FluentAssertions (6.x)
Microsoft.EntityFrameworkCore.InMemory (8.x)
```

---

## Unit Tests

### What to test

```
AuthServiceTests:
  Login_ValidCredentials_ReturnsLoginResult
  Login_WrongPassword_ThrowsUnauthorizedException
  Login_DisabledUser_ThrowsUnauthorizedException
  Login_UserNotFound_ThrowsUnauthorizedException

ProductServiceTests:
  Create_DuplicateBarcode_ThrowsConflictException
  Create_NegativeStock_ThrowsValidationException
  Delete_SetsIsActiveFalse
  GetByBarcode_NotFound_ReturnsNull (not exception)

InvoiceServiceTests:
  Create_EmptyItems_ThrowsValidationException
  Create_InsufficientStock_ThrowsInsufficientStockException
  Create_ValidInvoice_DeductsStockCorrectly
  Create_ValidInvoice_CalculatesTotalsCorrectly
  Cancel_PaidInvoice_ThrowsValidationException
  Cancel_OpenInvoice_RestoresStock

PosViewModelTests:
  AddByBarcode_ProductNotFound_SetsErrorMessage
  AddByBarcode_ValidProduct_AddsToCart
  Checkout_EmptyCart_CanExecuteIsFalse
  TotalAmount_UpdatesWhenDiscountChanges
```

---

## Validation Rules Summary

```
Product:
  Name:         required, max 200 chars
  Barcode:      required, max 50 chars, alphanumeric + hyphens, globally unique
  Price:        required, > 0
  CostPrice:    required, >= 0
  StockQuantity:required, >= 0
  Category:     optional, max 100 chars

Invoice:
  Items:        at least 1 item
  DiscountAmount: >= 0 and <= SubTotal
  TaxRate:      >= 0 and <= 1 (percentage stored as decimal)
  AmountPaid:   >= TotalAmount for Cash; == TotalAmount for Card
  Each item Quantity: >= 1

Login:
  Username:     required, not empty
  Password:     required, not empty
```

---

## SQL Injection Protection

```
EF Core parameterizes all queries by default — this covers all repository operations.
Never use raw SQL strings with string interpolation.
If raw SQL is ever needed (it shouldn't be): use FromSqlRaw with parameters only.
  CORRECT:   FromSqlRaw("SELECT * FROM Products WHERE Barcode = {0}", barcode)
  INCORRECT: FromSqlRaw($"SELECT * FROM Products WHERE Barcode = '{barcode}'")
```

---

## EF Core Migrations

```bash
# Run from solution root
dotnet ef migrations add InitialCreate \
  --project src/POSDesktopSystem.Infrastructure \
  --startup-project src/POSDesktopSystem.Presentation

dotnet ef database update \
  --project src/POSDesktopSystem.Infrastructure \
  --startup-project src/POSDesktopSystem.Presentation

# Export SQL script for deliverable
dotnet ef migrations script \
  --project src/POSDesktopSystem.Infrastructure \
  --startup-project src/POSDesktopSystem.Presentation \
  --output database_script.sql \
  --idempotent
```

---

## Implementation Order — Follow Exactly

```
Step 1 — Scaffold
  Create solution + 4 projects
  Add all project references (as specified in Structure section)
  Install all NuGet packages per project
  DO NOT write any logic yet

Step 2 — Domain
  BaseEntity
  Enums: UserRole, PaymentMethod, InvoiceStatus
  Entities: User, Product, Invoice, InvoiceItem, Payment
  All repository interfaces + IUnitOfWork

Step 3 — Application
  All DTOs
  All custom Exceptions
  All service interfaces (IAuthService, IProductService, IInvoiceService,
    ISessionService, IReceiptService, IDialogService, INavigationService)
  All service implementations

Step 4 — Infrastructure
  AppDbContext (all entity configs + seed)
  All Repository implementations
  UnitOfWork
  FileLogger
  ReceiptPrinter

Step 5 — Presentation: Foundation
  BaseViewModel + RelayCommand + AsyncRelayCommand
  Constants: AppConstants, NavigationKeys
  All XAML resource dictionaries (Colors, Typography, Buttons, Inputs, DataGrid)
  Merge all into App.xaml
  App.xaml.cs DI composition root (full wiring)
  DialogService + NavigationService

Step 6 — Presentation: Auth
  LoginViewModel
  LoginWindow.xaml (full XAML + binding)
  Wire LoginWindow in App.xaml.cs startup

Step 7 — Presentation: Shell
  ShellViewModel
  MainWindow.xaml (DockPanel: topbar + nav + ContentControl)
  Register DataTemplates in App.xaml
  Wire navigation between LoginWindow and MainWindow

Step 8 — Presentation: Products
  ProductsViewModel
  ProductFormViewModel
  ProductsView.xaml (UserControl)
  ProductFormDialog.xaml (Window)

Step 9 — Presentation: POS
  CartItemViewModel
  PosViewModel (full implementation — most complex)
  PosView.xaml (full layout: barcode input + cart grid + order summary panel)

Step 10 — Presentation: Invoices
  InvoicesViewModel
  InvoiceDetailViewModel
  InvoicesView.xaml
  InvoiceDetailView.xaml

Step 11 — Migrations + Database
  Create initial migration
  Update database
  Verify seed data (manager / Manager@123)
  Export database_script.sql

Step 12 — Tests
  All service tests (AuthService, ProductService, InvoiceService)
  PosViewModelTests

Step 13 — Deliverables
  README.md
  Screenshots of every view
```

---

## Default Credentials

```
Username: manager
Password: Manager@123
Role: Manager

Create cashier accounts from the app (Manager role only)
or seed a second user in AppDbContext.HasData
```

---

## README Outline

```markdown
# POS Desktop System

## Tech Stack
WPF .NET 8 · C# 12 · EF Core 8 · SQL Server · MVVM

## Architecture
Clean Architecture: Domain → Application → Infrastructure → Presentation
MVVM strictly enforced — zero code-behind logic

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB included with Visual Studio)
  or: SQL Server Express

## Setup
1. Open POSDesktopSystem.sln in Visual Studio 2022
2. Set POSDesktopSystem.Presentation as startup project
3. Run — migrations apply automatically on first launch
4. Login: manager / Manager@123

## Database Script
See database_script.sql — idempotent, safe to re-run

## Screenshots
[screenshots folder]
```

---

## Final Constraint Checklist

Before the agent considers any step complete, it must verify:

```
[ ] No business logic in any .xaml.cs file
[ ] No EF Core namespace imported in Application or Domain projects
[ ] No Infrastructure namespace imported in ViewModels
[ ] Every ViewModel inherits BaseViewModel
[ ] Every async operation uses AsyncRelayCommand
[ ] IsBusy is set true/false around every async command
[ ] Every database operation wrapped in try/catch in the ViewModel
    with appropriate ErrorMessage set — never unhandled exceptions shown to user
[ ] No MessageBox calls in ViewModels — IDialogService used everywhere
[ ] All colors reference resource dictionary keys — no hardcoded hex in XAML
[ ] Folder structure matches the specification exactly
[ ] Product delete is soft (IsActive = false) — never hard delete
[ ] Invoice cancel restores stock quantities
[ ] InvoiceItem snapshots product name and price at time of sale
[ ] Receipt printer failure does NOT cancel the saved invoice
[ ] All decimal fields use HasPrecision(18,2) in EF configuration
[ ] Logger writes to AppData, not project directory
[ ] SQL script generated and present in repository root
```
