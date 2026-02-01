namespace Shoe.Enum
{
    public enum Status
    {
        //Trạng thái chờ thanh toán VNPAY (Trước khi gọi API/Nhận kết quả)
        Pending,
        //Trạng thái thất bại (cho giao dịch VNPAY thất bại)
        Failed,

        InProgress,
        Confirmed,
        Shipping,
        Success,
        Canceled

    }
}