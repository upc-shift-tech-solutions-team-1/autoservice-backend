# AutoService Platform Workshop Management

## Overview

AutoService-730-AW-Backend is a RESTful API built with ASP.NET Core for managing automotive workshop operations. The platform supports vehicle maintenance workflows, work order execution, mechanic assignment, inventory management, customer administration, authentication, and public service tracking.

The application follows a Domain-Driven Design (DDD) approach and is organized into multiple bounded contexts to ensure scalability, maintainability, and separation of concerns.

---

## Architecture

The solution is structured around the following bounded contexts:

### IAM (Identity and Access Management)

Responsible for authentication and authorization.

Features:

* User registration
* User authentication
* JWT token generation
* Secure access control

### Customer Management

Responsible for customer administration.

Features:

* Create customers
* Update customer information
* Retrieve customer records
* Customer management operations

### Fleet Management

Responsible for vehicle administration.

Features:

* Vehicle registration
* Vehicle information management
* Vehicle history tracking

### Workshop Operations

Responsible for workshop processes.

Features:

* Work order management
* Task creation and execution
* Maintenance workflow tracking
* Cost calculation
* Service completion management

### Inventory Management

Responsible for spare parts and inventory control.

Features:

* Inventory item management
* Spare parts tracking
* Stock administration

### Staff Coordination

Responsible for workshop personnel management.

Features:

* Mechanic registration
* Mechanic assignment
* Staff management

### Public Tracking

Responsible for customer service tracking.

Features:

* Public work order tracking
* Service status consultation

### Tenant Management

Responsible for tenant-related operations and future multi-tenant support.

---

# Technology Stack

* ASP.NET Core
* .NET 10
* Entity Framework Core
* MySQL
* JWT Authentication
* Swagger / OpenAPI
* Pomelo MySQL Provider
* BCrypt Password Hashing

---

# Project Structure

```text
AutoServiceAW.API
│
├── CustomerManagement
├── FleetManagement
├── IAM
├── InventoryManagement
├── PublicTracking
├── StaffCoordination
├── TenantManagement
├── WorkshopOperations
│
├── Shared
│   ├── Domain
│   ├── Infrastructure
│   └── Persistence
│
├── Migrations
├── Properties
├── Program.cs
└── appsettings.json
```

---

# Main Features

* JWT-based authentication
* Customer management
* Vehicle management
* Mechanic management
* Work order lifecycle management
* Maintenance task execution
* Inventory and spare parts administration
* Public order tracking
* RESTful API architecture
* Swagger documentation

---

# Database

The application uses MySQL as its primary database.

Entity Framework Core is used as the ORM layer.

Database migrations are located in:

```text
Migrations/
```

---

# Getting Started

## Prerequisites

Before running the project, make sure you have:

* .NET SDK 10
* MySQL Server
* Git
* Visual Studio 2022 / Rider / VS Code

---

## Clone Repository

```bash
git clone https://github.com/your-organization/AutoService-730-AW-Backend.git

cd AutoService-730-AW-Backend
```

---

## Configure Database

Update the connection string in:

```json
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=autoservice_db;user=root;password=yourpassword"
  }
}
```

---

## Restore Dependencies

```bash
dotnet restore
```

---

## Apply Migrations

```bash
dotnet ef database update
```

---

## Run the Application

```bash
dotnet run
```

By default, the API will start on:

```text
https://localhost:5001
```

or

```text
http://localhost:5000
```

depending on your local configuration.

---

# API Documentation

Swagger UI is automatically enabled.

After starting the application, access:

```text
https://localhost:<port>/swagger
```

to explore and test the available endpoints.

---

# Authentication

The API uses JWT Bearer Authentication.

Workflow:

1. Authenticate using the IAM endpoints.
2. Receive a JWT token.
3. Include the token in subsequent requests:

```http
Authorization: Bearer <your_token>
```

---

# Available REST Modules

| Module      | Description                      |
| ----------- | -------------------------------- |
| Auth        | Authentication and authorization |
| Customers   | Customer management              |
| Vehicles    | Vehicle management               |
| Mechanics   | Staff management                 |
| Inventory   | Spare parts and inventory        |
| Work Orders | Maintenance order management     |
| Tasks       | Workshop task execution          |
| Tracking    | Public service tracking          |

---

# Development Principles

This project follows:

* Domain-Driven Design (DDD)
* Repository Pattern
* Service Layer Pattern
* Dependency Injection
* Clean Separation of Concerns
* RESTful API Design

---

# Build

Generate a production build:

```bash
dotnet publish -c Release
```

Output files will be generated in:

```text
bin/Release/
```

---

# Authors

AutoService Development Team

Academic Project – Software Engineering

---

# License

This project was developed for educational and academic purposes.
