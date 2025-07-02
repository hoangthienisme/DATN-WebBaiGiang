namespace WebBaiGiang.ViewModel
{
    public class SinhVienVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
