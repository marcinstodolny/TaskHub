#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/.." && pwd)
ENV_EXAMPLE_PATH="$REPO_ROOT/.env.example"
ENV_PATH="$REPO_ROOT/.env"

if [ ! -f "$ENV_PATH" ]; then
  cp "$ENV_EXAMPLE_PATH" "$ENV_PATH"
  echo "Created .env from .env.example"
else
  echo ".env already exists"
fi

if [ "${1:-}" = "--skip-compose" ]; then
  echo "Skipping docker compose startup"
  exit 0
fi

cd "$REPO_ROOT"
docker compose up --build
