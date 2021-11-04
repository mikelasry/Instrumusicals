// jquery page for cart updates

// $(function () { });

const Add_TO_CART_URL = "/Instruments/AddToCart"
const REMOVE_FROM_CART_URL = "/Instruments/RemoveFromCart"
const PLACE_ORDER_URL = "/Orders/PlaceOrder";


const TOO_FEW = "There is not enough occurrences of this insteument";
const INST_NOT_AVAIL = "The instrument is currently not available."
const NON_AUTH = "You are not authorized for this operation";
const ORDER_SUCCESS = "Your order has been successfully placed.";

const PAY_DET = "Payment method required!";
const FULFIL_PAY = "Please fill all payment details!";
const PROCEED = "Proceed to payment";
const NEXT_TIME = "Maybe next time!"
const GRATZ = "Congradulations!";
const LATER = "Pay later";
const OK = "Got it!";
const AW = "Awww";

const ERR = "Something went wrong!";
const DB_UPDATE_ERR = "Could not update the database.\nPlease try again soon!"

const DISP_NONE = "d-none";

const BTN_RED = "btn-danger"
const BTN_GRN = "btn-success"
const BTN_YLW = "btn-warning"
const BTN_BLU = "btn-primary"

const TXT_WHT = "text-light";
const TXT_BLK = "text-dark";

const BG_GRN = "bg-success";
const BG_YLW = "bg-warning";

var paymentVisible = false;

// ----------------------------------------------------- //
// ------------------ AJAX functions ------------------- //
// ----------------------------------------------------- //

function remove(_instrumentId, _userId) {
    var dataDict = { instrumentId: _instrumentId, userId: _userId, deleteAll: true }
    loading(true, _instrumentId);
    $.ajax({ url: REMOVE_FROM_CART_URL, data: dataDict }).done(
        function (result) {
            if (result != null) renderCart(result);
            loading(false, _instrumentId);
        }
    );;
}

function inc(_instrumentId, _userId) {    
    loading(true, _instrumentId);
    var dataDict = { instrumentId: _instrumentId, userId: _userId }

    $.ajax({ url: Add_TO_CART_URL, data: dataDict }).done(function (result) {
        if (result != null) {
            if (!result.success) {
                let header, content, closeMsg, color;
                switch (result.data.msg) {
                    case "o":
                        content = TOO_FEW;
                        header = result.data.inst.name;
                        closeMsg = NEXT_TIME;
                        color = "r";
                        break;

                    default:
                        header = result.data.inst.name;
                        content = ERR;
                        closeMsg = OK;
                        color = "r";
                }
                popModal(header, content, closeMsg, color);
            }
            renderCart(result);
        }
        loading(false, _instrumentId);
    });
}

function dec(_instrumentId, _userId) {
    const REMOVE_FROM_CART_URL = "/Instruments/RemoveFromCart"
    var dataDict = { instrumentId: _instrumentId, userId: _userId, deleteAll: false }
    loading(true, _instrumentId);

    $.ajax({ url: REMOVE_FROM_CART_URL, data: dataDict }).done(function (result) {
        if (result != null) renderCart(result);
        loading(false, _instrumentId);
    });
}

function placeOrder(_uid) {
    if (!paymentMethodSupplied()) {
        popModal(PAY_DET, FULFIL_PAY, OK, "y", null);
        return;
    }

    let dataDict = { uid: _uid };
    $.ajax({ url: PLACE_ORDER_URL, data: dataDict }).done(function (result) {
        if (result != null && result.success != null) 
            popOrderPlacedModal(result);        
    });
}

// ----------------------------------------------------- //
// ------------------ Util functions ------------------- //
// ----------------------------------------------------- //

function renderCart(result) {
    var cartTableBody = $("#cartBody");
    var cartItemTemplate = $('#cartItemTemplate').html();
    var defTotal = 0;
    var total = 0;

    if (result.data != null && result.data.insts != null) {
        if (result.data.insts.length > 0) {
            cartTableBody.html('');

            $.each(result.data.insts, function (_ix, _cartItem) {
                let template = cartItemTemplate;

                $.each(_cartItem.inst, function (_property, _value) {
                    template = template.replaceAll('${' + _property + '}', _value);
                });

                template = template.replaceAll('${count}', _cartItem.count);
                total = _cartItem.inst.quantity == 0 ? 0 : (_cartItem.inst.price * _cartItem.count);
                template = template.replaceAll('${total}', total.toFixed(2) + " $");
                defTotal += total;
                cartTableBody.append(template);
            });
            var defTotalSpan = $('#defTotal');
            defTotalSpan.html("" + defTotal.toFixed(2) + " $");
        } else {
            var cartTable = $("#cartTable");
            var cartSummary = $("#cartSummary");
            var emptyTemplate = $("#emptyTemplate").html();

            emptyTemplate = emptyTemplate.replaceAll("emptyclass", "text-warning");
            cartTable.addClass('d-none');
            cartSummary.html(emptyTemplate);
        }
    }
}

