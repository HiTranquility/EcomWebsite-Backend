#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$APP_DIR"

ENV_FILE=".env.dev"
FILES=( -f docker-compose.yml -f docker-compose.dev.yml )
PROFILES=( App Api Database Cache Message Dashboard )

PROFILE_ARGS=()
for p in "${PROFILES[@]}"; do
  PROFILE_ARGS+=( --profile "$p" )
done

if [ "$#" -eq 0 ]; then
  set -- up -d
fi

docker compose \
  --env-file "$ENV_FILE" \
  "${FILES[@]}" \
  "${PROFILE_ARGS[@]}" \
  "$@"

