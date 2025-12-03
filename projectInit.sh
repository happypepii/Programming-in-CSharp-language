#!/bin/bash

PROJECT_NAME=$1

if [ -z "$PROJECT_NAME" ]; then
  echo "Usage: ./create-dotnet-project.sh <project_name>"
  exit 1
fi

# use current directory as base location
LOCATION=$(pwd)
PROJECT_DIR="$LOCATION/$PROJECT_NAME"

# create root folder for the project
mkdir -p "$PROJECT_DIR"
cd "$PROJECT_DIR" || exit

# create solution
dotnet new sln -n $PROJECT_NAME

# create main app (console project)
dotnet new console -n $PROJECT_NAME --use-program-main

# create test project (xUnit)
dotnet new xunit -n "$PROJECT_NAME.Tests"

# add both projects to solution
dotnet sln add "$PROJECT_NAME/$PROJECT_NAME.csproj"
dotnet sln add "$PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj"

# add reference from test project to main project
dotnet add "$PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj" reference "$PROJECT_NAME/$PROJECT_NAME.csproj"

# install packages for test project
dotnet add "$PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj" package xunit
dotnet add "$PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj" package xunit.runner.visualstudio
dotnet add "$PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj" package Microsoft.NET.Test.Sdk

echo "✅ Project setup complete!"
echo "Structure:"
echo "$PROJECT_DIR/"
echo " ├── $PROJECT_NAME/"
echo " │    └── Program.cs"
echo " ├── $PROJECT_NAME.Tests/"
echo " │    └── UnitTest1.cs"
echo " └── $PROJECT_NAME.sln"
