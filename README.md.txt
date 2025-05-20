# 📄 Document Management and Ingestion System

This project is a full-featured **Document Management and Ingestion System** built with **.NET Core Web API**, **PostgreSQL**, and integrated with a **Spring Boot-based ingestion service**. It supports document CRUD operations, ingestion status tracking, and API-driven ingestion processing.

---

## 🚀 Features

- ✅ User Authentication (JWT-based)
- 📁 Document CRUD (Upload, List, Delete)
- ⚙️ Ingestion Trigger API
- 📊 Track Ingestion Status (In Progress / Failed / Completed)
- ⌛ Paginated & Sorted Ingestion History
- 📬 Spring Boot Integration for ingestion logic
- 🧪 xUnit Unit Testing with Moq
- 🧱 Clean Architecture with:
  - **Core**
  - **Infrastructure**
  - **API**
  - **Tests**
- 🗃️ PostgreSQL database support

---

## 🏗️ Project Structure

```plaintext
DocumentManagementSystem/
│
├── API/                        # ASP.NET Core Web API
│   ├── Controllers/           # Exposes endpoints
│   ├── Services/              # Business logic
│   └── Interfaces/           # Service contracts
│
├── Core/                      # Domain layer
│   ├── Entities/             # Domain models
│   ├── Enums/                # Status types etc.
│   ├── Dtos/                 # Request/response models
│   └── Interfaces/           # Repository interfaces
│
├── Infrastructure/           # Persistence logic
│   ├── Data/                 # DbContext
│   ├── Repositories/         # EF Core repositories
│   └── UnitOfWork/           # Transaction management
│
├── Tests/                     # xUnit test projects
│   └── Services/             # Service layer tests
│
└── docker-compose.yml        # Optional Docker config

⚙️ How It Works
📄 1. Document Upload
POST /api/documents

Stores metadata (name, folder, content-type, etc.)

🚀 2. Trigger Ingestion
POST /api/ingestion/trigger/{documentId}

Validates document

Logs initial InProgress status

Calls Spring Boot ingestion API

Updates final status based on response

📊 3. Ingestion Status
GET /api/ingestion/status

Filters by:

DocumentId

Status

FromDate / ToDate

Sort: TriggeredAt / Status

Supports pagination

💻 Setup Instructions
🧱 Prerequisites
.NET 8 SDK

PostgreSQL

Visual Studio 2022+ or VS Code

Optional: Docker Desktop

 1. Clone the Repository

 git clone "Repository_Link"
 cd DocumentManagementSystem



🗃️ 2. Configure the Database
Update appsettings.json:

Edit
"ConnectionStrings": {
  "DefaultConnection": "YOUR_LOCAL_PGAdmin_CONN_STRING"
}

3 Spring Boot Ingestion API
Configure endpoint in appsettings.json:

"IngestionSettings": {
  "TriggerApi": "SPRING_BOOT_API_URL"
}
Note:-Make sure the Spring Boot app is running and listening.


4 Run the API using API Project as Startup Project

5.Navigate to the test project and run:
cd Tests
dotnet test

🔐 Authentication
JWT token is required for all secured endpoints.

Use /api/auth/register --> /api/auth/login 
Pass token as Authorization: Bearer <token> header 

