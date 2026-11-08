#!/usr/bin/env bash
# Renombra el proyecto HARIA "ejemplo" a "SkinLink" (contenido, carpetas y archivos).
# Ejecutar desde la raiz del repositorio (donde esta la carpeta "ejemplo") con Git Bash:
#     bash renombrar-skinlink.sh
set -euo pipefail
export LC_ALL=C

[ -d ejemplo ] || { echo "No encuentro la carpeta 'ejemplo'. Ejecuta el script en la raiz del repositorio."; exit 1; }
[ -z "$(git status --porcelain)" ] || { echo "Hay cambios sin confirmar. Haz commit o stash antes de renombrar."; exit 1; }

# 1) Contenido de los archivos de texto versionados (se conservan BOM y saltos de linea)
git ls-files -z | while IFS= read -r -d '' f; do
  case "$f" in *.db) continue;; esac
  if grep -Iq 'ejemplo' "$f" 2>/dev/null; then sed -i 's/ejemplo/SkinLink/g' "$f"; fi
done

# 2) Nombres de archivos y carpetas
git ls-files -z | while IFS= read -r -d '' f; do
  n="${f//ejemplo/SkinLink}"
  if [ "$n" != "$f" ]; then
    mkdir -p "$(dirname "$n")"
    git mv "$f" "$n"
  fi
done
find ejemplo -depth -type d -empty -delete 2>/dev/null || true

echo "Hecho. Ahora:"
echo "  cd SkinLink/generated/InitializeDB && dotnet run   (recrea la base de datos)"
echo "  cd .. && dotnet test"
