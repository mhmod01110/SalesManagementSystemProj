-- Create Database
CREATE DATABASE SalesManagementDB;


USE SalesManagementDB;

-- 1.1 Users and Authentication
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    RoleID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    LastLogin DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedDate DATETIME,
    ModifiedBy INT
);

CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(200)
);

CREATE TABLE Permissions (
    PermissionID INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) NOT NULL,
    Module NVARCHAR(50) NOT NULL,
    CanView BIT DEFAULT 0,
    CanCreate BIT DEFAULT 0,
    CanEdit BIT DEFAULT 0,
    CanDelete BIT DEFAULT 0
);

CREATE TABLE RolePermissions (
    RolePermissionID INT PRIMARY KEY IDENTITY(1,1),
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    FOREIGN KEY (PermissionID) REFERENCES Permissions(PermissionID)
);

-- 1.2 Warehouses
CREATE TABLE Warehouses (
    WarehouseID INT PRIMARY KEY IDENTITY(1,1),
    WarehouseName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200),
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 1.3 Products and Inventory
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    ParentCategoryID INT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE Units (
    UnitID INT PRIMARY KEY IDENTITY(1,1),
    UnitName NVARCHAR(50) NOT NULL,
    ShortName NVARCHAR(10)
);

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductCode NVARCHAR(50) UNIQUE NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    CategoryID INT,
    Barcode NVARCHAR(50),
    BaseUnitID INT NOT NULL,
    Description NVARCHAR(1000),
    CostPrice DECIMAL(18,2) NOT NULL,
    RetailPrice DECIMAL(18,2) NOT NULL,
    WholesalePrice DECIMAL(18,2) NOT NULL,
    MinStockLevel INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    ImagePath NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (BaseUnitID) REFERENCES Units(UnitID)
);

CREATE TABLE ProductUnits (
    ProductUnitID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    UnitID INT NOT NULL,
    ConversionFactor DECIMAL(18,4) NOT NULL, -- How many base units in this unit
    Price DECIMAL(18,2) NOT NULL,
    Barcode NVARCHAR(50),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (UnitID) REFERENCES Units(UnitID)
);

CREATE TABLE Inventory (
    InventoryID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    WarehouseID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (WarehouseID) REFERENCES Warehouses(WarehouseID),
    UNIQUE(ProductID, WarehouseID)
);

-- 1.4 Customers and Suppliers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerCode NVARCHAR(50) UNIQUE NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerType NVARCHAR(20) DEFAULT 'Retail', -- Retail/Wholesale
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    Country NVARCHAR(100),
    TaxNumber NVARCHAR(50),
    CreditLimit DECIMAL(18,2) DEFAULT 0,
    Balance DECIMAL(18,2) DEFAULT 0,
    DiscountPercentage DECIMAL(5,2) DEFAULT 0,
    SalesRepID INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierCode NVARCHAR(50) UNIQUE NOT NULL,
    SupplierName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    ContactPerson NVARCHAR(100),
    Balance DECIMAL(18,2) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 1.5 Sales Representatives
CREATE TABLE SalesRepresentatives (
    SalesRepID INT PRIMARY KEY IDENTITY(1,1),
    RepCode NVARCHAR(50) UNIQUE NOT NULL,
    RepName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    CommissionPercentage DECIMAL(5,2) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 1.6 Currencies
CREATE TABLE Currencies (
    CurrencyID INT PRIMARY KEY IDENTITY(1,1),
    CurrencyCode NVARCHAR(10) UNIQUE NOT NULL,
    CurrencyName NVARCHAR(50) NOT NULL,
    Symbol NVARCHAR(10),
    ExchangeRate DECIMAL(18,6) NOT NULL DEFAULT 1,
    IsBaseCurrency BIT DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE()
);

-- 1.7 Sales
CREATE TABLE SalesInvoices (
    InvoiceID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) UNIQUE NOT NULL,
    InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
    CustomerID INT NOT NULL,
    WarehouseID INT NOT NULL,
    SalesRepID INT,
    CurrencyID INT NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    RemainingAmount DECIMAL(18,2) DEFAULT 0,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending', -- Pending/Partial/Paid
    PaymentMethod NVARCHAR(50),
    Notes NVARCHAR(1000),
    IsInstallment BIT DEFAULT 0,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (WarehouseID) REFERENCES Warehouses(WarehouseID),
    FOREIGN KEY (SalesRepID) REFERENCES SalesRepresentatives(SalesRepID),
    FOREIGN KEY (CurrencyID) REFERENCES Currencies(CurrencyID)
);

