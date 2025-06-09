#!/bin/bash
set -e

SRC_DIR="src"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

if [ -z "$NUGET_API_KEY" ]; then
  echo "Error: NUGET_API_KEY environment variable not set."
  exit 1
fi

cd "$SRC_DIR"

for dir in */; do
  csproj=$(find "$dir" -maxdepth 1 -name "*.csproj")
  if [ -f "$csproj" ]; then
    echo "Processing $csproj"

    current_version=$(grep '<Version>' "$csproj" | sed -E 's/.*<Version>([0-9]+\.[0-9]+\.[0-9]+)<\/Version>.*/\1/')
    
    if [ -z "$current_version" ]; then
      echo "No <Version> tag found in $csproj. Skipping."
      continue
    fi

    IFS='.' read -r major minor patch <<< "$current_version"
    patch=$((patch + 1))
    new_version="$major.$minor.$patch"

    sed -i.bak -E "s|<Version>$current_version</Version>|<Version>$new_version</Version>|" "$csproj"
    echo "Updated version: $current_version -> $new_version"

    dotnet pack "$csproj" -c Release

    nupkg_file=$(find "$dir"bin/Release -name "*.$new_version.nupkg" | head -n 1)

    if [ -f "$nupkg_file" ]; then
      echo "Publishing $nupkg_file"
      dotnet nuget push "$nupkg_file" --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE"
    else
      echo "Error: No .nupkg file found for $csproj"
    fi
  fi
done

echo "All done."
