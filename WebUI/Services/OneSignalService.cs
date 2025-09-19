using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WebUI.Models
{
    public class OneSignalService
    {
        private readonly string _appId = "fb64b473-8257-48aa-85ee-78661fcbb163";
        private readonly string _restApiKey = "os_v2_app_7nsli44ck5ekvbpopbtb7s5rmmlr3yx7vv7umcfxyszdn2hknc63jtxe2rl6hw4dkca5dwcxzk5jdeio2rutiv7a2trqwdy653aifjiY";

        public async Task SendNotification(string playerId, string message)
        {
            if (string.IsNullOrEmpty(playerId)) return;

            var payload = new
            {
                app_id = _appId,
                include_player_ids = new string[] { playerId },
                headings = new { en = "DHA Gujranwala" },
                contents = new { en = message }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", _restApiKey
                );

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://onesignal.com/api/v1/notifications", content);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
        }
    }
}
