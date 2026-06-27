# Viora

Viora is an AI-powered accessibility assistant designed to help blind and low-vision users, especially students, access educational materials and everyday information more independently through voice interaction.

Users can search and navigate documents, listen to content, ask questions about documents, generate summaries, and receive descriptions of their surroundings through AI-powered image understanding. The system supports natural Arabic speech, including both Modern Standard Arabic and the Egyptian dialect, making interaction simple and intuitive.

This repository contains the backend of Viora.

Built with **ASP.NET Core (.NET 10)**, the backend serves as the central API that connects the frontend with multiple AI services. It coordinates speech recognition, intent processing, text-to-speech, document analysis, OCR, and image description while exposing REST APIs and real-time communication through SignalR.


## Features

- **Voice pipeline** — Record audio → transcribe (Whisper) → classify intent (NLU) → execute action → respond with speech (Gemini TTS).
- **Document handling** — Upload PDFs, extract text, summarize content, ask questions against documents, and generate study quizzes.
- **Image description** — Upload an image, get an AI-generated description returned as audio.
- **Chat system** — Persistent conversation history with automatic summarization when threads grow long. Old audio files are cleaned up automatically.
- **Real-time notifications** — SignalR hub pushes events (e.g., new chat created) to connected clients.
- **Authentication** — JWT access + refresh token flow with ASP.NET Identity. Role-based access (User / Volunteer).
- **Account recovery** — Password reset via security questions (no email required).
- **Swagger docs** — Full OpenAPI documentation with XML comments, available in development mode.

## Technologies Used

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 (Web API) |
| Language | C# |
| Database | SQL Server + Entity Framework Core 10 |
| Authentication | ASP.NET Identity + JWT Bearer |
| Real-time | SignalR |
| Caching | In-memory cache (`IMemoryCache`) |
| HTTP Clients | `IHttpClientFactory` |
| API Docs | Swashbuckle (Swagger) |
| Testing | NUnit + Moq |
| AI Services | External microservices (Whisper STT, Gemini TTS, NLU, Vision) |

## Project Structure

```
Viora/
├── Viora/                        # Main API project
│   ├── Controllers/
│   │   ├── AccountController     # Auth: register, login, logout, token refresh, password reset
│   │   ├── SpeechController      # Core voice pipeline: STT, intent routing, TTS
│   │   ├── ChatController        # Chat history and audio URL retrieval
│   │   ├── DocumentController    # PDF upload, image upload + description
│   │   └── AudioController       # Stream saved audio files
│   ├── Services/
│   │   ├── SpeechToTextService           # Whisper API integration
│   │   ├── TextToSpeechService           # Gemini TTS API integration
│   │   ├── IntentClassificationService   # NLU intent detection
│   │   ├── DocumentHandlingService       # PDF extraction, summarization, Q&A, quiz generation, image analysis
│   │   ├── MessageHandlingService        # Message persistence + auto-summarization
│   │   ├── ChatHandlingService           # Chat CRUD
│   │   ├── JwtAuthenticationService      # Token generation
│   │   ├── NotificationService           # SignalR push notifications
│   │   ├── MangeAccountService           # Security question verification
│   │   ├── SaveAudioService              # Audio file persistence
│   │   └── FileCleanupService            # Background cleanup of old files
│   ├── Models/                   # EF Core entities (ApplicationUser, Chat, ChatMessage, UserFile, etc.)
│   ├── Repositories/             # Generic + specialized repositories
│   ├── Dtos/                     # Request/response DTOs
│   ├── AIResponses/              # Deserialization models for AI service responses
│   ├── Hubs/                     # SignalR ChatHub
│   ├── Data/                     # EF Core DbContext
│   ├── DataSeed/                 # Role seeding
│   └── Program.cs                # App configuration and DI setup
├── Viora.Tests/                  # Unit tests (NUnit + Moq)
│   └── Services/
│       ├── SpeechToTextServiceTests.cs
│       └── TextToSpeechServiceTests.cs
└── Viora.slnx                    # Solution file
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (local or remote)
- Access to the AI services (STT, TTS, NLU, Vision, Document processing)

## Installation

```bash
git clone https://github.com/<your-username>/Viora-Official-Backend.git
cd Viora
```

Restore dependencies and build:

```bash
dotnet restore
dotnet build
```

## Configuration

Copy the placeholder settings and fill in your values:

**`appsettings.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=VioraDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE",
    "Issuer": "VioraApp",
    "Audience": "VioraUsers",
    "TokenValidityMins": 15,
    "RefreshTokenValidityDays": 7
  },
  "ApiSettings": {
    "IntentEndPoint": "https://your-ai-host/.......",
    "SpeechToTextEndPoint": "https://your-ai-host/.......",
    "TextToSpeechEndPoint": "https://your-ai-host/.......",
    "UploadDocumentEndPoint": "https://your-ai-host/.......",
    "SummarizationEndPoint": "https://your-ai-host/.......",
    "Q&AEndPoint": "https://your-ai-host/.......",
    "GenenrateMaterialsEndPoint": "https://your-ai-host/.......",
    "AnalyzeImageEndPoint": "https://your-ai-host/.......",
    "SummarizeChatEndPoint": "https://your-ai-host/......."
  },
  "LocalPath": {
    "FolderPath": "your Path"
  }
}
```

Apply database migrations:

```bash
cd Viora
dotnet ef database update
```

> **Note:** Roles (`User`, `Volunteer`) are seeded automatically on startup.

## Usage

Run the API:

```bash
dotnet run --project Viora
```

The server starts on `http://0.0.0.0:5000` by default. In development mode, Swagger UI is available at:

