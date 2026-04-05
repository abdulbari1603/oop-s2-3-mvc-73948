

## Overview

This repository uses GitHub Actions to automatically build and test the solution whenever code changes are pushed or proposed.

## Workflow file


- Path  .github/workflows/ci.yml 
- Workflow name  ci 
- Job name  build-and-test |

## Triggers

- Push to any branch  
- Pull request (any branch)

## Runner environment

- OS: ubuntu-latest

## Pipeline steps

1. Checkout — Fetches the repository using actions/checkout@v4.
2. Setup .NET — Installs the .NET SDK (8.0.x) via actions/setup-dotnet@v4.
3. Restore — dotnet restore VcgCollege.sln
4. Build — dotnet build VcgCollege.sln --configuration Release --no-restore
5. Test — dotnet test VcgCollege.sln --configuration Release --no-build --verbosity normal

## What is verified

The full solution is validated in Release configuration:

- src/VcgCollege.Web — ASP.NET Core MVC application  
- tests/VcgCollege.Tests — xUnit test project  

A successful run means the code compiles and all tests pass.


## Local parity

To match CI locally from the repository root:

bash
dotnet restore VcgCollege.sln
dotnet build VcgCollege.sln --configuration Release --no-restore
dotnet test VcgCollege.sln --configuration Release --no-build

