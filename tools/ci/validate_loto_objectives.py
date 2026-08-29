#!/usr/bin/env python3
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LOTO_IDS_PATH = ROOT / "Assets/SafeMining/LOTOSimulation/Code/Data/LotoObjectiveIds.cs"

EXPECTED_WEIGHTS = {
    "loto_obj_epp_selection": 20,
    "loto_obj_epp_defect": 10,
    "loto_obj_hazard_report": 25,
    "loto_obj_loto_sequence": 35,
    "loto_obj_zero_energy": 10,
}

EXPECTED_THRESHOLD = 60
EXPECTED_SIM_SCENE_ID = "loto_simulation"

def parse_const_strings(source):
    constants = {}
    for match in re.finditer(
        r'public const string\s+(\w+)\s*=\s*"([^"]+)";',
        source,
    ):
        constants[match.group(1)] = match.group(2)
    return constants

def parse_objective_ids(source):
    constants = parse_const_strings(source)

    array_match = re.search(
        r"public static readonly string\[\] AllObjectiveIds\s*=\s*\{([^}]+)\}",
        source,
        re.DOTALL,
    )
    if not array_match:
        raise ValueError("AllObjectiveIds array not found in LotoObjectiveIds.cs")

    members = [m.strip() for m in array_match.group(1).split(",") if m.strip()]
    ids = []
    for member in members:
        if member.startswith('"') and member.endswith('"'):
            ids.append(member.strip('"'))
        elif member in constants:
            ids.append(constants[member])
        else:
            raise ValueError(f"Unable to resolve AllObjectiveIds member: {member}")

    if not ids:
        raise ValueError("AllObjectiveIds is empty")

    weights = {}
    for case in re.finditer(r"case\s+([^:]+):\s*return\s+(\d+);", source):
        label = case.group(1).strip()
        weight = int(case.group(2))
        if label.startswith('"') and label.endswith('"'):
            weights[label.strip('"')] = weight
        elif label in constants:
            weights[constants[label]] = weight

    threshold_match = re.search(
        r"DefaultApprovalThresholdPercent\s*=\s*(\d+)",
        source,
    )
    threshold = int(threshold_match.group(1)) if threshold_match else None

    sim_match = re.search(
        r'SimulationSceneId\s*=\s*"([^"]+)"',
        source,
    )
    sim_scene = sim_match.group(1) if sim_match else None

    return ids, weights, threshold, sim_scene

def validate():
    if not LOTO_IDS_PATH.exists():
        raise FileNotFoundError(f"Missing {LOTO_IDS_PATH.relative_to(ROOT)}")

    source = LOTO_IDS_PATH.read_text(encoding="utf-8")
    ids, weights, threshold, sim_scene = parse_objective_ids(source)

    errors = []

    if set(ids) != set(EXPECTED_WEIGHTS.keys()):
        errors.append(
            f"AllObjectiveIds mismatch. Expected {sorted(EXPECTED_WEIGHTS.keys())}, got {sorted(ids)}"
        )

    for objective_id, expected_weight in EXPECTED_WEIGHTS.items():
        actual = weights.get(objective_id)
        if actual is None:
            errors.append(f"Missing weight case for {objective_id}")
        elif actual != expected_weight:
            errors.append(
                f"Weight for {objective_id}: expected {expected_weight}, got {actual}"
            )

    total = sum(weights.get(oid, 0) for oid in ids)
    if total != 100:
        errors.append(f"Objective weights must sum to 100, got {total}")

    if threshold != EXPECTED_THRESHOLD:
        errors.append(
            f"DefaultApprovalThresholdPercent must be {EXPECTED_THRESHOLD}, got {threshold}"
        )

    if sim_scene != EXPECTED_SIM_SCENE_ID:
        errors.append(
            f"SimulationSceneId must be '{EXPECTED_SIM_SCENE_ID}', got '{sim_scene}'"
        )

    if errors:
        raise ValueError("\n".join(errors))

    print("LOTO objective matrix OK: 5 objectives, weights sum to 100, threshold 60%.")

def main():
    try:
        validate()
        return 0
    except Exception as exc:
        print(f"LOTO validation failed: {exc}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    raise SystemExit(main())
