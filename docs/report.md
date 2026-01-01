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

![Illustration of the _Chirp!_ data model as UML class diagram.](docs/images/domain_model.png)

## Architecture — In the small

## Architecture of deployed application

## User activities

## Sequence of functionality/calls trough _Chirp!_

# Process

## Build, test, release, and deployment

## Team work

## How to make _Chirp!_ work locally

## How to run test suite locally

# Ethics

## License

## LLMs, ChatGPT, CoPilot, and others
