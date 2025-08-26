FROM node:16-alpine

# Set working directory
WORKDIR /app

# Copy environment files
COPY . /app/

# Install global dependencies
RUN npm install -g typescript ts-node

# Install dependencies from package.json
RUN npm install

# Create a directory for test files
RUN mkdir -p /app/test

# Convert line endings and set permissions for scripts
RUN sed -i 's/\r$//' /app/run-tests.sh && chmod +x /app/run-tests.sh

# Set default command
CMD ["npm", "test"]