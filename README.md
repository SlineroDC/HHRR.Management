# Employee Management System – TalentoPlus S.A.S.

## Overview & Modules

This project is a comprehensive human resources management solution, specifically designed for **TalentoPlus S.A.S.** The system is composed of two main modules working together to offer a robust and scalable experience:

- **Web Admin (HHRR.Web):** A modern administrative web interface that allows HR managers to manage employees, visualize metrics, and handle daily operations.
- **API Core (HHRR.API):** The system core, a RESTful API built with .NET that serves as a centralized backend, handling business logic, security, and integration with external services.

## Key Features

The system implements advanced functionalities to optimize talent management:

- **Excel Import:** Bulk loading of employee data and updates to streamline information migration and updates.
- **AI Dashboard (Gemini 2.5 Flash):** Integration with Google Gemini for intelligent data analysis and generation of insights about personnel.
- **PDF Generation:** Automated creation of reports and documents in PDF format.
- **JWT Authentication:** Robust security using JSON Web Tokens to protect API endpoints and user sessions.
- **SMTP Email Sending:** Automatic email notifications for alerts and important communications.

## Tech Stack

The solution is built using cutting-edge technologies to ensure performance and maintainability:

| Component                   | Technology              | Description                                                       |
| :-------------------------- | :---------------------- | :---------------------------------------------------------------- |
| **Backend Framework**       | .NET 10                 | The latest version of Microsoft's framework for high performance. |
| **Database**                | PostgreSQL              | Powerful and open-source relational database engine.              |
| **ORM**                     | EF Core                 | Entity Framework Core for data handling and migrations.           |
| **Security**                | JWT                     | Standard for secure information transmission between parties.     |
| **Artificial Intelligence** | Google Gemini 2.5 Flash | AI engine for data analysis and processing.                       |
| **Testing**                 | xUnit                   | Unit and integration testing framework.                           |

## How to Run

### Prerequisites

Ensure you have the following installed:

- **.NET 10 SDK**
- **PostgreSQL** (Local server or Docker container)
- **Docker & Docker Compose** (for containerized deployment)

### Option 1: Local Development

#### 1. Configure Environment Variables

Create a `.env` file in the solution root with the following variables:

```env
DB_CONNECTION_STRING=Host=localhost;Database=hhrr_management;Username=postgres;Password=YourPassword
GEMINI_API_KEY=your_gemini_api_key_here
JWT_SECRET=your_jwt_secret_key_here
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your_email@gmail.com
SMTP_PASS=your_app_password
```

#### 2. Start the Services

Run the following commands in separate terminals:

**Start Web Admin:**

```bash
dotnet run --project HHRR.Web
```

**Start API Core:**

```bash
dotnet run --project HHRR.API
```

### Option 2: Docker Deployment

#### 1. Configure Environment Variables

Create a `.env` file in the solution root:

```env
GEMINI_API_KEY=your_gemini_api_key_here
```

> [!IMPORTANT]
> The `GEMINI_API_KEY` environment variable is **required** for AI features to work in Docker.

#### 2. Build and Start Containers

```bash
docker compose up --build
```

This will start three services:

- **PostgreSQL Database** on port `5433`
- **Web Admin** on port `5080` (http://localhost:5080)
- **API Core** on port `5081` (http://localhost:5081)

#### 3. Stop Containers

```bash
docker compose down
```

To remove volumes (database data):

```bash
docker compose down -v
```

## Default Credentials

> [!IMPORTANT]
> These credentials are for the local development environment. Ensure to change them in production.

- **Admin Email:** `admin@hhrr.io`
- **Password:** `Password123!`

## Environment Variables

The system requires the following variables to function correctly:

| Variable               | Description                           | Required    | Default                   |
| ---------------------- | ------------------------------------- | ----------- | ------------------------- |
| `DB_CONNECTION_STRING` | PostgreSQL connection string          | Yes (Local) | Auto-configured in Docker |
| `GEMINI_API_KEY`       | Google Gemini API Key for AI features | **Yes**     | None                      |
| `JWT_SECRET`           | Secret key for JWT token signing      | Yes (API)   | None                      |
| `SMTP_HOST`            | SMTP server hostname                  | Yes (Email) | None                      |
| `SMTP_PORT`            | SMTP server port                      | Yes (Email) | 587                       |
| `SMTP_USER`            | SMTP authentication username          | Yes (Email) | None                      |
| `SMTP_PASS`            | SMTP authentication password          | Yes (Email) | None                      |

### Configuration Hierarchy

The application reads configuration in this order:

1. Environment variables (`.env` file or Docker environment)
2. `appsettings.json` (fallback for local development)
3. `appsettings.Development.json` (development overrides)

## Testing

To run the automated test suite and ensure system integrity:

```bash
dotnet test HHRR.Tests
```

## Troubleshooting

### Gemini AI Not Working in Docker

If the Gemini AI features are not working in Docker but work locally:

1. **Verify the `.env` file exists** in the project root with `GEMINI_API_KEY`
2. **Rebuild containers** to pick up environment changes:
   ```bash
   docker compose down
   docker compose up --build
   ```
3. **Check container logs** for API key errors:
   ```bash
   docker compose logs web
   ```
4. **Verify the API key** is valid at [Google AI Studio](https://aistudio.google.com/app/apikey)

### Database Connection Issues

- **Local:** Ensure PostgreSQL is running on port `5432`
- **Docker:** Database runs on external port `5433` to avoid conflicts

### Port Conflicts

If ports are already in use, modify `docker-compose.yml`:

```yaml
ports:
  - "YOUR_PORT:8080" # Change YOUR_PORT to an available port
```
