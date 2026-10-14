# Quản lý món ăn — BIT240194

**Sinh viên:** Hoàng Minh Quân — **MSSV:** BIT240194

Ứng dụng ASP.NET Core MVC, Entity Framework Core 10.0.12, SQL Server.
Giao diện mặc định English; chọn Tiếng Việt ở góc trên để đổi ngôn ngữ.

## Chạy trên máy này

Máy đã có .NET SDK 10 và SQL Server LocalDB. Mở terminal trong thư mục dự án:

```powershell
dotnet restore
dotnet run --no-launch-profile --urls http://localhost:5080
```

Mở **http://localhost:5080**. Lần chạy đầu tự tạo database **BIT240194_MID**,
ba bảng và dữ liệu mẫu (3 loại, 5 món, mỗi món có 2 ảnh). Những lần sau giữ nguyên dữ liệu.

Có thể mở trực tiếp `FoodManager_BIT240194.csproj` bằng Visual Studio có hỗ trợ .NET 10.
Nếu chuyển máy, cần .NET SDK 10 và SQL Server LocalDB hoặc SQL Server.
Sửa `ConnectionStrings:FoodDatabase` trong `appsettings.json` cho phù hợp máy mới.
Ví dụ SQL Server Express dùng `Server=.\SQLEXPRESS;Database=BIT240194_MID;Trusted_Connection=True;TrustServerCertificate=True`.
Tài khoản kết nối cần quyền tạo database trong lần chạy đầu.

Nếu cổng 5080 đang có ứng dụng này chạy, dùng trang đã mở hoặc dừng phiên cũ bằng Ctrl+C trước khi chạy lại.

## Các chức năng

- Quản lý món: danh sách, thêm, sửa, chi tiết, xóa.
- Quản lý loại: danh sách, thêm, sửa, xóa; không xóa được loại đang có món.
- Kết hợp tìm một phần tên, loại món, trạng thái, giá từ/đến, sắp xếp giá tăng/giảm hoặc thời gian tăng.
- Truy vấn dùng IQueryable và thực thi ở SQL Server; form giữ các điều kiện sau khi lọc và đổi ngôn ngữ.
- Thời gian ≤ 15 phút hiển thị Quick/Nhanh; > 15 phút hiển thị Regular/Thông thường. Không có cột phân loại.
- Thêm/Sửa món có ô URL ảnh đại diện: dán link ảnh HTTP/HTTPS trên mạng hoặc đường dẫn nội bộ, lưu cùng thông tin món.
- Trang chi tiết vẫn cho thêm ảnh phụ qua ImageUrl, sửa URL từng ảnh, hiển thị nhiều ảnh, đổi ảnh đại diện và xóa ảnh.
- Món chưa có ảnh, link ảnh hỏng, tìm không có kết quả và Id không tồn tại có trạng thái riêng.
- Validation phía server bằng tiếng Anh hoặc tiếng Việt; các lỗi định dạng số cũng được dịch.
- Cookie ngôn ngữ lưu một năm; lần truy cập mới chưa có cookie luôn English, không phụ thuộc ngôn ngữ trình duyệt.
- Form thay đổi dữ liệu có antiforgery token, thao tác xóa món/loại có trang xác nhận.

## Thay ảnh ngay trong trang Sửa món

1. Mở Sửa món, tìm ô **URL ảnh đại diện** bên dưới Mô tả.
2. Trên trang web có ảnh, nhấp chuột phải vào ảnh và chọn **Sao chép địa chỉ hình ảnh**. Dán link HTTP/HTTPS trực tiếp vào ô URL.
3. Nhấn **Lưu thay đổi**. URL và thông tin món được lưu cùng nhau; ảnh đại diện hiển thị ở danh sách và chi tiết.

Để trống URL khi sửa sẽ giữ ảnh đang có. Món chưa có ảnh đại diện sẽ được thêm ảnh đại diện khi nhập URL. Có thể nhập URL ngay khi tạo món.
Ảnh nằm ở website gốc; database lưu URL. Link cần truy cập được để trình duyệt hiển thị ảnh.

## Database và ràng buộc

