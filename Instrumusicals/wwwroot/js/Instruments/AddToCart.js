$(function () {

    var modal = $(".modal");
    var btnModalClose = $("#modalClose");

    btnModalClose.click(function () {
        modal.toggle();
    });
});

function addToCart(iid, uid) {

    const URL = "/Instruments/AddToCart";
    const DISPLAY_NONE = "d-none";
    var dict = { instrumentId: iid, userId: uid };

    var modal = $(".modal");
    var modalHeader= $("#moadlHeader");
    var loader = $("#cartLoader");

    loader.removeClass(DISPLAY_NONE)

    $.ajax({ url: URL, data: dict })
        .done(function (result) {
            console.log(result);
            if (result != null && result.success) {
                modalHeader.html(result.data.name);
                modal.show();
            }
            loader.addClass(DISPLAY_NONE);
        });

}
