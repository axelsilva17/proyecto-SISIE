#!/usr/bin/env bash
set -euo pipefail

PRINCIPAL="Principal"

echo "[Git Sync] Sincronizando ramas de trabajo en la rama Principal: $PRINCIPAL"

git fetch --all --prune

# Asegurar que la rama Principal existe localmente o en remoto
if ! git rev-parse --verify --quiet "$PRINCIPAL"; then
  if git ls-remote --exit-code --heads origin "$PRINCIPAL" >/dev/null 2>&1; then
    git fetch origin "$PRINCIPAL":"$PRINCIPAL"
  else
    echo "La rama Principal no existe en remoto. Creando localmente..."
    git checkout -b "$PRINCIPAL" || exit 1
  fi
fi

git switch "$PRINCIPAL" 2>/dev/null || git checkout "$PRINCIPAL"
git pull origin "$PRINCIPAL" --ff-only || git pull origin "$PRINCIPAL"

# Mezclar ramas de feature en Principal (patrones comunes)
MAP_REGEX="^(feature/|feat/|hotfix/|bugfix/|release/)"
for br in $(git for-each-ref --format='%(refname:short)' refs/heads/); do
  if [ "$br" = "$PRINCIPAL" ]; then continue; fi
  if [[ "$br" =~ $MAP_REGEX ]]; then
    echo "[Git Sync] Fusionando $br -> $PRINCIPAL"
    git merge --no-ff "$br" -m "chore(sync): merge $br into $PRINCIPAL"
  fi
done

git push origin "$PRINCIPAL"

echo "[Git Sync] Sincronización completada."
