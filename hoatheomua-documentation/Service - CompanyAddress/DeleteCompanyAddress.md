#### Request
```json
{
	"id" = "..."
}
```

#### Response
```json
{
  "value": "Xóa địa chỉ công ty thành công.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Service Flow (DeleteCompanyAddress)
![[../Store-Image/DeleteCompanyAddress.drawio.png]]

#### Test case
**Test case 01**: Xóa địa chỉ thành công  
**Điều kiện:** ID tồn tại, quyền Admin.  
**Kết quả:** Soft delete (IsDeleted = true), trả về message thành công.

**Test case 02**: ID không hợp lệ  
**Điều kiện:** id = Guid.Empty.  
**Kết quả:** Throw BadRequestException.

**Test case 03**: Không tìm thấy địa chỉ  
**Điều kiện:** ID không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException.

**Test case 04**: Không có quyền Admin  
**Điều kiện:** User không có role Admin.  
**Kết quả:** Trả về 403 Forbidden.

**Test case 05**: Địa chỉ đã bị soft delete trước đó  
**Điều kiện:** ID thuộc về địa chỉ đã có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "Không tìm thấy địa chỉ công ty".