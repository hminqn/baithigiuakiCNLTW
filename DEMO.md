# Kịch bản demo — Hoàng Minh Quân, BIT240194

Video đề xuất khoảng 5–7 phút. Chuẩn bị ứng dụng ở http://localhost:5080 và SSMS.
Đọc hiểu các phần mã trước khi demo vì giảng viên có thể yêu cầu giải thích/sửa trực tiếp.

## 1. Chứng minh tên theo MSSV (40–60 giây)

- Kết nối SSMS đến `(localdb)\MSSQLLocalDB`, mở database BIT240194_MID.
- Chạy database/verify.sql, hiển thị đúng 3 bảng và toàn bộ cột đều có tiền tố BIT240194.
- Mở Models/FoodModels.cs: Dish_BIT240194, DishCategory_BIT240194, DishImage_BIT240194.
- Mở FoodDbContext.cs: vòng lặp SetColumnName, quan hệ FK và index.
- Trỏ phần unique index tên món theo loại và filtered index tối đa một ảnh đại diện.

## 2. English mặc định và danh sách (40 giây)

- Dùng cửa sổ riêng tư hoặc xóa cookie ngôn ngữ trước lần truy cập.
- Hiển thị menu và toàn bộ nhãn giao diện English.
- Danh sách có 5 món, 3 loại, có cả Available và Unavailable.
- Chỉ ra món 15 phút = Quick, món 20/25 phút = Regular.
- Mở Categories để xem danh sách loại.

## 3. Tìm/lọc/sắp xếp kết hợp (60 giây)

- Tên: Ph; loại: Main courses; trạng thái: Available; giá từ 60000 đến 70000; sort Price high to low.
- Apply filters: chỉ còn Phở bò; các điều kiện vẫn hiển thị.
- Chọn Tiếng Việt: bộ lọc, nút, tiêu đề được dịch, điều kiện và kết quả vẫn giữ.
- Reset, chọn giá tăng rồi giảm, sau đó thời gian tăng để thấy thứ tự thay đổi.
- Chọn Available: Chả giò đang ngừng bán không xuất hiện.
- Nhập tên không tồn tại: trạng thái không có kết quả.
- Nhập giá từ 70000, giá đến 10000: thông báo khoảng giá sai.

## 4. Create/Edit và validation (60–90 giây)

- Thêm món, để trống tên; giá = 0, thời gian = 0, chưa chọn loại. Submit để xem lỗi tiếng Việt.
- Nhập món hợp lệ, chọn loại từ database, thời gian 15, giá 45000, lưu.
- Trang chi tiết thể hiện Nhanh và trạng thái chưa có ảnh.
- Sửa thời gian thành 16 và bỏ chọn Đang bán: chi tiết chuyển Thông thường/Ngừng bán.
- Trong cùng trang Sửa món, dán link ảnh HTTP/HTTPS vào ô URL ảnh đại diện rồi lưu. Về danh sách và chi tiết để thấy ảnh mới.
- Thử thêm tên món đã có trong cùng loại để thấy lỗi trùng tên.
- Đổi English ở trang Edit rồi sang Details/List, ngôn ngữ tiếp tục English.

## 5. Ảnh và ảnh đại diện (60 giây)

- Vào chi tiết món vừa tạo; thêm /images/pho.svg, chọn Cover image.
- Thêm /images/rice.svg làm ảnh phụ.
- Hai ảnh cùng hiển thị; chọn Use as cover cho ảnh thứ hai.
- Chọn Edit image URL / Sửa ảnh bằng URL, thay URL bằng /images/dessert.svg và lưu. Ảnh thay đổi, trạng thái đại diện giữ nguyên.
- Có thể thêm hoặc sửa bằng URL đầy đủ HTTP/HTTPS trỏ trực tiếp đến ảnh.
- Chỉ có một ảnh ghi Cover image; về List thấy ảnh đại diện vừa chọn.
- Thử ImageUrl = javascript:alert(1): thông báo URL không hợp lệ.
- Ảnh mẫu dùng đường dẫn nội bộ; cũng có thể thêm URL ảnh HTTP/HTTPS hợp lệ.

## 6. Ràng buộc và ID không tồn tại (40 giây)

- Categories → Delete loại đang có món → nhấn Delete: không xóa, có thông báo.
- Truy cập /Dishes/Details/999999: trang 404 thân thiện.
- Truy cập /Categories/Edit/999999: trang 404.
- Có thể mở script smoke-tests.ps1 và kết quả để chứng minh xử lý DishImage Id không tồn tại.
- Kết thúc bằng danh sách món và tên Hoàng Minh Quân / BIT240194 ở cuối trang.

## Các ý cần giải thích

1. Vì sao lọc ở database? Biến query là IQueryable; Where/OrderBy tạo SQL, chỉ ToListAsync ở cuối mới lấy dữ liệu.
2. Vì sao không lưu Quick/Regular? Đây là dữ liệu suy ra từ PreparationTime, tính trực tiếp ở Razor.
3. Vì sao dùng filtered unique index? Chỉ index các ảnh IsThumbnail=true, cho phép nhiều ảnh phụ nhưng tối đa một ảnh đại diện.
4. Vì sao cần transaction khi đổi ảnh? Bỏ ảnh cũ và đặt ảnh mới phải cùng thành công, nếu lỗi thì rollback.
5. Vì sao không xóa được loại có món? Controller kiểm tra và database FK Restrict cũng chặn.
6. Ngôn ngữ được giữ như thế nào? CookieRequestCultureProvider đọc cookie; form đổi ngôn ngữ mang returnUrl gồm query string.
7. Cột SQL vẫn có tiền tố dù thuộc tính C# không có? Fluent API ánh xạ tất cả thuộc tính vật lý sang BIT240194_ + tên thuộc tính.
