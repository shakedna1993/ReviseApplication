var chatHub = $.connection.chatHub;
var projectMembersDetails;

$(function () {

    // Declare a proxy to reference the hub.
    //var chatHub = $.connection.chatHub;
    $.connection.hub.logging = true;
    chatHub.state.userid = $("#userid").val();
    chatHub.state.catid = $("#catid").val();
    chatHub.state.projid = $("#projid").val();

    registerClientMethods(chatHub);


    // Start Hub
    $.connection.hub.start().done(function () {
        isConnected();
        chatHub.server.join()
        registerEvents(chatHub);
        scrollingDownMessages();
    });


});


//Change the Status div if the conncetion to the hub Succeeded.
function isConnected() {
    $('#status').removeClass('label label-danger');
    $('#status').addClass('label label-success').html('online');


}

//Defined button's Events handelling.
function registerEvents(chatHub) {

    $('#btn-chat').click(function () { sendMessage(); });

    $('#btn-input').keypress(function (e) {
        if (e.which == 13) {
            sendMessage();
        }
    });

    $("#btn-input").keypress(function (e) {
        if (e.which == 13) {
            $('#btnSendMsg').click();
        }
    });
}

//Defined a Send button and Send a message to the Hub (Server side).
function sendMessage() {

    var msg = $("#btn-input").val();
    if (msg.length > 0) {

        var userName = $("#UserName").val(); // need to add user name from session
        chatHub.server.sendMessageToAll(userName, msg);
        $("#btn-input").val('');
        scrollingDownMessages();

    }
}

//Scrolling dowm the Chat Div to the bottom.
function scrollingDownMessages() {
    $('#messages-area').scrollTop($('#messages-area')[0].scrollHeight);
}


//Defined Proxy Hub Client side Functions.
function registerClientMethods(chatHub) {

    // Calls when user successfully logged in
    chatHub.client.onConnected = function (id, UserName, allUsers, messages, projMembersDetails) {

        projectMembersDetails = projMembersDetails;

        $('#hdId').val(id);
        $('#hdUserName').val(UserName);
        $('#spanUser').html(UserName);

        // Add All Users
        for (i = 0; i < allUsers.length; i++) {

            AddUser(chatHub, allUsers[i].ConnectionId, allUsers[i].UserName);
        }

        // Add Existing Messages
        for (i = 0; i < messages.length; i++) {

            AddMessage(messages[i].UserName, messages[i].Message, messages[i].Date);
        }


    }

    // On New User Connected
    chatHub.client.onNewUserConnected = function (id, name) {

        AddUser(chatHub, id, name);
    }


    // On User Disconnected
    chatHub.client.onUserDisconnected = function (id, userName) {

        $('#' + id).remove();

        var ctrId = 'private_' + id;
        $('#' + ctrId).remove();


        var disc = $('<div class="disconnect label-danger">"' + userName + '" logged off</div>');

        $(disc).hide();
        $('#divusers').prepend(disc);
        $(disc).fadeIn(200).delay(2000).fadeOut(200);

    }

    chatHub.client.messageReceived = function (UserName, message, createdDate) {

        AddMessage(UserName, message, createdDate);
    }


    chatHub.client.sendPrivateMessage = function (windowId, fromUserName, message) {

        var ctrId = 'private_' + windowId;


        if ($('#' + ctrId).length == 0) {

            createPrivateChatWindow(chatHub, windowId, ctrId, fromUserName);

        }

        $('#' + ctrId).find('#divMessage').append('<div class="message"><span class="userName">' + fromUserName + '</span>: ' + message + '</div>');

        // set scrollbar
        var height = $('#' + ctrId).find('#divMessage')[0].scrollHeight;
        $('#' + ctrId).find('#divMessage').scrollTop(height);

    }

}

//Adding user name to the online users list.
function AddUser(chatHub, id, name) {

    var userId = $('#hdId').val();

    var code = "";


    code = $('<a id="' + id + '" class="user" >' + name + '<a></br>');

    $(code).dblclick(function () {

        var id = $(this).attr('id');

        if (userId != id)
            OpenPrivateChatWindow(chatHub, id, name);

    });

    $("#divusers").append(code);

}

//Add a message to the chat div.
function AddMessage(userName, message, createdDate) {
    var memberDetails = getMemberDetails(userName);
    var picURL;

    if (memberDetails.picURL === "") {

        picURL = "http://www.bitrebels.com/wp-content/uploads/2011/02/Original-Facebook-Geek-Profile-Avatar-1.jpg";
    }
    else {
        picURL = memberDetails.picURL;
    }

    if (userName == $("#userName").val()) {


        $('#messages-area').append(
            '<div class="row msg_container base_sent">'
            + '<div class="col-lg-11 col-md-9 col-xs-10">'
            + '<div class="messages msg_receive">'
            + '<p>' + message + '</p>'
            + '<time datetime="' + createdDate + '">' + userName + ' • ' + setRoleNameTag(memberDetails) + ' • ' + createdDate + '</time></div></div>'
            + '<div class="col-lg-1 col-md-2 col-xs-2 avatar">'
            + '</div></div>');
    } else {
        $('#messages-area').append('<div class="row msg_container base_receive">'
            + '<div class="col-lg-1 col-md-2 col-xs-2 avatar">'
            + '<div class="col-lg-11 col-md-9 col-xs-10"><div class="messages msg_receive"><p>' + message
            + '</p><time datetime="' + createdDate + '">' + userName + '  •  ' + setRoleNameTag(memberDetails) + '  •  ' + createdDate + '</time>'
            + '</div></div></div>')
    }
    scrollingDownMessages();
    scrollingDownMessages();
}

//Returnd Member Role Name.
function setRoleNameTag(memberDetails) {
    if (memberDetails.roleId == 6) {
        return '<span class="label label-primary">' + memberDetails.roleName + '</span>'
    }
    else
        return '<span class="label label-info">' + memberDetails.roleName + '</span>'
}


//Return MemberDetails Object fron a global client side global variable.
function getMemberDetails(userName) {
    for (var i = 0; i < projectMembersDetails.length; i++) {
        if (projectMembersDetails[i].userName === userName) {
            return projectMembersDetails[i];
        }
    }

}



