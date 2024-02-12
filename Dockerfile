FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln .
COPY DVSAdmin/*.csproj DVSAdmin/
COPY DVSAdmin.BusinessLogic/*.csproj DVSAdmin.BusinessLogic/
COPY DVSAdmin.CommonUtility/*.csproj DVSAdmin.CommonUtility/
COPY DVSAdmin.Data/*.csproj DVSAdmin.Data/

RUN dotnet restore DVSAdmin/

COPY . .
RUN dotnet publish -c Release -o out

# final image stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/out . 
ENTRYPOINT ["dotnet", "DVSAdmin.dll"]
