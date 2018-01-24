FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY . ./
RUN ls -la
WORKDIR /app/Saasi.Microservices/Saasi.Microservices.Combined
RUN dotnet restore

# copy everything else and build
WORKDIR /app
WORKDIR /app/Saasi.Microservices/Saasi.Microservices.Combined
RUN dotnet publish -c Release -o out

FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/Saasi.Microservices/Saasi.Microservices.Combined/out ./
ENTRYPOINT ["dotnet", "Saasi.Microservices.Combined.dll"]
