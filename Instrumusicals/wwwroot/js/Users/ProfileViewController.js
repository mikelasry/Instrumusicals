// jquery page for Profile View Controller

const DISP_NONE = "d-none";
const Display = {
    PROFILE: 8001,
    ORDERS: 8002,
    REVIEWS: 8003,
    SECURITY: 8004
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
    btnProfile = $("#btnProfile");
    btnOrders = $("#btnOrders");
    btnReviews = $("#btnReviews");
    btnSecurity = $("#btnSecurity");

    profileContent = $("#profileContent");
    ordersContent = $("#ordersContent");
    reviewsContent = $("#reviewsContent");
    securityContent = $("#securityContent");
});

function switchContent(view) {
    hideCurrentView();
    switch (view) {

        case "p":
            profileContent.removeClass(DISP_NONE);
            CurrentView = Display.PROFILE;
            break;
        case "o":
            ordersContent.removeClass(DISP_NONE);            
            CurrentView = Display.ORDERS;
            break;
        case "r":
            reviewsContent.removeClass(DISP_NONE);
            CurrentView = Display.REVIEWS;
            break;
        case "s":
            securityContent.removeClass(DISP_NONE);
            CurrentView = Display.SECURITY;
            break;
    }
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