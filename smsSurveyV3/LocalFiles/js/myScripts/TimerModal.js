var TimerModal = (function () {
	var innerClass = {};
	var timer1;
	var timerBtn;
	var restartInterval = 9;
	innerClass.events = {
		SAVE_SURVEY_PARTIAL_RESULTS: "savePartialResultsEvent",
		SAVE_PERSONAL_INFO_PARTIAL_RESULTS: "savePersonalInfoPartialResultsEvent"
	};
	
	innerClass.pagesName = {
			QUESTION_PAGE: "questionPage",
			THANK_YOU_PAGE: "thankYouPage"
		};
	var currentPage = innerClass.pagesName.QUESTION_PAGE;
	
	innerClass.setPage = function(newPage) {
		currentPage = newPage;
	}
	
	innerClass.showModal = function() {
		$('#timer-modal').modal({
			  backdrop: 'static',
			  keyboard: false
		});
		$(".modal-text").html($("#no-activity-detected").val());
		$(".keep-survey-active-btn").html($("#yes-i-am-here").val());
		
		$(".keep-survey-active-btn").off("click", innerClass.hideModal);
		$(".keep-survey-active-btn").on("click", innerClass.hideModal);
				
		restartInterval = 9;
		$('#timer-modal').modal("show");
		$('#timer-modal').css("visibility", "visible");
		$(".time-remaining").html(restartInterval);
		timer1 = setInterval(innerClass.countdown, 1000);
		timerBtn = setInterval(innerClass.buttonBlink, 500);
	};
	innerClass.hideModal = function() {
		clearInterval(timer1);
		clearInterval(timerBtn);
		$("#timer-modal").modal("hide");
	};
	innerClass.countdown = function() {
		restartInterval = restartInterval - 1;
		if (restartInterval <= 0) {
			var event = (currentPage == innerClass.pagesName.QUESTION_PAGE) ? 
					innerClass.events.SAVE_SURVEY_PARTIAL_RESULTS : 
						innerClass.events.SAVE_PERSONAL_INFO_PARTIAL_RESULTS;
			$(document).trigger(event, true);
			innerClass.hideModal();
			Timer.partialResults();
		} else {
			$(".time-remaining").html(restartInterval);
		}
	};
	innerClass.buttonBlink = function() {
		if ($(".keep-survey-active-btn").hasClass("blink")) {
			$(".keep-survey-active-btn").removeClass("blink");
		} else {
			$(".keep-survey-active-btn").addClass("blink");
		}
	}
	return innerClass;
})();