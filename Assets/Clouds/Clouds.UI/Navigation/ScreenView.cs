namespace Clouds.UI
{
    /// <summary>
    /// Màn hình — nằm trong ScreenLayer, xếp chồng có lịch sử. Push đè màn mới lên (màn cũ Hide rồi tắt
    /// nhưng vẫn giữ trong lịch sử), Pop bỏ màn trên cùng và Show lại màn ngay dưới.
    /// Bị đè hay được lộ ra lại cũng đi qua đúng các hook Show/Hide của UIView.
    /// </summary>
    public class ScreenView : UIView
    {
    }
}
