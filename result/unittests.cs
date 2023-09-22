using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Firebase.Database;
using Firebase.Database.Query;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tweetinvi;
using Tweetinvi.Models;
using WebApi.Controllers;

namespace WebApi.Tests
{
    [TestFixture]
    public class TwitterControllerTests
    {
        private static readonly object[] TestCases =
        {
            new object[] { "testuser1", 100, "testuser1", "https://testuser1.com/logo.png", "https://testuser1.com/banner.png", "#000000", "#ffffff" },
            new object[] { "testuser2", 200, "testuser2", "https://testuser2.com/logo.png", "https://testuser2.com/banner.png", "#ffffff", "#000000" },
            new object[] { "testuser3", 300, "testuser3", "https://testuser3.com/logo.png", "https://testuser3.com/banner.png", "#0000ff", "#ffff00" }
        };

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task TestGetTwitterInfo(string username, int followers, string screenName, string logo, string banner, string linkColor, string textColor)
        {
            // Arrange
            var firebaseClientMock = new Mock<FirebaseClient>("https://nodejs-express-demo.firebaseio.com/");
            firebaseClientMock.Setup(x => x.Child("twitter").Child(username).PutAsync(It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);
            firebaseClientMock.Setup(x => x.Child("twitter").OnceAsync<Dictionary<string, object>>())
                .ReturnsAsync(new List<FirebaseObject<Dictionary<string, object>>>
                {
                    new FirebaseObject<Dictionary<string, object>>
                    {
                        Key = username,
                        Object = new Dictionary<string, object>
                        {
                            { "followers", followers },
                            { "screen_name", screenName },
                            { "logo", logo },
                            { "banner", banner },
                            { "profile_link_color", linkColor },
                            { "profile_text_color", textColor }
                        }
                    }
                });

            var tweetMock = new Mock<ITweet>();
            tweetMock.Setup(x => x.CreatedBy).Returns(new Mock<IUser>().Object);
            var tweetList = new List<ITweet> { tweetMock.Object };
            var twitterClientMock = new Mock<ITwitterClient>();
            twitterClientMock.Setup(x => x.Search.SearchTweetsAsync(It.IsAny<ITweetSearchParameters>()))
                .ReturnsAsync(tweetList);

            var controller = new TwitterController
            {
                FirebaseClient = firebaseClientMock.Object,
                TwitterClient = twitterClientMock.Object
            };

            // Act
            var result = await controller.GetTwitterInfo(username);

            // Assert
            result.Should().BeOfType<OkNegotiatedContentResult<object>>();
            var contentResult = (OkNegotiatedContentResult<object>)result;
            contentResult.Content.Should().HaveCount(1);
            var data = contentResult.Content[0].GetType().GetProperty("data").GetValue(contentResult.Content[0]);
            data.Should().BeEquivalentTo(new
            {
                followers,
                screen_name = screenName,
                logo,
                banner,
                profile_link_color = linkColor,
                profile_text_color = textColor
            });
        }
    }
}