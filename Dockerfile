FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

COPY . ./
RUN dotnet publish ./src/Summarize.PR/Summarize.PR.csproj -c Release -o out --no-self-contained

LABEL com.github.actions.name="Summarize PR"
LABEL com.github.actions.description="A Github action"
LABEL com.github.actions.icon="sliders"
LABEL com.github.actions.color="green"

FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/Summarize.PR.dll" ]