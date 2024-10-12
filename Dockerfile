FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

COPY . ./
RUN dotnet publish ./src/Summarize.PR/Summarize.PR.csproj -c Release -o out --no-self-contained

LABEL com.github.actions.name="Summarize PR"
LABEL com.github.actions.description="A simple GitHub action that can create a brief information about commits in a pull request(PR)."
LABEL com.github.actions.icon="file-text"
LABEL com.github.actions.color="purple"

LABEL maintainer="Arda Cetinkaya"
LABEL repository="https://github.com/ardacetinkaya/summarize-pull-request-action"
LABEL homepage="https://github.com/ardacetinkaya"

FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/Summarize.PR.dll" ]