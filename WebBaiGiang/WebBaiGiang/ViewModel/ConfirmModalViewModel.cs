namespace WebBaiGiang.ViewModel
{
    public class ConfirmModalViewModel
    {
        public string ModalId { get; set; } = "confirmModal";
        public string Message { get; set; } = "Bạn có chắc chắn?";
        public string ConfirmButtonText { get; set; } = "XÁC NHẬN";
        public string CancelButtonText { get; set; } = "HỦY";
        public string ActionUrl { get; set; } // URL để chuyển hướng hoặc submit
        public int DataId { get; set; }
        public string Mode { get; set; }
    }
}
