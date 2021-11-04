const Url = {
    CHANGE_PW: "/Users/ChangePassword",
    PROFILE: "/Users/Profile",
}

const Method = {
    POST: "POST",
    GET: "GET"
}

const Message = {
    EMPTY: "",
    OK: "Ok!",
    YAY: "YAY!!!",
    BUMMER: "Awwww... =\\",
    ERR: "Something went wrong",
    WRONG_PW: "Wrong Old Password! <br/>Please try again!",
    SUCCESS: "Your password has been successfully changed!",
    NOT_ALLOWED: "Access Denied! <br/>Please don't try again!",
    NOT_THE_SAME: "New passwords missmatch. <br/> Please try again!",
    PW_DETAILS_REQUIRED: "All password fields must be filled in order to change your password successfully!",
}

const Color = {
    RED: "r",
    GREEN: "g",
    YELLOW: "y",
    BLUE: "b",
}

const Button = {
    BLUE: "btn-primary",
    RED: "btn-danger",
    YELLOW: "btn-warning",
    GREEN: "btn-success",
}

var DetailsVisible = false;

var modal;
var modalHeader;
var modalMessage;
var modalClose;

var loader;

var newPwDetails;
var currPwInput;
var newPwInput;
var confirmPwInput;

var currPwInput;
var newPwInput;
var confirmPwInput;
var btnToggleChangePw;

$(function (){
    // -- Modal -- //
    modal = $(".modal");
    modalHeader = $("#moadlHeader");
    modalMessage = $("#modalMessage");
    btnModalClose = $("#modalClose")

    loader = $("#changePwSpinner");

    newPwDetails = $("#newPwDetails");
    currPwInput = $("#currPwInput");
    newPwInput = $("#newPwInput");
    confirmPwInput = $("#confirmPwInput");


    btnToggleChangePw = $("#btnToggleChangePw");
    btnToggleChangePw.on("click", function () {
        togglePwView();
    });

    // -- Change Password -- //
    currPwInput = $("#currPwInput");
    newPwInput = $("#newPwInput");
    confirmPwInput = $("#confirmPwInput");
    submitChangePwBtn = $("#submitChangePw");
    submitChangePwBtn.on("click", function () {
        validateNewPwFields();
    });

});

function togglePwView() {
    if (DetailsVisible) newPwDetails.addClass(DISP_NONE);
    else newPwDetails.removeClass(DISP_NONE);
    DetailsVisible = !DetailsVisible;
}

function changePassword(_uid) {
    currPwVal = currPwInput.val();
    newPwVal = newPwInput.val();
    confirmPwVal = confirmPwInput.val();

    if (!validateNewPwFields(currPwVal, newPwVal, confirmPwVal))
        return;

    loadingNewPw(true);
    var dataDict = { uid: _uid, cpw: currPwVal, npw: newPwVal };
    $.ajax({ url: Url.CHANGE_PW, data: dataDict, type: Method.POST }).done(function (result) {
        if (result.success) {
            currPwInput.val(Message.EMPTY);
            newPwInput.val(Message.EMPTY);
            confirmPwInput.val(Message.EMPTY);
            togglePwView();
            switchContent(Display.PROFILE);
        }

        popChangePwModal(result);
        loadingNewPw(false);
    });
}

function validateNewPwFields(currPwVal, newPwVal, confirmPwVal) {
    if (currPwVal == EMPTY || newPwVal == EMPTY || confirmPwVal == EMPTY) {
        popModal(Message.BUMMER,
            Message.PW_DETAILS_REQUIRED,
            Message.OK,
            Color.YELLOW,
            null
        ); return false;
    }

    if (newPwVal != confirmPwVal) {
        popModal(Message.BUMMER,
            Message.NOT_THE_SAME,
            Message.OK,
            Color.RED,
            null
        );
        newPwInput.val(Message.EMPTY);
        confirmPwInput.val(Message.EMPTY);
        return false;
    } return true;
}

function popModal(header, content, closeMsg, color, redirectUrl) {
    modalHeader.html(header);
    modalMessage.html(content);

    btnModalClose.html(closeMsg);
    switch (color) {
        case Color.RED:
            btnModalClose.addClass(Button.RED);
            btnModalClose.removeClass(Button.YELLOW);
            btnModalClose.removeClass(Button.BLUE);
            btnModalClose.removeClass(Button.GREEN);
            break;
        case Color.GREEN:
            btnModalClose.addClass(Button.GREEN);
            btnModalClose.removeClass(Button.YELLOW);
            btnModalClose.removeClass(Button.BLUE);
            btnModalClose.removeClass(Button.RED);
            break;
        case Color.YELLOW:
            btnModalClose.addClass(Button.YELLOW);
            btnModalClose.removeClass(Button.GREEN);
            btnModalClose.removeClass(Button.BLUE);
            btnModalClose.removeClass(Button.RED);
            break;
        default:
            btnModalClose.addClass(Button.BLUE);
            btnModalClose.removeClass(Button.YELLOW);
            btnModalClose.removeClass(Button.GREEN);
            btnModalClose.removeClass(Button.RED);
    }
    btnModalClose.click(function () {
        modal.hide();
        if (redirectUrl != null)
            Redirect(redirectUrl);
    });

    modal.show();
    btnModalClose.focus();
}

function popChangePwModal(result) {
    if (result.success) {
        popModal(Message.YAY,
            Message.SUCCESS,
            Message.OK,
            Color.GREEN,
            null
        ); return;
    }

    let content = Message.ERR;
    let color = Color.RED;

    switch (result.data.msg) {
        case "e":break;
        case "m": break;
        case "w": content = Message.WRONG_PW; break;
        case "a": content = Message.NOT_ALLOWED; break;
        default: color = Color.BLUE;
    }
    popModal(Message.BUMMER, content, Message.OK, color, null);
}

function Redirect(dst) {
    window.location.href = dst;
}

function loadingNewPw(isLoading) {
    if (isLoading) loader.removeClass(DISP_NONE);
    else loader.addClass(DISP_NONE);
}