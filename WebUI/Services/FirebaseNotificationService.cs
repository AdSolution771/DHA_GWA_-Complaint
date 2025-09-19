using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

public class FirebaseNotificationService
{
    private readonly string _projectId;
    private readonly string _serviceAccountFile;

    public FirebaseNotificationService(IConfiguration config)
    {
        _projectId = config["Firebase:ProjectId"];
        _serviceAccountFile = config["Firebase:ServiceAccountFile"];
    }

    private async Task<string> GetAccessTokenAsync()
    {
        GoogleCredential credential;
        using (var stream = new FileStream(_serviceAccountFile, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
        }

        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
    {
        var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
        var accessToken = await GetAccessTokenAsync();

        var message = new
        {
            message = new
            {
                token = fcmToken,
                notification = new
                {
                    title = title,
                    body = body
                }
            }
        };

        var jsonMessage = JsonConvert.SerializeObject(message);
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.PostAsync(url, new StringContent(jsonMessage, Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }
}
