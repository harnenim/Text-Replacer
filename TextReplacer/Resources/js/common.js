﻿var alt   = false;
var ctrl  = false;
var shift = false;
var onMouseUp = null;
var onMouseMove = null;

var showDrag = false;
function setShowDrag(dragging) {
	showDrag = dragging;
}
function getRectOf(selector) {
	var obj = $(selector);
	var pos = obj.offset();
	return [pos.left, pos.top, obj.outerWidth(), obj.outerHeight()];
}
function getPositionOf(selector) {
	return getRectOf(selector).join(",");
}

$(function() {
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
	window.onmousewheel = function() {
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
	/*
	doc.on("keydown", function(e) {
		switch (e.keyCode) {
			case 16: shift = true; break;
			case 17: ctrl  = true; break;
			case 18: alt   = true; break;
		}
	});
	doc.on("keyup", function(e) {
		switch (e.keyCode) {
			case 16: shift = false; break;
			case 17: ctrl  = false; break;
			case 18: alt   = false; break;
		}
	});
	*/
	doc.on("dragover", function() {
		if (showDrag) {
			window.external.hideDragging();
		}
	});
});