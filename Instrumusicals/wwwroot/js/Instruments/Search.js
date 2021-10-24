// js page for searching instruments


/*$(document).ready(function () {
    // ... do some code here ...
}); => equivalkent to: */

$(function () {

    // consts
    const NO_RESULTS = "<h2>No results found.</h2>";
    const DISPLAY_NONE = 'd-none';
    const SEARCH_JSON_URL = "/Instruments/SearchJson";

    // elements
    var btnSearch = $('#btnSearch');
    var loader = btnSearch.next();
    var nameInput = btnSearch.prev();
    var tableBody = $('tbody');
    var rowTemplateHTML = $('#rowTemplate').html();

    btnSearch.click(function () {


        let instrumentName = nameInput.val().trim();
        
        loader.removeClass(DISPLAY_NONE);
        $.ajax({
            url: SEARCH_JSON_URL,
            data: { name: nameInput.val().trim() }
        }).done(function (result) {

            tableBody.html('');
            let resLen = result != null ? result.length : 0;

            if (resLen > 0) {
                $.each(result, function (_, instrument) {
                    let template = rowTemplateHTML;

                    $.each(instrument, function (property, value) {
                        if (property == "image") {
                            template = (value != null) ?
                                template.replace('${' + property + '}', '<img src="data:image/png;base64,' + value + '" style="height:100px; width:100px; border-radius:100px" />') :
                                    template.replace('${' + property + '}', '<span class="text-danger">No available image</span>');
                        } else template = template.replaceAll('${' + property + '}', value);
                    });
                    tableBody.append(template);
                });
            } else tableBody.html(NO_RESULTS);

            loader.addClass(DISPLAY_NONE);
        });

    });
});
