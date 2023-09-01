/**
 * Created by Raymon on 4-12-2014.
 */

var express = require('express'),
    http = require('http'),
    https = require('https'),
    request = require('request'),
    app = express(),
    bodyParser = require('body-parser'),
    Twit = require('twit'),
    OpenGraph = require('facebook-open-graph'),
    openGraph = new OpenGraph('fhero_opengraph'),
    Firebase = require("firebase"),
    myFirebaseRef = new Firebase("https://nodejs-express-demo.firebaseio.com/"),
    FBtwitter = new Firebase("https://nodejs-express-demo.firebaseio.com/twitter"),
    FBtwUsername = new Firebase("https://nodejs-express-demo.firebaseio.com/twitter/username"),
    FBfaceBookPageData = myFirebaseRef.child('facebook/username');


app.use(express.static(__dirname + '/html'));

var server = app.listen(3000, function () {

    var host = server.address().address
    var port = server.address().port

    console.log('Example app listening at http://%s:%s', host, port)

});


/**
 * onComplete function to check error
 * @param error
 */
var onComplete = function(error) {
    if (error) {
        console.log('Synchronization failed');
    } else {
        console.log('Synchronization succeeded');
    }
}



/**
 * Firebase api
 */
function saveObjTwitterToFB(req, obj, promise) {
    console.log('Data is saved to Firebase');
    FBtwUsername.child(req).update(obj, promise);
}

function saveObjFacebookToFB(req, obj, promise) {
    console.log('Data is of Facebook is saved to Firebase');
    FBfaceBookPageData.child(req).update(obj, promise);
}


/**
 * Twitter API
 *
 */
var T = new Twit({
    consumer_key:         'CONSUMER_KEY',
    consumer_secret:      'CONSUMER_SECRET',
    access_token:         'ACCESS_TOKEN',
    access_token_secret:  'ACCESS_TOKEN_SECRET'
});


/**
 * Gets and save twitter info from Twitter API when a request is done to for example:
 * localhost:3000/twitter/username
 * It returns a json object
 */
app.get('/twitter/:name', function (req, res) {
    T.get('search/tweets', { q: req.params.name, count: 1 }, function(err, data, response) {
        var userProfile, twitterSavedObj;

        userProfile = data.statuses[0].user;

        twitterSavedObj = {
            followers: userProfile.followers_count,
            screen_name: userProfile.screen_name,
            logo: userProfile.profile_image_url,
            banner: userProfile.profile_banner_url,
            profile_link_color: userProfile.profile_link_color,
            profile_text_color: userProfile.profile_text_color
        };

        saveObjTwitterToFB(req.params.name, twitterSavedObj, onComplete);

        FBtwUsername.on("value", function(snapshot) {
            console.log('Got the data');
            res.send(snapshot.val());
        }, function (errorObject) {
            console.log("The read failed: " + errorObject.code);
        });
    });
});


app.get('/facebook/:name', function (req, res) {

    //openGraph.publish('139563692775523','320b20df6b9911f1788cad73bef04d8a','like','','url',function(err,response){
    //    // stuff
    //})

    openGraph.show('139563692775523','320b20df6b9911f1788cad73bef04d8a','like',function(err,response){
        // do stuff
        console.log(response);
    })

});

