FROM node:18-alpine

WORKDIR /app

# Install netcat for the wait script
RUN apk add --no-cache netcat-openbsd

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# We'll use the command from docker-compose.yml to run the script
CMD ["node", "upload-sample-data"]