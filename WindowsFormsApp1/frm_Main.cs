using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class frm_Main : Form
    {
        DatabaseDataContext db = new DatabaseDataContext();

        public frm_Main()
        {
            InitializeComponent();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            dgv_sinhvien.DataSource = db.tbl_sinhviens.ToList();
        }

        private void dgv_sinhvien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Bắt lỗi người dùng lỡ bấm vào dòng Tiêu đề (header) của cột
            if (e.RowIndex >= 0)
            {
                // Lấy cái dòng (row) mà người dùng vừa click vào
                DataGridViewRow row = dgv_sinhvien.Rows[e.RowIndex];

                // 1. Cột 0: MSSV
                // Dùng ?.ToString() thay vì .ToString() để tránh bị văng app nếu cột trên SQL có chứa ô Trống (Null)
                txt_mssv.Text = row.Cells[0].Value?.ToString();

                // 2. Cột 1: Họ và tên
                txt_hoten.Text = row.Cells[1].Value?.ToString();

                // 3. Cột 2: Giới tính
                cbx_gioitinh.Text = row.Cells[2].Value?.ToString();

                // 4. Cột 3: Ngày sinh
                if (row.Cells[3].Value != null && row.Cells[3].Value.ToString() != "")
                {
                    dtp_ngaysinh.Value = Convert.ToDateTime(row.Cells[3].Value);
                }

                // 5. Cột 4: Quê quán
                txt_quequan.Text = row.Cells[4].Value?.ToString();

                // 6. Cột 5: Mã lớp
                txt_malop.Text = row.Cells[5].Value?.ToString();
            }
        }

        private void btn_them_Click(object sender, EventArgs e)
        {
            // 1. KIỂM TRA ĐIỀU KIỆN (Tránh nhập rỗng)
            if (txt_mssv.Text.Trim() == "" || txt_hoten.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tối thiểu Mã Sinh Viên và Họ Tên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_mssv.Focus();
                return; // Dừng lại luôn không chạy các dòng code bên dưới
            }

            try
            {
                // 2. Kiểm tra xem Mã SV có bị Trùng chưa? (Mssv là khóa chính)
                var kiemTraTonTai = db.tbl_sinhviens.FirstOrDefault(s => s.mssv == txt_mssv.Text.Trim());
                if (kiemTraTonTai != null)
                {
                    MessageBox.Show("Mã Sinh Viên này đã tồn tại rồi! Xin nhập mã khác.", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. Tiến hành gói data (Object) vào khuôn của Bảng dbml
                tbl_sinhvien svMoi = new tbl_sinhvien();
                svMoi.mssv = txt_mssv.Text.Trim(); // .Trim() để gọt bỏ dấu cách thừa hai bên do ng dùng lỡ bấm
                svMoi.hoten = txt_hoten.Text.Trim();
                svMoi.gioitinh = cbx_gioitinh.Text;
                svMoi.ngaysinh = dtp_ngaysinh.Value;
                svMoi.quequan = txt_quequan.Text.Trim();
                svMoi.malop = txt_malop.Text.Trim();

                // 4. Nhét Sinh viên trên vào Database LINQ 
                db.tbl_sinhviens.InsertOnSubmit(svMoi); // Sắp xếp hồ sơ gửi lên
                db.SubmitChanges(); // MỆNH LỆNH ĐÓNG DẤU CHẠY SANG CSDL SQL SERVER

                MessageBox.Show("Thêm Sinh Viên Mới Thành Công!", "Chúc Mừng", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dgv_sinhvien.DataSource = db.tbl_sinhviens.ToList();
                btn_lammoi_Click(sender, e); // Lệnh dọn dẹp sạch ổ nháp thông tin (như ai vừa ấn LamMoi)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi Không thêm được:\n" + ex.Message, "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_lammoi_Click(object sender, EventArgs e)
        {
            // Xóa rỗng các Textbox
            txt_mssv.Clear();
            txt_hoten.Clear();
            txt_quequan.Clear();
            txt_malop.Clear();

            // Khôi phục Giới tính và Ngày sinh mặc định
            cbx_gioitinh.SelectedIndex = -1; // Hoặc gán = "" 
            dtp_ngaysinh.Value = DateTime.Now; // Đặt ngày mặc định là hôm nay

            // Nạp lại danh sách dữ liệu trên lưới cho chuẩn (Hàm bạn đã viết ở bước trước)
            dgv_sinhvien.DataSource = db.tbl_sinhviens.ToList();

            // Chuyển con trỏ chuột nháy nháy vào ô MSSV chờ nhập ngay lập tức
            txt_mssv.Focus();
        }

        private void btn_sua_Click(object sender, EventArgs e)
        {
            // Bắt lỗi rỗng Khóa Chính Mssv
            if (txt_mssv.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn 1 Sinh viên bên bảng danh sách trước khi nhấn Sửa!", "Hướng dẫn", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            try
            {
                // 1. Tra tay kiếm cuốn hồ sơ Sinh viên CŨ bằng MSSV từ trên Kệ DB.
                var svCanSua = db.tbl_sinhviens.FirstOrDefault(s => s.mssv == txt_mssv.Text.Trim());

                if (svCanSua == null)
                {
                    MessageBox.Show("Không kiếm được dữ liệu gốc này trong DB. Làm mới rồi chọn đúng bạn nhé!", "Lỗi tra xét");
                    return;
                }

                // 2. Chắp bút Cập nhật hồ sơ mọc vào ĐÚNG biến mà Bạn vừa mò móc tra ra! (Chỉ Không Cho ghi ĐÈ dòng Khóa MSSV)
                svCanSua.hoten = txt_hoten.Text.Trim();
                svCanSua.gioitinh = cbx_gioitinh.Text;
                svCanSua.ngaysinh = dtp_ngaysinh.Value;
                svCanSua.quequan = txt_quequan.Text.Trim();
                svCanSua.malop = txt_malop.Text.Trim();

                // 3. Tiến Hành LƯU. 
                // Khi dùng "LINQ lấy Record ra đổi Trực Tiếp Thế Này" nó tự hối đút lại ổ không cần lỉnh kỉnh Insert gì
                db.SubmitChanges();

                MessageBox.Show("Thông Tin Đã Cập Nhật Lại !", "Thông Cáo Mới Nhất", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nháy Data tự Re-F5 màn cho tươi mơn mởn.
                dgv_sinhvien.DataSource = db.tbl_sinhviens.ToList();
            }
            catch (Exception loiGopNhieu)
            {
                MessageBox.Show("Gãy! Coi Lỗi nè Bạn: " + loiGopNhieu.Message, "Hệ Thống Trở ngại Cập Nhật");
            }
        }

        private void btn_xoa_Click(object sender, EventArgs e)
        {
            // Tránh ngáo nhấp nhầm tay mờ không thâu đc gì
            if (txt_mssv.Text.Trim() == "")
            {
                MessageBox.Show("Hãy kích vô Người Muốn Thanh trừng bên Dòng Bảng Danh Sách trước ạ", "Kế sách Nhắc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Câu Hỏi Có Vấn Quyết RẤT QUAN TRỌNG KÈM Dấu (!) và hai phím YES/NO !!
            DialogResult chotKq = MessageBox.Show($"Chắn Cửa: Muốn xóa vĩnh viễn dòng [{txt_hoten.Text}] MS:[{txt_mssv.Text}] thiệt hông?", "Phân Xử Sống Chết DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (chotKq == DialogResult.Yes) // Nó bấm Có -> Thi Hành Chảm Nhất Luận!!
            {
                try
                {
                    // Trinh Nữ mò Kiếm Đích Ngồi Danh trong sổ 
                    var xoaBayBang = db.tbl_sinhviens.FirstOrDefault(d => d.mssv == txt_mssv.Text.Trim());

                    if (xoaBayBang != null)
                    {
                        // Chuyển rào thi án Xử trảm Database LINQ!  => Vâng là Lệnh: "Delete"
                        db.tbl_sinhviens.DeleteOnSubmit(xoaBayBang);
                        db.SubmitChanges(); // Cuốn vào Kí gửi

                        MessageBox.Show("Hoàn thiện xong bước tiễn biệt.. \nDữ kiện Bóc Mẻ Cỡ Khủng đã xoá !! ", "Hiểu rồi Tình Trạng Báo Cáo!!");

                        dgv_sinhvien.DataSource = db.tbl_sinhviens.ToList();
                        btn_lammoi_Click(sender, e);  // Quét trống luôn Hố Mỏ Phễu Text Bọc Trống Thẳng cẵng
                    }
                    else
                    {
                        MessageBox.Show("Đám cỏ trống: Ko rõ bị ai Quẩy Lâu Ròi (Refrest mẻ mỏi rồi hổng cần xử) !", "Phá sản !");
                    }
                }
                catch (Exception ex1) // Hay bị bắt Dính Vì Ngoại Khóa Mượn Sách Chưa Trả ..v/v Rành Vấn này nên Khống Box Nhó.
                {
                    MessageBox.Show($"Xoá DB Ko Cho !! Nguyên Cơ Cấu Báo : \n " + ex1.Message, "Phóng viên Ngoại tuyến SQL Box", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}