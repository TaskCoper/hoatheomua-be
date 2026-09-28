# Phone authentication v2 consumer contract

Taskcoper issues phone Customer sessions with a GUID UserId, Customer role, CanUseStorefront=true and IsVerified=false. Ordinary email-verified tokens remain supported. Forgot-password tokens are not storefront sessions. Order create/status/cancel allow CanUseStorefront or existing IsVerified. Ownership remains keyed by UserId; admin/staff/default authorization policies are unchanged. No database migration is needed here. Keep TaskcoperStorageSession using email login v1 for image storage.

Source rules: User Authentication BR-37–49 in the shared documentation vault. Test missing email, phone session, old verified token, forgot token, wrong owner and preserved role gates. Access-token revocation remains bounded by JWT expiry; refresh sessions are revoked by the issuer after password change.
