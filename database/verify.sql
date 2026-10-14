USE [BIT240194_MID];
GO
-- Danh sách đúng ba bảng ứng dụng.
SELECT name AS TableName FROM sys.tables ORDER BY name;
-- Tên tất cả cột vật lý.
SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS SqlType
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types ty ON ty.user_type_id=c.user_type_id
ORDER BY t.name, c.column_id;
-- Kết quả phải là 0.
SELECT COUNT(*) AS ColumnsWithoutStudentPrefix
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
WHERE t.name LIKE 'BIT240194[_]%' AND c.name NOT LIKE 'BIT240194[_]%';
-- Hai unique index; ảnh đại diện dùng điều kiện lọc.
SELECT OBJECT_NAME(object_id) AS TableName, name, is_unique, filter_definition
FROM sys.indexes WHERE is_unique=1 AND is_primary_key=0 AND OBJECT_NAME(object_id) LIKE 'BIT240194[_]%';
SELECT name, definition FROM sys.check_constraints;
-- Đếm và quan hệ dữ liệu.
SELECT c.BIT240194_Name, COUNT(d.BIT240194_Id) AS DishCount
FROM BIT240194_DishCategories c LEFT JOIN BIT240194_Dishes d
ON c.BIT240194_Id=d.BIT240194_DishCategoryId GROUP BY c.BIT240194_Name;
-- Kết quả phải rỗng: không có món có hơn một ảnh đại diện.
SELECT BIT240194_DishId, COUNT(*) AS ThumbnailCount FROM BIT240194_DishImages
WHERE BIT240194_IsThumbnail=1 GROUP BY BIT240194_DishId HAVING COUNT(*)>1;
GO
