// Javascript file to search orders

const SEARCH_ORDERS_URL = "/Orders/Search";

var modal;
var modalHeader;
var modalMessage;
var modalClose;

var btnToggleSearch;
var viewSearchParams;
var ordersLoader;
var btnOrdersSearch;
var btnOrdersClear;

var fCreate;
var tCreate;
var address;
var lPrice;
var uPrice;
var fShipping;
var tShipping;

var orderSearchParamsVisible = false;

$(function () {
    modal = $(".modal");
    modalHeader = $("#moadlHeader");
    modalMessage = $("#modalMessage");
    btnModalClose = $("#modalClose")

    btnToggleSearch = $("#btnToggleSearch");
    viewSearchParams = $("#viewSearchParams");
    ordersLoader = $("#ordersLoader");
    btnOrdersSearch = $("#btnOrdersSearch");
    btnOrdersClear = $("#btnOrdersClear");

    fCreate = $("#fCreate");
    tCreate = $("#tCreate");
    address = $("#address");
    lPrice = $("#lPrice");
    uPrice = $("#uPrice");
    fShipping = $("#fShipping");
    tShipping = $("#tShipping");

    btnToggleSearch.on("click", function () {
        toggleSearchParamsView();
    });

    btnOrdersSearch.on("click", function () {
        loadingOrders(true);
        $.ajax({ url: SEARCH_ORDERS_URL, data: getSearchParamsDict(false) })
            .done(
                function (result) {
                    if (result != null) {
                        if (result.success) renderOrders(result);
                        else {
                            var header, content;
                            switch (result.data.msg) {
                                case "xc":
                                    header = "Crossing 'Create Dates'";
                                    content = "From Create Date must be earlier than To Create Date";
                                    nullifyFields();
                                    break;
                                case "xs":
                                    header = "Crossing 'Shipping Dates'";
                                    content = "From Shipping Date must be earlier than To Shipping Date";
                                    nullifyFields();
                                    break;
                                default:
                                    header  = Message.BUMMER;
                                    content = Message.ERR;
                            }
                            popOrdersModal(header, content, Message.OK, Color.RED, null);
                        }
                        loadingOrders(false);
                    }
                }
            );
    });

    btnOrdersClear.on("click", function () {
        loadingOrders(true);
        $.ajax({ url: SEARCH_ORDERS_URL, data: getSearchParamsDict(true) })
            .done(function (result) {
                if (result != null && result.success) {
                    nullifyFields();
                    renderOrders(result);
                }
                loadingOrders(false);
            }
            );
    });
});

function toggleSearchParamsView() {
    if (orderSearchParamsVisible) {
        viewSearchParams.addClass(DISP_NONE);
        btnToggleSearch.html('search my orders');
        btnToggleSearch.removeClass("text-danger");
        btnToggleSearch.removeClass("border-danger");
        btnToggleSearch.addClass("text-primary");
        btnToggleSearch.addClass("border-primary");
        loadingOrders(false);
    } else {
        viewSearchParams.removeClass(DISP_NONE);
        btnToggleSearch.html('X close search');
        btnToggleSearch.removeClass("text-primary");
        btnToggleSearch.removeClass("border-primary");
        btnToggleSearch.addClass("text-danger");
        btnToggleSearch.addClass("border-danger");
    }
    orderSearchParamsVisible = !orderSearchParamsVisible;
}

function loadingOrders(isLoading) {
    if (isLoading) ordersLoader.removeClass(DISP_NONE);
    else ordersLoader.addClass(DISP_NONE);
}

function popOrdersModal(header, content, closeMsg, color, redirectUrl) {
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

function getSearchParamsDict(isAll) {
    return {
        uid: $("#uid").val(),
        all: isAll,
        fCreate: fCreate.val(),
        tCreate: tCreate.val(),
        address: address.val().trim(),
        lPrice: lPrice.val().trim(),
        uPrice: uPrice.val().trim(),
        fShipping: fShipping.val(),
        tShipping: tShipping.val()
    }
}

function renderOrders(result) {
    var ordersTableBody = $("#ordersTable");
    var orderRowTemplate = $('#orderRowTemplate').html();

    if (result.success) {
        ordersTableBody.html('');
        $.each(result.data, function (_ix, _orderItem) {
            let template = orderRowTemplate;

            $.each(_orderItem, function (_prop, _val) {
                if (_prop == "totalPrice") _val = _val.toLocaleString(undefined, { minimumFractionDigits: 2 });
                if (_prop == "create" || _prop == "shipping")
                    _val = formatDate(_val);
                template = template.replaceAll('${' + _prop + '}', _val);
            });

            ordersTableBody.append(template);
        });
    }
}

function formatDate(_str) {
    let DatePart = {
        YEAR: 0,
        MONTH: 1,
        DAY: 2
    }
    var date = _str.split("T")[0];
    var dateParts = date.split("-");
    return dateParts[DatePart.DAY] + "/" + dateParts[DatePart.MONTH] + "/" + dateParts[DatePart.YEAR];
}

function nullifyFields() {
    fCreate.val(null);
    tCreate.val(null);
    address.val('');
    lPrice.val('');
    uPrice.val('');
    fShipping.val(null);
    tShipping.val(null);
}