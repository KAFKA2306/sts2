#!/usr/bin/env bash
set -euo pipefail

expected="$(dirname -- "${BASH_SOURCE[0]}")/expected.json"
actual="${1:?usage: score.sh OBSERVED.json}"
jq -n --slurpfile expected "$expected" --slurpfile observed "$actual" '
  $observed[0] as $actual |
  ($expected[0] | to_entries | map(
    . as $case |
    ($case.value | keys) as $fields |
    ($actual[$case.key] // {}) as $observations |
    ([$fields[] | select(. as $key | $observations | has($key) | not)]) as $missing |
    {
      id: $case.key,
      replayed: ($missing | length == 0),
      passed: ($observations == $case.value),
      missing: $missing,
      mismatches: [$fields[] | . as $key | select(($observations | has($key)) and ($observations[$key] != $case.value[$key])) |
        {field: $key, expected: $case.value[$key], observed: $observations[$key]}]
    }
  )) as $cases |
  {
    total: ($cases | length),
    replayed: ([$cases[] | select(.replayed)] | length),
    passed: ([$cases[] | select(.passed)] | length),
    cases: $cases
  }' | jq -e '. | if .passed == .total then . else ., false end'
