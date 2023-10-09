/**
 * Created by Raymon on 4-12-2014.
 */

var express = require('express'),
	app = express(),
	Twit = require('twit'),
	Firebase = require('firebase'),
	TwUsername = new Firebase('https://nodejs-express-demo.firebaseio.com/twitter/username')

var server = app.listen(3000, function () {
	var host = server.address().address
	var port = server.address().port

	console.log('Example app listening at http://%s:%s', host, port)
})

/**
 * onComplete function to check error
 * @param error
 */
var onComplete = function (error) {
	if (error) {
		console.log('Synchronization failed')
	} else {
		console.log('Synchronization succeeded')
	}
}

/**
 * Firebase api
 */
function saveObjTwitter(req, obj, promise) {
	console.log('Data is saved to Firebase')
	TwUsername.child(req).update(obj, promise)
}

/**
 * Twitter API
 *
 */
var T = new Twit({
	consumer_key: 'CONSUMER_KEY',
	consumer_secret: 'CONSUMER_SECRET',
	access_token: 'ACCESS_TOKEN',
	access_token_secret: 'ACCESS_TOKEN_SECRET'
})

/**
 * Gets and save twitter info from Twitter API when a request is done to for example:
 * localhost:3000/twitter/username
 * It returns a json object
 */
app.get('/twitter/:name', function (req, res) {
	T.get('search/tweets', { q: req.params.name, count: 1 }, function (err, data, response) {
		var userProfile, twitterSavedObj

		userProfile = data.statuses[0].user

		twitterSavedObj = {
			followers: userProfile.followers_count,
			screen_name: userProfile.screen_name,
			logo: userProfile.profile_image_url,
			banner: userProfile.profile_banner_url,
			profile_link_color: userProfile.profile_link_color,
			profile_text_color: userProfile.profile_text_color
		}

		saveObjTwitter(req.params.name, twitterSavedObj, onComplete)

		TwUsername.on(
			'value',
			function (snapshot) {
				console.log('Got the data')
				res.send(snapshot.val())
			},
			function (errorObject) {
				console.log('The read failed: ' + errorObject.code)
			}
		)
	})
})
