# Kịch bản video demo — Hoàng Minh Quân, BIT240194

Quay khoảng 6–8 phút. Thực hiện lần lượt sáu mục dưới đây; mỗi thao tác cần giữ kết quả trên màn hình khoảng 2–3 giây để người xem đọc được.

## Chuẩn bị trước khi quay

- Nếu vừa cập nhật mã nguồn, dừng ứng dụng bằng Ctrl+C rồi chạy dotnet run trong thư mục dự án.
- Giữ terminal mở khi quay; mở website http://localhost:5080.
- Chuẩn bị SSMS và trình sửa mã nguồn.
- Mở website bằng cửa sổ ẩn danh/riêng tư chưa có cookie để giao diện ban đầu là English.
- Kiểm tra dữ liệu trước khi quay. Các giá trị Phở bò / Main courses / 65000 bên dưới áp dụng cho dữ liệu mẫu; nếu đã sửa thì chọn món đang có và điều chỉnh bộ lọc cho khớp.
- Chọn tên món demo chưa tồn tại: Demo BIT240194 (hoặc thêm hậu tố 02).
- Chuẩn bị /images/pho.svg và /images/rice.svg để minh họa ảnh. Nếu dùng link Internet, mở thử link trực tiếp tới ảnh trước khi quay.

Câu mở đầu: “Em là Hoàng Minh Quân, MSSV BIT240194. Đây là bài giữa kỳ quản lý món ăn bằng ASP.NET Core MVC, Entity Framework Core và SQL Server.”

## 1. Database, bảng, cột và Model có MSSV — khoảng 1 phút

1. Trong SSMS, kết nối Windows Authentication đến (localdb)\MSSQLLocalDB.
2. Mở database BIT240194_MID, hiển thị thư mục Tables.
3. Mở database/verify.sql. Chạy thêm câu sau để tên database xuất hiện rõ trong kết quả:

```sql
USE [BIT240194_MID];
SELECT DB_NAME() AS DatabaseName;
```

4. Chạy verify.sql và phóng to các kết quả:
   - BIT240194_DishCategories, BIT240194_Dishes, BIT240194_DishImages.
   - Các cột BIT240194_Id, BIT240194_Name, BIT240194_Price... của cả ba bảng.
   - ColumnsWithoutStudentPrefix = 0.
5. Mở Models/FoodModels.cs trong trình sửa mã, chỉ rõ ba lớp:
   - DishCategory_BIT240194.
   - Dish_BIT240194.
   - DishImage_BIT240194.

Lời nói: “Database, cả ba bảng và toàn bộ cột SQL có tiền tố BIT240194; ba Model có MSSV trong tên lớp.”

## 2. Danh sách, tra cứu, lọc, sắp xếp và giữ điều kiện — khoảng 1–1,5 phút

1. Mở trang danh sách trong cửa sổ riêng tư. Cho thấy giao diện ban đầu là English.
2. Hiển thị danh sách món; chọn Categories để hiển thị danh sách loại rồi về Dishes.
3. Với dữ liệu mẫu, điền đồng thời:
   - Search dishes: Ph.
   - Category: Main courses.
   - Status: Available.
   - Min price: 60000.
   - Max price: 70000.
   - Sort by: Price: high to low.
4. Nhấn Apply filters. Chỉ còn Phở bò; chỉ vào các ô vẫn giữ giá trị vừa nhập.
5. Nhấn F5. Các điều kiện và kết quả vẫn còn.
6. Nhấn Reset. Lần lượt chọn Price: low to high, Price: high to low, Preparation: fastest first; nhấn Apply mỗi lần để thấy thứ tự thay đổi.

Lời nói: “Các điều kiện tìm kiếm được kết hợp trong một truy vấn. Sau khi gửi form và tải lại trang, điều kiện lọc vẫn được giữ.”

## 3. Phân loại thời gian và Create/Edit — khoảng 1 phút

1. Chọn New dish.
2. Để Name trống; điền Price = 45000, Preparation time = 15; chọn Category = Main courses.
3. Nhấn Create để hiện thông báo “This field is required.”
4. Nhập Name = Demo BIT240194, giữ các giá trị trên và Available for sale được chọn. Để URL ảnh đại diện trống.
5. Nhấn Create. Trang chi tiết hiển thị Quick vì thời gian là 15 phút và No images yet vì chưa có ảnh.
6. Chọn Edit dish; sửa thời gian thành 16, bỏ chọn Available for sale, nhấn Save changes.
7. Chỉ ra Regular và Unavailable trên trang chi tiết.

Lời nói: “Thời gian nhỏ hơn hoặc bằng 15 phút là Quick; trên 15 phút là Regular. Phân loại được tính khi hiển thị, không lưu thêm cột SQL.”

Ghi chú: các ô số có giới hạn của trình duyệt. Dùng tên trống cùng các số hợp lệ để quay rõ validation do ứng dụng xử lý theo ngôn ngữ.

## 4. Thêm ảnh, nhiều ảnh và đổi ảnh đại diện — khoảng 1 phút

