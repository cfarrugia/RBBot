var dashboard = {};


// This is the google FCM setup. 
var configFCM = {
    apiKey: "AIzaSyB22OK5gWuzneKTQUl4v-e6L2XqBsJ3AFY",
    authDomain: "rbbot001.firebaseapp.com",
    databaseURL: "https://rbbot001.firebaseio.com",
    projectId: "rbbot001",
    storageBucket: "rbbot001.appspot.com",
    messagingSenderId: "399864383766",
    serviceWorker: "serviceWorker.js", // This is the service worker used by FCM to send the message to. 

    onTokenFetched: function (token) {
        console.log("On Token fetched: " + token);
    },
    onTokenFetchedError: function (error) {
        console.log("On Token fetch error: " + error);
    },
    onPermissionRequired: function (token) {
        console.log("On permission required: " + token);
    },
    onPermissionGranted: function (token) {
        console.log("On permission granted: " + token);
    },
    onPermissionDenied: function (token) {
        console.log("On permission denied: " + token);
    },
    onMessage: function (payload) {
        console.log("New message payload: " + JSON.stringify(payload));
    },
    sendTokenToServer: function (token) {
        console.log("On token sent to server: " + token);
    },
    onTokenDeleted: function () {
        console.log("On token deleted");
    },
    onTokenDeletedError: function () {
        console.log("On token deleted");
    }

};


dashboard.registerToTopic = function () {
    if (dashboard.FCMToken == null) {
        throw "Messaging not initialized yet! Cannot register to topic";
        return;
    }

    // This is bad, as we shouldn't store the server key on client side!
    // To be improved: create a server side handler for this!
    try
    {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.open("POST", config.FCM.url.replace("{topic}", config.FCM.topic).replace("{token}", dashboard.FCMToken));
        xmlhttp.setRequestHeader("Content-Type", "application/json");
        xmlhttp.setRequestHeader("Authorization", "key=" + config.FCM.serverKey);
        xmlhttp.send();
    }
    catch (er)
    {
        throw "Error registering to topic:" + er.toString();
    }

}

dashboard.fcmToken = null; // Start with a null token. 
dashboard.fcmPromise = null; // If needed, this is the promise returned from FCM for later use.

dashboard.init = function () {

    // On initialization, push the FCM configs.
    Push.config({
        FCM: configFCM
    });

    // 

    Push.FCM().then(function (FCM) {

        dashboard.fcmPromise = FCM;

        FCM.getToken().then(function (token) {

            dashboard.FCMToken = token; // Save token.
            dashboard.registerToTopic(); // Register token.
            console.log("Initialized with token " + token);
        }).catch(function (tokenError) {
            alert(tokenError);

            throw tokenError;
        });
    }).catch(function (initError) {
        alert(initError);
        throw initError;
    });

};

