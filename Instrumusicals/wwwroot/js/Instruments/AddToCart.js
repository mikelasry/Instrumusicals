$(function () {

    var modal = $(".modal");
    var btnModalClose = $("#modalClose");

    btnModalClose.click(function () {
        modal.toggle();
    });
});

function addToCart(iid, uid) {

    const DISPLAY_NONE = "d-none";
    const BTN_GREEN= "btn-success";
    const BTN_RED = "btn-danger";
    const URL = "/Instruments/AddToCart";
    const NEXT_TIME = "Maybe next time!"
    const INST_ADDED = "The instrument has been successfully added to your cart."
    const INST_NOT_AVAIL = "The instrument is currently not available. =("

    var dict = { instrumentId: iid, userId: uid };

    var modal = $(".modal");
    var loader = $("#cartLoader");

    var modalHeader= $("#moadlHeader");
    var modalContent = $("#modalMessage");
    var btnModalClose = $("#modalClose");

    loader.removeClass(DISPLAY_NONE)

    $.ajax({ url: URL, data: dict })
        .done(function (result) {
            //console.log(result);
            if (result != null ) {
                modalHeader.html(result.data.inst.name);
                var content = INST_ADDED;
                if (!result.success && result.data.msg == "NAV") {
                    btnModalClose.addClass(BTN_RED);
                    btnModalClose.removeClass(BTN_GREEN);
                    btnModalClose.html(NEXT_TIME);
                    content = INST_NOT_AVAIL;
                }
                modalContent.html(content);
                modal.show();
            }
            loader.addClass(DISPLAY_NONE);
        });

}
