// js page for searching instruments


/*$(document).ready(function () {
    // ... do some code here ...
}); => equivalent to: */

// consts
const NO_RESULTS_HEADING = "<h2>No results found.</h2>";
const DISPLAY_NONE = 'd-none';
const IMAGE = "image";
const EMPTY = "";
const SEARCH_JSON_URL = "/Instruments/SearchJson";

// @@ -- elements -- @@ //
var tableBody;
var btnSearch;
var btnClear;
var loader;

// Search Inputs
var nameInput;

var categoryInput;
var brandInput;
var lPriceInput;
var uPriceInput;

$(function () {
    tableBody = $('tbody');
    btnSearch = $('#btnSearch');
    btnClear = $('#btnClear');
    loader = $('#searchLoader');

    nameInput = $('#nameInput');
    categoryInput = $("#categoryInput");
    brandInput = $("#brandInput");
    lPriceInput = $("#lPriceInput");
    uPriceInput = $("#uPriceInput");

    btnSearch.click(function () {
        nameInputVal = nameInput.val();
        categoryInputVal = categoryInput.val();
        brandInputVal = brandInput.val().trim();
        lPriceInputVal = lPriceInput.val().trim();
        uPriceInputVal = uPriceInput.val().trim();

        if (lPriceInputVal != EMPTY && isNaN(lPriceInputVal)) {
            alert("Lowest price must be a number");
            return;
        }

        if (uPriceInputVal != EMPTY && isNaN(uPriceInputVal)) {
            alert("Highest price must be a number");
            return;
        }

        if (lPriceInputVal != EMPTY && uPriceInputVal != EMPTY && uPriceInputVal < lPriceInputVal) {
            alert("Highest price must be greater than the lowest price!");
            return;
        }

        sendAJAX(
            getSearchDict(
                nameInputVal,
                categoryInputVal,
                brandInputVal,
                lPriceInputVal,
                uPriceInputVal
            )
        );

    });

    btnClear.on("click", function () {
        sendAJAX({ all: true });
        clearSearchFields();
    });
});

function sendAJAX(dataDict) {
    loader.removeClass(DISPLAY_NONE);
    $.ajax({ url: SEARCH_JSON_URL, data: dataDict })
        .done(function (result) {
            if (result != null)
                renderInstrumentsTable(result);
            loader.addClass(DISPLAY_NONE);
        });
}

function renderInstrumentsTable(result) {
    let resLen = result != null ? result.length : 0;
    if (resLen != 0 && resLen > 0) {
        tableBody.html('');
        $.each(result, function (_ix, _instrument) {
            let template = $('#rowTemplate').html();

            $.each(_instrument, function (_property, _value) {
                if (_property == IMAGE) {
                    template = (_value != null) ?
                        template.replaceAll('${' + _property + '}', '<img src="data:image/png;base64,' + _value + '" style="height:100px; width:100px; border-radius:100px" />') :
                        template.replaceAll('${' + _property + '}', '<span class="text-danger">No available image.</span>');
                }
                else template = template.replaceAll('${' + _property + '}', _value);
            });
            tableBody.append(template);
        });
    } else tableBody.html(NO_RESULTS_HEADING);
}

function clearInputFields() {
    nameInput = nameInput.val(EMPTY);
    categoryInput = categoryInput.val(EMPTY);
    brandInput = brandInput = nameInput.val(EMPTY);
    lPriceInput = lPriceInput.val(EMPTY);
    uPriceInput = uPriceInput.val(EMPTY);
}

function getSearchDict(_name, _category, _brand, _lPrice, _uPrice) {
    return {
        all: isAllEmpty(_name, _category, _brand, _lPrice, _uPrice),
        name: _name,
        category: _category,
        brand: _brand,
        lPrice: _lPrice == EMPTY ? -1 : _lPrice,
        uPrice: _uPrice == EMPTY ? -1 : _uPrice
    };
}

function isAllEmpty(_name, _category, _brand, _lPrice, _uPrice) {
    return (
        _name == EMPTY
        && _category == EMPTY
        && _brand == EMPTY
        && _lPrice == EMPTY
        && _uPrice == EMPTY
    );
}