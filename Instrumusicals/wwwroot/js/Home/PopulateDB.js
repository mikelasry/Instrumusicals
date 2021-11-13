
var instrument_ix = 0;
var output;

const CREATE_INST_URL = "/Instruments/Create";
const POST = "POST";

// on document load
$(function () {
    output = $("#output");
});

function populateDB() {
   // alert();
   createInstrument(data[0]);
}

function createInstrument(inst) {

    var dataDict = getDataDict(inst);

    toDataURL(inst.imgPath, function (dataUrl) {
        console.log('RESULT:', dataUrl);
    });
    alert("done!!!!!!!");
    return;

    $.ajax({
        url: "Instruments/Populate",
        data: dataDict,
        type: "POST"
    })
    .done(function (result) {
        alert("Created");
        
    });
}

function getDataDict(inst) {
    var dict = {
        name: inst.name,
        brand: inst.brand,
        categoryId: getCategoryId(inst.category),
        description: inst.description,
        quantity: inst.quantity,
        price: inst.price
    };
    return dict;
}

function getCategoryId(catName) {
    switch (catName) {
        case "Drums": return 1;
        case "Keys": return 2;
        case "DJ-Gear": return 5;
        case "Strings": return 11;
        case "Accessories": return 13;
        default: return 0;
    }
}

function toDataURL(src, callback) {
    let image = new Image();
    image.crossOrigin = 'Anonymous';
    image.onload = function () {
        let canvas = document.createElement('canvas');
        let ctx = canvas.getContext('2d');
        let dataURL;
        canvas.height = 80;//this.naturalHeight;
        canvas.width = 80;//this.naturalWidth;
        ctx.drawImage(this, 0, 0);
        dataURL = canvas.toBlob(function () { alert("done2");});
        callback(dataURL);
        canvas.to
    };
    image.src = src;
    if (image.complete || image.complete === undefined) {
        image.src = "data:image/gif;base64, R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==";
        image.src = src;
    }
}
