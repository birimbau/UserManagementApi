# User Management API

A RESTful API built with ASP.NET Core (.NET 10) for managing users with comprehensive validation, error handling, and authentication.

## 🚀 Features

- **CRUD Operations** - Create, Read, Update, Delete users
- **Pagination** - Efficient data retrieval with page-based pagination
- **Input Validation** - Comprehensive validation for name, age, and email
- **API Key Authentication** - Secure endpoints with API key middleware
- **Global Exception Handling** - Standardized error responses (RFC 7807 Problem Details)
- **HTTP Request Logging** - Detailed logging for debugging and monitoring
- **In-Memory Storage** - Simple data persistence for demonstration

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/)

## 🛠️ Getting Started

### Clone the repository

```bash
git clone https://github.com/birimbau/UserManagementApi.git
cd UserManagementApi
```

### Run the application

```bash
dotnet run
```

Or use watch mode for development:

```bash
dotnet watch run
```

The API will be available at:
- HTTP: `http://localhost:5104`
- HTTPS: `https://localhost:7104`

## 📖 API Endpoints

### Users

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/users` | Get all users (paginated) | No |
| GET | `/users/{id}` | Get user by ID | No |
| POST | `/users` | Create a new user | No |
| PUT | `/users/{id}` | Update a user | No |
| DELETE | `/users/{id}` | Delete a user | No |
| GET | `/users/protected` | Protected endpoint | Yes |

### Query Parameters

**GET /users**
- `page` (default: 1) - Page number
- `pageSize` (default: 10, max: 100) - Items per page

## 🔐 Authentication

The API uses API Key authentication for protected endpoints.

### Header

```
X-API-Key: your-secret-api-key-12345
```

### Example Request

```bash
curl -X GET http://localhost:5104/users/protected \
  -H "X-API-Key: your-secret-api-key-12345"
```

## 📝 Request/Response Examples

### Create User

**Request:**
```http
POST /users
Content-Type: application/json

{
  "name": "John Doe",
  "age": 30,
  "email": "john.doe@example.com"
}
```

**Response (201 Created):**
```json
{
  "id": 4,
  "name": "John Doe",
  "age": 30,
  "email": "john.doe@example.com"
}
```

### Get All Users

**Request:**
```http
GET /users?page=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "users": [
    { "id": 1, "name": "John Doe", "age": 30, "email": "john.doe@example.com" },
    { "id": 2, "name": "Jane Smith", "age": 25, "email": "jane.smith@example.com" }
  ],
  "page": 1,
  "pageSize": 10,
  "totalUsers": 2,
  "totalPages": 1
}
```

### Error Response

**Response (400 Bad Request):**
```json
{
  "errors": [
    "Name must be between 2 and 100 characters.",
    "Please provide a valid email address."
  ]
}
```

## ✅ Validation Rules

| Field | Rules |
|-------|-------|
| Name | Required, 2-100 characters |
| Age | Required, 1-150 |
| Email | Required, valid email format, unique |

## 🧪 Testing

Use the included `UserManagementAPI.http` file with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) VS Code extension to test all endpoints.

## 📁 Project Structure

```
UserManagementAPI/
├── Program.cs              # Main application file with all endpoints
├── UserManagementAPI.csproj # Project configuration
├── UserManagementAPI.http   # HTTP test requests
├── appsettings.json         # Application settings
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json  # Launch configuration
└── README.md
```

## 🔧 Configuration

### Environment Variables

In production, configure the API key using environment variables or a secrets manager:

```json
{
  "Authentication": {
    "ApiKey": "your-production-api-key"
  }
}
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 👤 Author

**Tiago Neto**

- GitHub: [@birimbau](https://github.com/birimbau)
