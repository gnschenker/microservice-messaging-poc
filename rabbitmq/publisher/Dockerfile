# FROM  mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim
FROM mcr.microsoft.com/dotnet/core/sdk:3.0
WORKDIR /app
COPY publisher.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet build -o /app/build
RUN dotnet publish -o /app/publish
WORKDIR /app/publish
CMD dotnet publisher.dll