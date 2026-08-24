#!/bin/bash
set -ex

# dotnet tool update -g docfx
# dotnet restore --use-lock-file

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

DOC_DIR=${ROOT_DIR}/docs
OUT_DIR=${DOC_DIR}/_site

rm -rf "${OUT_DIR}"

dotnet build "${ROOT_DIR}/FEFF.TestFixtures.slnx" -c Release 
docfx build "${DOC_DIR}/docfx.json" --warningsAsErrors
