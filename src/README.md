# Virtual Grocer Application

> This sample is meant to feature a generic deployable OpenAI implementation. 

# About Virtual Grocer

This application allows you to ask a chatbot for help with grocery shopping.

# Manual Setup and Local Deployment

## Configure your environment

Before you get started, make sure you have the following requirements in place:

- [.NET 6.0/7.0 SDK](https://dotnet.microsoft.com/en-us/download)
- [Azure OpenAI](https://aka.ms/oai/access) resource or an account with [OpenAI](https://platform.openai.com).
- [Visual Studio Code](https://code.visualstudio.com/Download) **(Optional)** 

To launch the application, you simply need to run the back-end API server.

    1. Open up a terminal (Ctrl+Shift+`) and navigate to `VirtualGrocer/src/`
    2. Run `dotnet build` to build the project.
    3. Run `dotnet watch InventoryPoc.Web/Server/InventoryPoc.Web.Server.csproj`

> This will, for now, deploy the project locally to `localhost:5012`. In future implementation, we will set up ARM to deploy to a cluster.

## Usage

    1. In the browser, you can see the web application has loaded.
    2. Type into the textbox to ask the chatbot any grocery-related questions you may have.
    ![](/Grocer.png)


