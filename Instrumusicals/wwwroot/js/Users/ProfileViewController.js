// jquery page for Profile View Controller

const EMPTY = "";

const BTN_BLU = "btn-primary";
const BTN_GRAY = "btn-secondary";

const DISP_NONE = "d-none";
const Display = {
    PROFILE: 8001,
    ORDERS: 8002,
    REVIEWS: 8003,
    SECURITY: 8004,
};

var btnProfile;
var btnOrders;
var btnReviews;
var btnSecurity;

var profileContent;
var ordersContent;
var reviewsContent;
var securityContent;

var CurrentView = Display.PROFILE;

$(function () {
    // -- Dynamic Content -- //
    profileContent = $("#profileContent");
    ordersContent = $("#ordersContent");
    reviewsContent = $("#reviewsContent");
    securityContent = $("#securityContent");

    btnProfile = $("#btnProfile");
    btnProfile.on("click", function () {
        switchContent(Display.PROFILE);
    });

    btnOrders = $("#btnOrders");
    btnOrders.on("click", function () {
        switchContent(Display.ORDERS);
    });

    btnReviews = $("#btnReviews");
    btnReviews.on("click", function () {
        switchContent(Display.REVIEWS);
    });

    btnSecurity = $("#btnSecurity");
    btnSecurity.on("click", function () {
        switchContent(Display.SECURITY);
    });


    swapBtnColors(Display.PROFILE);
});

function switchContent(view) {
    swapBtnColors(view);
    hideCurrentView();
    toggleView(view);
}

function hideCurrentView() {
    switch (CurrentView) {
        case Display.PROFILE:
            profileContent.addClass(DISP_NONE);
            break;
        case Display.ORDERS:
            ordersContent.addClass(DISP_NONE);
            break;
        case Display.REVIEWS:
            reviewsContent.addClass(DISP_NONE);
            break;
        case Display.SECURITY:
            securityContent.addClass(DISP_NONE);
            break;
    }
}

function swapBtnColors(view) {
    // Turn Blue Off
    switch (CurrentView) {
        case Display.PROFILE:
            btnProfile.removeClass(BTN_BLU);
            btnProfile.addClass(BTN_GRAY);
            break;
        case Display.ORDERS:
            btnOrders.removeClass(BTN_BLU);
            btnOrders.addClass(BTN_GRAY);
            break;
        case Display.REVIEWS:
            btnReviews.removeClass(BTN_BLU);
            btnReviews.addClass(BTN_GRAY);
            break;
        case Display.SECURITY:
            btnSecurity.removeClass(BTN_BLU);
            btnSecurity.addClass(BTN_GRAY);
            break;
    }
    // Turn Blue On
    switch (view) {
        case Display.PROFILE:
            btnProfile.removeClass(BTN_GRAY);
            btnProfile.addClass(BTN_BLU);
            break;
        case Display.ORDERS:
            btnOrders.removeClass(BTN_GRAY);
            btnOrders.addClass(BTN_BLU);
            break;
        case Display.REVIEWS:
            btnReviews.removeClass(BTN_GRAY);
            btnReviews.addClass(BTN_BLU);
            break;
        case Display.SECURITY:
            btnSecurity.removeClass(BTN_GRAY);
            btnSecurity.addClass(BTN_BLU);
            break;
    }
}

function toggleView(view) {
    switch (view) {
        case Display.PROFILE:
            profileContent.removeClass(DISP_NONE);
            CurrentView = Display.PROFILE;
            break;
        case Display.ORDERS:
            ordersContent.removeClass(DISP_NONE);
            CurrentView = Display.ORDERS;
            break;
        case Display.REVIEWS:
            reviewsContent.removeClass(DISP_NONE);
            CurrentView = Display.REVIEWS;
            break;
        case Display.SECURITY:
            securityContent.removeClass(DISP_NONE);
            CurrentView = Display.SECURITY;
            break;
    }
}
