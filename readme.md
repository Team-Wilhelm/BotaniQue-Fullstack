# BotaniQue Backend

## Overview

BotaniQue is a project designed to simplify plant care by providing real-time monitoring and insights into plant conditions. This backend server is a key part of the BotaniQue ecosystem, supporting data processing, real-time updates, and integration with cloud services.

## Project Components

* IoT Device (Smart Plant Pot): An ESP32 microcontroller with sensors for soil moisture, air humidity, temperature, and light, and an OLED screen for displaying the plant's mood.
* Mobile App (Frontend application): Developed in Flutter, it allows users to manage plants and view their conditions.
* Backend Server: Processes data from the IoT device, determines plant health, and provides real-time updates to the mobile application as well as handling authentication, data storage and business logic for the mobile   app.

## Technologies Used

* Programming Language: .NET 8 (backend)
* MQTT Broker: Flespi
* Database: PostgreSQL
* WebSockets(using Fleck): For real-time communication
* Azure Cognitive Services: Azure vision for background removal from images
* Azure Blob Storage: For image storage

Run database:
```bash
docker run --name botanique_db -p 5432:5432 -e POSTGRES_PASSWORD=password -e POSTGRES_USER=root -d postgres:14
```

To run everything you need to run the command
```bash
docker-compose up
```

