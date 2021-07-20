// line number & other addon for highlight.js
// by alikong.tistory.com

window.addEventListener("load", function() {
  var codes = document.querySelectorAll('pre > code');
  for (var i = 0; i < codes.length; i++) {
    var code = codes[i];
    var wrapper = (code.parentNode.nodeName == "PRE") ? code.parentNode : code;
		var hidden = !(wrapper.offsetHeight);
		if (hidden) { wrapper.style.display = 'block'; }		
		if (!wrapper.dataset.skip) {
			var source = code.innerHTML;		
			if (!wrapper.dataset.noline) {  
				code.classList.add('lnWrap');
			} else {
				code.classList.add('noline');
			}
			var color = window.getComputedStyle(code, null).getPropertyValue("color");
			var start = (wrapper.dataset.start === undefined) ? 1 : Number(wrapper.dataset.start);
			var tempMarker = (!wrapper.dataset.marker) ? '' : wrapper.dataset.marker;
			tempMarker = tempMarker.replace(/\s/g, '').replace(/\[|\]/g, '').split(',').map(function(mark){
				var res = [];
				if (mark.indexOf('-') > -1) {
					var nums = mark.split('-');
					for (var i = Math.min.apply(null, nums); i <= Math.max.apply(null, nums); i++) {
						res.push(i);
					}
					return res;
				} else {
					return Number(mark);
				}
			});
			var marker = tempMarker.reduce(function(a, b) { return a.concat(b); }, []); 
			source = source.replace(/<span class="hljs-comment">(.|\n)*?<\/span>/g, function(comments) {
				return comments.replace(/\r?\n/g, function() {
					return '</span>\n<span class="hljs-comment">'
				});
			});
			
			if (!wrapper.dataset.nomatch) {		
				// for matching braces
				source = source.replace(/\{/g, '<span class="brace">{</span>');
				source = source.replace(/\[/g, '<span class="brace">[</span>');
				source = source.replace(/\(/g, '<span class="brace">(</span>');
				source = source.replace(/\}/g, '<span class="brace">}</span>');
				source = source.replace(/\]/g, '<span class="brace">]</span>');
				source = source.replace(/\)/g, '<span class="brace">)</span>');
			}
			
			var lns = source.split('\n'), nLns = lns.length, newLns = [];
			for (var ii = 0; ii < nLns; ii++) {
				var ln = lns[ii];
				if ((ii != nLns - 1) || (ii == nLns - 1 && ln.length && ln != '</span>')) {
					var lnTag = (wrapper.dataset.noline)? '' : '<span class="ln' + ((marker.indexOf(ii + start) > -1) ? ' marker' : '') + (((ii + start) % 2) ? ' lnA' : ' lnB') + '" data-ln="' + (ii + start) + '" style="color:' + color + ';"></span>';
					var lncodeTag = '<span class="lc' + ((marker.indexOf(ii + start) > -1) ? ' marker' : '') + (((ii + start) % 2) ? ' lnA' : ' lnB') + '">';
					var opens = (ln.match(/<span/g) || []).length,
							ends = (ln.match(/span>/g) || []).length;
					if (opens == ends) {
						ln = lnTag + lncodeTag + ln + '</span>';
					} else {
						if (opens < ends) {
							var regex = /^(\s*)(<\/span>)/i,
									startClosed = ln.match(regex);
							if (startClosed) {
								ln = ln.replace(regex, startClosed[2] + lnTag + lncodeTag + startClosed[1])+'</span>';
							} else {
								ln = lnTag + lncodeTag + ln + '</span>';
							}
						} else {
							var regex = /(\s*<span[^>]+>)$/i,
									endOpen = ln.match(regex);
							if (endOpen) {
								ln = lnTag + lncodeTag + ln.replace(regex, '</span>' + endOpen[0]);
							} else {
								ln = lnTag + lncodeTag + ln + '</span></span>';
							}
						}
					}
					newLns.push(ln);
				}
			}
			code.innerHTML = ((wrapper.dataset.noline)? '': '<div class="lnBorder" style="border-right-color:' + color + ';"></div>') + newLns.join('\n');

			if (!wrapper.dataset.noline) {
				// make line number panel togglable
				code.querySelector('div.lnBorder').addEventListener("click", function() {
					var wrapper = (this.parentNode.parentNode.nodeName == "PRE")? this.parentNode.parentNode : this.parentNode;
					var disp = (wrapper.dataset.disp === undefined || wrapper.dataset.disp == 1)? "none" : "inline-block";
					var lns = wrapper.querySelectorAll("span.ln");
					for (var ii = 0; ii < lns.length; ii++) { lns[ii].style.display = disp; }
					wrapper.dataset.disp = (disp == "none")? 0 : 1;
					// fix for weird select begaviour seen only in Chrome
					if (!!window.chrome && (!!window.chrome.webstore || !!window.chrome.runtime) && disp == 'inline-block') {
						var ws = window.getComputedStyle(wrapper, null).getPropertyValue("white-space");
						wrapper.style.whiteSpace = (ws == 'pre')? 'pre-wrap' : 'pre';
						window.setTimeout(function() { wrapper.style.whiteSpace = ws; }, 0);
					}
				});
				setLnHeight(wrapper);
				// if data-hideline=1 then begin with hidden line numbers
				if (wrapper.dataset.hideline) {
					var lns = wrapper.querySelectorAll("span.ln");
					for (var ii = 0; ii < lns.length; ii++) { lns[ii].style.display = 'none'; }
					wrapper.dataset.disp = 0;
				}
			}

			//preserve indent when wrapped
			if (!wrapper.dataset.nopad) {
				var lcs = wrapper.querySelectorAll('span.lc');
				if (lcs.length) {
					for (var ii=0; ii < lcs.length; ii++) {
						var lc = lcs[ii], ws = lc.innerHTML.match(/^\s*/)[0];
						if (ws.length > 0) {
							var t = (ws.match(/	/g) || []).length, s = (ws.match(/ /g) || []).length, newWS = '';
							for (var iii=0; iii < t; iii++) { newWS += '‌'; }
							for (var iii=0; iii < s; iii++) {	newWS += '​';	}				
							lc.innerHTML = lc.innerHTML.replace(/^\s*/, newWS);
							var newItem = document.createElement("span");
							newItem.style.display = 'inline-block';
							newItem.innerHTML = ws;
							lc.insertBefore(newItem, lc.childNodes[0]);
							var curPad = window.getComputedStyle(lc, null).getPropertyValue("padding-left");
							var indent = lc.firstChild.offsetWidth;
							lc.style.paddingLeft = parseInt(curPad) + (indent) + 'px';
							lc.removeChild(newItem);
						}
					}
					wrapper.addEventListener('copy', function(e) {
						var selectedText = window.getSelection().toString();
						selectedText = selectedText.replace(/‌/g, '	').replace(/​/g, ' ');
						var clipboardData = (e.clipboardData || window.clipboardData || e.originalEvent.clipboardData);
						if (window.clipboardData) {
							clipboardData.setData('Text', selectedText);
						} else {
							clipboardData.setData('text/plain', selectedText);		
						}
						e.preventDefault();
					});
				}
			}
			////
			
			//add color box
			if (!wrapper.dataset.nocolor) {
				var lcs = wrapper.querySelectorAll('span.lc');
				if (lcs.length) {
					for (var ii=0; ii < lcs.length; ii++) {
						var lc = lcs[ii];
						// color by #hexcode
						lc.innerHTML = lc.innerHTML.replace(/#[\da-f]{3,6}/gi, function(col) {
							var tempElem = new Option().style;
							tempElem.color = col;
							return (tempElem.color)? '<span class="colBox" style="background-color:'+col+';"></span>' + col : col ;
						});
						// color by name
						var matches, output = [], lcText = lc.innerText;
						var regex = new RegExp(/:\s*([^#;]+[a-z]+).*[\s;\'\"\n]+?/gi);
						while (matches = regex.exec(lcText)) {
							if (matches[1].length) {
								var cols = matches[1].split(" ");
								for (var iii=0; iii<cols.length; iii++) {
									var col = cols[iii];
									var tempElem = new Option().style;
									tempElem.color = col;
									if (tempElem.color) {
										if (output.indexOf(col) < 0) {
											output.push(col);
										}
										lc.innerHTML = lc.innerHTML.replace(new RegExp(col+'([^​])'), '<span class="colBox" style="background-color:'+col+'​;"></span>'+col+'​$1');
									}
								}
							}
						}
						// color by rgb/rgba/hsl value
						var regex = new RegExp(/((rgb|rgba|hsl)+(\([\d\s,.%\-]+\)))/gi);
						while (matches = regex.exec(lcText)) {
						if (matches[0].length) {
							var tempElem = new Option().style;
							tempElem.color = matches[0]; 
								if (tempElem.color) {
									if (output.indexOf(matches[1]) < 0) {
										output.push(matches[1]);
									}							
									lc.innerHTML = lc.innerHTML.replace(new RegExp(matches[1]+'([^​])'), '<span class="colBox" style="background-color:'+matches[1]+'​'+matches[2]+';"></span>'+matches[1]+'​$1');
									}
								}
						}
						var colBoxes = lc.querySelectorAll('.colBox');
						for (var iii=0; iii < colBoxes.length; iii++) {
								if (colBoxes[iii].parentNode.className.indexOf('comment') > 0) {
									colBoxes[iii].parentNode.removeChild(colBoxes[iii]);
								}
						}
						for (var iii=0; iii < output.length; iii++) {
							lc.innerHTML = lc.innerHTML.replace(new RegExp('('+output[iii]+')​','g'), '$1');				
						}	
					} 
				}
			}
			////

			//show matching brace
			if (!wrapper.dataset.nomatch) {
				var braces = wrapper.querySelectorAll('.brace');
				var stack = [], brs = 0;
				var open = {'{': '}', '[': ']', '(': ')' };
				for (var ii = 0; ii < braces.length; ii++) {
					var brace = braces[ii];
					var char = brace.textContent;
					if (brace.parentNode.className.indexOf('comment') > 0 || brace.parentNode.parentNode.className.indexOf('comment') > 0) {
						brace.parentNode.replaceChild(document.createTextNode(char), brace);
						continue;
					}				
					brace.classList.add('solo');
					if (open[char]) {
						brs++;
						stack.push([char, 'b'+brs]);
						brace.dataset.pair = 'b'+brs;
					} else {
						var lastOpen = stack.pop();
						if (lastOpen && open[lastOpen[0]] === char) {
							brace.dataset.pair = lastOpen[1];
							brace.classList.remove('solo');
							var partner = wrapper.querySelector('[data-pair="'+lastOpen[1]+'"]');
							partner.classList.remove('solo');			
						}
					}
					brace.addEventListener("mouseenter", function() {
						if (!this.classList.contains('clicked')) {
							highlightPair(this);
						}
					});
					brace.addEventListener("mouseleave", function() {
						highlightPair(this, true);
					});		
				}
				//show matching tag
				var tags = wrapper.querySelectorAll('.hljs-tag .hljs-name');
				var stack = [];
				var brs = 0;
				for (var ii = 0; ii < tags.length; ii++) {
					var tag = tags[ii];
					tag.classList.add('solo');
					var char = tag.textContent;
					var open = (tag.previousSibling.textContent.indexOf('</') < 0)? true : false;
					if (open) {
						brs++;
						stack.push([char, 't'+brs]);
						tag.dataset.pair = 't'+brs;
					} else {
						var lastOpen = stack.pop();
						if (lastOpen && lastOpen[0] === char) {
							tag.dataset.pair = lastOpen[1];
							tag.classList.remove('solo');
							var partner = wrapper.querySelector('[data-pair="'+lastOpen[1]+'"]');
							partner.classList.remove('solo');
						}
					}
					tag.addEventListener("mouseenter", function() {
						if (!this.classList.contains('clicked')) {
							highlightPair(this);
						}
					});
					tag.addEventListener("mouseleave", function() {
						highlightPair(this, true);
					});	
				}
				//common for matching brace/tag
				var solos = wrapper.querySelectorAll('.solo');
				for (var iii=0; iii < solos.length; iii++) {
					var solo = solos[iii];
					solo.removeAttribute('data-pair');
					solo.outerHTML = solo.outerHTML;
				}				
				wrapper.addEventListener("click", function(e) {
					if (!e.target.classList.contains('lnBorder')) {
						var clicked = this.querySelectorAll('.clicked');
						for (var iii=0; iii < clicked.length; iii++) {
							clicked[iii].classList.remove('clicked');
						}
					}
					if (e.target.classList.contains('brace') || e.target.classList.contains('hljs-name') && !e.target.classList.contains('solo')) {
						var clicked = e.target;
						var pair = this.querySelectorAll('[data-pair="'+clicked.dataset.pair+'"]');
						var partner = (clicked == pair[0])? pair[1]: pair[0];
						if (!clicked.classList.contains('clicked')) {
							clicked.classList.add('clicked');
							clicked.classList.remove('hover');
							if (partner) {
								partner.classList.add('clicked');	
								partner.classList.remove('hover');	
							}
						}					
					}
				});		
			}
			////end of matching brace/tag
		} ////end of if (!data-skip)
		if (wrapper.dataset.hide) { 
			wrapper.style.display = 'none'; 
		}
  } //end of forloop with i
	window.addEventListener("resize", function() { setLnHeight(); });
	window.addEventListener("orientationChange", function() { setLnHeight(); });	
}); //end of window.onload

//setting line number height when code is wrapped
function setLnHeight(elem) {
  var lcs = (elem || document).querySelectorAll("span.lc.marker");
  for (var i = 0; i < lcs.length; i++) { var lc = lcs[i];
    if (!lc.innerText.length) { lc.innerText += ' '; }
    var h = lc.offsetHeight;
    if (h && lc.previousElementSibling && lc.previousElementSibling.classList.contains('ln')) { lc.previousElementSibling.style.height = h + 'px'; }
  }
}
//highlight brace/tag pair when hover
function highlightPair(elem, clear) {
	var getClosest = function (el, tag) {
		while (el.tagName != tag.toUpperCase()) { el = el.parentNode; if (!el) { return null; } } return el;
	};
	var pair = (getClosest(elem, 'pre')||getClosest(elem, 'div')).querySelectorAll('[data-pair="'+elem.dataset.pair+'"]');
	var partner = (elem == pair[0])? pair[1]: pair[0];
	if (pair.length == 2) {
		(!clear)? elem.classList.add('hover') : elem.classList.remove('hover');
		(!clear)? partner.classList.add('hover') : partner.classList.remove('hover');
	}
}	
//end of addon