```
http://localhost:5000/swagger
```

## API Overview

### Authentication (`/api/Account`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/register` | No | Create account (User or Volunteer) |
| POST | `/login` | No | Login, receive JWT + refresh token |
| POST | `/refresh-token` | No | Exchange refresh token for new token pair |
| POST | `/logout` | Yes | Revoke refresh token |
| POST | `/verify-account` | No | Verify identity via security questions |
| POST | `/forget-password` | No | Reset password with verification token |

### Voice Pipeline (`/api/Speech`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/stt` | Yes | Speech-to-text only (returns JSON) |
| POST | `/transcribe` | Yes | Full pipeline: STT → intent → action (returns JSON or MP3) |
| POST | `/tts` | Yes | Text-to-speech (returns MP3) |

### Chat (`/api/Chat`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/GetAllChats` | Yes | List user's chats |
| GET | `/GetAudioUrls/{id}` | Yes | Get audio references for a chat |

### Documents (`/api/Document`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/upload-file` | Yes | Upload PDF (max 10 MB) |
| POST | `/upload-image` | Yes | Upload image, get audio description |

### Audio (`/api/Audio`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/GetAudio/{messageId}` | Yes | Stream audio file for a message |

## Architecture

```
┌─────────────┐        ┌──────────────────────┐        ┌──────────────────┐
│   Client    │──HTTP──▶│     Viora API        │──HTTP──▶│  AI services │
│ (Mobile/Desktop)│◀──────── │  (ASP.NET Core 10)   │◀──────── 
└─────────────┘        │                      │        ├──────────────────┤
       ▲               │  Controllers         │        │ /transcribe (STT)│
       │               │  Services            │        │ /gemini-tts (TTS)│
       │  SignalR       │  Repositories        │        │ /process   (NLU) │
       └───WebSocket────│  EF Core + SQL Server│        │ /upload_pdf      │
                       └──────────────────────┘        │ /summarize       │
                                                       │ /chat      (Q&A) │
                                                       │ /study   (Quiz)  │
                                                       │ /vision  (Image) │
                                                       └──────────────────┘
```

**Key design decisions:**

- **Thin controllers, fat services** — Controllers validate input and return responses. Business logic lives in the service layer.
- **Repository pattern** — A `GenericRepository<T>` handles common CRUD. Specialized repositories (`ChatRepository`, `UserFileRepository`) handle domain-specific queries.
- **Auto-summarization** — When a chat exceeds 15 unsummarized messages, the oldest batch is summarized via AI and the associated audio files are deleted to save storage.
- **In-memory caching** — Small PDFs (< 50K characters) are cached for 4 minutes after upload to speed up follow-up operations like summarization and Q&A.

## Testing

Run the unit tests:

```bash
dotnet test
```

Tests use **NUnit** as the test framework and **Moq** for mocking HTTP dependencies. Current coverage includes:

- `SpeechToTextServiceTests` — Success and failure scenarios for the Whisper API integration.
- `TextToSpeechServiceTests` — Success and failure scenarios for the TTS API integration.



## AI Services

This backend communicates with a separate repository that hosts the AI services used by Viora. These services are responsible for speech recognition, text-to-speech, intent classification, document processing, OCR, image understanding, and other AI-powered features.

The AI services repository is available here:

https://github.com/MarinaaMaged/Visual-Disability-Graduation-Project


## Author

**Mark Sidrak**
