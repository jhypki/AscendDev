# AscendDev - E-Learning Platform

A comprehensive e-learning platform built with ASP.NET Core backend and React frontend.

## Quick Start with Docker Compose

### Prerequisites

- Docker and Docker Compose installed
- Git

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ascenddev
   ```

2. **Set up environment variables**
   ```bash
   cp .env.example .env
   ```
   Edit the `.env` file with your specific configuration values.

3. **Build and start all services**
   ```bash
   docker-compose up -d --build
   ```
   
   Note: Use `--build` flag to ensure Docker images are rebuilt with the latest changes, especially for Node.js version updates.

   This will start:
   - **PostgreSQL Database** (port 5432)
   - **Redis Cache** (port 6379)
   - **Backend API** (port 5171)
   - **Frontend App** (port 3000)
   - **Configuration Upload Service** (runs once to populate database)

4. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5171
   - API Documentation: http://localhost:5171/swagger

### Development Mode

The Docker Compose setup is configured for development with:
- Hot reload for frontend changes
- Volume mounts for live code updates
- Development environment settings

### Stopping the Application

```bash
docker-compose down
```

To also remove volumes (database data):
```bash
docker-compose down -v
```

## Services Overview

### Backend API
- **Technology**: ASP.NET Core 8
- **Database**: PostgreSQL
- **Cache**: Redis
- **Features**: JWT Authentication, OAuth, Code Execution, Course Management

### Frontend
- **Technology**: React 18 + TypeScript
- **Build Tool**: Vite 7.1.6 (requires Node.js 20.19+ or 22.12+)
- **UI Library**: Mantine
- **State Management**: Redux Toolkit + React Query

### Database
- **PostgreSQL 16** with initialization scripts
- Sample data automatically loaded on first run

## Environment Variables

Key environment variables (see `.env.example` for full list):

```env
# Database
POSTGRES_PASSWORD=your_secure_password
POSTGRES_DB=ascenddev

# Redis
REDIS_PASSWORD=your_redis_password

# JWT
JwtSettings__Key=your-256-bit-secret-key

# API URL for frontend
VITE_API_URL=http://localhost:5171/api
```

## Development

### Running Individual Services

**Backend only:**
```bash
docker-compose up postgres redis api -d
```

**Frontend only (after backend is running):**
```bash
cd frontend
npm install
npm run dev
```

### Logs

View logs for all services:
```bash
docker-compose logs -f
```

View logs for specific service:
```bash
docker-compose logs -f frontend
docker-compose logs -f api
```

## Production Deployment

For production deployment, consider:
1. Using production-optimized Dockerfiles
2. Setting up proper secrets management
3. Configuring reverse proxy (nginx)
4. Setting up SSL certificates
5. Enabling New Relic monitoring (optional)

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 3000, 5171, 5432, and 6379 are available
2. **Database connection**: Wait for PostgreSQL to be fully initialized
3. **Frontend API calls**: Ensure `VITE_API_URL` points to the correct backend URL

### Health Checks

The services include health checks. Check service status:
```bash
docker-compose ps
```

### Reset Everything

To completely reset the application:
```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d