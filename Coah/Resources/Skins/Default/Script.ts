"use strict";

module Coah
{
	export function jumpToComment(commentIndex: number)
	{
		document.getElementById(`res-${commentIndex}`).scrollIntoView();
	}

	export function renderComments(commentMarkerId: string, comments: string[])
	{
		let marker = document.evaluate(`//comment()[. = "${commentMarkerId}"]`, document.body, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
		let parent = marker.parentElement;

		parent.insertAdjacentHTML("beforeend", comments.join(""));
		parent.appendChild(marker);
	}

	export function clearComments(commentMarkerId: string)
	{
		let marker = document.evaluate(`//comment()[. = "${commentMarkerId}"]`, document.body, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
		let parent = marker.parentElement;

		for (let i of <HTMLElement[]>Array.prototype.slice.call(parent.children))
			i.remove();

		parent.appendChild(marker);
	}

	export function updateSenderIdentifierCounter(senderIdentifier: string, commentIndices: number[])
	{
		for (let commentIndex of commentIndices)
		{
			let elem = document.querySelector(`#res-${commentIndex} .sender-identifier-counter`)

			if (elem)
				elem.textContent = `(${(commentIndices.indexOf(commentIndex) + 1) }/${commentIndices.length})`;
		}
	}

	export function updateReferenceCounter(commentIndex: number, commentIndices: number[])
	{
		let elem = document.querySelector(`#res-${commentIndex} .reference-counter`)

		if (elem)
			elem.textContent = commentIndices.length ? `*${commentIndices.length}` : "";
	}
}

window.addEventListener("scroll", (ev) =>
{
	if (window.pageYOffset == 0)
		document.documentElement.classList.remove("scrolled");
	else
		document.documentElement.classList.add("scrolled");
});