#!/usr/bin/env python3
import json
import subprocess
import sys
from pathlib import Path

WATCH_PREFIXES = (
    "Assets/SafeMining/",
    "Assets/XR Standard Menu/",
)
WATCH_SUFFIX = ".cs"
LOTO_IDS = Path("Assets/SafeMining/LOTOSimulation/Code/Data/LotoObjectiveIds.cs")
VALIDATE_SCRIPT = Path("tools/ci/validate_loto_objectives.py")

def read_input():
    raw = sys.stdin.read()
    if not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {}

def extract_paths(payload):
    paths = []
    for key in ("file_path", "filePath", "path", "uri"):
        value = payload.get(key)
        if isinstance(value, str) and value.strip():
            paths.append(value.replace("\\", "/"))
    tool_input = payload.get("tool_input")
    if isinstance(tool_input, dict):
        for key in ("path", "file_path", "filePath", "target_file"):
            value = tool_input.get(key)
            if isinstance(value, str) and value.strip():
                paths.append(value.replace("\\", "/"))
    files = payload.get("files")
    if isinstance(files, list):
        for item in files:
            if isinstance(item, str):
                paths.append(item.replace("\\", "/"))
            elif isinstance(item, dict):
                for key in ("file_path", "filePath", "path"):
                    if key in item and isinstance(item[key], str):
                        paths.append(item[key].replace("\\", "/"))
    edits = payload.get("edits")
    if isinstance(edits, list):
        for edit in edits:
            if isinstance(edit, dict):
                for key in ("file_path", "filePath", "path"):
                    if key in edit and isinstance(edit[key], str):
                        paths.append(edit[key].replace("\\", "/"))
    return paths

def normalize_path(path):
    normalized = path.replace("\\", "/")
    if normalized.startswith("file://"):
        normalized = normalized[7:]
    if len(normalized) > 2 and normalized[1] == ":":
        parts = normalized.split("/")
        for i, part in enumerate(parts):
            candidate = "/".join(parts[i:])
            if candidate.startswith("Assets/"):
                return candidate
    idx = normalized.find("Assets/")
    if idx >= 0:
        return normalized[idx:]
    return normalized

def is_watched(path):
    return path.endswith(WATCH_SUFFIX) and any(path.startswith(prefix) for prefix in WATCH_PREFIXES)

def run_loto_validation():
    if not VALIDATE_SCRIPT.exists():
        return None
    try:
        result = subprocess.run(
            [sys.executable, str(VALIDATE_SCRIPT)],
            capture_output=True,
            text=True,
            timeout=10,
            check=False,
        )
        if result.returncode == 0:
            return None
        return (result.stdout or "") + (result.stderr or "")
    except (OSError, subprocess.TimeoutExpired):
        return None

def main():
    payload = read_input()
    paths = [normalize_path(p) for p in extract_paths(payload)]
    watched = [p for p in paths if is_watched(p)]

    if not watched:
        print("{}")
        return 0

    lines = [
        "SafeMining VR — MCP Gate 2 (afterFileEdit)",
        f"Edited C# under watched paths: {', '.join(watched)}",
        "",
        "Before continuing, run:",
        "1. Unity_ReadConsole → logTypes: Error (require 0 entries)",
        "2. Unity_ValidateScript on each modified script",
    ]

    if any(p.endswith("LotoObjectiveIds.cs") for p in watched) or LOTO_IDS.as_posix() in watched:
        loto_error = run_loto_validation()
        if loto_error:
            lines.extend([
                "",
                "LOTO objective weight validation FAILED locally:",
                loto_error.strip(),
            ])
        else:
            lines.append("LOTO objective weights: local validation passed.")

    print(json.dumps({"additional_context": "\n".join(lines)}))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
