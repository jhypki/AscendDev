FROM node:20-alpine

# Set working directory
WORKDIR /app

# Install TypeScript globally
RUN npm install -g typescript ts-node

# Copy environment files
COPY . /app/

# Create a directory for code
RUN mkdir -p /app/code

# Convert line endings and set permissions for scripts
RUN sed -i 's/\r$//' /app/run-code.sh && chmod +x /app/run-code.sh

# Set default command
CMD ["node", "--version"]