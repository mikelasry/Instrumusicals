// js page for searching instruments


/*$(document).ready(function () {
    // ... do some code here ...
}); => equivalent to: */


$(function () {

    // consts
    const NO_RESULTS_HEADING = "<h2>No results found.</h2>";
    const DISPLAY_NONE = 'd-none';
    const IMAGE = "image";
    const EMPTY = "";
    const SEARCH_JSON_URL = "/Instruments/SearchJson";

    // elements
    var btnSearch = $('#btnSearch');
    var btnClear = $('#btnClear');
    var loader = $('#searchLoader');
    var nameInput = $('#nameInput');

    function renderInstrumentsTable(dataDict) {
        loader.removeClass(DISPLAY_NONE);
        let tableBody = $('tbody');

        $.ajax({ url: SEARCH_JSON_URL, data: dataDict }).done(function (result) {
            tableBody.html('');
            let resLen = result != null ? result.length : 0;
            if (resLen != null && resLen > 0) {

                $.each(result, function (_ix, _instrument) {
                    let template = $('#rowTemplate').html();

                    $.each(_instrument, function (_property, _value) {
                        if (_property == IMAGE) {
                            template = (_value != null) ?
                                template.replace('${' + _property + '}', '<img src="data:image/png;base64,' + _value + '" style="height:100px; width:100px; border-radius:100px" />') :
                                template.replace('${' + _property + '}', '<span class="text-danger">No image yet.</span>');
                        }
                        else template = template.replaceAll('${' + _property + '}', _value);
                    });
                    tableBody.append(template);
                });

            } else tableBody.html(NO_RESULTS_HEADING);

            loader.addClass(DISPLAY_NONE);
        });
    }

    btnSearch.click(function () {
        renderInstrumentsTable({
            all: nameInput.val().trim() == EMPTY ? true : false,
            name: nameInput.val().trim()
        });
    });

    btnClear.click(function () {
        renderInstrumentsTable({ all: true, name: EMPTY });
        nameInput.val(EMPTY);
    });
});


