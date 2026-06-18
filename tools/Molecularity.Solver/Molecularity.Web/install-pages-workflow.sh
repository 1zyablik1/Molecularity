#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(git -C "$project_dir" rev-parse --show-toplevel)"

mkdir -p "$repo_root/.github/workflows"
cp "$project_dir/pages-workflow.yml" "$repo_root/.github/workflows/deploy-pages.yml"

echo "Installed $repo_root/.github/workflows/deploy-pages.yml"
echo "Commit and push the file, then select GitHub Actions in Settings > Pages > Source."
