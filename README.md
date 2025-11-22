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

### API Endpoints

- `GET /api/crossword/puzzles` - Get list of available puzzle IDs
- `GET /api/crossword/puzzle` - Get the default puzzle (puzzle1)
- `GET /api/crossword/puzzle/{id}` - Get a specific puzzle by ID

### Available Puzzles

- `puzzle1` - Easy Crossword (10x10)
- `puzzle2` - Animals & Nature (8x8)

You can switch between puzzles by adding `?puzzle=puzzle2` to the URL.

## Project Structure

```
CrossWords/
├── src/
│   ├── server/                   # Backend API
│   │   ├── Controllers/
│   │   │   └── CrosswordController.cs    # API endpoints
│   │   ├── Models/
│   │   │   └── CrosswordPuzzle.cs        # Data models
│   │   ├── Services/
│   │   │   └── CrosswordService.cs       # Business logic and hardcoded puzzles
│   │   ├── Properties/
│   │   │   └── launchSettings.json       # Launch configuration
│   │   ├── Program.cs                    # Application entry point
│   │   ├── CrossWords.csproj             # Project file
│   │   ├── Directory.Packages.props      # Central package management
│   │   └── global.json                   # SDK version
│   └── client/                   # Frontend
│       └── wwwroot/
│           ├── index.html        # Main page
│           ├── styles.css        # Styling
│           └── script.js         # Frontend logic
├── Dockerfile                    # Docker image definition
├── docker-compose.yml            # Docker Compose configuration
├── .dockerignore                 # Docker ignore file
└── README.md
```

## Future Enhancements

- Database integration for puzzle storage
- Puzzle generation algorithms
- User accounts and progress tracking
- Puzzle difficulty levels
- Daily puzzles
- Multiplayer functionality

## Development

The application uses:
- ASP.NET Core 9.0 for the backend
- Vanilla JavaScript for the frontend
- Central package management with Directory.Packages.props
- No external dependencies (yet!)

To add more puzzles, edit `src/server/Services/CrosswordService.cs` and add entries to the `InitializePuzzles()` method.
