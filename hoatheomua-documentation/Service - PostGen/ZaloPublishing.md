# PostGen: Zalo OA article publishing

Implemented with the existing publication preview, immediate/scheduled job, revision/idempotency, durable outbox, worker lease and reconciliation lifecycle. Does not broadcast articles or send messages to followers.

## Configuration

Keep the existing `POSTGEN_ZALO_ENABLED`, app ID, app secret, OAuth callback and encryption key configuration. Add:

```dotenv
POSTGEN_PUBLISH_ZALO_ENABLED=true
POSTGEN_ZALO_ARTICLE_AUTHOR="Hoa theo mùa"
```

Both Compose files and the deployment workflow map these variables. Production uses GitHub environment secrets; editing a local `.env` alone does not update production. Publishing defaults to disabled. No database migration is needed: article metadata and the encrypted process reference use existing JSON snapshot/checkpoint columns.

The Zalo app needs OA information and article management permissions, with an authorized OA eligible to publish under its service plan. Connect the OA through the admin platform accounts screen. Do not reuse tokens from unrelated notification integrations.

## Article contract

- A normal article requires a title (1–150 Unicode scalars), body, author (1–50), description (1–300), and photo cover.
- Snapshot author comes from `POSTGEN_ZALO_ARTICLE_AUTHOR` (default `PostGen`); description is the title, shown in preview. No silent truncation.
- First selected image in sort order is the visible cover; remaining images are body paragraphs after the text and hashtags. Product cap: 10 images. JPEG/PNG, maximum 1,000,000 bytes each, public URLs checked by the existing SSRF-protected media client.
- Article and comments are created with `status=show`; no fabricated public URL is returned. The external article ID is saved on success. Zalo insights are not implemented and the UI does not call Meta insights for Zalo.

## Safety and recovery

1. Read OA identity through `getoa`; validate snapshot and media before creating.
2. Persist publication intent before the create request.
3. Encrypt the `article/create` processing token in the private checkpoint; do not expose it in API responses or attempt logs.
4. Poll `article/verify`, then verify the returned ID/title/type/visible status using `article/getdetail`. Only then mark Succeeded.
5. Timeout or malformed response after create is NeedsReview. Never replay a potentially successful create. An arbitrary manually supplied ID cannot substitute for the processing reference.
6. Work is bounded by existing operation timeout, step and attempt limits. An inconclusive job remains NeedsReview for operator investigation in OA Manager.
7. Background jobs refresh nearly expired Zalo credentials through the account service's durable one-use refresh lease. Both rotated credentials are encrypted and saved together. Refresh is blocked during an active publication lease. Interrupted refresh requires reconnecting the OA.

## Verification

Adapter tests exercise JSON/header contracts, ordered cover/body mapping, encrypted references, polling, missing references, timeouts, transport errors, invalid JSON, HTTP 401/403/429, image size and Unicode validation. Worker tests exercise preview → queued → prepare → create → verify with duplicate deliveries, persistence and secret-free responses. Account tests cover background token rotation without an HTTP user and skipping refresh before it is due.

A live OA publication still requires an authorized OA connection and an approved article fixture. Local contract tests do not prove the OA service plan or live permissions. See `results/api-integration-tester.json` for actual evidence and limitations.

## Official references

- [Create article](https://docs.zaloplatforms.com/docs/OA/noi-dung/noi-dung-dang-bai-viet)
- [Verify process](https://docs.zaloplatforms.com/docs/OA/noi-dung/noi-dung-dang-bai-viet/kiem-tra-ket-qua-tien-trinh-tao-noi-dung)
- [Article detail](https://docs.zaloplatforms.com/docs/OA/noi-dung/noi-dung-dang-bai-viet/lay-chi-tiet-cua-noi-dung-dang-bai-viet)
- [OAuth and token rotation](https://docs.zaloplatforms.com/docs/OA/bat-dau/xac-thuc-va-uy-quyen-cho-ung-dung-new)
