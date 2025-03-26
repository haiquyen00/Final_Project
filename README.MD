# SP25_PRN222_DE170075

## Mô tả dự án
Dự án này là một ứng dụng quản lý thư viện được xây dựng bằng ASP.NET Core. Ứng dụng cung cấp các chức năng quản lý sách, người dùng, và các bản ghi mượn trả sách. Dự án sử dụng kiến trúc phân lớp với các thành phần chính:

- **Business**: Chứa các mô hình dữ liệu và logic nghiệp vụ.
- **DataAccess**: Chứa các lớp DAO để truy cập cơ sở dữ liệu.
- **Repositories**: Chứa các lớp Repository để giao tiếp giữa tầng Business và DataAccess.
- **FUView**: Chứa giao diện người dùng và các controller.

## Công nghệ sử dụng
- **ASP.NET Core 8.0**: Framework chính để xây dựng ứng dụng web.
- **Entity Framework Core**: ORM để làm việc với cơ sở dữ liệu SQL Server.
- **Microsoft SQL Server**: Cơ sở dữ liệu chính của ứng dụng.
- **Bootstrap**: Framework CSS để xây dựng giao diện người dùng.
- **Font Awesome**: Thư viện biểu tượng để cải thiện giao diện.
- **Cookie Authentication**: Xác thực người dùng dựa trên cookie.

## Cấu trúc dự án
- **Business**: Chứa các mô hình như `Books`, `Users`, `BorrowRecords`, và các migration.
- **DataAccess**: Chứa các lớp DAO như `BookDAO`, `AuthenticationDAO`, và `BorrowRecordDAO`.
- **Repositories**: Chứa các lớp Repository để xử lý logic nghiệp vụ.
- **FUView**: Chứa giao diện người dùng với các view như `Home`, `Books`, và `BorrowRecords`.

## Hướng dẫn cài đặt
1. Clone dự án:
   ```bash
   git clone <repository-url>
   ```
2. Cài đặt các gói NuGet:
   ```bash
   dotnet restore
   ```
3. Cấu hình chuỗi kết nối trong `appsettings.json`:
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=LibraryDB;Trusted_Connection=True;"
   }
   ```
4. Chạy migration để tạo cơ sở dữ liệu:
   ```bash
   dotnet ef database update --project Business
   ```
5. Chạy ứng dụng:
   ```bash
   dotnet run --project FUView
   ```

## Các chức năng hiện tại
- Quản lý sách: Thêm, sửa, xóa, tìm kiếm sách.
- Quản lý người dùng: Đăng ký, đăng nhập, phân quyền (Admin, User).
- Quản lý mượn trả sách: Theo dõi các bản ghi mượn trả.

## Đề xuất cải tiến
### Chức năng mới
1. **Quản lý danh mục sách**:
   - Thêm bảng `Categories` để phân loại sách.
   - Mỗi sách có thể thuộc một hoặc nhiều danh mục.

2. **Hệ thống thông báo**:
   - Gửi email nhắc nhở người dùng trả sách trước hạn.
   - Hiển thị thông báo trong giao diện người dùng.

3. **Báo cáo và thống kê**:
   - Thống kê số lượng sách mượn theo tháng.
   - Báo cáo sách được mượn nhiều nhất.

4. **Tích hợp API bên thứ ba**:
   - Tích hợp API để lấy thông tin sách từ các nguồn như Google Books.

### Cải tiến cơ sở dữ liệu
1. **Thêm bảng `Categories`**:
   ```sql
   CREATE TABLE Categories (
       ID INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL
   );

   ALTER TABLE Books ADD CategoryID INT FOREIGN KEY REFERENCES Categories(ID);
   ```

2. **Thêm bảng `Notifications`**:
   ```sql
   CREATE TABLE Notifications (
       ID INT PRIMARY KEY IDENTITY,
       UserID INT FOREIGN KEY REFERENCES Users(Id),
       Message NVARCHAR(255) NOT NULL,
       CreatedAt DATETIME DEFAULT GETDATE(),
       IsRead BIT DEFAULT 0
   );
   ```

3. **Cải thiện bảng `BorrowRecords`**:
   - Thêm cột `Fine` để lưu phí phạt nếu trả sách muộn.
   ```sql
   ALTER TABLE BorrowRecords ADD Fine DECIMAL(10, 2) DEFAULT 0;
   ```

## Đóng góp
Nếu bạn muốn đóng góp cho dự án, vui lòng tạo một pull request hoặc mở issue để thảo luận.

## Liên hệ
- **Email**: support@example.com
- **Website**: [example.com](https://example.com)