1. Ở Details của món Demo BIT240194, nhấn Add image by URL.
2. Nhập /images/pho.svg; chọn Cover image; nhấn Add image.
3. Nhập /images/rice.svg; bỏ chọn Cover image; nhấn Add image.
4. Cho thấy hai ảnh cùng hiển thị và chỉ ảnh đầu được ghi Cover image.
5. Ở ảnh thứ hai, nhấn Use as cover. Chỉ ảnh thứ hai được ghi Cover image.
6. Về danh sách. Món Demo BIT240194 sử dụng ảnh đại diện vừa chọn.
7. Nếu minh họa phần bổ sung: vào Edit dish, thay Cover image URL bằng /images/dessert.svg hoặc link ảnh HTTP/HTTPS đã chuẩn bị rồi lưu.

Lời nói: “Mỗi món có nhiều ảnh nhưng tối đa một ảnh đại diện. Khi đổi ảnh đại diện, ảnh ở danh sách cũng được cập nhật.”

Món demo đã ngừng bán vẫn xuất hiện khi danh sách dùng All statuses. Có thể chọn Available để minh họa món này bị loại khỏi kết quả.

## 5. Chặn xóa loại đang dùng và ID không tồn tại — khoảng 40–60 giây

1. Mở Categories, chọn một loại có số món lớn hơn 0 (ví dụ Main courses).
2. Chọn Delete trong danh sách, rồi nhấn Delete ở trang xác nhận.
3. Giữ thông báo “This category is used by dishes and cannot be deleted.” trên màn hình.
4. Cancel để về danh sách, cho thấy loại vẫn còn.
5. Trên thanh địa chỉ, truy cập:
   - http://localhost:5080/Dishes/Details/999999
   - http://localhost:5080/Categories/Edit/999999
   - http://localhost:5080/Dishes/EditImage/999999
6. Mỗi trang hiển thị lỗi 404 và lời giải thích dữ liệu không tồn tại. Dùng Back to dishes để tiếp tục.

Lời nói: “Ứng dụng chặn xóa loại đang có món sử dụng. Các ID không tồn tại được xử lý bằng trang 404 thân thiện.”

## 6. English mặc định, Tiếng Việt, validation và giữ ngôn ngữ — khoảng 1–1,5 phút

Giao diện mặc định English đã được chứng minh ở bước 2 bằng cửa sổ riêng tư chưa có cookie.

1. Ở trang Dishes, nhập lại bộ lọc ví dụ ở bước 2 và nhấn Apply filters.
2. Nhấn Tiếng Việt ngay trên trang đang lọc.
3. Cho thấy các điều kiện và số kết quả vẫn giữ; các nhãn đã đổi sang tiếng Việt:
   - Main courses → Món chính.
   - Starters → Khai vị.
   - Desserts → Tráng miệng.
   - Available/Unavailable → Đang bán/Ngừng bán.
   - Quick/Regular → Nhanh/Thông thường.
4. Chọn Thêm món. Để Tên trống, nhập giá 45000, thời gian 15, chọn Món chính; nhấn Thêm mới.
5. Giữ thông báo “Trường này không được để trống.” trên màn hình.
6. Nhấn Hủy, mở Chi tiết của món Demo BIT240194, mở Sửa món ăn rồi về danh sách. Các trang vẫn dùng tiếng Việt.
7. Ở danh sách, nhập giá từ 70000 và giá đến 10000, nhấn Lọc kết quả để hiện thông báo khoảng giá không hợp lệ bằng tiếng Việt.
8. Nhấn English; cho thấy nhãn và thông báo lỗi ở trang danh sách đổi lại tiếng Anh.

Lời nói: “English là ngôn ngữ mặc định. Sau khi đổi sang tiếng Việt, giao diện, trạng thái, loại món và thông báo validation được dịch; ngôn ngữ được giữ khi điều hướng.”

## Kết thúc

“Em đã trình bày quy tắc đặt tên theo MSSV, tìm kiếm và lọc, CRUD, phân loại thời gian, quản lý ảnh, các ràng buộc dữ liệu và giao diện hai ngôn ngữ.”

Dừng quay, xem lại video để chắc chắn chữ, thao tác và thông báo đọc được. Nộp video cùng mã nguồn lên EDUX theo đề.

## Các ý nên nắm để giải thích khi giảng viên hỏi

- DishesController.Index dùng IQueryable; Where/OrderBy được SQL Server thực hiện trước ToListAsync.
- FoodDbContext ánh xạ tên cột thành BIT240194_ + tên thuộc tính C#.
- Quan hệ loại → món là một-nhiều, món → ảnh là một-nhiều.
- Unique index (DishCategoryId, Name) chặn trùng tên món trong cùng loại.
- Filtered unique index IsThumbnail = 1 đảm bảo tối đa một ảnh đại diện.
- FK Restrict chặn xóa loại có món; transaction giữ việc đổi ảnh đại diện nhất quán.
- Cookie ngôn ngữ được đọc bởi CookieRequestCultureProvider.
- Form đổi ngôn ngữ mang URL hiện tại kèm query string, nên các điều kiện lọc được giữ.
