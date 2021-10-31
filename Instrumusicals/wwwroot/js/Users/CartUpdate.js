// js page for cart updates

$(function () {


})

function remove(_instrumentId, _userId)
{
    const URL = "/Instruments/RemoveFromCart"
    var dataDict = { instrumentId: _instrumentId, userId: _userId}
    loading(true, _instrumentId);
    $.ajax({ url: URL, data: dataDict }).done(function (result) {
        var cartTableBody = $("#cartBody");
        var cartItemTemplate = $('#cartItemTemplate').html();
        var defTotal = 0;
        var total = 0;

        if (result != null) {

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
        loading(false, _instrumentId);
    });;
}

function inc(instrumentId, userId) {
    alert("incresing inst " + instrumentId + " from " + userId + " wishlist");
}

function dec(instrumentId, userId) {
    alert("decresing inst " + instrumentId + " from " + userId + " wishlist");
}

function placeOrder() {
}

function loading(isLoading, _id) {
    var loader = $("#" + _id);
    const DISPLAY_NONE = "d-none";
    
    if (isLoading) loader.removeClass(DISPLAY_NONE);
    else loader.addClass(DISPLAY_NONE);
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}