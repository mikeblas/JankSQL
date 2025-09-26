# Clean Dockerfile without ANTLR complexity
# Prerequisites: Run `antlr -Dlanguage=CSharp -listener -no-visitor TSqlLexer.g4 TSqlParser.g4` in Parsing/ directory

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY JankSQL/*.csproj JankSQL/
COPY Parsing/*.csproj Parsing/
COPY Tests/*.csproj Tests/
COPY JankSh/*.csproj JankSh/
COPY ScratchWork/*.csproj ScratchWork/

# Restore dependencies
RUN dotnet restore JankSQL.sln

# Copy the rest of the source code
COPY . .

# Build the solution (CSharpTest.Net.Collections now comes from NuGet)
RUN dotnet build JankSQL.sln -c Release --no-restore

# Run tests (optional - comment out if you want faster builds)
# RUN dotnet test Tests/Tests.csproj -c Release --no-build --verbosity minimal

# Create runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

WORKDIR /app

# Copy built applications from build stage
COPY --from=build /app/JankSQL/bin/Release/net8.0 ./JankSQL
COPY --from=build /app/JankSh/bin/Release/net8.0 ./JankSh

# Create directories for data persistence
RUN mkdir -p /data/btree

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production

# Default to running the interactive shell
# Users can override this to run other components
ENTRYPOINT ["dotnet", "JankSh/JankSh.dll"]

# Expose any ports if needed (none specified in current implementation)
# EXPOSE 5432

# Add labels for metadata
LABEL maintainer="JankSQL Project"
LABEL description="JankSQL - A simple SQL database server implementation in C#"
LABEL version="1.0"
