// js page for cart updates

// $(function () { });

function remove(_instrumentId, _userId) {
    const URL = "/Instruments/RemoveFromCart"
    var dataDict = { instrumentId: _instrumentId, userId: _userId, deleteAll: true }
    loading(true, _instrumentId);
    $.ajax({ url: URL, data: dataDict }).done(function (result) {
        if (result != null) renderCart(result);
        loading(false, _instrumentId);
    });;
}

function inc(_instrumentId, _userId) {
    const URL = "/Instruments/AddToCart"
    const TOO_FEW = "There is not enough occurrences of this insteument";
    const INST_NOT_AVAIL = "The instrument is currently not available."
    const NEXT_TIME = "Maybe next time!"
    const ERR = "Something went wrong!"
    var dataDict = { instrumentId: _instrumentId, userId: _userId }

    loading(true, _instrumentId);

    $.ajax({ url: URL, data: dataDict }).done(function (result) {
        console.log(result);
        if (result != null) {
            if (!result.success) {
                let content, header, color, closeMsg;
                switch (result.data.msg) {
                    case "o":
                        content = TOO_FEW;
                        header = result.data.inst.name;
                        closeMsg = NEXT_TIME;
                        color = "y";
                        break;

                    default:
                        content = ERR;
                        header = result.data.inst.name;
                        closeMsg = "OK";
                        color = "r";
                } popModal(header, content, closeMsg, color);
            }
            renderCart(result);
        }
        loading(false, _instrumentId);
    });
}

function dec(_instrumentId, _userId) {
    const URL = "/Instruments/RemoveFromCart"
    var dataDict = { instrumentId: _instrumentId, userId: _userId, deleteAll: false }
    loading(true, _instrumentId);

    $.ajax({ url: URL, data: dataDict }).done(function (result) {
        if (result != null) renderCart(result);
        loading(false, _instrumentId);
    });
}

function placeOrder() { }

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

function popModal(header, content, closeMsg, color) {
    var modal = $(".modal");
    var modalHeader = $("#moadlHeader");
    var modalContent = $("#modalMessage");
    var btnModalClose = $("#modalClose");

    let color_class;
    switch (color) {
        case "r": color_class = "btn-danger"; break;
        case "g": color_class = "btn-success"; break;
        case "y": color_class = "btn-warning"; break;
        default: color_class = "btn-primary";
    }

    modalHeader.html(header);
    modalContent.html(content);

    btnModalClose.html(closeMsg);
    btnModalClose.addClass(color_class)
    btnModalClose.click(function () {
        modal.toggle();
    });

    modal.show();
    btnModalClose.focus();
}

function loading(isLoading, _id) {
    var loader = $("#" + _id);
    const DISPLAY_NONE = "d-none";

    if (isLoading) loader.removeClass(DISPLAY_NONE);
    else loader.addClass(DISPLAY_NONE);
}