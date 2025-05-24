FROM node:20-alpine

# Set working directory
WORKDIR /app

# Install TypeScript globally
RUN npm install -g typescript ts-node

# Copy environment files
COPY ./environments/runners/typescript/ /app/

# Create a directory for code
RUN mkdir -p /app/code

# Set permissions for scripts
RUN chmod +x /app/run-code.sh

# Set default command
CMD ["node", "--version"]