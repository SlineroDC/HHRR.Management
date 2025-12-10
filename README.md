# Employee Management System – TalentoPlus S.A.S.

## Overview & Modules

This project is a comprehensive human resources management solution, specifically designed for **TalentoPlus S.A.S.** The system is composed of two main modules working together to offer a robust and scalable experience:

- **Web Admin (HHRR.Web):** A modern administrative web interface that allows HR managers to manage employees, visualize metrics, and handle daily operations.
- **API Core (HHRR.API):** The system core, a RESTful API built with .NET that serves as a centralized backend, handling business logic, security, and integration with external services.

## Key Features

The system implements advanced functionalities to optimize talent management:

- **Excel Import:** Bulk loading of employee data and updates to streamline information migration and updates.
- **AI Dashboard (Gemini):** Integration with Google Gemini for intelligent data analysis and generation of insights about personnel.
- **PDF Generation:** Automated creation of reports and documents in PDF format.
- **JWT Authentication:** Robust security using JSON Web Tokens to protect API endpoints and user sessions.
- **SMTP Email Sending:** Automatic email notifications for alerts and important communications.

## Tech Stack

The solution is built using cutting-edge technologies to ensure performance and maintainability:

| Component                   | Technology    | Description                                                       |
| :-------------------------- | :------------ | :---------------------------------------------------------------- |
| **Backend Framework**       | .NET 10       | The latest version of Microsoft's framework for high performance. |
| **Database**                | PostgreSQL    | Powerful and open-source relational database engine.              |
| **ORM**                     | EF Core       | Entity Framework Core for data handling and migrations.           |
| **Security**                | JWT           | Standard for secure information transmission between parties.     |
| **Artificial Intelligence** | Google Gemini | AI engine for data analysis and processing.                       |
| **Testing**                 | xUnit         | Unit and integration testing framework.                           |

## How to Run (Local Development)

Follow these steps to set up the development environment locally.

### Prerequisites

Ensure you have the following installed:

- **.NET 10 SDK**
- **PostgreSQL** (Local server or Docker container)

### Crucial Steps

1.  **Clone the repository:**
    Download the source code to your local machine.

2.  **Configure Environment Variables:**
    It is **critical** to configure the `.env` file in the solution root with the correct credentials before starting.

### Start Commands

To start the services, run the following commands in separate terminals:

**Start Web Admin:**

```bash
dotnet run --project HHRR.Web
```

**Start API Core:**

```bash
dotnet run --project HHRR.API
```

## Default Credentials

> [!IMPORTANT]
> These credentials are for the local development environment. Ensure to change them in production.

- **Admin Web:** `admin@hhrr.io`
- **Password:** `Password123!`

## Environment Variables

The system requires the following variables in your `.env` file to function correctly:

- `DB_CONNECTION_STRING`: Connection string to your PostgreSQL database.
- `JWT_SECRET`: Secret key for signing JWT tokens.
- `GEMINI_API_KEY`: API Key to access Google Gemini services.
- `SMTP_HOST`: SMTP server for sending emails.
- `SMTP_PORT`: SMTP server port.
- `SMTP_USER`: User for SMTP authentication.
- `SMTP_PASS`: Password for SMTP authentication.

## Testing

To run the automated test suite and ensure system integrity:

```bash
dotnet test HHRR.Tests
```
