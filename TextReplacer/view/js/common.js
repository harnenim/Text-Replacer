﻿var alt   = false;
var ctrl  = false;
var shift = false;
var onMouseUp = null;
var onMouseMove = null;

var showDrag = false;
function setShowDrag(dragging) {
    showDrag = dragging;
}
function getRectOf(id) {
    var obj = $("#" + id);
    var pos = obj.offset();
    return [pos.left, pos.top, obj.outerWidth(), obj.outerHeight()];
}
function getPositionOf(id) {
    return getRectOf(id).join(",");
}
function setDroppable(id) {
    var view = $("#" + id);
    view.on("dragleave", function () {
        return false;
    });
    view.on("dragover", function () {
        if (!showDrag) {
            window.external.showDragging(id);
        }
        return false;
    });
}
function setClickEvent(id, action) {
    $("#" + id).on("click", function () {
        window.external[action]();
    });
}

function call(names, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9) {
    var func = window;
    names = names.split(".");
    for (var i = 0, name; name = names[i]; i++) {
        func = func[name];
    }
    return func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);
}

$(function () {
    var doc = $(document);
    var views = $(".view");
    views.css({"height": window.innerHeight});
    window.addEventListener("resize", function() {
        views.css({"height": window.innerHeight});
    });
    window.onkeydown = function() {
        switch(event.keyCode) {
            case 16: shift = true; break;
            case 17: ctrl  = true; break;
            case 18: alt   = true; break;
            case 116: return false; // F5 새로고침 방지
        }
    };
    window.onkeyup = function() {
        switch(event.keyCode) {
            case 16: shift = false; break;
            case 17: ctrl  = false; break;
            case 18: alt   = false; break;
        }
    };
    window.onmousewheel = function () {
        // 확대/축소 방지
        if (ctrl) {
            return false;
        }
    };
    window.addEventListener("mousemove", function() {
        if (onMouseMove) {
            onMouseMove(event);
        }
    });
    window.addEventListener("mouseup", function() {
        if (onMouseUp && typeof onMouseUp == "function") {
            onMouseUp(event);
            onMouseUp = true;
        }
    });
    doc.on("dragover", function () {
        // 드래그 중 벗어났으면 해제
        if (showDrag) {
            window.external.hideDragging();
        }
    });

        // 우클릭 방지
    doc.on("contextmenu", function () {
        return false;
    });

    if (window.external.InitAfterLoad) window.external.InitAfterLoad();
});