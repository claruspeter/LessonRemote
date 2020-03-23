"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").build();
var myClass = null;

function logMessage(user, msg) {
    $('#msglist').prepend("<li>" + user + ": " + msg + "</li>");
    console.info(user + ": " + msg);
}

connection.on("Navigated", function (who, location) {
    window.open(location, "_lesson", null, true);
    logMessage(who, location);
});


connection.start().then(function () {
    console.info("Starting Hub...");
    connection.invoke("Refresh").catch(function (err) {
        return console.error(err.toString());
    });
    
}).catch(function (err) {
    return console.error(err.toString());
});

function JoinClass(name) {
    connection.invoke("JoinClass", name).catch(function (err) {
        return console.error(err.toString());
    }).then( function() {
        myClass = name;
    });
}

function NavigateTo(location) {
    console.info("making move: " + location);
    connection.invoke("NavigateTo", myClass, location)
        .then( function () {
            logMessage( "Me", location);
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}

connection.on("ReceivedMessage", function (className, msg) {
    logMessage( className, msg);
});