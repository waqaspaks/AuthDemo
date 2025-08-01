# AuthDemo Solution

## Overview
AuthDemo is a comprehensive .NET 8 solution demonstrating modern authentication and authorization using OpenIddict and ASP.NET Core Identity. The solution includes secure communication between Blazor/Razor web applications and protected API services.

## Project Structure

### Client Applications

#### AuthDemo.Transport (.NET 8)
- Blazor web application for transport management
- Features:
  - JWT token authentication
  - Dynamic API communication
  - Role-based UI components
  - Secure logout functionality
- Key Components:
  - `CustomAuthenticationStateProvider`: Manages user authentication state
  - `LoginService`: Handles user authentication
  - `ApiService`: Centralizes API communication
  - `FlightService`, `BusService`: Domain-specific services

#### AuthDemo.Sports (.NET 8)
- Blazor web application for sports management
- Features:
  - User registration and authentication
  - Protected sports data access
  - Real-time updates
- Key Services:
  - `RegisterService`: Handles user registration
  - `ApiClientFactory`: Creates configured HTTP clients
  - Integration with sports API endpoints

### API Services

#### AuthDemo.Identity (.NET 8)
- Central authentication server
- Features:
  - OpenIddict OAuth2/OpenID Connect implementation
  - JWT token generation and validation
  - User and role management
- Configuration:
  - Seeds default users and roles for testing (see below).

#### AuthDemo.TransportApi (.NET 8)
- Transport domain API
- Protected Endpoints:
  - `GET /api/flight`: Flight schedules
  - `GET /api/bus`: Bus schedules
  - `GET /api/test`: API health check
- Scopes:
  - `admin.transport.api`
  - `manager.transport.api`
  - `user.transport.api`

#### AuthDemo.SportsApi (.NET 8)
- Sports domain API
- Protected Endpoints:
  - `GET /api/gamematch`: Game schedules
  - `GET /api/weatherforecast`: Weather data
- Scopes:
  - `admin.sports.api`
  - `manager.sports.api`
  - `user.sports.api`

### Shared Components

#### AuthDemo.Shared (.NET 8)
- Common library
- Contains:
  - Data models
  - DTOs
  - Shared interfaces

---

## Default Users, Roles, and Scopes

The Identity server seeds the following default users and roles for testing:

| Username           | Password     | Role    | Linked Scopes (Transport)      | Linked Scopes (Sports)         | Example API Access                |
|--------------------|-------------|---------|-------------------------------|-------------------------------|-----------------------------------|
| admin@test.com     | Admin123$   | Admin   | admin.transport.api           | admin.sports.api              | All endpoints (admin only)        |
| manager@test.com   | Manager123$ | Manager | manager.transport.api         | manager.sports.api            | Manager endpoints                 |
| user@test.com      | User123$    | User    | user.transport.api            | user.sports.api               | User endpoints                    |

- **Admin** users receive both `admin.transport.api` and `admin.sports.api` scopes.
- **Manager** users receive both `manager.transport.api` and `manager.sports.api` scopes.
- **User** users receive both `user.transport.api` and `user.sports.api` scopes.

> **Note:** Use these credentials to log in from the Blazor or Razor client and test access to the protected APIs. The assigned scopes determine which endpoints each user can access.

---

## Authentication & Authorization

### Available Scopes

#### Transport API Scopes

- `admin.transport.api`: Grants full administrative access to all transport API endpoints.
- `manager.transport.api`: Grants management-level access to manager endpoints.
- `user.transport.api`: Grants basic user access to user endpoints.

#### Sports API Scopes

- `admin.sports.api`: Grants full administrative access to all sports API endpoints.
- `manager.sports.api`: Grants management-level access to manager endpoints.
- `user.sports.api`: Grants basic user access to user endpoints.

### Authentication Flow
1. User initiates login from Blazor/Razor client
2. Identity server authenticates and issues JWT with appropriate scopes (based on user role)
3. Client stores token securely
4. APIs validate token, scopes, and audience for each request

### Token Validation
- Each API validates:
  - Token signature
  - Issuer (https://localhost:7214/)
  - Audience (TransportApi or SportsApi)
  - Required scopes
  - Token expiration

---

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Development Setup

1. Database Setup: Apply migrations for the Identity project.
2. Configure Connection Strings: Update `appsettings.json` in each project.
3. Start Services (in order): Identity server, then API projects, then Blazor/Razor clients.

---

## API Documentation

### Identity API Endpoints
- `POST /connect/token`: Token generation
  - Required fields: username, password, scope
- `GET /connect/authorize`: Authorization endpoint
- `POST /connect/logout`: Logout endpoint
- `POST /api/Account/register`: User registration

### Transport API Endpoints
- `GET /api/flight`: Flight schedules (user.transport.api)
- `GET /api/bus`: Bus schedules (manager.transport.api)
- `GET /api/test`: Health check (admin.transport.api)

### Sports API Endpoints
- `GET /api/gamematch`: Game schedules (user.sports.api)
- `GET /api/weatherforecast`: Weather data (admin.sports.api)

---

## Security Considerations
- Use HTTPS in production
- Secure connection strings
- Implement proper logging
- Regular security audits
- Keep dependencies updated

---
**Note:** This is a demonstration project. Review and enhance security before using in production.