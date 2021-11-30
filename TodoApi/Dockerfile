FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src/TodoApi
COPY . .
RUN dotnet build "TodoApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish

FROM base AS final
COPY --from=publish /app/publish .
WORKDIR /opt/datadog
ARG DATADOG_TRACE=1.30.1
RUN curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v${DATADOG_TRACE}/datadog-dotnet-apm_${DATADOG_TRACE}_amd64.deb && \
    dpkg -i ./datadog-dotnet-apm_${DATADOG_TRACE}_amd64.deb && \
    rm -f /datadog-dotnet-apm_${DATADOG_TRACE}_amd64.deb
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_INTEGRATIONS=/opt/datadog/integrations.json
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
WORKDIR /app
ENTRYPOINT ["dotnet", "TodoApi.dll"]
