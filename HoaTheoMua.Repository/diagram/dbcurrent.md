```text
// Current database schema inferred from HoaTheoMua.Repository entities and EF Core configurations.
// Table names follow the AppDbContext DbSet convention, except tables explicitly mapped with ToTable().
// Base columns inherited by every entity: Id, IsDeleted.

// Source enums not explicitly converted to strings are persisted by EF Core as integer values.
Enum CategoryProductType {
  Fix
  Campaign
}

Enum InvoiceType {
  Personal
  Business
}

Table Addresses {
  Id uuid [pk]
  IsDeleted boolean [not null]
  UserId uuid [not null, note: 'External user ID; no local FK']
  ReceiverName varchar(150) [not null]
  PhoneNumber varchar(20) [not null]
  AddressLine varchar(500) [not null]
  Ward varchar(100)
  City varchar(100) [not null]
  Country varchar(100) [not null]
  Note varchar(500)
  MapId varchar(300)
  Latitude decimal
  Longitude decimal
  IsDefault boolean [not null]
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    UserId
  }
}

Table AuditLogs {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Action integer [not null, note: 'ActionAuditLog: Write, Read, Delete']
  Description text [not null]
  OldValue text [not null]
  NewValue text [not null]
  EntityType varchar(255) [not null]
  EntityId uuid [not null, note: 'Configured required although nullable in CLR entity']
  UserId uuid [not null, note: 'External user ID; no local FK']
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table CategoryNewsList {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Title varchar(255) [not null]
  UserId uuid [not null, note: 'External user ID; no local FK']
  ParentCategoryNewsId uuid
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table NewsList {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Title varchar(500) [not null]
  Description text [not null]
  ViewCount integer [not null, default: 0]
  CoverImage varchar(500) [not null]
  Status integer [not null, default: 0, note: 'NewsStatus: Draft, Published']
  slug text
  UserId uuid [not null, note: 'External user ID; no local FK']
  AuthorName text
  AuthorEmail text
  AuthorAvatar text
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table CategoryNewsDetails {
  Id uuid [pk]
  IsDeleted boolean [not null]
  CategoryNewsId uuid [not null]
  NewsId uuid [not null]
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    (CategoryNewsId, NewsId) [unique]
  }
}

Table CategoryProducts {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Name varchar(255) [not null]
  Name_En varchar(255) [not null]
  Description varchar(1000)
  Description_En varchar(1000)
  Position double
  MetaData jsonb
  Page text[]
  StartTime timestamptz
  EndTime timestamptz
  Type varchar(50) [not null, note: 'CategoryProductType stored as string']
  IsActive boolean [not null, default: true]
  UserId uuid [not null, note: 'External user ID; no local FK']
  ParentCategoryProductId uuid
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table Products {
  Id uuid [pk]
  IsDeleted boolean [not null]
  SourceRowNumber integer [not null]
  NhanhProductId varchar(100)
  ParentProductId varchar(100)
  ParentProductCode varchar(200)
  ParentProductName varchar(500)
  Barcode varchar(200)
  ProductType varchar(100)
  ProductCode varchar(200)
  ProductName varchar(500)
  OtherName varchar(500)
  Unit varchar(50)
  CoverImage varchar(2000)
  OtherImages text[]
  WarrantyMonths integer
  Weight decimal
  ImportPrice decimal
  ImportVat decimal
  ImportPriceWithVat decimal
  ImportVatMode varchar(100)
  Price decimal
  PriceWithVat decimal
  BranchPrice decimal
  BranchPriceWithVat decimal
  SaleVatMode varchar(100)
  OldPrice decimal
  Vat decimal
  ProfitPercentage decimal
  CostPrice decimal
  WholesalePrice decimal
  WholesalePriceWithVat decimal
  WholesaleBranchPrice decimal
  WholesaleBranchPriceWithVat decimal
  TotalStock decimal
  StockTransferring decimal
  Stock decimal
  DeliveringStock decimal
  WarehouseStock decimal
  StockError decimal
  Held decimal
  AvailableForSale decimal
  WarrantyStock decimal
  HeldComponents decimal
  PendingImportStock decimal
  PreOrder decimal
  Status varchar(100)
  CategoryCode varchar(100)
  CategoryName varchar(300)
  InternalCategoryCode varchar(100)
  Brand varchar(200)
  NhanhCategoryId integer
  NhanhBrandId integer
  NhanhTypeId integer
  Supplier varchar(500)
  WarrantyAddress varchar(1000)
  Slug varchar(500)
  Length decimal
  Width decimal
  Height decimal
  WebOrder integer
  NhanhCreatedAt timestamptz
  NhanhUpdatedAt timestamptz
  ProductLabel varchar(300)
  BranchPriceDuplicate decimal
  WholesaleBranchPriceDuplicate decimal
  PriceLabel varchar(100)
  Size varchar(100)
  Color varchar(100)
  Origin varchar(200)
  Position double
  IsPin boolean [not null, default: false]
  Factor decimal
  IsActive boolean [not null, default: false]
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    NhanhProductId [unique, note: 'Only when NhanhProductId IS NOT NULL']
    (ParentProductId, ProductType) [note: 'Only when IsDeleted = false']
    (Position, CreatedAt) [note: 'CreatedAt DESC; only when IsDeleted = false']
  }
}

Table CategoryProductDetails {
  Id uuid [pk]
  IsDeleted boolean [not null]
  CategoryProductId uuid [not null]
  ProductId uuid [not null]
  Position double
  IsPin boolean [not null, default: false]
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    (CategoryProductId, ProductId) [unique, note: 'Only when IsDeleted = false']
  }
}

Table ComboSpecifications {
  Id uuid [pk]
  IsDeleted boolean [not null]
  SubId uuid [not null]
  MatId uuid [not null]
  Quantity decimal
  IsCore boolean [not null]
  IsActive boolean [not null, default: true]
  CreatedAt timestamptz [not null]
  UpdatedAt timestamptz

  indexes {
    (SubId, MatId) [unique, note: 'Only when IsDeleted = false']
  }
}

Table CompanyAddresses {
  Id uuid [pk]
  IsDeleted boolean [not null]
  AddressLine varchar(500) [not null]
  Ward varchar(100) [not null, note: 'Configured required although nullable in CLR entity']
  City varchar(100) [not null]
  Country varchar(100) [not null]
  FullName varchar(100) [not null]
  PhoneNumber text [not null]
  Remarks text
  MapId varchar(300)
  Latitude decimal
  Longitude decimal
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table Configs {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Key text [not null]
  Value jsonb [not null]
  IsPublic boolean [not null, default: false]
  Group text [not null]
  Kind text
  UserId uuid [not null, note: 'External user ID; no local FK']
  CreatedAt timestamptz [not null]
  UpdatedAt timestamptz

  indexes {
    Group
  }
}

Table JobDescriptions {
  Id uuid [pk]
  IsDeleted boolean [not null]
  title varchar(250) [not null]
  location varchar(500) [not null]
  avatarUrl varchar(2048) [not null]
  coverImageUrl varchar(2048) [not null]
  minExperienceYears real
  maxExperienceYears real
  employmentType integer [not null, note: 'EmploymentType: FullTime, PartTime']
  salaryMin real
  salaryMax real
  salaryNegotiable boolean [not null, default: false]
  description text [not null]
  status integer [not null, note: 'StatusJobDescriptions: Open, Closed']
  openings integer [not null, default: 1]
  slug varchar(300) [not null]
  CreatedAt timestamptz [not null]
  UpdatedAt timestamptz

  indexes {
    slug [unique]
  }
}

Table RecruitmentRequest {
  Id uuid [pk]
  IsDeleted boolean [not null]
  title text [not null]
  coverLetter text [not null]
  attachments jsonb [not null]
  isRead boolean [not null, default: false]
  UserId uuid [not null, note: 'External user ID; no local FK']
  AuthorName text
  AuthorEmail text
  AuthorAvatar text
  JobDescriptionId uuid [not null]
  CreatedAt timestamptz [not null]
  UpdatedAt timestamptz
}

Table Orders {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Latitude decimal [not null]
  Longitude decimal [not null]
  Ward integer [not null]
  City integer [not null]
  Address text [not null]
  ShortAddress text [not null]
  FullName text [not null]
  Phone text [not null]
  Remarks text [not null]
  Status text [not null]
  TrackNumber text [not null]
  NhanhOrderId integer
  NhanhTrackingUrl text
  AhahaMoveOrderId text
  AhahaMoveTrackingUrl text
  PaymentUrl text
  QrUrl text
  ShippingFee decimal [not null]
  ProductFee decimal [not null]
  TotalFee decimal [not null]
  OrderTime bigint [not null]
  PhoneReceiver text [not null]
  FullNameReceiver text [not null]
  TagIds bigint[]
  TaxCode text
  IdentityNumber text
  ReceiptFullName text
  ReceiptPhoneNumber text
  ReceiptAddress text
  ReceiptEmail text
  IsExportInvoice boolean
  Vat decimal [default: 1.08]
  InvoiceType varchar [note: 'InvoiceType stored as string']
  PaymentMethod integer [not null, note: 'PaymentMethod enum']
  UserId uuid [not null, note: 'External user ID; no local FK']
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table OrderDetails {
  Id uuid [pk]
  IsDeleted boolean [not null]
  Name text [not null]
  ItemId integer [not null]
  ProductId uuid [not null, note: 'Scalar ID only; no FK configured']
  Metadata jsonb
  Number integer [not null]
  Price decimal [not null]
  Height double [not null]
  Weight double [not null]
  Length double [not null]
  Width double [not null]
  OrderId uuid [not null]
  CreatedAt timestamptz [not null]
  UpdatedAt timestamptz
}

Table Transactions {
  Id uuid [pk]
  IsDeleted boolean [not null]
  RequestId varchar(100) [not null]
  Gateway varchar(50) [not null]
  TransactionDate timestamp [not null]
  AccountNumber varchar(100) [not null]
  SubAccount varchar(100) [not null]
  Code varchar(100) [not null]
  Content varchar(500) [not null]
  TransferType varchar(50) [not null]
  Description varchar(500) [not null]
  TransferAmount decimal [not null]
  Accumulated decimal [not null]
  ReferenceCode varchar(100) [not null]
  paymentMethod integer [not null, note: 'PaymentMethod enum']
  OrderId uuid [not null]
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz
}

Table OrderNhanhVns {
  Id uuid [pk]
  IsDeleted boolean [not null]
  IdDonHangNhanh varchar(100)
  IdDonHangApi varchar(100)
  LoaiDon text
  ThoiGianDonHang timestamptz
  NguoiTaoDon text
  CuaHang text
  Nguon text
  KhoTaoDon text
  ThanhPhoKhoHang text
  QuanHuyenKhoHang text
  KhoLayHang text
  ThanhPhoLayHang text
  QuanHuyenLayHang text
  TenKhachHang varchar(250)
  MaKhachHang varchar(100)
  SoDienThoaiKhachHang varchar(30)
  EmailKhachHang varchar(250)
  CapDoKhachHang text
  NgaySinhKhachHang timestamptz
  ThanhPhoKhachHang text
  QuanHuyenKhachHang text
  PhuongXaKhachHang text
  DiaChiKhachHang text
  TongKhoiLuong decimal
  GiaTriDonHang decimal
  PhiVanChuyen decimal
  PhiVuotCan decimal
  PhiKhaiGia decimal
  PhiChuyenHoan decimal
  PhiThuTienHo decimal
  PhiShipBaoKhach decimal
  CongVanChuyen varchar(100)
  HangVanChuyen varchar(150)
  DichVuVanChuyen text
  SoNgayChuyenHang integer
  MaVanDon varchar(100)
  BienBan text
  TienChuyenKhoan decimal
  TaiKhoanChuyenKhoan text
  TienChietKhau decimal
  TienDatCoc decimal
  TaiKhoanDatCoc text
  TongThu decimal
  TaiKhoanQuetThe text
  TienQuetThe decimal
  MaGiaoDichQuetThe text
  GhiChuNoiBo text
  GhiChuCuaKhach text
  TrangThaiHoaDonDienTu text
  SoHoaDonDienTu text
  NgayXuatHoaDonDienTu timestamptz
  MaCoQuanThueHoaDonDienTu text
  TrangThaiDonHang varchar(100)
  LyDoHuy text
  NhanVienChamSocKhachHang text
  NgayXacNhan timestamptz
  NgayGuiHangVanChuyen timestamptz
  NgayBanGiao timestamptz
  NgayThanhCong timestamptz
  GioThanhCong text
  NguonDonHang varchar(100)
  GhiChuHangVanChuyen text
  CanNangHangVanChuyen decimal
  MaGiamGia varchar(100)
  IdDonTra text
  IdDonGoc text
  SoThuTuTheoNgay integer
  BienBanBanGiao text
  MaAffiliate text
  NhomKhachHang text
  TienVat decimal
  TyLeVat text
  TienTieuDiem decimal
  Nhan text
  NgayHenThanhToan timestamptz
  TrangThaiDoiSoat text
  NgayDoiSoat timestamptz
  TrangThaiThanhToanDoanhNghiep text
  NgayThanhToanDoanhNghiep timestamptz
  NhanVienBanHang text
  LoiNhuan decimal
  TyLeLoiNhuan text
  FacebookPsid text
  FacebookPageUserId text
  SoDienThoaiFacebook text
  GiaTriFacebook text
  IdPhapNhan text
  MaSoThue varchar(50)
  EmailNguoiLienHe varchar(250)
  EmailNguoiLienHeThuHai varchar(250)
  SoDienThoaiNguoiLienHe text
  TenNguoiLienHe varchar(250)
  MaThamChieu varchar(100)
  DiaChiKhachHangTrenHoaDonDienTu text
  TenKhachHangTrenHoaDonDienTu text
  LoaiKhachHangHoaDon text
  CoLayHoaDon text
  MaCoQuanThueHoaDon text
  SoHoaDon text
  NgayPhatHanhHoaDon timestamptz
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    IdDonHangNhanh [unique]
    IdDonHangApi
    MaVanDon
    MaKhachHang
    TenKhachHang
    SoDienThoaiKhachHang
    TrangThaiDonHang
    NguonDonHang
    CreatedAt
  }
}

Table OrderNhanhVnDetails {
  Id uuid [pk]
  IsDeleted boolean [not null]
  OrderNhanhVnId uuid
  TenSanPham varchar(250)
  QuaTangKem varchar(250)
  Imei varchar(100)
  LoHang varchar(100)
  MaSanPhamCha varchar(100)
  TenSanPhamCha varchar(250)
  MaSanPham varchar(100)
  MaVach varchar(100)
  DanhMucSanPham varchar(250)
  KhoiLuongSanPham decimal
  GiaBan decimal
  LoaiGiaSanPham varchar(50)
  GiaBanTruocVat decimal
  TienVatSanPham decimal
  SoLuong integer
  ChietKhauMotSanPham decimal
  ChietKhauSanPham decimal
  GiaSanPhamSauChietKhau decimal
  GhiChuSanPham text
  GiaVon decimal
  GiaVonDonHang decimal
  DonViTinh varchar(50)
  SoLuongDonViNhoNhat integer
  NhanVienDongGoi varchar(250)
  CreatedAt timestamptz [not null, default: `now()`]
  UpdatedAt timestamptz

  indexes {
    OrderNhanhVnId
    TenSanPham
    MaSanPham
    CreatedAt
  }
}

// Relationships configured by EF Core
Ref: CategoryNewsList.ParentCategoryNewsId > CategoryNewsList.Id
Ref: CategoryNewsDetails.CategoryNewsId > CategoryNewsList.Id [delete: cascade]
Ref: CategoryNewsDetails.NewsId > NewsList.Id [delete: cascade]
Ref: CategoryProducts.ParentCategoryProductId > CategoryProducts.Id
Ref: CategoryProductDetails.CategoryProductId > CategoryProducts.Id [delete: cascade]
Ref: CategoryProductDetails.ProductId > Products.Id [delete: cascade]
Ref: ComboSpecifications.SubId > Products.Id
Ref: ComboSpecifications.MatId > Products.Id
Ref: RecruitmentRequest.JobDescriptionId > JobDescriptions.Id [delete: cascade]
Ref: OrderDetails.OrderId > Orders.Id [delete: cascade]
Ref: Transactions.OrderId > Orders.Id [delete: cascade]
Ref: OrderNhanhVnDetails.OrderNhanhVnId > OrderNhanhVns.Id [delete: cascade]
```