CREATE TABLE SalesInvoiceDetails (
    InvoiceDetailID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceID INT NOT NULL,
    ProductID INT NOT NULL,
    UnitID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) DEFAULT 0,
    TaxPercentage DECIMAL(5,2) DEFAULT 0,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Profit DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceID) REFERENCES SalesInvoices(InvoiceID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (UnitID) REFERENCES Units(UnitID)
);

-- 1.8 Installments
CREATE TABLE InstallmentPlans (
    InstallmentPlanID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceID INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    RemainingAmount DECIMAL(18,2) NOT NULL,
    NumberOfInstallments INT NOT NULL,
    InstallmentAmount DECIMAL(18,2) NOT NULL,
    StartDate DATE NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Active', -- Active/Completed/Defaulted
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceID) REFERENCES SalesInvoices(InvoiceID)
);

CREATE TABLE Installments (
    InstallmentID INT PRIMARY KEY IDENTITY(1,1),
    InstallmentPlanID INT NOT NULL,
    InstallmentNumber INT NOT NULL,
    DueDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    PaidDate DATE,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending/Paid/Overdue
    Notes NVARCHAR(500),
    FOREIGN KEY (InstallmentPlanID) REFERENCES InstallmentPlans(InstallmentPlanID)
);

-- 1.9 Purchases
CREATE TABLE PurchaseOrders (
    PurchaseOrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(50) UNIQUE NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    SupplierID INT NOT NULL,
    WarehouseID INT NOT NULL,
    CurrencyID INT NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending',
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending/Received/Cancelled
    Notes NVARCHAR(1000),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    FOREIGN KEY (WarehouseID) REFERENCES Warehouses(WarehouseID),
    FOREIGN KEY (CurrencyID) REFERENCES Currencies(CurrencyID)
);

CREATE TABLE PurchaseOrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    PurchaseOrderID INT NOT NULL,
    ProductID INT NOT NULL,
    UnitID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL,
    TotalCost DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (PurchaseOrderID) REFERENCES PurchaseOrders(PurchaseOrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (UnitID) REFERENCES Units(UnitID)
);

-- 1.10 Stock Transfers
CREATE TABLE StockTransfers (
    TransferID INT PRIMARY KEY IDENTITY(1,1),
    TransferNumber NVARCHAR(50) UNIQUE NOT NULL,
    TransferDate DATETIME NOT NULL DEFAULT GETDATE(),
    FromWarehouseID INT NOT NULL,
    ToWarehouseID INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending/Completed/Cancelled
    Notes NVARCHAR(1000),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (FromWarehouseID) REFERENCES Warehouses(WarehouseID),
    FOREIGN KEY (ToWarehouseID) REFERENCES Warehouses(WarehouseID)
);

