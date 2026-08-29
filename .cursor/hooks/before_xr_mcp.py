#!/usr/bin/env python3
import json
import re
import sys

META_ALLOW_TOOLS = (
    "meta_docs_search",
    "meta_docs_get_page",
    "get_api_details",
    "search_api_reference",
    "auth_status",
    "metavr_cli_help",
)

UNITY_XR_TOOL_PREFIXES = (
    "Unity_ManageGameObject",
    "Unity_ManageScene",
    "Unity_ApplyTextEdits",
    "Unity_ScriptApplyEdits",
    "Unity_CreateScript",
    "Unity_ManageScript",
)

XR_PATH_MARKERS = (
    "/Interactables/",
    "/XR Standard Menu/",
    "XRS_HandGrab",
    "LotoInputModalityVisuals",
    "LotoOutlineHint",
    "XRGrabInteractable",
    "XRSimpleInteractable",
    "XRSocketInteractor",
    "XRPokeInteractor",
    "XRInputModalityManager",
    "Teleportation",
    "hand tracking",
    "HandGrab",
)

XR_COMPONENT_PATTERN = re.compile(
    r"(XR|XRI|OpenXR|OVR|LotoInput|LotoOutline|Teleport|HandGrab|XRS_)",
    re.IGNORECASE,
)

def read_input():
    raw = sys.stdin.read()
    if not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {}

def flatten(value):
    if isinstance(value, str):
        return value
    if isinstance(value, dict):
        parts = []
        for item in value.values():
            parts.append(flatten(item))
        return " ".join(parts)
    if isinstance(value, list):
        return " ".join(flatten(item) for item in value)
    if value is None:
        return ""
    return str(value)

def extract_tool_name(payload):
    for key in ("tool_name", "toolName", "name", "tool"):
        value = payload.get(key)
        if isinstance(value, str) and value.strip():
            return value
    mcp = payload.get("mcp")
    if isinstance(mcp, dict):
        for key in ("tool_name", "toolName", "name"):
            value = mcp.get(key)
            if isinstance(value, str) and value.strip():
                return value
    return ""

def extract_server(payload):
    for key in ("server", "namespace", "mcp_server", "mcpServer"):
        value = payload.get(key)
        if isinstance(value, str) and value.strip():
            return value
    mcp = payload.get("mcp")
    if isinstance(mcp, dict):
        for key in ("server", "namespace"):
            value = mcp.get(key)
            if isinstance(value, str) and value.strip():
                return value
    return ""

def is_meta_verification(tool_name, server):
    combined = f"{server} {tool_name}".lower()
    if "meta" in combined and any(token in tool_name for token in META_ALLOW_TOOLS):
        return True
    return tool_name in META_ALLOW_TOOLS

def is_xr_unity_operation(tool_name, blob):
    if not any(tool_name.startswith(prefix) for prefix in UNITY_XR_TOOL_PREFIXES):
        return False
    if any(marker.lower() in blob.lower() for marker in XR_PATH_MARKERS):
        return True
    if XR_COMPONENT_PATTERN.search(blob):
        return True
    return False

def main():
    payload = read_input()
    tool_name = extract_tool_name(payload)
    server = extract_server(payload)
    blob = flatten(payload)

    if is_meta_verification(tool_name, server):
        print(json.dumps({"permission": "allow"}))
        return 0

    if not is_xr_unity_operation(tool_name, blob):
        print(json.dumps({"permission": "allow"}))
        return 0

    message = (
        "MCP Gate 3 — XR change detected. Before modifying XR/interaction logic:\n"
        "1. meta_docs_search(query='<API or feature>', scope='unity', mode='verify')\n"
        "2. meta_docs_get_page if signatures are needed\n"
        "3. Unity_ManageGameObject → get_components on target GameObject before edits\n"
        "4. Unity_ReadConsole (Errors) after changes\n"
        "Reference: .cursor/rules/02-mcp-bindings.mdc and .cursor/rules/40-vr-interaction.mdc"
    )

    print(json.dumps({
        "permission": "allow",
        "agent_message": message,
        "user_message": "XR MCP guardrail: verify Meta/XRI docs before applying this change.",
    }))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
