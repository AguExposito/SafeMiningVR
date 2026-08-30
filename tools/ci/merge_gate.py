#!/usr/bin/env python3
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

SECURITY_PATH_MARKERS = (
    "Assets/SafeMining/LOTOSimulation/Code/Adapters/",
    "Assets/SafeMining/LOTOSimulation/Code/Data/",
    "Assets/XR Standard Menu/Code/LOTOSimulation/Managers/SyncQueueManager.cs",
    "Assets/XR Standard Menu/Code/LOTOSimulation/Managers/SimulationSessionManager.cs",
    "Assets/XR Standard Menu/Code/Managers/IBManager.cs",
    "Assets/XR Standard Menu/Code/Managers/ScoreManager.cs",
)

UNITY_YAML_MARKERS = (".unity", ".prefab", ".asset")

def run(cmd, cwd=ROOT):
    result = subprocess.run(
        cmd,
        cwd=cwd,
        capture_output=True,
        text=True,
        check=False,
        shell=isinstance(cmd, str),
    )
    return result.returncode, result.stdout, result.stderr

def git_output(*args):
    code, out, err = run(["git"] + list(args))
    if code != 0:
        return []
    return [line.strip() for line in out.splitlines() if line.strip()]

def changed_files():
    files = set()
    files.update(git_output("diff", "--name-only", "HEAD"))
    files.update(git_output("diff", "--name-only", "--cached"))
    untracked = git_output("ls-files", "--others", "--exclude-standard")
    files.update(untracked)
    return sorted(f.replace("\\", "/") for f in files)

def matches_security_surface(path):
    for marker in SECURITY_PATH_MARKERS:
        if marker.endswith("/"):
            if path.startswith(marker):
                return True
        elif path == marker:
            return True
    return False

def infer_domain(files):
    domains = []
    if any("Interactables" in f or "XRS_" in f or "40-vr" in f for f in files):
        domains.append("xr")
    if any(matches_security_surface(f) for f in files):
        domains.append("network")
    if any("LotoScenario" in f or "LotoSequence" in f or "LotoObjective" in f for f in files):
        domains.append("compliance")
    if any("Editor" in f and "Lighting" in f or "perf" in f.lower() for f in files):
        domains.append("perf")
    if not domains:
        domains.append("toolchain")
    return domains[0] if len(domains) == 1 else "+".join(domains[:2])

def run_ci(security):
    cmd = [sys.executable, str(ROOT / "tools/ci/run_all.py")]
    if security:
        cmd.append("--security")
    code, out, err = run(cmd)
    return code == 0, (out + err).strip()

def branch_name():
    lines = git_output("branch", "--show-current")
    return lines[0] if lines else "unknown"

def ahead_of_origin():
    code, out, _ = run(["git", "rev-list", "--count", "origin/main..HEAD"])
    if code != 0:
        return None
    try:
        return int(out.strip())
    except ValueError:
        return None

def print_human_block(title, items):
    if not items:
        return
    print(f"\n## HUMAN ACTION REQUIRED — {title}")
    for item in items:
        print(f"- {item}")

def main():
    files = changed_files()
    security_needed = any(matches_security_surface(f) for f in files)
    yaml_hand_edits = [
        f for f in files
        if any(f.endswith(ext) for ext in UNITY_YAML_MARKERS)
        and "Assets/" in f
    ]

    auto_pass = []
    auto_fail = []
    human = []

    ci_ok, ci_log = run_ci(security_needed)
    if ci_ok:
        auto_pass.append("CI validators (tools/ci/run_all.py" + (" --security" if security_needed else "") + ")")
    else:
        auto_fail.append("CI validators failed — run: python tools/ci/run_all.py" + (" --security" if security_needed else ""))
        if ci_log:
            auto_fail.append(f"CI output: {ci_log[:500]}")

    if yaml_hand_edits:
        auto_fail.append(
            "Possible hand-edited Unity YAML: " + ", ".join(yaml_hand_edits[:5])
            + (" ..." if len(yaml_hand_edits) > 5 else "")
        )

    if security_needed:
        auto_pass.append("Security surface paths touched — --security scan included")
    else:
        auto_pass.append("No backend security paths in diff — security scan optional")

    domain = infer_domain(files)
    branch = branch_name()
    commits_ahead = ahead_of_origin()

    print("=" * 60)
    print("MERGE GATE REPORT — Phase E/F automation")
    print("=" * 60)
    print(f"Branch: {branch}")
    print(f"Domain (inferred): {domain}")
    print(f"Changed files: {len(files)}")
    if commits_ahead is not None:
        print(f"Commits ahead of origin/main: {commits_ahead}")

    print("\n## AUTO — PASS")
    for item in auto_pass:
        print(f"[PASS] {item}")

    if auto_fail:
        print("\n## AUTO — FAIL")
        for item in auto_fail:
            print(f"[FAIL] {item}")

    print("\n## SUGGESTED PR TITLE")
    print(f"[{domain}] <short description>")

    print("\n## SUGGESTED PR BODY (paste into GitHub)")
    body = {
        "classification": domain,
        "gates": "See merge gate report",
        "ci": "PASS" if ci_ok else "FAIL",
        "security_paths": security_needed,
        "changed_files": len(files),
    }
    print(json.dumps(body, indent=2))

    human_mcp = [
        "Unity_ReadConsole (Errors) → must be 0 — open Unity Editor with project, or ask agent to run MCP gate",
        "Unity_ValidateScript on each new/modified .cs — agent runs via MCP after code changes",
    ]
    if any(f.endswith(".cs") for f in files):
        human.extend(human_mcp)

    if any("Interactables" in f or "VR-LOTO" in f for f in files):
        human.append(
            "WF1 dual-mode spot-check if XR changed: hand tracking + controller + PC bypass (Editor Play Mode)"
        )

    if commits_ahead and commits_ahead > 0:
        human.append(
            f"git push -u origin {branch} — then open PR on GitHub (or: gh pr create)"
        )
    else:
        human.append("No unpushed commits — push branch before opening PR")

    human.append(
        "Agent will run /review-bugbot automatically when branch has C# changes; "
        "confirm zero CRITICAL/HIGH before merge"
    )
    if security_needed:
        human.append(
            "Agent will run /review-security automatically — confirm zero CRITICAL/HIGH before merge"
        )

    if not auto_fail and ci_ok:
        human.append(
            "If all reviews pass: merge PR on GitHub (or approve if using branch protection)"
        )

    print_human_block("only if automated checks passed", human)

    print("\n## VERDICT")
    if auto_fail or not ci_ok:
        print("BLOCKED — fix AUTO FAIL items before PR/merge")
        return 1
    print("READY FOR PR PACKAGE — complete HUMAN items if any, then merge after reviews")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
