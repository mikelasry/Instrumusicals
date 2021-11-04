const CHANGE_PW_URL = "/Users/ChangePassword"

var DetailsVisible = false;

var newPwDetails;
var currPwInput;
var newPwInput;
var confirmPwInput;

$(function () {
    newPwDetails = $("#newPwDetails");
    currPwInput = $("#currPwInput");
    newPwInput = $("#newPwInput");
    confirmPwInput = $("#confirmPwInput");
});

function togglePwView() {
    if (DetailsVisible) newPwDetails.addClass(DISP_NONE);
    else newPwDetails.removeClass(DISP_NONE);
    DetailsVisible = !DetailsVisible;
}

function changePassword(uid) {
    // create modal
    // check if passwords match (new & confirm)
    // send ajax post method to change the pw with old & new (try encryption)
}