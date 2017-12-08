FROM microsoft/dotnet:1.1-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out ./
VOLUME "/data" 
ENTRYPOINT ["dotnet", "Monitor.dll"]
