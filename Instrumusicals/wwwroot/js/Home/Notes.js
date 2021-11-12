var canvas;
var ctx;

const Status = {
    PLAY: 2800,
    WAIT: 2801,
    STOP: 2802
};

const RESOLUTION = 7;
const MAX_LEVELS = 1.5;
const FLOOR = 72;
const CIEL = 0;
const RIGHT = 172;
const LEFT = 0;
const CENTER = RIGHT / 2;
const MIDDLE = FLOOR / 2;
const STEP = FLOOR / RESOLUTION;
const NOTE_RADIUS = STEP / 3;
const NOTE_HEIGHT = STEP * 1.75;
const NOTE_WIDTH = 2;
const LINE1 = STEP * 2;
const LINE2 = STEP * 3;
const LINE3 = STEP * 4;
const LINE4 = STEP * 5;
const LINE5 = STEP * 6;

const RED = "red";
const GREEN = "green";
const BLUE = "blue";
const YELLOW = "yellow";
const WHITE = "white";
const BLACK = "black";
const ORANGE = "orange";


var lines = [LINE1, LINE2, LINE3, LINE4, LINE5];
var colors = [RED, GREEN, BLUE, YELLOW, RED, GREEN, BLUE, YELLOW, RED, GREEN, BLUE, YELLOW, ORANGE, WHITE, BLACK];

var btnPlay;

var canvasIntervalId;
var notesIntervalId;

var notes;
var frames = 0;

var playing = false;
var rendering = false;
var pushingNotes = false;

$(function () {
    canvas = document.getElementById("notesCanvas"); //$("#notesCanvas");
    ctx = canvas.getContext("2d");
    notes = [];

    btnPlay = $("#btnsPlayCanvas");
    btnPlay.on("click", function () {
        if (rendering && !pushingNotes) {            
            return;
        } if (!playing) {
            canvasStart();
        } else { canvasEnd(); }

        playing = !playing;
    });

    drawCanvasBaseLines();
});

// @@@@@@@@@@@@@@@@@@@@@@@@@ Canvas Fns @@@@@@@@@@@@@@@@@@@@@@@@ //

function canvasStart() {
    setRenderCanvasInterval();
    setPushingNotesInterval();
    changeBtnView(Status.STOP);
}

function canvasEnd() {
    clearInterval(notesIntervalId);
    // console.log("Shutting notes pushing....");
    pushingNotes = false;
    changeBtnView(Status.WAIT);
}

function setRenderCanvasInterval() {
    if (!rendering && !pushingNotes) {
        // console.log("Starting rendering ....");  
        canvasIntervalId = setInterval(function () {
            clearCanvas();
            if (notes.length < 1 && !pushingNotes) {
                clearInterval(canvasIntervalId)
                // console.log("Shutting rendering ....");  
                rendering = false;
                changeBtnView(Status.PLAY);
            }
            for (var i = 0; i < notes.length; i++) {
                let note = notes[i];
                if (note.x < LEFT) {
                    notes.shift();
                    continue;
                }
                renderNote(note);
                note.x -= 5;
            }
        }, 333);
        rendering = true;
    }
}

function setPushingNotesInterval() {
    if (!pushingNotes) {
        // console.log("Starting notes pushing....");  
        notesIntervalId = setInterval(
            function () {
                notes.push(newNote(randBool(), randColor(), randSpace(), randLevels()));
            },
            2500
        );
        pushingNotes = true;
    }
}
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

function drawLine(xStart, yStart, xEnd, yEnd, width, color) {
    ctx.beginPath();
    ctx.moveTo(xStart, yStart);
    ctx.lineTo(xEnd, yEnd);
    ctx.lineWidth = width;
    ctx.strokeStyle = color;
    ctx.stroke();
}

function drawCircle(x, y, color) {
    ctx.beginPath();
    ctx.arc(x, y, NOTE_RADIUS, 0, 2 * Math.PI);
    ctx.fillStyle = color;
    ctx.strokeStyle = color;
    ctx.fill();
    ctx.stroke();
}

function drawCanvasBaseLines() {
    for (var i = 0; i < lines.length; i++)
        drawLine(LEFT, lines[i], RIGHT, lines[i], 1, '#000');
}

function clearCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    drawCanvasBaseLines();
}

