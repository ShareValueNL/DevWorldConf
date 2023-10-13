using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Web.Http.Results;
using System.Web.Http.Cors;
using System.Configuration;
using System;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Firebase.Database;
using Firebase.Database.Query;

namespace TwitterAPI.Controllers
{
    /// <summary>
    /// Controller for retrieving Twitter user information.
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TwitterController : ApiController
    {
        private readonly TwitterClient _twitterClient;
        private readonly FirebaseClient _firebaseClient;

        /// <summary>
        /// Constructor for the TwitterController.
        /// </summary>
        /// <param name="twitterClient">The TwitterClient used to retrieve Twitter data.</param>
        /// <param name="firebaseClient">The FirebaseClient used to store Twitter data.</param>
        public TwitterController(TwitterClient twitterClient, FirebaseClient firebaseClient)
        {
            _twitterClient = twitterClient;
            _firebaseClient = firebaseClient;
        }

        /// <summary>
        /// Retrieves Twitter user information.
        /// </summary>
        /// <param name="name">The Twitter user screen name.</param>
        /// <returns>The Twitter user information.</returns>
        [HttpGet]
        [Route("api/twitter/{name}")]
        public async Task<IHttpActionResult> Get(string name)
        {
            try
            {
                var user = await _firebaseClient.Child("twitter").Child(name).OnceSingleAsync<TwitterUser>();

                if (user != null)
                {
                    return Ok(user);
                }

                var searchParameter = new SearchTweetsParameters(name)
                {
                    MaximumNumberOfResults = 1
                };
                var tweets = await _twitterClient.Search.SearchTweetsAsync(searchParameter);
                var tweet = tweets[0];
                var userProfile = tweet.CreatedBy;

                var twitterSavedObj = CreateTwitterUserObject(userProfile);

                await _firebaseClient.Child("twitter").Child(name).PutAsync(twitterSavedObj);

                return Ok(twitterSavedObj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Creates a TwitterUser object from a IUser object.
        /// </summary>
        /// <param name="userProfile">The IUser object to create the TwitterUser object from.</param>
        /// <returns>The TwitterUser object.</returns>
        private TwitterUser CreateTwitterUserObject(IUser userProfile)
        {
            return new TwitterUser
            {
                Followers = userProfile.FollowersCount,
                ScreenName = userProfile.ScreenName,
                Logo = userProfile.ProfileImageUrl,
                Banner = userProfile.ProfileBannerURL,
                ProfileLinkColor = userProfile.ProfileLinkColor,
                ProfileTextColor = userProfile.ProfileTextColor
            };
        }
    }

    /// <summary>
    /// Represents a Twitter user.
    /// </summary>
    public class TwitterUser
    {
        /// <summary>
        /// The number of Twitter user followers.
        /// </summary>
        public long Followers { get; set; }

        /// <summary>
        /// The Twitter user screen name.
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// The URL of the Twitter user profile image.
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// The URL of the Twitter user profile banner.
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// The Twitter user profile link color.
        /// </summary>
        public string ProfileLinkColor { get; set; }

        /// <summary>
        /// The Twitter user profile text color.
        /// </summary>
        public string ProfileTextColor { get; set; }
    }
}