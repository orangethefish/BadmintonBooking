# Dockerized Badminton Booking Application

This document describes how to run the Badminton Booking application using Docker containers.

## Prerequisites

- [Docker](https://www.docker.com/get-started) installed on your machine
- [Docker Compose](https://docs.docker.com/compose/install/) installed on your machine

## Getting Started

1. Clone this repository to your local machine
2. Navigate to the root directory of the project (where the `docker-compose.yml` file is located)

## Running the Application

To start all services (frontend, backend, and database):

```bash
docker-compose up -d
```

This will:
- Build and start the Angular frontend (accessible at http://localhost:80)
- Build and start the .NET backend API (accessible at http://localhost:5000)
- Start a MySQL database container

## Accessing the Services

- **Frontend**: http://localhost:80
- **Backend API**: http://localhost:5000
- **Database**: MySQL on localhost:3306 (credentials as defined in docker-compose.yml)

## Stopping the Application

To stop all services:

```bash
docker-compose down
```

To stop all services and remove volumes (this will delete your database data):

```bash
docker-compose down -v
```

## Viewing Logs

To see logs from all services:

```bash
docker-compose logs
```

To follow logs from a specific service:

```bash
docker-compose logs -f [service_name]
```

Where `[service_name]` is one of: `frontend`, `backend`, or `db`.

## Rebuilding Services

If you make changes to the code and need to rebuild:

```bash
docker-compose build
```

Then restart the services:

```bash
docker-compose up -d
```

## Database Persistence

The database data is stored in a Docker volume named `mysql-data`, which persists between container restarts. To completely reset the database, you can remove this volume using:

```bash
docker volume rm badmintonbooking_mysql-data
``` 