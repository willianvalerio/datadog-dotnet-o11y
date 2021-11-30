# datadog-dotnet-o11y
This repo is a an example of instrumenting a dotnet application with Datadog, Custom Metrics and others stuffs.

## Prerequisites

- docker
- docker-compose

## Up and Running

Set these following variables

```bash
export DD_API_KEY=YOUR_DATADOG_API_KEY
```

Up and build containers

```bash
docker-compose up -d --build --remove-orphans
```

## Dashboard

You can find the dashboard json [here](datadog-dashboard.json), just import it to Datadog.



## Clean Up

```bash
docker-compose down --remove-orphans
```