# Kết quả kiểm tra

Ngày: 14/10/2026. Sinh viên: Hoàng Minh Quân — BIT240194.

- dotnet build --no-restore: thành công, 0 lỗi.
- scripts/smoke-tests.ps1: 58/58 kiểm tra tích hợp HTTP thành công trên SQL Server LocalDB.
- scripts/cover-url-tests.ps1: 29/29 kiểm tra URL ảnh trực tiếp trên form Thêm/Sửa món thành công; tổng 87 kiểm tra.
- Bản cập nhật có ô URL ảnh đại diện và ảnh hiện tại ngay dưới Mô tả. Hỗ trợ link HTTP/HTTPS trên mạng, URL trống giữ ảnh cũ, URL sai không lưu thay đổi món/ảnh. Giữ ID ảnh đại diện và các ảnh phụ.
- Kiểm thử ở cổng 5081; phiên người dùng trên 5080 được giữ nguyên.
- SQL trực tiếp: database BIT240194_MID có đúng ba bảng ứng dụng.
- Cả 14 cột đều có tiền tố BIT240194_; số cột không đúng tiền tố = 0.
- Có unique index cho tên món trong cùng loại và filtered unique index cho ảnh đại diện.
- Có hai check constraints Price > 0 và PreparationTime > 0.
- Dữ liệu mẫu: 3 loại, 5 món, 2 ảnh/món.
- Truy vấn kiểm tra món có > 1 ảnh đại diện: 0 dòng.
- Trang danh sách đã được mở và kiểm tra giao diện trong trình duyệt của Codex.
- Validation, giữ bộ lọc khi đổi ngôn ngữ, ngôn ngữ qua điều hướng, số thập phân tiếng Việt,
  CRUD món/loại, thêm ảnh qua URL HTTP tuyệt đối, sửa URL ảnh, Id không tồn tại và antiforgery được kiểm thử bằng HTTP.

Giới hạn: NuGet báo NU1900 vì không lấy được dữ liệu kiểm tra lỗ hổng từ api.nuget.org.
Các gói cần dùng đã có trên máy; restore/build thành công. Video demo chưa được quay;
DEMO.md chứa kịch bản quay theo đề. Chưa thực hiện nộp EDUX.

- Cập nhật sửa URL ảnh: kiểm thử URL hợp lệ/sai/trống/thiếu, giữ ảnh đại diện và món liên kết, dữ liệu không đổi khi validation lỗi, UI/validation tiếng Việt, GET/POST Id không tồn tại. Bản cập nhật được thử với named pipe của cùng LocalDB khi API tìm phiên LocalDB báo lỗi khởi động.

- Bản dịch loại món: Main courses / Món chính, Starters / Khai vị, Desserts / Tráng miệng được kiểm chứng ở bộ lọc, thẻ món, dropdown Create/Edit, trang Details, danh sách/xác nhận xóa loại. ID bộ lọc và tên gốc khi sửa loại được giữ đúng.
