#!/bin/bash
set -ex

# dotnet tool install --global dotnet-validate --version 0.0.1-preview.582

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

NUGET_DIR=$ROOT_DIR/nupkgs

#TODO: check/create/...
#mkdir "$NUGET_DIR"
rm -f "$NUGET_DIR"/*
# rm -rf "$NUGET_DIR"

dotnet pack \
  -p:ContinuousIntegrationBuild=true \
  --output "$NUGET_DIR" \
  --configuration Release #\
  #-p:PackageVersion="1.0.4"

dotnet-validate package local "$NUGET_DIR/*"
