using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Database;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tweetinvi;
using Tweetinvi.Logic.QueryParameters;
using Tweetinvi.Models;

namespace WebApi.Tests.Controllers
{
    [TestFixture]
    public class TwitterControllerTests
    {
        private TwitterController _controller;
        private Mock<ITwitterClient> _twitterClientMock;
        private Mock<FirebaseClient> _firebaseClientMock;

        [SetUp]
        public void SetUp()
        {
            _twitterClientMock = new Mock<ITwitterClient>();
            _firebaseClientMock = new Mock<FirebaseClient>();

            _controller = new TwitterController(_twitterClientMock.Object, _firebaseClientMock.Object);
        }

        [Test]
        [TestCaseSource(nameof(GetTwitterInfoTestCases))]
        public async Task GetTwitterInfo_ReturnsCorrectResponse(string username, ITweetSearchResult searchResult, HttpStatusCode expectedStatusCode, object expectedData)
        {
            // Arrange
            _twitterClientMock.Setup(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()))
                .ReturnsAsync(searchResult);

            // Act
            var response = await _controller.GetTwitterInfo(username);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(expectedStatusCode);
            var content = await response.Content.ReadAsAsync<IEnumerable<object>>();
            content.Should().NotBeNull().And.HaveCount(1);
            var data = content.Should().ContainSingle().Which["data"];
            data.Should().NotBeNull().And.BeEquivalentTo(expectedData);
            _twitterClientMock.Verify(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()), Times.Once);
            _firebaseClientMock.Verify(c => c.Child("twitter").Child(username).PutAsync(It.IsAny<object>()), Times.Once);
        }

        private static IEnumerable<object[]> GetTwitterInfoTestCases()
        {
            var userMock = new Mock<IUser>();
            userMock.SetupGet(u => u.FollowersCount).Returns(100);
            userMock.SetupGet(u => u.ScreenName).Returns("testuser");
            userMock.SetupGet(u => u.ProfileImageUrl).Returns("https://example.com/profile.jpg");
            userMock.SetupGet(u => u.ProfileBannerURL).Returns("https://example.com/banner.jpg");
            userMock.SetupGet(u => u.ProfileLinkColor).Returns("#000000");
            userMock.SetupGet(u => u.ProfileTextColor).Returns("#ffffff");
            var tweetMock = new Mock<ITweet>();
            tweetMock.SetupGet(t => t.CreatedBy).Returns(userMock.Object);
            var searchResultMock = new Mock<ITweetSearchResult>();
            searchResultMock.SetupGet(s => s[0]).Returns(tweetMock.Object);

            yield return new object[]
            {
                "testuser",
                searchResultMock.Object,
                HttpStatusCode.OK,
                new
                {
                    followers = 100,
                    screen_name = "testuser",
                    logo = "https://example.com/profile.jpg",
                    banner = "https://example.com/banner.jpg",
                    profile_link_color = "#000000",
                    profile_text_color = "#ffffff"
                }
            };

            yield return new object[]
            {
                "nonexistentuser",
                new TweetSearchResult(new ITweet[0]),
                HttpStatusCode.NotFound,
                null
            };
        }
    }
}