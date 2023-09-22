using System.Threading.Tasks;
using System.Web.Http;
using Firebase.Database;
using Tweetinvi;
using Tweetinvi.Models;

namespace WebApi.Controllers
{
    public class TwitterController : ApiController
    {
        private readonly ITwitterClient _twitterClient;
        private readonly FirebaseClient _firebaseClient;

        public TwitterController(ITwitterClient twitterClient, FirebaseClient firebaseClient)
        {
            _twitterClient = twitterClient;
            _firebaseClient = firebaseClient;
        }

        [HttpGet]
        [Route("twitter/{username}")]
        public async Task<IHttpActionResult> GetTwitterInfo(string username)
        {
            try
            {
                var searchParameters = new SearchTweetsParameters(username)
                {
                    MaximumNumberOfResults = 1
                };
                var searchResult = await _twitterClient.Search.SearchTweetsAsync(searchParameters);
                var userProfile = searchResult[0].CreatedBy;

                var twitterSavedObj = CreateTwitterSavedObj(userProfile);

                await _firebaseClient.Child("twitter").Child(username).PutAsync(twitterSavedObj);

                return Ok(new[] { new { data = twitterSavedObj } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object CreateTwitterSavedObj(IUser userProfile)
        {
            return new
            {
                followers = userProfile.FollowersCount,
                screen_name = userProfile.ScreenName,
                logo = userProfile.ProfileImageUrl,
                banner = userProfile.ProfileBannerURL,
                profile_link_color = userProfile.ProfileLinkColor,
                profile_text_color = userProfile.ProfileTextColor
            };
        }
    }
}