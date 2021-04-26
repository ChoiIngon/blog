$.fn.table_of_contents = function(options) {
	let scrollToHeadline = function(target) 
	{
		$("body, html").animate(
			{'scrollTop': $(target).offset().top + options.scrollOffset},
			options.scrollSpeed,
			function() {}
		);
	};
		
	options = (typeof(options) == 'undefined') ? {} : options;
	
	let toc_container = $(options.container);
	let this_container = $(this);
	let headlines = $(":header", this_container);
		
	if(0 == headlines.length)
	{
		return;
	}
	
	let tocHTML = "<ul>";
	let top_level = headlines[0].tagName.replace(/[^\d]/g, ""); // 숫자가 아닌 모든 것을 ""변경. 결국 숫자만 뽑겠다는 의미
		
	headlines.each(function(headline_index, headline) {
		var sub_level = headline.tagName.replace(/[^\d]/g, "");
		if (sub_level > top_level) { 
			for (let i = sub_level; i > top_level; i--) {
				tocHTML += "<ul>";
			}
		} 
		else if(sub_level < top_level) {
			for (let i = sub_level; i < top_level; i++) {
				tocHTML += "</ul>";
			}
		}
		
		top_level = sub_level;
		
		let headline_unique_id = "headline_unique_id_" + headline_index;
		let headlineElmt = $(headline);
		let headlineText = headlineElmt.text();
		
		headlineElmt.prop("id", headline_unique_id);
		tocHTML += "<li><"+headline.tagName + "><a href='#" + headline_unique_id + "'>" + headlineText + "</" +headline.tagName +"></a></li>";
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
		$(document).scroll(function() {
			const middle = window.scrollY + (window.innerHeight / 2);
										
			$(toc_container).find("li a").each(function(index, elmt) {
				$(elmt).removeAttr("selected");
			});
					
			headlines.each(function(index, headline) {
				let headline_id = $(headline).prop("id");
				let href = $("li a[href='#" + headline_id + "']");
						
				if(window.scrollY <= $(headline).offset().top && $(headline).offset().top <= window.scrollY + window.innerHeight)
				{
					let selected_elmt_top = href.position().top;
					let selected_elmt_height = href.height();
					let viewport_height = Math.min($(window).height(), $(window).height() - (toc_container.offset().top - $(document).scrollTop()));
					
					if(selected_elmt_top > viewport_height)
					{
						toc_container.css({top: viewport_height - selected_elmt_top - selected_elmt_height * 2});
					}
					else
					{
						toc_container.css({top: 0});
					}
					console.log(
						"selected_elmt_top:" + selected_elmt_top
						+ "\nselected_elmt_height:" + selected_elmt_height
						+ "\nviewport_height:" + viewport_height
						+ "\ntoc top:" + toc_container.offset().top + ", docuement scroll top:" + $(document).scrollTop()
					);
					href.attr("selected", "selected");
					return false;
				}
			});
		});
	}
};