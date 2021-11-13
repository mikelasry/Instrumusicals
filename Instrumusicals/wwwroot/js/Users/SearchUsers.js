// js page for searching users
const DISP_NONE = "d-none";
const EMPTY = "";
const SEARCH_USERS_URL = "/Users/Search";

const UserType = {
    CLIENT: 1001,
    ADMIN: 1004
};

var btnSearchUsersView;
var userSearchView;

var btnUsersSearch;
var btnUsersClear;
var usersLoader;

var emailParam;
var fNameParam;
var lNameParam;
var addressParam;

var searchUserViewVisible = false;

$(function () {
    btnSearchUsersView = $("#btnSearchUsersView");
    userSearchView = $("#userSearchView");

    btnUsersSearch = $("#btnUsersSearch");
    btnUsersClear = $("#btnUsersClear");
    usersLoader = $("#usersLoader");

    emailParam = $("#emailParam");
    fNameParam = $("#fNameParam");
    lNameParam = $("#lNameParam");
    addressParam = $("#addressParam");

    btnSearchUsersView.on("click", function () {
        toggleSearchView();
    });


    btnUsersSearch.on("click", function () {
        userSearchLoading(true);
        $.ajax({ url: SEARCH_USERS_URL, data: getSearchUsersDict()}).done(function (result) {
            if (result != null) renderUsers(result);
            userSearchLoading(false);
        });
    });

    btnUsersClear.on("click", function () {
        userSearchLoading(true);
        $.ajax({ url: SEARCH_USERS_URL, data: { all: true } }).done(function (result) {
            if (result != null) renderUsers(result);
            nullifyFields();
            userSearchLoading(false);
        });
    });
});

function toggleSearchView() {
    if (searchUserViewVisible) {
        userSearchView.addClass(DISP_NONE);
        btnSearchUsersView.html('search users');
        btnSearchUsersView.removeClass("text-danger");
        btnSearchUsersView.removeClass("border-danger");
        btnSearchUsersView.addClass("text-primary");
        btnSearchUsersView.addClass("border-primary");
        userSearchLoading(false);
    } else {
        userSearchView.removeClass(DISP_NONE);
        btnSearchUsersView.html('X close search');
        btnSearchUsersView.removeClass("text-primary border-primary");
        btnSearchUsersView.removeClass("");
        btnSearchUsersView.addClass("text-danger");
        btnSearchUsersView.addClass("border-danger");
    }
    searchUserViewVisible = !searchUserViewVisible;
}

function userSearchLoading(isLoading) {
    if (isLoading) usersLoader.removeClass(DISP_NONE);
    else usersLoader.addClass(DISP_NONE);
}

function isAllFieldsEmpty() {
    return (emailParam.val().trim() == "") &&
        (fNameParam.val().trim() == "") &&
        (lNameParam.val().trim() == "") &&
        (addressParam.val().trim() == "") 
}

function nullifyFields() {
    emailParam.val('');
    fNameParam.val('');
    lNameParam.val('');
    addressParam.val('');
}

function getSearchUsersDict() {
    return {
        all: isAllFieldsEmpty(),
        email: emailParam.val(),
        fName: fNameParam.val(),
        lName: lNameParam.val(),
        address: addressParam.val()
    };
}

function renderUsers(result) {
    var usersTable = $("#usersTable");
    var userRowTemplate = $('#userRowTemplate').html();

    if (result.success) {
        usersTable.html('');
        $.each(result.data, function (_ix, _userItem) {
            let template = userRowTemplate;
            $.each(_userItem, function (_prop, _val) {
               template = template.replaceAll('${' + _prop + '}', _val);
            });

            var isAdmin = _userItem.userType == UserType.ADMIN;
            var btnColor = isAdmin ? 'danger' : 'warning';
            var action = isAdmin ? 'Deadminate' : 'Adminate';

            template = template.replaceAll("${btnColor}", btnColor);
            template = template.replaceAll("${action}", action);
            template = template.replaceAll("${isAdmin}", !isAdmin);

            usersTable.append(template);
        });
    }
}

function toggleAdminateBtn(isAdmin, uid) {
    var btnid = "#godModeBtn-" + uid;
    let godModeBtn = $(btnid);
    if (isAdmin) {
        godModeBtn.removeClass("text-warning");
        godModeBtn.removeClass("border-warning");
        godModeBtn.addClass("text-danger");
        godModeBtn.addClass("border-danger");
        godModeBtn.html("Deadminate");
    } else{
        godModeBtn.removeClass("text-danger");
        godModeBtn.removeClass("border-danger");
        godModeBtn.addClass("text-warning");
        godModeBtn.addClass("border-warning");
        godModeBtn.html("Adminate");
    }
    godModeBtn.prop("onclick", null).off("click");
    godModeBtn.on("click", function () {
        adminate(!isAdmin, uid);
    });
}

function adminate(_isAdmin, _uid) {
    var url = "/Users/ChangeAccess";
    loadAdminating(true);
    $.ajax({ url: url, type: "POST", data: { uid: _uid, isAdmin: _isAdmin } })
        .done(function (result) {
            toggleAdminateBtn(_isAdmin, _uid);
            loadAdminating(false);
        });
}

function loadAdminating(isLoading) {
    var adminateLoader = $("#adminateLoader");
    if (isLoading) adminateLoader.removeClass('d-none');
    else adminateLoader.addClass('d-none');
}