#!/usr/bin/env python3
import json
import os
import sys
from pathlib import Path

EXPECTED_ROOT = Path("D:/Unity/Projects/SafeMiningVR")
MARKER = Path("ProjectSettings/ProjectVersion.txt")

def read_input():
    raw = sys.stdin.read()
    if not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {}

def resolve_workspace_root():
    env_root = os.environ.get("CURSOR_WORKSPACE_ROOT") or os.environ.get("WORKSPACE_FOLDER")
    if env_root:
        return Path(env_root)
    cwd = Path.cwd()
    if (cwd / MARKER).exists():
        return cwd
    for parent in [cwd, *cwd.parents]:
        if (parent / MARKER).exists():
            return parent
    return cwd

def main():
    payload = read_input()
    event = payload.get("hook_event_name", "sessionStart")
    root = resolve_workspace_root()
    marker_exists = (root / MARKER).exists()
    root_matches = root.resolve() == EXPECTED_ROOT.resolve()

    if event == "workspaceOpen":
        if not marker_exists or not root_matches:
            print("{}")
            return 1 if not marker_exists else 0
        print("{}")
        return 0

    lines = [
        "SafeMining VR — MCP Gate 1 (sessionStart)",
        f"Workspace root: {root}",
        f"Expected root: {EXPECTED_ROOT}",
        f"Project marker ({MARKER}): {'found' if marker_exists else 'missing'}",
        f"Root match: {'yes' if root_matches else 'no — abort Unity work until aligned'}",
        "",
        "Mandatory before any Unity/XR work:",
        "1. Unity_ManageEditor → Action: GetProjectRoot (must be D:/Unity/Projects/SafeMiningVR)",
        "2. Unity_ReadConsole → logTypes: Error (baseline should be 0)",
        "3. Before XR/Quest changes: meta_docs_search (scope: unity, mode: verify)",
        "4. After C# edits: Unity_ValidateScript on modified scripts",
        "",
        "Orchestrator: /safemining-orchestrator | Rules: .cursor/rules/02-mcp-bindings.mdc",
    ]

    if not marker_exists:
        lines.append("WARNING: Not a Unity project root. Open SafeMiningVR workspace before editing.")

    print(json.dumps({"additional_context": "\n".join(lines)}))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
