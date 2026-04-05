#!/bin/bash
set -ex

# dotnet tool install --global dotnet-validate --version 0.0.1-preview.582

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

NUGET_DIR=$ROOT_DIR/nupkgs

read -s -p "Enter Api Key:" API_KEY

dotnet nuget push "$NUGET_DIR/*.nupkg"          \
  --api-key "$API_KEY"                          \
  --source https://api.nuget.org/v3/index.json  \
  --skip-duplicate

