#!/bin/bash

cd "$(dirname "$0")/.."

echo "Building HyprScribe..."

mcs \
    -pkg:gtk-sharp-3.0 \
    -target:exe \
    -out:build/HyprScribe.exe \
    -r:lib/Newtonsoft.Json.dll \
    -r:Mono.Data.Sqlite \
    -r:System.Data \
    src/Program.cs \
    $(find src/UI -name "*.cs") \
    $(find src/Handlers -name "*.cs") \
    $(find src/Logic -name "*.cs") \
    $(find src/Utils -name "*.cs") \
    $(find src/Models -name "*.cs") \
    $(find src/Config -name "*.cs")


if [ $? -eq 0 ]; then
	echo "Build successful! -> build/HyprScribe.exe"
else
	echo "Build failed."
	exit 1
fi

