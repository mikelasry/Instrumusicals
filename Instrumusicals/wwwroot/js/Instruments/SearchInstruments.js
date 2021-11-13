// js page for searching instruments


/*$(document).ready(function () {
    // ... do some code here ...
}); => equivalent to: */

// consts
const NO_RESULTS_HEADING = "No Instruments Found!";
const DISPLAY_NONE = 'd-none';
const ADMIN_DATA = '${admin-data}';
const IMAGE = "image";
const EMPTY = "";
const INSTS_FOUND = " Instruments Found!";
const ALL_CTGRS = "All Categories";
const ALL_BRNDS = "All Brands";
const SEARCH_INSTRUMENTS_URL = "/Instruments/SearchJson";

// @@ -- elements -- @@ //
var tableBody;
var btnSearch;
var btnClear;
var loader;

var instCounter;
var rowTemplate;
var adminDataTemplate;

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

    instCounter = $('#instCounter');

    rowTemplate = $('#rowTemplate').html();
    adminDataTemplate = $('#adminDataTemplate').html();

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
        clearInputFields();
    });
});

function sendAJAX(dataDict) {
    loader.removeClass(DISPLAY_NONE);
    $.ajax({ url: SEARCH_INSTRUMENTS_URL, data: dataDict })
        .done(function (result) {
            if (result != null)
                renderInstrumentsTable(result);
            loader.addClass(DISPLAY_NONE);
        });
}

function renderInstrumentsTable(result) {

    let condA = result != null;
    let condB = result.success;
    let condC = result.data != null;
    let condD = result.data.insts != null;

    let valid = condA && condB && condC && condD;
    if (!valid) {
        alert("something went wrong!");
        return;
    }

    let resLen = result.data.insts.length;
    var instruments = result.data.insts;
    var isSearcherAdmin = result.data.isAdmin;

    tableBody.html('');

    if (resLen > 0) {
        instCounter.html(resLen + INSTS_FOUND);
        $.each(instruments, function (_ix, _instrument) {

            let userRow = rowTemplate;
            let adminCols = adminDataTemplate;

            userRow = isSearcherAdmin ?
                userRow.replaceAll(ADMIN_DATA, adminCols) :
                userRow.replaceAll(ADMIN_DATA, EMPTY);            

            $.each(_instrument, function (_property, _value) {
                if (_property == "price")
                    _value = _value.toLocaleString(undefined, { minimumFractionDigits: 2 });
                if (_property == IMAGE) {
                    userRow = (_value != null) ?
                        userRow.replaceAll('${' + _property + '}', '<img src="data:image/png;base64,' + _value + '" style="height:100px; width:100px; border-radius:100px" />') :
                        userRow.replaceAll('${' + _property + '}', '<span class="text-danger">No available image.</span>');
                } else userRow = userRow.replaceAll('${' + _property + '}', _value);
            });
            tableBody.append(userRow);
        });
    } else instCounter.html(NO_RESULTS_HEADING);
}

function clearInputFields() {
    nameInput = nameInput.val(EMPTY);
    categoryInput = categoryInput.val(ALL_CTGRS).change();
    brandInput = brandInput.val(ALL_BRNDS).change();
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
        && _category == "All Categories"
        && _brand == "All Brands"
        && _lPrice == EMPTY
        && _uPrice == EMPTY
    );
}