function popModal(header, content, closeMsg, color, redirectUrl) {
    var modal = $(".modal");
    var modalHeader = $("#moadlHeader");
    var modalContent = $("#modalMessage");
    var btnModalClose = $("#modalClose");
    
    modalHeader.html(header);
    modalContent.html(content);

    btnModalClose.html(closeMsg);
    switch (color) {
        case "r":
            btnModalClose.addClass(BTN_RED);
            btnModalClose.removeClass(BTN_YLW);
            btnModalClose.removeClass(BTN_BLU);
            btnModalClose.removeClass(BTN_GRN);
            break;
        case "g":
            btnModalClose.addClass(BTN_GRN);
            btnModalClose.removeClass(BTN_YLW);
            btnModalClose.removeClass(BTN_BLU);
            btnModalClose.removeClass(BTN_RED);
            break;
        case "y":
            btnModalClose.addClass(BTN_YLW);
            btnModalClose.removeClass(BTN_GRN);
            btnModalClose.removeClass(BTN_BLU);
            btnModalClose.removeClass(BTN_RED);
            break;
        default:
            btnModalClose.addClass(BTN_BLU);
            btnModalClose.removeClass(BTN_YLW);
            btnModalClose.removeClass(BTN_GRN);
            btnModalClose.removeClass(BTN_RED);
    }
    btnModalClose.click(function () {
        modal.hide();
        if (redirectUrl != null)
            Redirect("/Users/Profile");
    });

    modal.show();
    btnModalClose.focus();
}

function popOrderPlacedModal(result) {
    if (result.success) {
        popModal(GRATZ, ORDER_SUCCESS, OK, "g", "/Users/Profile");
        return;
    }

    switch (result.data.msg) {
        case "o": // -o-ut of stock
            let content = TOO_FEW + ((result.data.left != null && result.data.left > 0) ?
                ("<br/>" + result.data.left + " left!") : "");
            popModal(result.data.inst.name, content, OK, "r", null);
            break;

        // not -a-uthrorized
        case "a": popModal(AW, NON_AUTH, OK, "r", null); break;

        // -m-alfunction
        case "m": popModal(AW, ERR, OK, "r", null); break;

        // -u-pdate exception
        case "u": popModal(AW, DB_UPDATE_ERR, OK, "r", null); break;

        default: popModal(AW, ERR, OK, "y", null);
    }
}

function loading(isLoading, _id) {
    var loader = $("#" + _id);
    const DISPLAY_NONE = "d-none";

    if (isLoading) loader.removeClass(DISPLAY_NONE);
    else loader.addClass(DISPLAY_NONE);
}

function togglePaymentDetails() {
    var paymentDetails = $("#paymentDetails");
    var btnProceed = $("#btnProceed");
    var creditNumberInput = $("#creditNumber");

    if (paymentVisible) {
        btnProceed.addClass(BG_GRN);
        btnProceed.removeClass(BG_YLW);
        btnProceed.addClass(TXT_WHT);
        btnProceed.removeClass(TXT_BLK);
        paymentDetails.addClass(DISP_NONE);
        btnProceed.html(PROCEED);
    } else { // !paymentVisible
        btnProceed.addClass(BG_YLW);
        btnProceed.removeClass(BG_GRN);

        btnProceed.removeClass(TXT_WHT);
        btnProceed.addClass(TXT_BLK);
                
        paymentDetails.removeClass(DISP_NONE);
        btnProceed.html(LATER);
        creditNumberInput.focus();
    }

    paymentVisible = !paymentVisible;
}

function paymentMethodSupplied() {
    var creditNumberInput = $("#creditNumberInput");
    var creditExpInput = $("#creditExpInput");
    var cvvInput = $("#cvvInput");

    let creditNumber = creditNumberInput.val();
    let creditExp = creditExpInput.val();
    let cvv = cvvInput.val();

    return ((creditNumber != "") && (creditExp != "") && (cvv != ""));
}

function Redirect(dst) {
    window.location.href = dst;
}