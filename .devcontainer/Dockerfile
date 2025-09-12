FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Install Docker CLI, Node LTS and Python 3.11
RUN apt-get update \
 && apt-get install -y docker.io python3.11 python3.11-venv \
 && npm install -g npm@latest \
 && apt-get clean && rm -rf /var/lib/apt/lists/*
