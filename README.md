# Kafh Organic Farm REST API (C# & PostgreSQL)

A high-performance RESTful Web API for the **Kafh** organic farming community and marketplace platform, built with **ASP.NET Core (C#)**, **Entity Framework Core**, and **PostgreSQL (Npgsql)**.

---

## 🌟 Technology Stack & Dependencies

- **Language & Runtime:** C# (.NET 10.0 Web API)
- **Database Engine:** PostgreSQL
- **ORM:** Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3)
- **Migrations & Tools:** `Microsoft.EntityFrameworkCore.Design` (10.0.13), `dotnet-ef` (10.0.13)
- **Authentication:** JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.13)
- **Password Security:** Salted Hashing with `BCrypt.Net-Next` (4.2.0)
- **Interactive Documentation:** Swagger / OpenAPI (`Swashbuckle.AspNetCore` 10.3.1) with JWT Bearer authorization
- **Cross-Origin Resource Sharing (CORS):** Configured for Angular (`http://localhost:4200`)

---

## 📂 Project Structure

```
farma-API/
├── Controllers/
│   ├── AuthController.cs          # Register (Farmer/Customer), Login, Profile
│   ├── ProductsController.cs      # Product catalog, create (farmer), update, delete
│   ├── OrdersController.cs        # Order placement, user order tracking
│   └── FarmsController.cs         # Farm profiles, acreage, and soil types
├── Data/
│   └── AppDbContext.cs            # PostgreSQL EF Core DbContext & relationship mapping
├── DTOs/
│   ├── Auth/                      # Signup & Login DTOs matching Angular forms
│   │   ├── RegisterRequestDto.cs
│   │   ├── LoginRequestDto.cs
│   │   ├── AuthResponseDto.cs
│   │   └── UserDto.cs
│   ├── Products/                  # Product creation & response DTOs
│   │   └── ProductDtos.cs
│   ├── Orders/                    # Checkout & Order DTOs
│   │   └── OrderDtos.cs
│   └── Farms/                     # Farm management DTOs
│       └── FarmDtos.cs
├── Migrations/
│   ├── 20261023191647_InitialCreate.cs
│   ├── AppDbContextModelSnapshot.cs
│   └── PostgreSql_Schema.sql      # Idempotent raw SQL schema script
├── Models/
│   ├── User.cs                    # User entity (role: farmer / customer)
│   ├── Product.cs                 # Agricultural crops & organic produce
│   ├── Order.cs                   # Purchase orders
│   ├── OrderItem.cs               # Items in an order
│   └── Farm.cs                    # Farm plots & organic certification
├── Properties/
│   └── launchSettings.json        # Port configuration (http://localhost:5275)
├── Services/
│   ├── ITokenService.cs           # Token generation interface
│   └── TokenService.cs            # JWT claims & token generator
├── appsettings.json               # Main config (PostgreSQL, JWT, CORS)
├── appsettings.Development.json   # Development config
├── FarmaApi.csproj                # Project file & NuGet dependencies
├── FarmaApi.http                  # HTTP test requests for IDE REST clients
├── Program.cs                     # Middleware pipeline & service registration
└── README.md                      # Documentation
```

---

## ⚙️ Configuration (`appsettings.json`)

Configure your PostgreSQL credentials and JWT secret in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=farma_db;Username=postgres;Password=postgres;Include Error Detail=true"
  },
  "Jwt": {
    "Key": "Kafh_Organic_Farm_Secure_Key_2026_Development_Secret_Key!",
    "Issuer": "FarmaApi",
    "Audience": "FarmaApiUsers"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200",
      "http://127.0.0.1:4200"
    ]
  }
}
```

---

## 🗄️ Database Setup (PostgreSQL)

You can apply the database schema in **either** of two ways:

### Option A: Using EF Core Migrations (Recommended)
Make sure your PostgreSQL instance is running, then run:
```bash
dotnet dotnet-ef database update
```

### Option B: Using SQL Script
Execute the generated script in `pgAdmin` or `psql`:
```bash
psql -U postgres -d farma_db -f Migrations/PostgreSql_Schema.sql
```

---

## 🚀 Running the API

To start the API in development mode:
```bash
dotnet run
```

- **Base URL:** `http://localhost:5275`
- **Interactive Swagger UI:** `http://localhost:5275/swagger`
- **Health / Root Info:** `http://localhost:5275/`

---

## 📡 REST API Endpoints Overview

### 1. Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/auth/signup` | Register a new user (`farmer` or `customer`) | No |
| `POST` | `/api/auth/login` | Authenticate with email & password, returns JWT | No |
| `GET` | `/api/auth/me` | Fetch profile of current logged-in user | Yes (`Bearer <token>`) |

#### Register Payload Example (`POST /api/auth/signup`)
```json
{
  "name": "Ramesh Patil",
  "email": "ramesh.farmer@example.com",
  "password": "Password123!",
  "mobileNumber": "9876543210",
  "age": 35,
  "role": "farmer",
  "city": "Pune",
  "state": "Maharashtra"
}
```

#### Login Payload Example (`POST /api/auth/login`)
```json
{
  "email": "ramesh.farmer@example.com",
  "password": "Password123!",
  "rememberMe": true
}
```

---

### 2. Products / Crops (`/api/products`)

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/api/products` | List produce with filtering (`?category=Fruits&isOrganic=true&search=mango`) | No |
| `GET` | `/api/products/{id}` | Get product details | No |
| `POST` | `/api/products` | Create produce listing | Yes (Farmer/Admin) |
| `PUT` | `/api/products/{id}` | Update produce listing | Yes (Owner Farmer/Admin) |
| `DELETE` | `/api/products/{id}` | Remove produce listing | Yes (Owner Farmer/Admin) |

#### Create Product Payload Example (`POST /api/products`)
```json
{
  "name": "Organic Alphonso Mangoes",
  "description": "Chemical-free, naturally ripened Alphonso mangoes from Ratnagiri.",
  "category": "Fruits",
  "pricePerUnit": 450.00,
  "unit": "kg",
  "quantityAvailable": 120.0,
  "imageUrl": "/images/mangoes.jpg",
  "isOrganic": true
}
```

---

### 3. Orders (`/api/orders`)

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/orders` | Place order for farm products (deducts stock) | Yes (Customer) |
| `GET` | `/api/orders` | Get orders for authenticated user (customers see their purchases; farmers see orders for their produce) | Yes |
| `GET` | `/api/orders/{id}` | Get detailed order summary | Yes |

#### Create Order Payload Example (`POST /api/orders`)
```json
{
  "deliveryAddress": "Flat 402, Shivajinagar, Pune, Maharashtra 411005",
  "contactPhone": "9123456780",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 5.0
    }
  ]
}
```

---

### 4. Farms (`/api/farms`)

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/api/farms` | List registered organic farms | No |
| `POST` | `/api/farms` | Register farm plot details (acres, soil type, organic cert) | Yes (Farmer/Admin) |

---

## 🔗 Angular Frontend (`farmaUI`) Integration

The Angular frontend (`farmaUI`) is configured in the CORS policy.

To connect your Angular service:
```typescript
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = 'http://localhost:5275/api/auth';

  constructor(private http: HttpClient) {}

  signup(data: any) {
    return this.http.post(`${this.apiUrl}/signup`, data);
  }

  login(credentials: any) {
    return this.http.post(`${this.apiUrl}/login`, credentials);
  }

  getCurrentUser() {
    return this.http.get(`${this.apiUrl}/me`);
  }
}
```
