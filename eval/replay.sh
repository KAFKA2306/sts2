#!/usr/bin/env bash
set -euo pipefail

scenario=${1:?usage: replay.sh SCENARIO.json NEW_EVIDENCE_DIRECTORY}
evidence=${2:?usage: replay.sh SCENARIO.json NEW_EVIDENCE_DIRECTORY}
port=${STS2_AGENT_PORT:-8080}
[[ $port =~ ^[0-9]{1,5}$ ]] && ((10#$port > 0 && 10#$port <= 65535)) || exit 2
base="http://127.0.0.1:$port"

jq -e '
  type == "object" and (.id | type == "string" and length > 0) and
  (.steps | type == "array" and length > 0 and all(
    type == "object" and
    (.before | type == "string" and length > 0) and
    (.action | type == "string" and length > 0) and
    (.after | type == "string" and length > 0)
  ))
' "$scenario" >/dev/null

mkdir -- "$evidence"
step=0
trap 'printf "{\"result\":\"blocked\",\"step\":%d}\n" "$step" >&2' EXIT
cp -- "$scenario" "$evidence/scenario.json"

api() {
    curl --disable --noproxy '*' --fail --silent --show-error --max-time 20 "$@"
}

api "$base/health" > "$evidence/health.json"
jq -e '.ok == true and .data.service == "sts2-ai-agent" and
    (.data.mod_version | ltrimstr("v")) == "0.16.2" and
    .data.status == "ready" and .data.play_running == false' "$evidence/health.json" >/dev/null

while IFS= read -r item; do
    step=$((step + 1))
    printf -v path '%s/%02d' "$evidence" "$step"
    before=$(jq -r '.before' <<< "$item")
    after=$(jq -r '.after' <<< "$item")

    api "$base/state" > "$path.before.json"
    jq -e ".ok == true and (.data | ($before) == true)" "$path.before.json" >/dev/null
    action=$(jq -ce ".data | $(jq -r '.action' <<< "$item") | select(type == \"object\")" "$path.before.json")
    name=$(jq -er '.action | select(type == "string" and length > 0)' <<< "$action")

    api "$base/actions/available" > "$path.available.json"
    jq -e --arg name "$name" '.ok == true and any(.data.actions[]; .name == $name)' "$path.available.json" >/dev/null
    printf '%s\n' "$action" > "$path.request.json"
    api --json "@$path.request.json" "$base/action" > "$path.response.json"
    jq -e '.ok == true and (.data.status == "completed" or .data.status == "pending")' "$path.response.json" >/dev/null

    ready=false
    for ((attempt = 0; attempt < 20; attempt++)); do
        api "$base/state" > "$path.after.json"
        if jq -e ".ok == true and (.data | ($after) == true and
            (.combat.action_readiness.snapshot_stable // (.screen != \"COMBAT\")))" "$path.after.json" >/dev/null; then
            ready=true
            break
        fi
        sleep 0.25
    done
    [[ $ready == true ]]
    if jq -e '.screenshot == true' <<< "$item" >/dev/null; then
        api "$base/vision/screenshot" > "$path.jpg"
        test -s "$path.jpg"
    fi
done < <(jq -c '.steps[]' "$evidence/scenario.json")

trap - EXIT
jq -cn --argjson steps "$step" '{result:"replayed",steps:$steps}'
