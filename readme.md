# Semantic Kernel Demos

## Setup

You need to provide **deployment**, **endpoint** and **apikey** for your deployments. Set these as environment
variables.

```bash
export deployment="your-deployment"
export endpoint="your-endpoint"
export apikey="your-apikey"
```

You can also use `dotnet user-secrets` to store these values.

```bash
dotnet user-secrets init
dotnet user-secrets set "deployment" "your-deployment"
dotnet user-secrets set "endpoint" "your-endpoint"
dotnet user-secrets set "apikey" "your-apikey"
```

## Basic Chat

An example of a basic chatbot with a system prompt that set the tone of the conversation.

