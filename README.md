# CrossWords - ASP.NET Core Web Application

A web-based crossword puzzle application built with ASP.NET Core and vanilla JavaScript.

## Features

- Interactive crossword grid with keyboard navigation
- RESTful API for puzzle data
- Multiple hardcoded puzzles (easily extensible)
- Check answers, reveal solutions, and clear grid functionality
- Responsive design
- Docker support for easy deployment

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Docker (optional, for containerized deployment)

### Running Locally

1. Navigate to the server directory:
   ```bash
   cd src/server
   ```

2. Restore dependencies and run the application:
   ```bash
   dotnet restore
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   http://localhost:5000
   ```

### Running with Docker

1. Build and run using Docker Compose:
   ```bash
   docker-compose up --build
   ```

2. Open your browser and navigate to:
   ```
   http://localhost:5000
   ```

3. To stop the container:
   ```bash
   docker-compose down
   ```

### Running with Docker (Debug Mode)

For development and debugging:

1. Build and run using the debug compose file:
   ```bash
   docker-compose -f docker-compose.debug.yml up --build
   ```

2. In VS Code:
   - Press `F5` or go to Run and Debug
   - Select "Docker .NET Attach" configuration
   - Choose the running CrossWords process

The debug configuration includes:
- Hot reload with `dotnet watch`
- Remote debugging support with vsdbg
- Source code mapping for breakpoints
- Development environment variables

### Running with Docker (without compose)

1. Build the Docker image:
   ```bash
   docker build -t crosswords .
   ```

2. Run the container:
   ```bash
   docker run -d -p 5000:8080 --name crosswords-app crosswords
   ```

3. Open your browser and navigate to:
   ```
   http://localhost:5000
   ```

## Development

The application uses:
- ASP.NET Core 10.0 for the backend
- Vanilla JavaScript for the frontend
- Central package management with Directory.Packages.props