CREATE TABLE StockTransferDetails (
    TransferDetailID INT PRIMARY KEY IDENTITY(1,1),
    TransferID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (TransferID) REFERENCES StockTransfers(TransferID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- 1.11 Expenses
CREATE TABLE ExpenseCategories (
    ExpenseCategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE Expenses (
    ExpenseID INT PRIMARY KEY IDENTITY(1,1),
    ExpenseNumber NVARCHAR(50) UNIQUE NOT NULL,
    ExpenseDate DATETIME NOT NULL DEFAULT GETDATE(),
    ExpenseCategoryID INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CurrencyID INT NOT NULL,
    PaymentMethod NVARCHAR(50),
    Description NVARCHAR(1000),
    ReceiptNumber NVARCHAR(50),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ExpenseCategoryID) REFERENCES ExpenseCategories(ExpenseCategoryID),
    FOREIGN KEY (CurrencyID) REFERENCES Currencies(CurrencyID)
);

-- 1.12 Accounting
CREATE TABLE ChartOfAccounts (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    AccountCode NVARCHAR(50) UNIQUE NOT NULL,
    AccountName NVARCHAR(200) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL, -- Asset/Liability/Equity/Revenue/Expense
    ParentAccountID INT NULL,
    IsActive BIT DEFAULT 1,
    Balance DECIMAL(18,2) DEFAULT 0,
    FOREIGN KEY (ParentAccountID) REFERENCES ChartOfAccounts(AccountID)
);

CREATE TABLE JournalEntries (
    JournalEntryID INT PRIMARY KEY IDENTITY(1,1),
    EntryNumber NVARCHAR(50) UNIQUE NOT NULL,
    EntryDate DATETIME NOT NULL DEFAULT GETDATE(),
    Description NVARCHAR(500),
    ReferenceType NVARCHAR(50), -- Sales/Purchase/Expense/Transfer
    ReferenceID INT,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE JournalEntryDetails (
    EntryDetailID INT PRIMARY KEY IDENTITY(1,1),
    JournalEntryID INT NOT NULL,
    AccountID INT NOT NULL,
    DebitAmount DECIMAL(18,2) DEFAULT 0,
    CreditAmount DECIMAL(18,2) DEFAULT 0,
    Description NVARCHAR(500),
    FOREIGN KEY (JournalEntryID) REFERENCES JournalEntries(JournalEntryID) ON DELETE CASCADE,
    FOREIGN KEY (AccountID) REFERENCES ChartOfAccounts(AccountID)
);

-- 1.13 Notifications
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    NotificationType NVARCHAR(50), -- LowStock/DueInstallment/PendingInvoice
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    IsRead BIT DEFAULT 0,
    Priority NVARCHAR(20) DEFAULT 'Normal', -- Low/Normal/High
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- 1.14 WhatsApp Messages
CREATE TABLE WhatsAppMessages (
    MessageID INT PRIMARY KEY IDENTITY(1,1),
    RecipientPhone NVARCHAR(20) NOT NULL,
    MessageType NVARCHAR(50), -- Invoice/Payment/Reminder/Promotional
    MessageContent NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending/Sent/Failed
    SentDate DATETIME,
    ReferenceType NVARCHAR(50),
    ReferenceID INT,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 1.15 System Settings
CREATE TABLE SystemSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) UNIQUE NOT NULL,
    SettingValue NVARCHAR(500),
    Description NVARCHAR(500)
);

-- 1.16 Audit Trail
CREATE TABLE AuditTrail (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100),
    RecordID INT,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ActionDate DATETIME DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Insert Default Data
INSERT INTO Roles (RoleName, Description) VALUES 
('Admin', 'Full system access'),
('Manager', 'Management level access'),
('Cashier', 'Sales and POS access'),
('Accountant', 'Accounting module access'),
('Warehouse', 'Inventory management access');

INSERT INTO Currencies (CurrencyCode, CurrencyName, Symbol, ExchangeRate, IsBaseCurrency) VALUES
('USD', 'US Dollar', '$', 1.00, 1),
('EUR', 'Euro', '€', 0.85, 0),
('EGP', 'Egyptian Pound', 'E£', 30.90, 0);

INSERT INTO Units (UnitName, ShortName) VALUES
('Piece', 'PC'),
('Box', 'BOX'),
('Carton', 'CTN'),
('Kilogram', 'KG'),
('Liter', 'L');

INSERT INTO ExpenseCategories (CategoryName, Description) VALUES
('Rent', 'Office and warehouse rent'),
('Utilities', 'Electricity, water, internet'),
('Salaries', 'Employee salaries'),
('Marketing', 'Advertising and promotions'),
('Transportation', 'Delivery and logistics'),
('Maintenance', 'Equipment and facility maintenance');

INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
('CompanyName', 'My Company', 'Company name for reports'),
('CompanyAddress', '123 Main St', 'Company address'),
('CompanyPhone', '+1234567890', 'Company phone'),
('CompanyEmail', 'info@company.com', 'Company email'),
('TaxRate', '14', 'Default tax rate percentage'),
('InvoicePrefix', 'INV', 'Sales invoice prefix'),
('PurchasePrefix', 'PO', 'Purchase order prefix'),
('SessionTimeout', '30', 'Session timeout in minutes'),
('LowStockThreshold', '10', 'Low stock alert threshold'),
('BackupPath', 'C:\\Backups', 'Database backup path'),
('WhatsAppAPIKey', '', 'WhatsApp Business API Key'),
('SMTPServer', 'smtp.gmail.com', 'Email SMTP server'),
('SMTPPort', '587', 'Email SMTP port'),
('SMTPUsername', '', 'Email username'),
('SMTPPassword', '', 'Email password');
