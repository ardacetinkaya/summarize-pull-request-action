FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

COPY . ./
RUN dotnet publish ./src/PR.Action/PR.Action.csproj -c Release -o out --no-self-contained

LABEL com.github.actions.name=".NET code metric analyzer"
LABEL com.github.actions.description="A Github action"
LABEL com.github.actions.icon="sliders"
LABEL com.github.actions.color="green"

FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/PR.Action.dll" ]