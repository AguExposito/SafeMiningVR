#!/usr/bin/env python3
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

FORBIDDEN_UNDER_ASSETS_STRICT = (
    "Assets/.github/",
    "Assets/.agent-docs/",
)

FORBIDDEN_UNDER_ASSETS_CRITICAL = (
    "Assets/.cursor/hooks.json",
    "Assets/.cursor/agents/",
)

FORBIDDEN_EXTENSIONS_WARN = (
    ".mdc",
)

TRACKED_NOISE_PREFIXES = (
    "Library/",
    "Temp/",
    "Logs/",
    "UserSettings/",
    "Builds/",
)

def git_tracked_files():
    result = subprocess.run(
        ["git", "ls-files"],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        return []
    return [line.strip() for line in result.stdout.splitlines() if line.strip()]

def validate_tracked_files(files):
    errors = []
    warnings = []
    for rel in files:
        normalized = rel.replace("\\", "/")
        if normalized.startswith(TRACKED_NOISE_PREFIXES):
            errors.append(f"Tracked Unity cache/temp file must not be committed: {normalized}")
            continue
        if not normalized.startswith("Assets/"):
            continue
        for forbidden in FORBIDDEN_UNDER_ASSETS_STRICT:
            if normalized.startswith(forbidden):
                errors.append(f"Forbidden tooling path under Assets/: {normalized}")
        for forbidden in FORBIDDEN_UNDER_ASSETS_CRITICAL:
            if normalized.startswith(forbidden) or forbidden in normalized:
                errors.append(f"Forbidden Cursor automation under Assets/: {normalized}")
        if normalized.startswith("Assets/.cursor/"):
            warnings.append(
                f"Legacy .cursor path under Assets/ (migrate tooling to repo root .cursor/): {normalized}"
            )
        for ext in FORBIDDEN_EXTENSIONS_WARN:
            if normalized.endswith(ext):
                warnings.append(
                    f"Legacy rules file under Assets/ (migrate to .cursor/rules/): {normalized}"
                )
    return errors, warnings

def validate_required_paths():
    errors = []
    required = [
        "ProjectSettings/ProjectVersion.txt",
        "Packages/manifest.json",
        "Assets/SafeMining/LOTOSimulation/Code/Data/LotoObjectiveIds.cs",
        ".cursor/hooks.json",
    ]
    for rel in required:
        if not (ROOT / rel).exists():
            errors.append(f"Missing required project file: {rel}")
    return errors

def main():
    errors = validate_required_paths()
    file_errors, warnings = validate_tracked_files(git_tracked_files())
    errors.extend(file_errors)

    for warning in warnings:
        print(f"WARNING: {warning}")

    if errors:
        for err in errors:
            print(f"ERROR: {err}", file=sys.stderr)
        return 1

    print("Workspace hygiene OK.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
