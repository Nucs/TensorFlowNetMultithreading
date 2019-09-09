# Build
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

COPY ./*.sln ./

COPY ./CalcEventsTFS/*.csproj ./CalcEventsTFS/

RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -f netcoreapp2.2 -o ./out

#  Run
FROM mcr.microsoft.com/dotnet/core/runtime:2.2

WORKDIR /app

COPY --from=build-env /app/out ./

COPY model/ model

CMD ["dotnet", "CalcEventsTFS.dll"]
