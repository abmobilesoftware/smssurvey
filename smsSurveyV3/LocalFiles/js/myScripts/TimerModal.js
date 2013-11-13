var TimerModal = (function () {
	var innerClass = {};
	var timer1;
	var timerBtn;
	var restartInterval = 9;
	innerClass.events = {
		SAVE_PARTIAL_RESULTS: "savePartialResultsEvent"	
	};
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
			$(document).trigger(innerClass.events.SAVE_PARTIAL_RESULTS, true);
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