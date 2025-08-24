# AscendDev API Postman Collections

This directory contains comprehensive Postman collections for testing the AscendDev E-Learning Platform API. The collections are organized by functionality and include automated tests, environment variables, and pre-request scripts.

## 📁 Directory Structure

```
postman/
├── environments/
│   └── AscendDev-Environment.json          # Environment variables
├── collections/
│   ├── 01-Health-Check.json                # Health check endpoints
│   ├── 02-Authentication.json              # User authentication
│   ├── 03-OAuth-Authentication.json        # OAuth integration
│   ├── 04-Courses.json                     # Course management
│   ├── 05-Code-Execution.json              # Code execution
│   └── 06-Tests.json                       # Test execution
├── AscendDev-API-Collection.json           # Combined collection (legacy)
└── README.md                               # This file
```

## 🚀 Quick Start

### 1. Import Environment
1. Open Postman
2. Click **Import** → **Upload Files**
3. Select `environments/AscendDev-Environment.json`
4. Set the environment as active in the top-right dropdown

### 2. Import Collections
Import all collections from the `collections/` directory:
1. Click **Import** → **Upload Files**
2. Select all `.json` files from the `collections/` folder
3. Click **Import**

### 3. Configure Environment Variables
Update the following variables in your environment:
- `baseUrl`: Your API base URL (default: `https://localhost:7000`)
- `testUserEmail`: Email for testing (default: `test@example.com`)
- `testUserPassword`: Password for testing (default: `TestPassword123!`)

## 📋 Collections Overview

### 01 - Health Check
**Purpose**: Monitor API availability and health status
- ✅ Get Health Status

**Usage**: Run this first to verify the API is running.

### 02 - Authentication
**Purpose**: User registration, login, and token management
- ✅ Register User
- ✅ Login User  
- ✅ Refresh Token
- ✅ Revoke Token

**Features**:
- Automatic token storage in environment variables
- Auto-refresh expired tokens via pre-request script
- Comprehensive test assertions

**Usage Flow**:
1. Register a new user OR Login with existing credentials
2. Tokens are automatically stored for subsequent requests
3. Use other collections (tokens are auto-managed)

### 03 - OAuth Authentication
**Purpose**: GitHub and Google OAuth integration
- ✅ Get GitHub Authorization URL
- ✅ Get Google Authorization URL
- ✅ GitHub OAuth Callback
- ✅ Google OAuth Callback
- ✅ Link GitHub Account
- ✅ Link Google Account
- ✅ Unlink GitHub Account
- ✅ Unlink Google Account
- ✅ Get Linked Providers

**OAuth Flow**:
1. Get authorization URL from the API
2. Copy the URL from response and open in browser
3. Complete OAuth flow in browser
4. Extract `code` parameter from callback URL
5. Set `oauthCode` environment variable
6. Run callback request to complete login

### 04 - Courses
**Purpose**: Course and lesson management
- ✅ Get All Courses
- ✅ Get Course by ID
- ✅ Get Course Lessons
- ✅ Get Lesson by ID

**Features**:
- Automatic course/lesson ID extraction and storage
- Bearer token authentication
- Detailed response logging

### 05 - Code Execution
**Purpose**: Execute code in various programming languages
- ✅ Run Python Code
- ✅ Run JavaScript Code
- ✅ Run C# Code
- ✅ Run TypeScript Code
- ✅ Run Code with Error (error handling demo)

**Supported Languages**:
- `python` - Python 3.x
- `javascript` - Node.js
- `csharp` - .NET Core
- `typescript` - TypeScript with Node.js

### 06 - Tests
**Purpose**: Run lesson tests and validate user code
- ✅ Run Tests (anonymous)
- ✅ Run Tests Authenticated (with user tracking)
- ✅ Run Tests - Python Arrays Lesson
- ✅ Run Tests - Failing Code (demo)

**Features**:
- Detailed test result analysis
- Pass/fail statistics
- Individual test case reporting

## 🔧 Environment Variables

### Required Variables
| Variable | Description | Example |
|----------|-------------|---------|
| `baseUrl` | API base URL | `https://localhost:7000` |
| `testUserEmail` | Test user email | `test@example.com` |
| `testUserPassword` | Test user password | `TestPassword123!` |