// @@@@@@@@@@@@@@@@@@@@ Render Notes Fns @@@@@@@@@@@@@@@@@@@@@@@@@@@ //

function renderNote(note) {
    if (note.isSingular) drawSingleNote(note.x, note.y, note.color);
    else drawDoubleNote(note.x, note.y, note.color, note.space, note.levels);
}

function baseNote(x, baseLine, color, isStandAlone) {
    drawCircle(x, baseLine, color);

    var xNoteLine = getTopNoteX(x);
    var topOfNoteY = getTopNoteY(baseLine);
    drawLine(xNoteLine, topOfNoteY, xNoteLine, baseLine, NOTE_WIDTH, color);

    if (isStandAlone) {
        var chupchikTopX = xNoteLine;
        var chupchikTopY = topOfNoteY + (NOTE_HEIGHT / 4);
        var chupchikBottomX = xNoteLine + NOTE_RADIUS;
        var chupchikBottomY = topOfNoteY + (NOTE_HEIGHT / 2);
        drawLine(chupchikTopX, chupchikTopY, chupchikBottomX, chupchikBottomY, NOTE_WIDTH, color);
    }
}

function drawSingleNote(x, y, color) {
    baseNote(x, y, color, true);
}

function drawDoubleNote(x, y, color, _space, _levels) {
    _space = fixNoteSpace(_space);
    _levels = fixNoteLevels(_levels);
    baseNote(x, y, color, false);
    baseNote(x + _space * NOTE_RADIUS, y + (_levels * STEP), color, false);
    drawLine(
        getTopNoteX(x),
        getTopNoteY(y),
        getTopNoteX(x + _space * NOTE_RADIUS),
        getTopNoteY(y + (_levels * STEP)), NOTE_WIDTH + 1.5,
        color
    );
}

// @@@@@@@@@@@@@@@@@@@@@ Note Util Fns @@@@@@@@@@@@@@@@@@@@@@@@@ //

function newNote(_isSingular, _color, _space = 4, _levels = 1) {
    var line = randLine();
    return {
        x: RIGHT,
        y: line,
        isSingular: _isSingular,
        color: _color,
        space: _space,
        levels: _levels,
    };
}

function getTopNoteX(xBase) {
    return xBase + NOTE_RADIUS;
}

function getTopNoteY(yBase) {
    return yBase - NOTE_HEIGHT;
}

function fixNoteSpace(_space) {
    _space = _space == null || isNaN(_space) ? 5 : _space;
    _space = _space < 3 ? 3 : _space;
    _space = _space > 7 ? 7 : _space;
    return _space;
}

function fixNoteLevels(_levels) {
    _levels = _levels == null || isNaN(_levels) ? 1 : _levels;
    _levels = _levels < -MAX_LEVELS ? -MAX_LEVELS : _levels;
    _levels = _levels > MAX_LEVELS ? MAX_LEVELS : _levels;
    return _levels;
}

// @@@@@@@@@@@@@@@@@@@@@@@@@ Random Util Fns @@@@@@@@@@@@@@@@@@@ //

function randInt(max) {
    return Math.floor(Math.random() * max);
}

function randBool() {
    return randInt(2) % 2 == 0;
}

function randLine() {
    var addition = randBool() ? 0.5 : 0;
    var reversed = randBool() ? -1 : 1;
    return (reversed * addition * STEP) + lines[randInt(lines.length)];
}

function randColor() {
    return colors[randInt(colors.length)];
}

function randSpace() {
    return randInt(5) + 3;
}

function randLevels() {
    return randInt(2);
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

function changeBtnView(status) {
    switch (status) {
        case Status.PLAY:
            btnPlay.removeClass('text-danger');
            btnPlay.removeClass('text-warning');
            btnPlay.addClass('text-success');
            btnPlay.html("PLAY");
            break;
        case Status.STOP:
            btnPlay.removeClass('text-success');
            btnPlay.removeClass('text-warning');
            btnPlay.addClass('text-danger');
            btnPlay.html("STOP");
            break;
        case Status.WAIT:
            btnPlay.removeClass('text-danger');
            btnPlay.removeClass('text-success');
            btnPlay.addClass('text-warning');
            btnPlay.html("WAIT");
            break;
    }

}