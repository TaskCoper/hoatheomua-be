#!/usr/bin/env bash
# Validate exported deployment variables without printing credentials or calling providers.
set -euo pipefail

fail() { echo "PostGen configuration error: $1" >&2; exit 1; }
warn() { echo "::warning::$1"; }

# RabbitMQ backs the fulfilment saga and PostGen publishing; the API cannot run without it.
for key in RABBITMQ_USERNAME RABBITMQ_PASSWORD; do
  [[ -n "${!key:-}" ]] || fail "$key is required."
done

# AI proxy values are optional (Compose supplies defaults) but must be valid when set.
if [[ -n "${AI_CHAT_COMPLETIONS_URL:-}" ]]; then
  [[ "$AI_CHAT_COMPLETIONS_URL" =~ ^https?://[^/@?#[:space:]]+(/[^[:space:]]*)?$ ]] \
    || fail 'AI_CHAT_COMPLETIONS_URL must be an absolute HTTP(S) URL without userinfo.'
fi
if [[ -n "${AI_TIMEOUT_SECONDS:-}" ]]; then
  [[ "$AI_TIMEOUT_SECONDS" =~ ^([1-9]|[1-9][0-9]|1[0-7][0-9]|180)$ ]] || fail 'AI_TIMEOUT_SECONDS must be 1–180.'
fi

# XanhSM webhooks fail closed when unconfigured; warn instead of blocking unrelated deploys.
if [[ -z "${XANHSM_WEBHOOK_SIGNING_KEY:-}" ]]; then
  warn 'XANHSM_WEBHOOK_SIGNING_KEY is empty; XanhSM webhooks will be rejected.'
fi
case "${XANHSM_WEBHOOK_ENVIRONMENT:-}" in
  production|sandbox) ;;
  *) warn 'XANHSM_WEBHOOK_ENVIRONMENT must be production or sandbox; XanhSM webhooks will be rejected.' ;;
esac

case "${POSTGEN_ZALO_ENABLED:-false}" in
  false) echo "PostGen Zalo disabled; skipping credential validation." ;;
  true)
    for key in POSTGEN_ZALO_APP_ID POSTGEN_ZALO_APP_SECRET POSTGEN_ZALO_CALLBACK_URL; do
      [[ -n "${!key:-}" ]] || fail "$key is required when Zalo is enabled."
    done
    [[ "$POSTGEN_ZALO_APP_ID" =~ ^[0-9]+$ ]] || fail 'POSTGEN_ZALO_APP_ID must contain digits.'
    [[ "$POSTGEN_ZALO_CALLBACK_URL" =~ ^https://[^/@?#[:space:]]+(/[^?#[:space:]]*)?/platform-accounts/zalo/callback$ ]] \
      || fail 'POSTGEN_ZALO_CALLBACK_URL must be an HTTPS .../platform-accounts/zalo/callback URL without query, fragment or userinfo.'
    echo 'PostGen Zalo environment validated (credentials redacted).'
    ;;
  *) fail 'POSTGEN_ZALO_ENABLED must be true or false.' ;;
esac

case "${POSTGEN_META_ENABLED:-false}" in
  false) echo "PostGen Meta disabled; skipping credential validation."; exit 0 ;;
  true) ;;
  *) fail 'POSTGEN_META_ENABLED must be true or false.' ;;
esac

for key in POSTGEN_META_APP_ID POSTGEN_META_APP_SECRET POSTGEN_META_GRAPH_API_VERSION \
  POSTGEN_META_FACEBOOK_CALLBACK_URL POSTGEN_META_INSTAGRAM_CALLBACK_URL; do
  [[ -n "${!key:-}" ]] || fail "$key is required when Meta is enabled."
done
[[ "$POSTGEN_META_APP_ID" =~ ^[0-9]+$ ]] || fail 'POSTGEN_META_APP_ID must contain digits.'
[[ "$POSTGEN_META_GRAPH_API_VERSION" =~ ^v[0-9]+\.[0-9]+$ ]] || fail 'POSTGEN_META_GRAPH_API_VERSION must have format vN.N.'
if [[ -n "${POSTGEN_META_CONFIG_ID:-}" ]]; then
  [[ "$POSTGEN_META_CONFIG_ID" =~ ^[0-9]+$ ]] || fail 'POSTGEN_META_CONFIG_ID must contain digits.'
fi
for platform in FACEBOOK INSTAGRAM; do
  key="POSTGEN_META_${platform}_CALLBACK_URL"
  # Deployment callbacks use HTTPS; the application also supports HTTP on loopback for local tests.
  [[ "${!key}" =~ ^https://[^/@?#[:space:]]+(/[^?#[:space:]]*)?/platform-accounts/[^/?#]+/callback$ ]] \
    || fail "$key must be an HTTPS callback without query, fragment or userinfo."
  platform_lower=$(printf '%s' "$platform" | tr '[:upper:]' '[:lower:]')
  [[ "${!key}" == */platform-accounts/"$platform_lower"/callback ]] || fail "$key has the wrong platform path."
done

active_key="${POSTGEN_ACTIVE_ENCRYPTION_KEY_ID:-k1}"
case "$active_key" in
  k1) active_env=POSTGEN_ENCRYPTION_KEY_K1 ;;
  k2) active_env=POSTGEN_ENCRYPTION_KEY_K2 ;;
  *) fail 'POSTGEN_ACTIVE_ENCRYPTION_KEY_ID must be k1 or k2 for the supplied Compose mapping.' ;;
esac
[[ -n "${!active_env:-}" ]] || fail "$active_env is required for the active encryption key."
for key in POSTGEN_ENCRYPTION_KEY_K1 POSTGEN_ENCRYPTION_KEY_K2; do
  value="${!key:-}"
  [[ -n "$value" ]] || continue
  [[ "$value" =~ ^[A-Za-z0-9+/]{43}=$ ]] || fail "$key must encode 32 random bytes as Base64."
done

state_minutes="${POSTGEN_OAUTH_STATE_MINUTES:-10}"
timeout_seconds="${POSTGEN_REQUEST_TIMEOUT_SECONDS:-30}"
[[ "$state_minutes" =~ ^([1-9]|[12][0-9]|30)$ ]] || fail 'POSTGEN_OAUTH_STATE_MINUTES must be 1–30.'
[[ "$timeout_seconds" =~ ^([1-9]|[1-5][0-9]|60)$ ]] || fail 'POSTGEN_REQUEST_TIMEOUT_SECONDS must be 1–60.'
echo 'PostGen Meta environment validated (credentials redacted).'
