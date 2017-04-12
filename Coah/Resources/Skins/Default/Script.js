"use strict";
var Coah;
(function (Coah) {
    function jumpToComment(commentIndex) {
        document.getElementById("res-" + commentIndex).scrollIntoView();
    }
    Coah.jumpToComment = jumpToComment;
    function renderComments(commentMarkerId, comments) {
        var marker = document.evaluate("//comment()[. = \"" + commentMarkerId + "\"]", document.body, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
        var parent = marker.parentElement;
        parent.insertAdjacentHTML("beforeend", comments.join(""));
        parent.appendChild(marker);
    }
    Coah.renderComments = renderComments;
    function clearComments(commentMarkerId) {
        var marker = document.evaluate("//comment()[. = \"" + commentMarkerId + "\"]", document.body, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
        var parent = marker.parentElement;
        for (var _i = 0, _a = Array.prototype.slice.call(parent.children); _i < _a.length; _i++) {
            var i = _a[_i];
            i.remove();
        }
        parent.appendChild(marker);
    }
    Coah.clearComments = clearComments;
    function updateSenderIdentifierCounter(senderIdentifier, commentIndices) {
        for (var _i = 0; _i < commentIndices.length; _i++) {
            var commentIndex = commentIndices[_i];
            var elem = document.querySelector("#res-" + commentIndex + " .sender-identifier-counter");
            if (elem)
                elem.textContent = "(" + (commentIndices.indexOf(commentIndex) + 1) + "/" + commentIndices.length + ")";
        }
    }
    Coah.updateSenderIdentifierCounter = updateSenderIdentifierCounter;
    function updateReferenceCounter(commentIndex, commentIndices) {
        var elem = document.querySelector("#res-" + commentIndex + " .reference-counter");
        if (elem)
            elem.textContent = commentIndices.length ? "*" + commentIndices.length : "";
    }
    Coah.updateReferenceCounter = updateReferenceCounter;
})(Coah || (Coah = {}));
window.addEventListener("scroll", function (ev) {
    if (window.pageYOffset == 0)
        document.documentElement.classList.remove("scrolled");
    else
        document.documentElement.classList.add("scrolled");
});
