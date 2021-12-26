FROM balenalib/raspberrypi3-debian-dotnet:buster-build as base

RUN [ "cross-build-start" ]

RUN install_packages \
    fswebcam \
    gcc \
    libasound2 \
    libasound2-dev \
    alsa-utils \
    avahi-utils

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SayTheName.csproj", "."]
RUN dotnet restore "./SayTheName.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SayTheName.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SayTheName.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN [ "cross-build-end" ]  

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SayTheName.dll"]