### Auto-Managed Variables
| Variable | Description | Auto-Set By |
|----------|-------------|-------------|
| `accessToken` | JWT access token | Authentication requests |
| `refreshToken` | JWT refresh token | Authentication requests |
| `courseId` | Current course ID | Course requests |
| `lessonId` | Current lesson ID | Lesson requests |
| `oauthState` | OAuth state parameter | OAuth authorization |
| `oauthCode` | OAuth authorization code | Manual (from browser) |

## 🧪 Testing Features

### Automated Tests
Each request includes comprehensive test scripts:
- ✅ Status code validation
- ✅ Response structure validation
- ✅ Data type checking
- ✅ Business logic validation
- ✅ Error handling verification

### Pre-Request Scripts
- 🔄 Automatic token refresh when expired
- 📝 Environment variable management
- 🔧 Dynamic request configuration

### Console Logging
- 📊 Detailed execution results
- 🐛 Error information and debugging
- 📈 Test statistics and metrics

## 🔐 Authentication Flow

### Standard Authentication
1. **Register/Login** → Get tokens
2. **Use API** → Tokens auto-included
3. **Token Refresh** → Automatic when expired

### OAuth Authentication
1. **Get Auth URL** → Copy to browser
2. **Complete OAuth** → Extract code from callback
3. **Set oauthCode** → In environment variables
4. **Run Callback** → Complete authentication

## 📝 Request Format

All requests use **PascalCase** for JSON properties to match C# DTOs:

```json
{
  "Email": "user@example.com",
  "Password": "password123",
  "Language": "python",
  "Code": "print('Hello, World!')",
  "LessonId": "arrays",
  "RefreshToken": "your-refresh-token"
}
```

## 🚨 Common Issues & Solutions

### Issue: 401 Unauthorized
**Solution**: 
1. Run authentication request first
2. Check if token is expired (auto-refresh should handle this)
3. Verify `accessToken` environment variable is set

### Issue: OAuth Code Invalid
**Solution**:
1. Get fresh authorization URL
2. Complete OAuth flow in browser immediately
3. Extract code quickly (codes expire fast)
4. Set `oauthCode` environment variable correctly

### Issue: Course/Lesson Not Found
**Solution**:
1. Run "Get All Courses" first
2. Check available course IDs in response
3. Update `courseId` environment variable
4. Ensure lesson exists in the specified course

### Issue: Code Execution Timeout
**Solution**:
1. Reduce code complexity
2. Check for infinite loops
3. Verify language syntax is correct
4. Try with simpler code first

## 🔄 Recommended Testing Workflow

### Initial Setup
1. Import environment and collections
2. Configure environment variables
3. Run health check

### Authentication Testing
1. Register new user
2. Login with credentials
3. Test token refresh
4. Test OAuth flows (optional)

### API Testing
1. Get all courses
2. Select a course and get lessons
3. Run code execution tests
4. Run lesson tests
5. Test error scenarios

### Advanced Testing
1. Test with different programming languages
2. Test OAuth account linking/unlinking
3. Test edge cases and error conditions
4. Performance testing with multiple requests

## 📊 Collection Statistics

| Collection | Requests | Tests | Features |
|------------|----------|-------|----------|
| Health Check | 1 | 2 | Basic monitoring |
| Authentication | 4 | 8 | Token management, auto-refresh |
| OAuth | 9 | 18 | GitHub/Google integration |
| Courses | 4 | 8 | Course/lesson browsing |
| Code Execution | 5 | 10 | Multi-language support |
| Tests | 4 | 8 | Lesson validation |
| **Total** | **27** | **54** | **Full API coverage** |

## 🤝 Contributing

When adding new endpoints:
1. Use PascalCase for JSON properties
2. Add comprehensive test scripts
3. Include detailed descriptions
4. Update environment variables as needed
5. Follow existing naming conventions
6. Add console logging for debugging

## 📞 Support

For issues with the Postman collections:
1. Check this README first
2. Verify environment configuration
3. Check console logs for detailed errors
4. Ensure API is running and accessible
5. Validate request format matches DTOs

---

**Happy Testing! 🚀**