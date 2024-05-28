# BotaniQue Backend

## Overview

BotaniQue is a project designed to simplify plant care by providing real-time monitoring and insights into plant conditions. This backend server is a key part of the BotaniQue ecosystem, supporting data processing, real-time updates, and integration with cloud services.

Created as an exam project by Júlia Ilášová and Maria Nielsen

![image](https://github.com/Team-Wilhelm/BotaniQue-Fullstack/assets/113031776/3186f28e-8c35-4f65-a378-7affeaf01646)

## Project Components

* Backend Server: Processes data from the IoT device, determines plant health, and provides real-time updates to the mobile application as well as handling authentication, data storage and business logic for the mobile   app.
* [IoT Device](https://github.com/team-wilhelm/botanique-iot) (Smart Plant Pot): An ESP32 microcontroller with sensors for soil moisture, air humidity, temperature, and light, and an OLED screen for displaying the plant's mood.
* [Mobile App](https://team-wilhelm.github.io/BotaniQue-MobDev/) (Frontend application): Developed in Flutter, it allows users to manage plants and view their conditions.

## Technologies Used

* Programming Language: .NET 8 (backend)
* MQTT Broker: Flespi
* Database: PostgreSQL
* WebSockets(using Fleck): For real-time communication
* Azure Cognitive Services: Azure vision for background removal from images
* Azure Blob Storage: For image storage

## Project Setup

Run database:
```bash
docker run --name botanique_db -p 5432:5432 -e POSTGRES_PASSWORD=password -e POSTGRES_USER=root -d postgres:14
```

To run everything you need to run the command
```bash
docker-compose up
```

