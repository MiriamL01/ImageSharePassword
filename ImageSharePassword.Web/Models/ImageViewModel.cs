using ImageSharePassword.Data;

namespace ImageSharePassword.Web.Models
{
    public class ImageViewModel
    {
        public Image Image { get; set; }
        public bool HasPermission { get; set; }
        public string Message { get; set; }
    }
}
