#!/usr/bin/env python3
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

def main():
    scripts = [
        "tools/ci/validate_workspace.py",
        "tools/ci/validate_loto_objectives.py",
        "tools/ci/validate_csharp_sanity.py",
    ]
    if "--security" in sys.argv:
        scripts.append("tools/ci/validate_security_surface.py")
    for script in scripts:
        result = subprocess.run(
            [sys.executable, str(ROOT / script)],
            cwd=ROOT,
            check=False,
        )
        if result.returncode != 0:
            return result.returncode
    print("All local CI validations passed.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
