#!/bin/bash
set -ex

# dotnet tool update -g docfx
# dotnet restore --use-lock-file

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

DOC_DIR=${ROOT_DIR}/docs
OUT_DIR=${DOC_DIR}/_site
API_DIR=${DOC_DIR}/api

rm -rf "${OUT_DIR}"
find "${API_DIR}" -maxdepth 1 -type f -not -name "index.md" -delete

# split this phase to fix docfx warning: 
#"The analyzer assembly references version '...' of the compiler, which is newer than the currently running version '...'.""

# generate API section
docfx metadata "${DOC_DIR}/docfx.json"

# final doc creation
docfx build "${DOC_DIR}/docfx.json" --warningsAsErrors