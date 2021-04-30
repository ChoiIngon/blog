$.fn.table_of_contents = function(options) {
	options = (typeof(options) == 'undefined') ? {} : options;
	
	let scrollToHeadline = function(target) 
	{
		$("body, html").animate(
			{'scrollTop': $(target).offset().top + options.scrollOffset},
			options.scrollSpeed,
			function() {}
		);
	};

	let toc_container = $(options.container);
	let this_container = $(this);
	let headlines = $(":header", this_container);

	if(0 == headlines.length)
	{
		return;
	}
	
	let prev_level = headlines[0].tagName.replace(/[^\d]/g, ""); // 숫자가 아닌 모든 것을 ""변경. 결국 숫자만 뽑겠다는 의미
	const top_level = prev_level;
	let tocHTML = "<ul>";
		
	headlines.each(function(headline_index, headline) {
		var curr_level = headline.tagName.replace(/[^\d]/g, "");
		if (curr_level > prev_level) { 
			for (let i = curr_level; i > prev_level; i--) {
				tocHTML += "<ul>";
			}
		} 
		else if(curr_level < prev_level) {
			for (let i = curr_level; i < prev_level; i++) {
				tocHTML += "</ul>";
			}
		}
		
		prev_level = curr_level;
		
		let headline_unique_id = "headline_unique_id_" + headline_index;
		let headlineElmt = $(headline);
		let headlineText = headlineElmt.text();
		
		headlineElmt.prop("id", headline_unique_id);
		tocHTML += "<li depth='" + (curr_level - top_level) + "'><a href='#" + headline_unique_id + "'>" + headlineText + "</a></li>";
	});
		
	tocHTML += "</ul>";
	toc_container.append($(tocHTML));
		
	$("li a", toc_container).click(function(e) {
		e.preventDefault();

		$(toc_container).find("li a").each(function(index, elmt) {
			$(elmt).removeAttr("selected");
		});
			
		$(e.target).attr("selected", "selected");
				
		const target = $(e.target).attr('href');
		scrollToHeadline(target);
	});

	if(true == options.sticky)
	{
		toc_container.css("position", "sticky");
		toc_container.css("top", "0px");
	
		const first_headline_ref = toc_container.find("ul:first-child");
		let last_scroll_pos = $(document).scrollTop();
		
		$(document).scroll(function() {
			$(toc_container).find("li a").each(function(index, elmt) {
				$(elmt).removeAttr("selected");
			});
			
			headlines.each(function(index, headline) {
				let headline_id = $(headline).prop("id");
						
				if(window.scrollY <= $(headline).offset().top && $(headline).offset().top <= window.scrollY + window.innerHeight)
				{
					let href = $("li a[href='#" + headline_id + "']");
					href.attr("selected", "selected");
					
					let selected_elmt = href;
					let viewport_top = Math.max(0, toc_container.offset().top - $(window).scrollTop());
					let viewport_height = Math.min($(window).height() - viewport_top, toc_container.offset().top + $(toc_container).height() - $(window).scrollTop());
					let viewport_mid = (viewport_top + viewport_height) / 2;
					let selected_elmt_top = selected_elmt.offset().top - $(window).scrollTop();
					
					if(last_scroll_pos < $(window).scrollTop() && $(window).height() <= viewport_height)
					{
						if(selected_elmt_top > viewport_mid)
						{
							toc_container.offset({top: toc_container.offset().top + (viewport_mid - selected_elmt_top)});
						}
					}
					else
					{
						if(selected_elmt_top + selected_elmt.height() < viewport_mid)
						{
							toc_container.offset({top: toc_container.offset().top - (selected_elmt_top + selected_elmt.height() - viewport_mid)});
						}
						if($(window).scrollTop() < toc_container.offset().top)
						{
							toc_container.css("top", "0px");
						}
					}
					
					return false;
				}
			});
			
			last_scroll_pos = $(window).scrollTop();
		});
	}

	/*
	$("body").append("<div id='_result'></div>");
	let result = $("#_result");
	result.css("position", "fixed");
	result.css("top", "25px");
	result.css("right", "0px");
	result.css("color", "#ffffff");
	let last_scroll_pos = $(document).scrollTop();
	let display = function() {
		let selected_elmt = toc_container.find("li a[selected]");
		if(0 == selected_elmt.length)
		{
			return;
		}
		
		let viewport_top = Math.max(0, toc_container.offset().top - $(window).scrollTop());
		let viewport_height = Math.min($(window).height() - viewport_top, toc_container.offset().top + $(toc_container).height() - $(window).scrollTop());
		let viewport_mid = (viewport_top + viewport_height) / 2;
		let selected_elmt_top = selected_elmt.offset().top - $(window).scrollTop();
		
		if(last_scroll_pos < $(window).scrollTop() && $(window).height() <= viewport_height)
		{
			if(selected_elmt_top > viewport_mid)
			{
				toc_container.offset({top: toc_container.offset().top + (viewport_mid - selected_elmt_top)});
			}
		}
		else
		{
			
			console.log("up up");
			if(selected_elmt_top + selected_elmt.height() < viewport_mid)
			{
				toc_container.offset({top: toc_container.offset().top - (selected_elmt_top + selected_elmt.height() - viewport_mid)});
			}
			if($(window).scrollTop() < toc_container.offset().top)
			{
				toc_container.css("top", "0px");
			}
		}

		last_scroll_pos = $(window).scrollTop();
		
		result.html(
			"document:{scrollTop:" + $(document).scrollTop() + ", height:" + $(document).height() + "}<br>"
			+ "window:{scrollTop:" + $(window).scrollTop() + ", height:" + $(window).height() + "}<br>"
			+ "toc_container:{position.top:" + toc_container.position().top + ", offset.top:" + toc_container.offset().top + ", height:" + toc_container.height() +"}<br>"
			+ "viewport     :{position.top:" + viewport_top + ", offset.top:" + toc_container.offset().top + ", height:" + viewport_height +"}<br>"
			+ "selected elmt:{position.top:" + (selected_elmt.offset().top - $(window).scrollTop()) + ", offset.top:" + selected_elmt.offset().top + ", height:" + selected_elmt.height() +"}<br>"
		);
	};	
	*/
};