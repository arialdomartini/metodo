FROM microsoft/dotnet AS build
COPY . /src
WORKDIR /src/Metodo
RUN dotnet restore
RUN dotnet publish -c Debug -o /app

FROM microsoft/dotnet AS base
WORKDIR /app
COPY --from=build /app .
#ENTRYPOINT ["./TestClone"]