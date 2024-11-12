using Newtonsoft.Json.Linq;

namespace RedditReader.Data;

public class RedditService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://www.reddit.com/";
    private DateTime _rateLimitReset;

    public RedditService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _rateLimitReset = DateTime.MinValue;
    }

    public async Task<RedditResponse> GetPostsAsync(string subreddit)
    {
        var redditResponse = new RedditResponse();

        if (DateTime.UtcNow < _rateLimitReset)
        {
            await Task.Delay(_rateLimitReset - DateTime.UtcNow);
        }

        string url = $"{_baseUrl}r/{subreddit}/.json";
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Console App");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        
        if (response.Headers.TryGetValues("x-ratelimit-reset", out var resetValues))
        {
            _rateLimitReset = DateTime.UtcNow.AddSeconds(Convert.ToDouble(resetValues.FirstOrDefault()));
        }

        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseBody);
        redditResponse = json.ToObject<RedditResponse>();
        return redditResponse;
    }

    public async Task FetchAndCountPosts(string username)
    {
        string url = $"{_baseUrl}user/{username}/.json";
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "C# Console App");

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseBody);

        int postCount = json["data"]["children"].Count();
        Console.WriteLine($"User {username} has made {postCount} posts.");
    }
}
public class RedditResponse
{
    public RedditData data { get; set; }
}

public class RedditData
{
    public RedditPost[] children { get; set; }
}

public class RedditPost
{
    public RedditPostData data { get; set; }
}

public class RedditPostData
{
    public string author { get; set; }
    public decimal upvote_ratio { get; set; }
    public string subreddit { get; set; }
    public string title { get; set; }
    public string selftext { get; set; }
    public int ups { get; set; }
}
