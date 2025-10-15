# Use Node.js 20 Alpine as base image
FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application for production
RUN npm run build

# Expose port 3000
EXPOSE 3000

# Start the preview server (serves built files without HMR)
CMD ["npm", "run", "preview", "--", "--host", "0.0.0.0", "--port", "3000"]