#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="${PROJECT_PATH:-./GameLibrary.Api.csproj}"
CONFIGURATION="${CONFIGURATION:-Release}"
OUTPUT_ROOT="${OUTPUT_ROOT:-./publish}"
RUNTIMES="${RUNTIMES:-linux-x64 linux-arm64 win-x64}"
SELF_CONTAINED="${SELF_CONTAINED:-true}"

echo "Project: ${PROJECT_PATH}"
echo "Configuration: ${CONFIGURATION}"
echo "Output root: ${OUTPUT_ROOT}"
echo "Runtimes: ${RUNTIMES}"
echo "SelfContained: ${SELF_CONTAINED}"

for rid in ${RUNTIMES}; do
  out_dir="${OUTPUT_ROOT}/${rid}"
  echo ""
  echo "Publishing runtime ${rid} -> ${out_dir}"

  dotnet publish "${PROJECT_PATH}" \
    -c "${CONFIGURATION}" \
    -r "${rid}" \
    --self-contained "${SELF_CONTAINED}" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -p:InvariantGlobalization=true \
    -o "${out_dir}"
done

echo ""
echo "Publish completed."
