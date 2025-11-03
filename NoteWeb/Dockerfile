FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NoteWeb/NoteWeb.csproj", "NoteWeb/"]
RUN dotnet restore "NoteWeb/NoteWeb.csproj"
COPY . .
WORKDIR "/src/NoteWeb"
RUN dotnet build "NoteWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NoteWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoteWeb.dll"]
