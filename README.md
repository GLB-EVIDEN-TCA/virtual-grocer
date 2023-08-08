# Virtual Grocer Application

> This sample is meant to feature a generic deployable OpenAI implementation. In its current state, it is a grocery assistant, breaking down dishes into their ingredients and pulling relevant images to present to users. It can be repurposed for many different customer experiences. For example

# About Virtual Grocer

This application allows you to ask a chatbot for help with grocery shopping.

# Manual Setup and Local Deployment

## Configure your environment

Before you get started, make sure you have the following requirements in place:

- [.NET 6.0/7.0 SDK](https://dotnet.microsoft.com/en-us/download)
- [Azure OpenAI](https://aka.ms/oai/access) resource or an account with [OpenAI](https://platform.openai.com).
- [Visual Studio Code](https://code.visualstudio.com/Download) **(Optional)** 

To launch the application, you simply need to run the back-end server application.

1. Open up a terminal and navigate to `virtual-grocer/src/Eviden.VirtualGrocer/Server/`
2. Run `dotnet build` to build the project.
3. Run `dotnet watch` to launch the project.

> This will, for now, deploy the project locally. In future implementation, we will set up an ARM template to cloud-deploy to an Azure tenant.

## Usage

1. In the browser, you can see the web application has loaded.
![](https://github.com/GLB-EVIDEN-TCA/virtual-grocer/blob/main/src/Grocer.png)
2. Type into the textbox to ask the chatbot any grocery-related questions you may have.



