# XanhSM delivery completion webhook

## Endpoint and GreenSM setup

Webhook URL: `https://<backend-domain>/api/v1/xanhsm/webhook` (POST, application/json).

In GreenSM Developer Dashboard, select the application, open Settings → Webhooks,
enter the HTTPS URL and Signing Key, then Save. Select Basic + HMAC signing.
The endpoint verifies HMAC; it does not implement an OAuth token endpoint.
Use a separate signing key and backend configuration for sandbox and production.

Docker variables (all compose variants forward these):

```dotenv
XANHSM_WEBHOOK_ENVIRONMENT=sandbox
XANHSM_WEBHOOK_SIGNING_KEY=<random-secret-matching-the-dashboard>
XANHSM_WEBHOOK_SECONDARY_SIGNING_KEY=
```

Generate a key, for example with `openssl rand -hex 32`, then store it in your
runtime secret store and the dashboard. Never commit the real key. For production,
set the environment to `production` and use the production XanhSM API credentials
and URLs. No default signing key or environment is accepted.

For a non-Docker deployment the corresponding environment variables are
`XanhSmWebhookOptions__Environment`, `XanhSmWebhookOptions__SigningKey`, and
`XanhSmWebhookOptions__SecondarySigningKey`.

The NhanhVN client reuses `nhanhvn` database configuration with `NhanhVnOption`
fallbacks: `base_url`, `app_id`, `business_id`, `token`. The base URL must point to
NhanhVN v3.0 over HTTPS. The app needs order listing and editing permissions.

Deploy the migration `20260909090000_AddNhanhCompletedShippingOrderId` before
processing webhooks. This API already runs migrations at startup. PostgreSQL,
RabbitMQ and the API-hosted MassTransit consumers/outbox must be running.
The new receive queue is `xanh-sm-order-changed` (MassTransit kebab-case naming).

## Processing

1. Verify `X-Environment` and HMAC-SHA256 against the original bytes. Accept the
   primary signature/key or the secondary signature/key. Missing configuration
   fails closed. Limit request size to 256 KiB.
2. Accept `order_status_changed`; require `event_id`, `data.order_id` and
   `data.status`. Other events, including `point_status_changed` and GPS, get
   HTTP 200 after authentication and JSON validation. Point completion alone
   does not establish order completion.
3. Publish `XanhSmOrderChanged` with `Status` and `SubStatus` into the PostgreSQL
   transactional bus outbox and commit before returning HTTP 200. A failed
   commit returns an error so GreenSM can retry.
4. The consumer ignores non-COMPLETED statuses and RETURNING/RETURNED/FAILED
   sub-statuses before querying or locking an order. It uses the signed webhook
   status directly; it makes no XanhSM detail call and never follows resource_href.
5. For a successful COMPLETED event, find the matching XanhSm order and lock it
   in the EF outbox transaction. Skip already synchronized deliveries. Require
   a local NhanhOrderId and reject failed local fulfillment.
6. Query NhanhVN `/order/list` filtered by that NhanhOrderId. If status is already
   60, treat it as synchronized. Otherwise call `/order/edit` with only
   `{"info":{"id":123,"status":60}}`. Cancelled, failed and returning POS
   orders are not overwritten; these messages require operator review.
7. Only after verified success save `Order.NhanhCompletedShippingOrderId`.
   This marker survives restarts and deduplicates different event IDs for the
   same completed delivery. The pre-edit read also handles a crash after remote
   success but before local commit. Local payment/fulfillment Status is unchanged.

Completion is based on the order event, without checking individual drop-off
points or fetching the latest courier state. Delayed non-completion events do
not undo completion. This assumes a successful order COMPLETED event is final.

Messages queued with the previous contract have no Status and will go to the
error queue; reconcile those deliveries and replay with a verified status when
upgrading an already running deployment.

## Retries and operation

The existing consumer retry policy waits 1, 5, 15, 30 and 60 seconds; permanent
business conflicts go directly to the error queue. HTTP/network errors and
NhanhVN code != 1 do not write a success marker. Exhausted messages remain in
`xanh-sm-order-changed_error` for inspection and replay after fixing the cause.
No periodic reconciliation job is added in this change.

Unknown shipping IDs and a not-yet-created NhanhVN order are retried because
booking/POS persistence may still be in progress. A delivery belonging to another
application will eventually require error-queue inspection. Configure the correct
GreenSM application and environment to avoid that traffic.

The MassTransit inbox deduplicates the stable hash of environment + event_id for
its configured 30-minute window; the completion marker protects completed orders
beyond that window. Invalid signature/environment returns 401, invalid JSON or
missing status-event identifiers returns 400, wrong content type returns 415,
and missing signing configuration returns 500.

There is no distributed transaction with NhanhVN: an independent operator or
integration can change the POS order between our read and edit. NhanhVN enforces
its own transition restrictions. API rejection is retried and ultimately visible
in the error queue, rather than being marked successful locally.

## Verification before enabling production

Use a sandbox delivery mapped to a test NhanhVN order. Check invalid signatures
are rejected, valid callbacks return 200, successful deliveries result in status
60 and a local completion marker, duplicates cause no additional edit, and
returning/failed deliveries do not update the POS. Reconcile the real XanhSM
order `status` and `sub_status` payload with the success guard during this test.

Automated tests use webhook payloads and mocked NhanhVN responses; they do not modify live orders.
Dashboard registration, runtime secret installation and deployment are separate
operational steps; they have not been performed by this code change.

Sources checked 2026-09-09:
- GreenSM webhook specification supplied in the task.
- https://apidocs.nhanh.vn/v3/order/edit
- https://apidocs.nhanh.vn/v3/order/list
- https://apidocs.nhanh.vn/v3/modelconstant#order-status
