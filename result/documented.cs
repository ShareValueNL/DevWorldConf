using System.Threading.Tasks;
using System.Web.Http;
using Firebase.Database;
using Firebase.Database.Query;
using Tweetinvi;
using Tweetinvi.Models;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controller for retrieving Twitter profile information.
    /// </summary>
    public class TwitterController : ApiController
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly TwitterClient _twitterClient;

        public TwitterController()
        {
            _firebaseClient = new FirebaseClient("https://nodejs-express-demo.firebaseio.com/");
            _twitterClient = new TwitterClient("CONSUMER_KEY", "CONSUMER_SECRET", "ACCESS_TOKEN", "ACCESS_TOKEN_SECRET");
        }

        /// <summary>
        /// Retrieves Twitter profile information for the specified username.
        /// </summary>
        /// <param name="username">The Twitter username to search for.</param>
        /// <returns>An IHttpActionResult containing the retrieved profile information.</returns>
        [HttpGet]
        [Route("api/twitter/{username}")]
        public async Task<IHttpActionResult> GetTwitterInfo(string username)
        {
            var searchParameter = new Tweetinvi.Parameters.SearchTweetsParameters(username)
            {
                MaximumNumberOfResults = 1
            };

            var searchResult = await _twitterClient.Search.SearchTweetsAsync(searchParameter);

            var userProfile = searchResult[0].CreatedBy;

            var twitterSavedObj = new Dictionary<string, object>
            {
                { "followers", userProfile.FollowersCount },
                { "screen_name", userProfile.ScreenName },
                { "logo", userProfile.ProfileImageUrl },
                { "banner", userProfile.ProfileBannerUrl },
                { "profile_link_color", userProfile.ProfileLinkColor },
                { "profile_text_color", userProfile.ProfileTextColor }
            };

            await _firebaseClient.Child("twitter").Child(username).PutAsync(twitterSavedObj);

            var snapshot = await _firebaseClient.Child("twitter").OnceAsync<Dictionary<string, object>>();

            return Ok(snapshot.Select(x => new { username = x.Key, data = x.Object }));
        }
    }
}