#!/bin/bash
set -ex

# dotnet tool update -g docfx

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/../..")

DOC_DIR=${ROOT_DIR}/docs
OUT_DIR=${DOC_DIR}/_site

rm -rf "${OUT_DIR}"

docfx "${DOC_DIR}/docfx.json" --warningsAsErrors --serve
