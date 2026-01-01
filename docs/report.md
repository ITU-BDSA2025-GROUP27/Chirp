---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2025 Group `27`
author:
- "Lukas Schultz Stryg <luss@itu.dk>"
numbersections: true
---

# Design and Architecture of _Chirp!_

## Domain model

The Author class inherits from IdentityUser (ASP.NET Core Identity) for authentication. We have added AuthorId, Cheeps collection, and Following/Followers collections.

The Cheep class has CheepId, Text (with 160 characters constraint), TimeStamp, and reference to Author.

The Hashtag class has HashtagId and TagName (with 50 characters constraint, unique).

The many-to-many relationships are handled by two join tables: CheepHashtag (linking Cheeps and Hashtags) and AuthorFollows (linking Authors for the follow feature).

![Illustration of the _Chirp!_ data model as UML class diagram.](images/domain_model.png)

## Architecture — In the small

The application follows the Onion Architecture pattern, where each layer depends only on inner layers.

Below is the Onion Architecture diagram.

![Illustration of the _Chirp!_ architecture as onion architecture diagram.](images/onion_architecture.png)

### Chirp.Core
Chirp.Core is the innermost layer and contains the domain entities (Author, Cheep, Hashtag, CheepHashtag) and Data Transfer Objects (DTO's). It defines the interfaces for both services (ICheepService, IAuthorService), and repositories (ICheepRepository, IAuthorRepository, IHashtagRepository). This layer has no external dependencies.

### Chirp.Infrastructure
Chirp.Infrastructure depends on Chirp.Core and contains implementations of its interfaces. It consists of two layers:

- **Services:**
Contains service implementations (AuthorService, CheepService) that orchestrate business logic between the presentation and repository layers.

- **Repositories:**
Contains repository implementations (AuthorRepository, CheepRepository, HashtagRepository), the database context (ChirpDBContext), and migrations.

### Chirp.Web
Chirp.Web is the outermost layer. It contains the Razor Pages that render the UI and their corresponding page models. It depends on Chirp.Infrastructure.

## Architecture of deployed application

The application follows a client-server architecture. Users access it through a web browser that communicates with the server over HTTPS.

The server is deployed to Azure App Service. It uses a SQLite database and supports user authentication through both email+password and GitHub OAuth.

![Illustration of the _Chirp!_ deployment architecture as deployment diagram.](images/deployment_architecture.png)

## User activities

### Unauthenticated users

- View the Public Timeline with all cheeps
- View author Private Timelines by clicking on author names
- Click hashtags to view all cheeps with that hashtag

To access the rest of the application, they must either log in with an existing user or register a new user.

![Illustration of unauthenticated user activities as UML activity diagram.](images/unauthenticated_user_activity.png)

### Authenticated users

- View the Public Timeline with all cheeps
- View author Private Timelines by clicking on author names
- Click hashtags to view all cheeps with that hashtag
- Post cheeps
- Follow and unfollow authors
- Log out
- View the About Me page:
  - User information (username and email)
  - List of followed users
  - All user's cheeps
  - Download personal data
  - Delete account

![Illustration of authenticated user activities as UML activity diagram.](images/authenticated_user_activity.png)

## Sequence of functionality/calls trough _Chirp!_

The sequence diagram shows the flow when a client requests the Public Timeline. It illustrates how the HTTP request is processed through Azure App Service, Routing, PublicModel, CheepService, CheepRepository, EF Core, and the SQLite database, then returned as rendered HTML.

![Illustration of the request flow through _Chirp!_ as UML sequence diagram.](images/sequence_diagram.png)

# Process

## Build, test, release, and deployment

The application uses GitHub Actions workflows for automated build, test, release, and deployment.

### Build and Test

Triggered on push to main or Pull Request. The workflow checks out the code, sets up .NET, restores dependencies, builds the project, installs Playwright browsers, and runs tests.

![Illustration of the build and test workflow as UML activity diagram.](images/build_and_test_workflow.png)

### Release

Triggered on tag v*. The workflow builds and tests the application, then publishes for Windows, macOS, and Linux in parallel. ZIP files are created and attached to a release with the version tag.

![Illustration of the release workflow as UML activity diagram.](images/release_workflow.png)

### Deploy to Azure

Triggered on push to main. The workflow has two jobs: Build (checkout, setup, build, publish, upload artifact) and Deploy (download artifact, login to Azure, deploy to Azure App Service).

![Illustration of the Azure deployment workflow as UML activity diagram.](images/azure_deploy_workflow.png)

## Team work

![Illustration of the project board as screenshot.](images/project_board.png)

At the time of writing this report, 47 issues have been completed. One issue remains in **TODO** (Session 13 - Add code documentation) and one is **In Progress** (Session 12/13 - Report).
All required functionality has been implemented.

### Issue workflow

Issues are created with the format: `Session <number> - <description>`. The issue body contains a user story at the top following the format `"In order to <receive benefit> as a <role>, I can <goal/desire>"`, followed by acceptance criteria below.

A branch is created using the naming convention `<type>/<description>`, where type is `feature/`, `refactor/`, or `docs/` depending on the issue type. This follows trunk-based development with short-lived feature branches. When a feature is complete, a pull request is created that needs to pass the Build and Test workflow to be merged to main. Usually reviewers would be assigned to the pull request, but as I work alone, I was allowed by TAs to merge without having to review my own pull requests. Once merged, the branch is deleted.

## How to make _Chirp!_ work locally

## How to run test suite locally

# Ethics

## License

## LLMs, ChatGPT, CoPilot, and others
