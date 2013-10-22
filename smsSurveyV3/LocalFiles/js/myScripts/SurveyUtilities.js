﻿var SurveyUtilities = SurveyUtilities || {};
SurveyUtilities.Utilities = (function () {
   var innerClass = {};
   innerClass.generateUUID = function () {
      var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
         var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
         return v.toString(16);
      });
      return uuid;
   },
   innerClass.CONSTANTS_SURVEY = {
      TYPE_SMS: "sms",
      TYPE_MOBILE_WEBSITE: "mobile website"
   },
   innerClass.CONSTANTS_QUESTION = {
      TYPE_RATING: "Rating",
      TYPE_FREE_TEXT: "FreeText",
      TYPE_SELECT_ONE_FROM_MANY: "SelectOneFromMany",
      TYPE_SELECT_MANY_FROM_MANY: "SelectManyFromMany",
      TYPE_YES_NO: "YesNo",
      TYPE_NUMERIC: "Numeric"
   },
   innerClass.CONSTANTS_MISC = {
      SEPARATOR_ANSWERS: ";",
      NEW_SURVEY: "new_survey"
   }
   innerClass.CONSTANTS_CLASS = {
      INVALID_FIELD: "invalidField"
   },
   innerClass.GLOBAL_EVENTS = {
      ATTRIBUTE_CHANGED: "attributeChanged"
   },
   innerClass.trim = function(str) {
      str = str.replace(/^\s+/, '');
      for (var i = str.length - 1; i >= 0; i--) {
         if (/\S/.test(str.charAt(i))) {
            str = str.substring(0, i + 1);
            break;
         }
      }
      return str;
   }
   return innerClass;
})();

var Timer = (function () {
	var innerClass = {};
	var noOfMouseMoves = 0;
	// 
	var mouseDetectionInterval = 20000; // ms
	var restartInterval = 3000;
	var timer1 = null;
		
	innerClass.showSlider = function() {
		$("#slider-modal").modal("show");
		$("#slider-modal").css("visibility", "visible");
		$('.carousel').carousel();
	};
	innerClass.hideSlider = function() {
		$("#slider-modal").modal("hide");
		innerClass.startTimer();
	};
	innerClass.events = {
		RESTART_SURVEY : "restartSurveyEvent"	
	};
	innerClass.startTimer = function () {
		noOfMouseMoves = 0;
		//alert("Valoarea de la timer 1 inainte de stop " + timer1);
		if (timer1 != null && timer1 != undefined) {
			//innerClass.stopTimer();
			timer1 = clearInterval(timer1);
			//alert("Stop pe timer 1 = " + timer1);
		}
		//alert("Valoarea de la timer 1 dupa stop " + timer1);
		//DA make sure we only listen once to each event
		document.removeEventListener("touchend", innerClass.trackMouseMovement);
		document.addEventListener("touchend", innerClass.trackMouseMovement);
		Backbone.off("touch", innerClass.trackMouseMovement);
		Backbone.on("touch", innerClass.trackMouseMovement);
		timer1 = setInterval(innerClass.checkIfUserIsActive, mouseDetectionInterval);	      
	};
	innerClass.stopTimer = function() {
		clearInterval(timer1);
	};
	innerClass.trackMouseMovement = function () {
		//alert("No of moves = " + noOfMouseMoves);
		++noOfMouseMoves;
	};
	innerClass.checkIfUserIsActive = function () {
		//alert("No of touches " + noOfMouseMoves);
		var isUserAwayFromPage = noOfMouseMoves == 0 ? true : false;
	    if (isUserAwayFromPage) {
	    	document.removeEventListener("touchstart", innerClass.trackMouseMovement);
	    	innerClass.stopTimer();
	    	innerClass.startSurvey();
	    	innerClass.showSlider();
	    } else {
	    	noOfMouseMoves = 0;
	    }
	};	 
	innerClass.restartSurvey = function() {
		setTimeout(innerClass.startSurvey, restartInterval);
	};
	innerClass.startSurvey = function() {
		$(innerClass).trigger(innerClass.events.RESTART_SURVEY);
	}
	$(document).on("click", innerClass.hideSlider);
	return innerClass;
})();