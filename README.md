# Reservation System

### Overview
This is a reservation system designed to take the hassle out of reserving flights, rental cars and hotels. The process is initiated by a click of a button on the web page, after which the system orchestrates the reservation of a flight, car rental and hotel for the user.

The UI is a React SPA that interfaces with an Azure Function App. The user initiates a reservation via a button click on the UI, which calls through to an Http triggered Azure Function. This function then initates the orchestration by calling a Durable Function. All events throughout the orchestration are reported back to the UI via an Azure SignalR service that the Durable Function interacts with. The user has the option to simulate a failure at any one of the reservation steps, which would then demonstrate compensating actions cancelling any prior reservations, ensuring the reservation is not left in an inconsistent state.

### Architecture Diagram
![Architecture Diagram](./frontend/src/assets/design.svg)

### Sequence Diagram
![Sequence Diagram](./frontend/src/assets/sequence.svg)

### Local Development
1. You'll need to install & run the [SignalR emulator](https://learn.microsoft.com/en-au/azure/azure-signalr/signalr-howto-emulator).
1. Open the **Reservation** solution (found in the **Backend** folder) and  solution up in Visual Studio and run the project.
1. Open the **Frontend** folder in VSCode and run `npm run dev`.
1. You'll also need to login to Azure via the Azure CLI by running the following (replacing TENANT_ID with your tenant ID):
    ```
    az login --tenant <TENANT_ID> // e.g. az login --tenant dca5775e-99b4-497c-90c1-c8e73396999f
    ```