- Database: `BIT240194_MID`.
- Bảng: `BIT240194_DishCategories`, `BIT240194_Dishes`, `BIT240194_DishImages`.
- Tất cả cột vật lý đều được ánh xạ thành `BIT240194_` + tên thuộc tính C#.
- Model: `DishCategory_BIT240194`, `Dish_BIT240194`, `DishImage_BIT240194`.
- Giá SQL dùng decimal(18,2); validation yêu cầu giá > 0 và thời gian nguyên > 0.
- Unique index (DishCategoryId, Name) chặn trùng tên trong cùng một loại, kể cả khi có yêu cầu đồng thời.
- Unique filtered index trên DishId WHERE IsThumbnail = 1 đảm bảo tối đa một ảnh đại diện.
- Đổi ảnh đại diện dùng transaction Serializable: bỏ ảnh đại diện cũ rồi đặt ảnh mới.
- FK loại → món dùng Restrict; FK món → ảnh dùng Cascade.
- Dùng EnsureCreated cho database mới, không tạo bảng lịch sử migrations; có đúng ba bảng ứng dụng.
  Nếu tự thay đổi schema khi phát triển tiếp, cần thêm migrations hoặc cập nhật database chủ động.

`database/BIT240194_schema.sql` là DDL xuất trực tiếp từ EF Core.
`database/create-database.sql` là script khởi tạo database mới nếu muốn thao tác thủ công.
Chỉ dùng script khởi tạo khi chưa có ba bảng; cách chạy tự động ở trên đã đủ.
`database/verify.sql` cung cấp các truy vấn để chứng minh tên bảng/cột và ràng buộc khi demo.

## Các file chính để giải thích trước giảng viên

| File | Vai trò |
|---|---|
| Models/FoodModels.cs | Ba Model và navigation properties |
| Data/FoodDbContext.cs | Mapping tên SQL, quan hệ, unique index và check constraints |
| Data/SeedData.cs | Dữ liệu mẫu, không ghi đè dữ liệu đã có |
| ViewModels/FoodViewModels.cs | Dữ liệu form và validation dịch theo ngôn ngữ |
| Controllers/DishesController.cs | CRUD, IQueryable lọc/sắp xếp, quản lý ảnh |
| Controllers/CategoriesController.cs | CRUD loại và chặn xóa loại đang được dùng |
| Controllers/LanguageController.cs | Lưu ngôn ngữ bằng cookie và quay về URL hiện tại |
| Services/UiText.cs | Từ điển giao diện và thông báo English/Tiếng Việt |
| Views/ | Razor Views |
| wwwroot/css/site.css | Giao diện responsive |
| scripts/smoke-tests.ps1 | Kiểm thử tích hợp HTTP với dữ liệu tạm |

Ảnh mẫu là SVG nội bộ của dự án, nên demo không cần kết nối dịch vụ ảnh bên ngoài.
Ba loại mẫu hiển thị Anh–Việt: Main courses / Món chính, Starters / Khai vị, Desserts / Tráng miệng; mô tả loại mẫu cũng có bản dịch.
Tên và mô tả món ăn, cùng tên loại tự nhập chưa có trong từ điển, được giữ nguyên. Form sửa loại món hiển thị dữ liệu gốc để bạn chỉnh sửa.

## Kiểm thử

Mở ứng dụng trước, sau đó ở terminal PowerShell 7 khác:

```powershell
./scripts/smoke-tests.ps1
```

Bộ kiểm thử chính tạo dữ liệu có tên SmokeTest rồi xóa dữ liệu đó khi xong.
Kiểm thử URL ảnh ngay trên form món: `./scripts/cover-url-tests.ps1 -BaseUrl http://localhost:5080` (29 kiểm tra riêng, tạo và dọn dữ liệu tạm).
Không chạy trên database đang được sửa đổi đồng thời.
Kết quả đã kiểm chứng trên máy này: **87 kiểm tra tích hợp đều đạt**, build không có lỗi.
NuGet có cảnh báo NU1900 vì môi trường không truy cập được nguồn dữ liệu kiểm tra lỗ hổng;
restore và build vẫn thành công từ các gói đã có trên máy.

## Nộp bài

Xem `DEMO.md` để quay video đầy đủ theo đề.
Nộp mã nguồn và video lên EDUX; tự quay video và tự nộp bài.
Không đưa bin/obj hoặc dữ liệu máy cá nhân vào gói nộp.
