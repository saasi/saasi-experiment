FROM microsoft/dotnet:1.1-runtime
ARG source
WORKDIR /app
# COPY ${source:-obj/Docker/publish} .
COPY ${source:-bin/Debug/netcoreapp1.1/publish/} .
ENTRYPOINT ["dotnet", "Memory_microservice.dll"]
