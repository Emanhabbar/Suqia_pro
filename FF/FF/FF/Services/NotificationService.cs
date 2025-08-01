using FirebaseAdmin.Messaging;
using System.Threading.Tasks;

namespace FF.Services 
{
    public class NotificationService
    {
        public async Task SendNotification(string deviceToken, string title, string body)
        {
            if (string.IsNullOrEmpty(deviceToken))
            {
                // لا يمكن إرسال إشعار بدون رمز جهاز
                return;
            }

            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Token = deviceToken,
            };

            // إرسال الرسالة
            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // يمكنك هنا تسجيل الرد لأغراض تصحيح الأخطاء
                // Console.WriteLine($"Successfully sent message: {response}");
            }
            catch (FirebaseMessagingException ex)
            {
                // تسجيل الأخطاء إذا فشل الإرسال
                // Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}