#### Request
```json
GET /api/v1/category-news/44444444-4444-4444-4444-444444444441
```
#### Response
```json
{
  "value": {
    "categoryId": "44444444-4444-4444-4444-444444444441",
    "title": "Technology",
    "userId": "11111111-1111-1111-1111-111111111111",
    "parentId": null,
    "parent": null,
    "children": [
      {
        "categoryId": "44444444-4444-4444-4444-444444444442",
        "title": "Software Development",
        "userId": "11111111-1111-1111-1111-111111111111",
        "parentId": "44444444-4444-4444-4444-444444444441"
      }
    ],
    "count": 5
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Service Flow (GetCategory)
![[../Store-Image/GetCategory.drawio (1).png]]
#### Test case

**Test case 01**: Lấy danh mục theo ID thành công  
**Điều kiện:** Category tồn tại và IsDeleted = false.  
**Kết quả:** Trả về thông tin danh mục kèm parent, children, và số lượng news thuộc danh mục.

**Test case 02**: Danh mục là con (có parent)  
**Điều kiện:** Category có ParentCategoryNewsId != null.  
**Kết quả:** Trả về thêm thông tin parent.

**Test case 03**: Danh mục không tồn tại  
**Điều kiện:** CategoryId không có trong DB
**Kết quả:** Hệ thống ném NotFoundException với message "CATEGORY_NOT_FOUND".

**Test case 04: Danh mục không tồn tại  
**Điều kiện:** CategoryId  có Deleted = true.  
**Kết quả:** Hệ thống ném NotFoundException với message "CATEGORY_NOT_FOUND".

**Test case 05**: Category không có children  
**Điều kiện:** Category là leaf, không có category nào có ParentCategoryNewsId = id.  
**Kết quả:** Trả về children = [], count = số news thuộc category.

**Test case 06**: Category không có news nào  
**Điều kiện:** Category tồn tại nhưng chưa có news nào được gắn category này.  
**Kết quả:** Trả về count = 0.

**Test case 07**: Category có children nhưng children đã bị soft delete  
**Điều kiện:** Có category con nhưng tất cả đều IsDeleted = true.  
**Kết quả:** children = [] (chỉ lấy children đang active).
