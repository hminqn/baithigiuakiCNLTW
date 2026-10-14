using System.Globalization;
namespace FoodManager_BIT240194.Services;
public class UiText
{
    public bool Vietnamese => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
    public string this[string key] => Message(key);
    public string Price(decimal value) => value.ToString(decimal.Truncate(value) == value ? "N0" : "N2");
    public static string Message(string key)
    {
        if (!Words.TryGetValue(key, out var pair)) return key;
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? pair.Vi : pair.En;
    }
    private static readonly Dictionary<string, (string En, string Vi)> Words = new()
    {
        ["App"] = ("The Kitchen", "Bếp Nhà"),
        ["Dishes"] = ("Dishes", "Món ăn"), ["Categories"] = ("Categories", "Loại món"),
        ["MenuTitle"] = ("Your menu, thoughtfully managed.", "Quản lý thực đơn của bạn."),
        ["MenuIntro"] = ("Find a dish, refine your menu, and keep every detail in one place.", "Tra cứu món ăn, cập nhật thực đơn và quản lý mọi thông tin tại một nơi."),
        ["Admin"] = ("MENU MANAGEMENT", "QUẢN LÝ THỰC ĐƠN"), ["Student"] = ("Student", "Sinh viên"),
        ["NewDish"] = ("New dish", "Thêm món"), ["CreateDish"] = ("Create a dish", "Thêm món ăn"),
        ["EditDish"] = ("Edit dish", "Sửa món ăn"), ["Details"] = ("Details", "Chi tiết"),
        ["Name"] = ("Name", "Tên"), ["Price"] = ("Price (VND)", "Giá (VNĐ)"),
        ["PreparationTime"] = ("Preparation time (minutes)", "Thời gian chế biến (phút)"),
        ["Description"] = ("Description", "Mô tả"), ["Category"] = ("Category", "Loại món"),
        ["IsAvailable"] = ("Available for sale", "Đang bán"), ["Available"] = ("Available", "Đang bán"),
        ["Unavailable"] = ("Unavailable", "Ngừng bán"), ["Quick"] = ("Quick", "Nhanh"),
        ["Regular"] = ("Regular", "Thông thường"), ["Minutes"] = ("min", "phút"),
        ["Search"] = ("Search dishes", "Tìm món ăn"), ["SearchPlaceholder"] = ("Search by name…", "Nhập tên món…"),
        ["AllCategories"] = ("All categories", "Tất cả loại"), ["AllStatuses"] = ("All statuses", "Tất cả trạng thái"),
        ["Status"] = ("Status", "Trạng thái"), ["MinPrice"] = ("Min price", "Giá từ"), ["MaxPrice"] = ("Max price", "Giá đến"),
        ["Sort"] = ("Sort by", "Sắp xếp"), ["NameSort"] = ("Name", "Tên món"),
        ["PriceAsc"] = ("Price: low to high", "Giá tăng dần"), ["PriceDesc"] = ("Price: high to low", "Giá giảm dần"),
        ["TimeAsc"] = ("Preparation: fastest first", "Thời gian chế biến tăng dần"),
        ["Apply"] = ("Apply filters", "Lọc kết quả"), ["Reset"] = ("Reset", "Đặt lại"),
        ["Results"] = ("dishes found", "món ăn tìm thấy"), ["Empty"] = ("No dishes found", "Không tìm thấy món ăn"),
        ["EmptyHint"] = ("Try another name or adjust your filters.", "Thử tên khác hoặc điều chỉnh bộ lọc."),
        ["Save"] = ("Save changes", "Lưu thay đổi"), ["Create"] = ("Create", "Thêm mới"), ["Cancel"] = ("Cancel", "Hủy"),
        ["Edit"] = ("Edit", "Sửa"), ["Delete"] = ("Delete", "Xóa"), ["Back"] = ("Back to dishes", "Về danh sách món"),
        ["Required"] = ("This field is required.", "Trường này không được để trống."),
        ["NameLength"] = ("Use no more than 120 characters.", "Nhập tối đa 120 ký tự."),
        ["DescriptionLength"] = ("Use no more than 2,000 characters.", "Nhập tối đa 2.000 ký tự."),
        ["PositivePrice"] = ("Price must be greater than zero (up to 999,999,999 VND, at most 2 decimal places).", "Giá phải lớn hơn 0 (tối đa 999.999.999 VNĐ, tối đa 2 chữ số thập phân)."),
        ["PositiveTime"] = ("Preparation time must be a positive whole number.", "Thời gian chế biến phải là số nguyên lớn hơn 0."),
        ["InvalidCategory"] = ("The selected category does not exist.", "Loại món đã chọn không tồn tại."),
        ["DuplicateDish"] = ("A dish with this name already exists in this category.", "Tên món đã tồn tại trong cùng loại món."),
        ["InvalidNumber"] = ("Enter a valid number.", "Vui lòng nhập số hợp lệ."),
        ["InvalidValue"] = ("The entered value is invalid.", "Giá trị nhập không hợp lệ."),
        ["InvalidRange"] = ("Prices must be non-negative and the minimum must not exceed the maximum.", "Giá phải không âm và giá từ không được lớn hơn giá đến."),
        ["InvalidSort"] = ("The selected sort option is invalid.", "Tùy chọn sắp xếp không hợp lệ."),
        ["InvalidStatus"] = ("The selected status is invalid.", "Trạng thái đã chọn không hợp lệ."),
        ["Images"] = ("Dish images", "Ảnh món ăn"), ["ImageUrl"] = ("Image URL", "Đường dẫn ảnh"),
        ["ImageHint"] = ("Use an HTTPS/HTTP image URL or a local path such as /images/pho.svg.", "Nhập URL ảnh HTTPS/HTTP hoặc đường dẫn nội bộ như /images/pho.svg."),
        ["CoverImageUrl"] = ("Cover image URL", "URL ảnh đại diện"),
        ["CoverImageUrlHint"] = ("Paste a direct HTTP/HTTPS link to an image on the Internet, or a local /images/ path. Leave blank to keep the current image.", "Dán link trực tiếp đến ảnh trên mạng (HTTP/HTTPS), hoặc đường dẫn /images/ nội bộ. Để trống để giữ ảnh hiện tại."),
        ["InvalidUrl"] = ("Enter an HTTP/HTTPS URL or a local /images/ path.", "Nhập URL HTTP/HTTPS hoặc đường dẫn /images/ nội bộ."),
        ["Thumbnail"] = ("Cover image", "Ảnh đại diện"), ["SetThumbnail"] = ("Use as cover", "Chọn làm đại diện"),
        ["AddImage"] = ("Add image", "Thêm ảnh"), ["NoImages"] = ("No images yet", "Chưa có ảnh"),
        ["AddImageUrl"] = ("Add image by URL", "Thêm ảnh bằng URL"),
        ["EditImageUrl"] = ("Edit image URL", "Sửa ảnh bằng URL"),
        ["ImageUpdated"] = ("Image URL updated.", "Đã cập nhật URL ảnh."),
        ["CurrentImage"] = ("Current image", "Ảnh hiện tại"),
        ["BackToDish"] = ("Back to dish details", "Về chi tiết món ăn"),
        ["EditImageHint"] = ("Replace the image URL below. The image keeps its current cover status.", "Thay đường dẫn ảnh bên dưới. Ảnh sẽ giữ trạng thái đại diện hiện tại."),
        ["NoImagesHint"] = ("Add the first image using the form below.", "Thêm ảnh đầu tiên bằng biểu mẫu bên dưới."),
        ["ImageUnavailable"] = ("Image unavailable", "Không tải được ảnh"),
        ["NewCategory"] = ("New category", "Thêm loại món"), ["EditCategory"] = ("Edit category", "Sửa loại món"),
        ["CategoryTitle"] = ("A place for every dish.", "Phân loại cho từng món ăn."),
        ["CategoryIntro"] = ("Organise your menu into clear, useful categories.", "Sắp xếp thực đơn thành các loại món rõ ràng."),
        ["DishCount"] = ("Dishes", "Số món"), ["CategoryInUse"] = ("This category is used by dishes and cannot be deleted.", "Không thể xóa loại món đang được sử dụng."),
        ["EmptyCategories"] = ("No categories yet.", "Chưa có loại món."),
        ["Saved"] = ("Changes saved.", "Đã lưu thay đổi."), ["Deleted"] = ("Deleted successfully.", "Đã xóa thành công."),
        ["ImageAdded"] = ("Image added.", "Đã thêm ảnh."), ["CoverChanged"] = ("Cover image updated.", "Đã đổi ảnh đại diện."),
        ["SaveError"] = ("Could not save. The data may have changed; check your input and try again.", "Không thể lưu. Dữ liệu có thể đã thay đổi; kiểm tra và thử lại."),
        ["NotFoundTitle"] = ("We couldn't find that item.", "Không tìm thấy dữ liệu."),
        ["NotFoundHint"] = ("The requested dish, category or image does not exist.", "Món ăn, loại món hoặc ảnh được yêu cầu không tồn tại."),
        ["ErrorTitle"] = ("Something went wrong.", "Đã xảy ra lỗi."),
        ["ErrorHint"] = ("Please return to the menu and try again.", "Vui lòng về danh sách và thử lại."),
        ["RequestError"] = ("The request could not be processed.", "Không thể xử lý yêu cầu."),
        ["DeleteDishTitle"] = ("Delete this dish?", "Xóa món ăn này?"),
        ["DeleteDishHint"] = ("The dish and all its images will be removed.", "Món ăn và tất cả ảnh của món sẽ bị xóa."),
        ["DeleteCategoryTitle"] = ("Delete this category?", "Xóa loại món này?"),
        ["DeleteImageConfirm"] = ("Delete this image?", "Xóa ảnh này?"),
        ["FormIntro"] = ("Good food starts with the details. Fill in the essentials below.", "Hoàn thiện thông tin món ăn trong biểu mẫu bên dưới."),
        ["SelectCategory"] = ("Choose a category", "Chọn loại món"),
        ["ManageImagesHint"] = ("Manage additional images on the dish detail page after saving.", "Bạn có thể quản lý thêm các ảnh phụ ở trang chi tiết món sau khi lưu."),
        ["Footer"] = ("Midterm project · ASP.NET Core MVC · Entity Framework Core", "Bài giữa kỳ · ASP.NET Core MVC · Entity Framework Core")
    };
}
