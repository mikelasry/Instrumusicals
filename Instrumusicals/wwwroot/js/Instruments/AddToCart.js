$(function () {

    var modal = $(".modal");
    var btnModalClose = $("#modalClose");

    btnModalClose.click(function () {
        modal.toggle();
    });
});

function addToCart(iid, uid) {

    const TOO_FEW = "There is not enough occurrences of this insteument";
    const ANOTHER_ONE= "Another instance of this instrument has been added to your cart";
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
            if (result != null) {
                modalHeader.html(result.data.inst.name);
                var content = "";

                if (!result.success) {
                    btnModalClose.addClass(BTN_RED);
                    btnModalClose.removeClass(BTN_GREEN);
                    btnModalClose.html(NEXT_TIME);
                }

                switch (result.data.msg) {
                    case 'f':
                        content = ANOTHER_ONE;
                        break;
                    case 'o':
                        content = TOO_FEW;
                        break;
                    case 's':
                        content = INST_ADDED;
                        break;
                    case "NAV":
                        content = INST_NOT_AVAIL;
                        break;
                }

                modalContent.html(content);
                modal.show();
                btnModalClose.focus();
            }
            loader.addClass(DISPLAY_NONE);
        });

}
