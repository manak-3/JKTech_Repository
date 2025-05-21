# 📄 Document Management and Ingestion System

This project is a full-featured **Document Management and Ingestion System** built with **.NET Core Web API**, **PostgreSQL**, and integrated with a **Spring Boot-based ingestion service**. It supports document CRUD operations, ingestion status tracking, and API-driven ingestion processing.

---

## 🚀 Features

- ✅ User Authentication (JWT-based)
- 📁 Document CRUD (Upload, List, Delete)
- ⚙️ Ingestion Trigger API
- 📊 Track Ingestion Status (In Progress / Failed / Completed)
- ⌛ Paginated & Sorted Ingestion History
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

6. To Run Application on Docker Container:-
-- run command --
docker-compose up --build

7. CI/CD Pipeline with GitHub Actions
This project includes a fully automated CI/CD pipeline configured using GitHub Actions to streamline the process of building, testing, and deploying the application. The pipeline triggers automatically on every push or pull request to the master branch, ensuring that every code change is validated and deployed consistently.

What We Did:

Automated Build: The pipeline restores all NuGet dependencies and builds the .NET Core Web API and test projects to verify that the code compiles successfully.

Automated Testing: It runs all unit tests using xUnit to ensure that code changes do not introduce regressions or bugs.

Docker Image Creation: On passing all tests, the pipeline builds a Docker image of the application based on the provided Dockerfile.

Docker Hub Integration: The Docker image is pushed to Docker Hub securely using credentials stored in GitHub Secrets, making it ready for deployment.

Seamless Deployment Ready: The image can then be pulled and deployed on any container platform, such as Kubernetes or cloud-based Docker hosts.

What Happens During Pipeline Execution:

The pipeline listens for changes on the master branch.

It checks out the source code, sets up the .NET environment, and restores dependencies.

The code is built, and all tests are executed automatically.

Upon success, a Docker image is built and pushed to your Docker Hub repository.

This ensures that your latest application version is always available as a container image for deployment.

How to Test It:

Push any changes to the master branch or create a pull request targeting master to trigger the pipeline.

Monitor the workflow execution in the GitHub Actions tab of your repository.

Upon successful completion, pull the Docker image from Docker Hub using:

->docker pull <your-dockerhub-username>/jktech-api:latest
Run the container locally or deploy it on your preferred hosting platform.


 

