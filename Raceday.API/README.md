# RaceDay API

A RESTful API for the RaceDay event management system. Built with ASP.NET Core 8, Entity Framework Core, and SQL Server.

This project was developed as part of the PROG6212 Portfolio of Evidence (POE) - Part 2.

---

## Table of Contents

- [Overview](#overview)
- [Technologies Used](#technologies-used)
- [User Roles](#user-roles)
- [Setup and Installation](#setup-and-installation)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [YouTube Demo](#youtube-demo)

---

## Overview

RaceDay is a system that allows event organisers to create and manage sporting events (runs, walks, cycles), and allows participants to enrol in these events and view their results.

This is the backend API that powers the system. A separate MVC frontend will be built in Part 3.

---

## Technologies Used

- Framework: ASP.NET Core 8 Web API
- Language: C#
- Database: SQL Server (LocalDB) with Entity Framework Core (Code-First)
- Authentication: Session-based with BCrypt password hashing
- Testing: xUnit with EF Core InMemory database
- CI/CD: GitHub Actions
- API Documentation: Swagger (Swashbuckle)

---

## User Roles

The system supports two distinct user roles:

### Organiser
- Register and log in
- Create, update, and delete events
- Define age/distance categories for events (e.g., "Senior", "10km")
- View all enrolments for their events
- Capture finishing times and positions for participants

### Participant
- Register and log in
- View all available events
- Enrol in events by selecting a category
- View their own enrolments
- View their own race results

Role-based access is enforced using a custom `SessionAuthorize` filter.

---

## Setup and Installation

### Prerequisites
- .NET 8 SDK
- SQL Server (or LocalDB)
- Visual Studio 2022 (or VS Code)

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/Pabii08/RaceDay.API
   cd RaceDay.API
1. ![CI/CD Success](./ci-cd-success.png)
1. https://youtu.be/369RPY8M1yg?si=DbPjF9RJqwgOI63g