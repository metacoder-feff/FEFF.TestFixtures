#!/bin/bash
set -ex

# dotnet tool install --global dotnet-reportgenerator-globaltool
# dotnet restore --use-lock-file

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT_DIR=$(realpath "${SCRIPT_DIR}/..")

OUT_DIR=${ROOT_DIR}/TestResults
RAW_DIR=${OUT_DIR}/raw
COV_FILE=${OUT_DIR}/coverage/cobertura.xml
HTML_DIR=${OUT_DIR}/coverage/html

rm -rf ${OUT_DIR}/* || echo ok

dotnet test   \
  --no-restore \
  --solution "${ROOT_DIR}/FEFF.TestFixtures.slnx" \
  --results-directory "${RAW_DIR}" \
  --coverage  \
  --coverage-output-format cobertura \
  --coverage-settings "${ROOT_DIR}/tests.runsettings"

# merge all cobertura XMLs
# generate HTML
# generate Summary
reportgenerator                                       \
  -reports:"${RAW_DIR}/*.cobertura.xml"               \
  -targetdir:"${HTML_DIR}"                            \
  -reporttypes:"Cobertura;HtmlInline;TextSummary"

mv "${HTML_DIR}/Cobertura.xml"  "${COV_FILE}"
mv "${HTML_DIR}/Summary.txt"    "${OUT_DIR}/"

echo "See ${HTML_DIR}/index.html"

# print total coverage to be captured by gitlab-ci
grep "Line coverage" "${OUT_DIR}/Summary.txt"
