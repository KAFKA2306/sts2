#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "${BASH_SOURCE[0]}")"
: "${Sts2DataDir:?set Sts2DataDir to the absolute path to the STS2 game data directory}"

dotnet build KarakuriZa.csproj --nologo --configuration Release -p:RestoreLockedMode=true
if [[ ${1:-} == --compile-only ]]; then
    exit 0
fi

: "${MEGADOT:?set MEGADOT to the MegaDot 4.5.1-m.14 .NET editor executable}"
version=$("$MEGADOT" --version)
if [[ $version != 4.5.1.m.14.mono.* ]]; then
    printf 'Unexpected MegaDot version: %s\n' "$version" >&2
    exit 1
fi

mkdir -p dist
stage=$(mktemp -d "$PWD/dist/.build.XXXXXX")
trap 'rm -rf -- "$stage"' EXIT
mkdir "$stage/KarakuriZa" "$stage/resources"
cp .godot/mono/temp/bin/Release/KarakuriZa.dll KarakuriZa.json "$stage/KarakuriZa/"

cp -R project.godot export_presets.cfg KarakuriZa "$stage/resources/"
"$MEGADOT" --headless --path "$stage/resources" --editor --import
"$MEGADOT" --headless --path "$stage/resources" --export-pack KarakuriZa "$stage/KarakuriZa/KarakuriZa.pck"
test -s "$stage/KarakuriZa/KarakuriZa.pck"

(cd "$stage" && zip -q KarakuriZa.zip KarakuriZa/KarakuriZa.{dll,json,pck})
mv -- "$stage/KarakuriZa.zip" dist/KarakuriZa.zip
(cd dist && sha256sum KarakuriZa.zip)
