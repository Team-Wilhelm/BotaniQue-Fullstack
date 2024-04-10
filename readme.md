Run database:
```bash
docker run --name botanique_db -p 5432:5432 -e POSTGRES_PASSWORD=password -e POSTGRES_USER=root -d postgres:14
```

To run everything you need to run the command
```bash
docker-compose up
```

