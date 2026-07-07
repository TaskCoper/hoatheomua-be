#### Request
```json
GET /api/v1/category-news?pageIndex=1&pageSize=10&keyword=Tech
```
#### Response
```json
{
  "value": {
    "items": [
      {
        "categoryId": "44444444-4444-4444-4444-444444444441",
        "title": "Technology",
        "userId": "11111111-1111-1111-1111-111111111111",
        "parentId": null,
        "children": [
          {
            "categoryId": "44444444-4444-4444-4444-444444444442",
            "title": "Software Development",
            "userId": "11111111-1111-1111-1111-111111111111",
            "parentId": "44444444-4444-4444-4444-444444444441"
          },
          {
            "categoryId": "44444444-4444-4444-4444-444444444443",
            "title": "AI & Machine Learning",
            "userId": "11111111-1111-1111-1111-111111111111",
            "parentId": "44444444-4444-4444-4444-444444444441"
          }
        ]
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000002",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Service Flow (GetCategories)
![[../Store-Image/GetCategories.drawio (1).png]]
#### Test case
**Test case 01**: Lấy danh sách danh mục thành công (không keyword)  
**Điều kiện:** Có dữ liệu category trong DB, IsDeleted = false.  
**Kết quả:** Trả về danh sách phân trang, parent-child được nhóm đúng cấu trúc.

**Test case 02**: Lọc theo keyword  
**Điều kiện:** keyword = "Tech", có category chứa từ khóa.  
**Kết quả:** Trả về danh sách category có title chứa "Tech".

**Test case 03**: PageIndex/PageSize không hợp lệ  
**Điều kiện:** pageIndex <= 0 hoặc pageSize <= 0.  
**Kết quả:** Tự động chuẩn hóa pageIndex = 1, pageSize = 10.

**Test case 04**: Không có dữ liệu  
**Điều kiện:** DB rỗng hoặc tất cả category đều IsDeleted = true.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 06**: Category cha không có children  
**Điều kiện:** Có category cha nhưng chưa có category con nào được tạo.  
**Kết quả:** Trả về children = [] (mảng rỗng) cho category cha đó.

**Test case 07**: pageSize > 100  
**Điều kiện:** pageSize = 200.  
**Kết quả:** Tự động chuẩn hóa pageSize = 100.

**Test case 08**: Tất cả category đều là root (không có parent)  
**Điều kiện:** Tất cả category đều có ParentCategoryNewsId = null.  
**Kết quả:** Tất cả đều là root, mỗi category có children = [].

**Test case 09**: Lọc theo keyword không tìm thấy kết quả  
**Điều kiện:** keyword không khớp với bất kỳ title nào.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.