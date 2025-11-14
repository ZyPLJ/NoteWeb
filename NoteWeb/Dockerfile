FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY *.sln .
COPY ["NoteWeb/NoteWeb.csproj", "NoteWeb/"]
RUN dotnet restore "NoteWeb/NoteWeb.csproj"

COPY . .
WORKDIR "/src/NoteWeb"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

COPY fonts/opentype /usr/share/fonts/opentype

ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
ENTRYPOINT ["dotnet", "NoteWeb.dll"]
