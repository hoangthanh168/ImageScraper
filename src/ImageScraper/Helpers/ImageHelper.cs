using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageScraper.Helpers
{
    public class ImageHelper
    {
        public static async Task<bool> IsImageURLBlocked(string imageUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

                    // Kiểm tra mã trạng thái của phản hồi
                    // Nếu mã trạng thái là 200 (OK), URL không bị chặn
                    // Nếu mã trạng thái là 403 (Forbidden) hoặc 451 (Unavailable For Legal Reasons), URL có thể bị chặn
                    // Đối với các mã trạng thái khác, bạn có thể xem xét thêm tùy thuộc vào yêu cầu của bạn
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}