using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Database;
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
        public async Task GetTwitterInfo_ReturnsOk_WhenUserExists()
        {
            // Arrange
            const string username = "testuser";
            var searchResultMock = new Mock<ITweetSearchResult>();
            var tweetMock = new Mock<ITweet>();
            var userMock = new Mock<IUser>();
            userMock.SetupGet(u => u.FollowersCount).Returns(100);
            userMock.SetupGet(u => u.ScreenName).Returns(username);
            userMock.SetupGet(u => u.ProfileImageUrl).Returns("https://example.com/profile.jpg");
            userMock.SetupGet(u => u.ProfileBannerURL).Returns("https://example.com/banner.jpg");
            userMock.SetupGet(u => u.ProfileLinkColor).Returns("#000000");
            userMock.SetupGet(u => u.ProfileTextColor).Returns("#ffffff");
            tweetMock.SetupGet(t => t.CreatedBy).Returns(userMock.Object);
            searchResultMock.SetupGet(s => s[0]).Returns(tweetMock.Object);
            _twitterClientMock.Setup(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()))
                .ReturnsAsync(searchResultMock.Object);

            // Act
            var response = await _controller.GetTwitterInfo(username);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsAsync<IEnumerable<object>>();
            content.Should().NotBeNull().And.HaveCount(1);
            var data = content.Should().ContainSingle().Which["data"];
            data.Should().NotBeNull().And.BeEquivalentTo(new
            {
                followers = 100,
                screen_name = username,
                logo = "https://example.com/profile.jpg",
                banner = "https://example.com/banner.jpg",
                profile_link_color = "#000000",
                profile_text_color = "#ffffff"
            });
            _twitterClientMock.Verify(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()), Times.Once);
            _firebaseClientMock.Verify(c => c.Child("twitter").Child(username).PutAsync(It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task GetTwitterInfo_ReturnsInternalServerError_WhenTwitterClientThrowsException()
        {
            // Arrange
            const string username = "testuser";
            _twitterClientMock.Setup(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()))
                .ThrowsAsync(new TwitterException("An error occurred"));

            // Act
            var response = await _controller.GetTwitterInfo(username);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            _twitterClientMock.Verify(c => c.Search.SearchTweetsAsync(It.IsAny<SearchTweetsParameters>()), Times.Once);
            _firebaseClientMock.Verify(c => c.Child("twitter").Child(username).PutAsync(It.IsAny<object>()), Times.Never);
        }
    }
}