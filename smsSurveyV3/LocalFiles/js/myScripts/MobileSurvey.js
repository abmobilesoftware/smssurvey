var MobileSurvey = MobileSurvey || {};

MobileSurvey.ButtonView = Backbone.View.extend({
	events: {
		"click": "click"
	},
	initialize: function () {
		_.bindAll(this, "click", "close");
		this.constants = {
				PROP_DISABLED: "disabled",
				CLASS_DISABLED: "disabled",
				CLASS_ENABLED: "enabled",
				EVENT_CLICK: "click"
		}
		this.on(this.constants.EVENT_DISABLE, this.disableBtn);
		this.on(this.constants.EVENT_ENABLE, this.enableBtn);
	},
	enable: function () {
		this.$el.prop(this.constants.PROP_DISABLED, false);
		if (this.$el.hasClass(this.constants.CLASS_DISABLED)) {
			this.$el.removeClass(this.constants.CLASS_DISABLED);
		}
		this.$el.addClass(this.constants.CLASS_ENABLED);
	},
	disable: function () {
		this.$el.prop(this.constants.PROP_DISABLED, true);
		if (this.$el.hasClass(this.constants.CLASS_ENABLED)) {
			this.$el.removeClass(this.constants.CLASS_ENABLED);
		}
		this.$el.addClass(this.constants.CLASS_DISABLED);
	},
	click: function (event) {
		event.preventDefault();
		this.trigger(this.constants.EVENT_CLICK);
	},
	getTitle: function () {
		return this.$el.html();
	},
	setTitle: function (title) {
		this.$el.html(title);
	},
	close: function () {
		this.remove();
	}
});

MobileSurvey.QuestionMobileView = Backbone.View.extend({
	keys: {
		ENTER: 13
	},
	events: {
		"click .numeric-radio": "numericScaleSelected",
		"keydown .comment": "keyPressListener"
	},
	initialize: function () {
		_.bindAll(this, "render", "toggleDisplay", "updateQuestionDisplay", "updateAnswer",
		"numericScaleSelected", "close");
		this.questionMobileTemplate = _.template($("#question-mobile-template").html());
		this.questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;			
		this.listenTo(this.model, "change:ValidAnswer", this.updateQuestionDisplay);
		this.childViews = [];
	},
	render: function () {
		this.$el.html(this.questionMobileTemplate(this.model.toJSON()));
		
		this.childViews.length = 0;
		if (this.model.get("Type") == this.questionConstants.TYPE_RATING) {
			var ratingsSeparator = SurveyUtilities.Utilities.CONSTANTS_MISC.SEPARATOR_ANSWERS;
			var noOfRatings = this.model.get("ValidAnswersDetails") != null ?
					this.model.get("ValidAnswersDetails").split(ratingsSeparator).length
					: 5;
					noOfRatings = noOfRatings == 1 ? 0 : noOfRatings;
			var starBarView = new SurveyElements.StarBarView({ el: $(".answerArea", this.$el), noOfElements: noOfRatings });
			this.childViews.push(starBarView);
		}		
		return this;
	},
	updateQuestionDisplay: function () {
		if (this.model.get("ValidAnswer")) {
			this.$el.removeClass("invalidAnswer");
		} else {
			this.$el.addClass("invalidAnswer");
		}
	},
	toggleDisplay: function (event) {
		if (!_.isUndefined(event)) {
			event.preventDefault();
		}
		if ($(".answerArea", this.$el).css("display") == "none") {
			$(".answerArea", this.$el).show();
		} else {
			$(".answerArea", this.$el).hide();
		}
	},
	updateAnswer: function () {
		var questionType = this.model.get("Type");
		if (questionType == this.questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
			var selectedOption = $("select[name='answer']", this.$el).find(":selected").val();
			this.model.set({ "PickedAnswer": selectedOption }, { validate: true });
		} else if (questionType == this.questionConstants.TYPE_YES_NO) {
			var yesNoAnswer = $('input[name=yes-no-answer' + this.model.get("Id") + ']:checked', this.$el).val();
			yesNoAnswer = yesNoAnswer != undefined ? yesNoAnswer : Question.noValueAnswer;
			this.model.set({ "PickedAnswer": yesNoAnswer }, { validate: true });
		} else if (questionType == this.questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
			var selectedValuesArr = $.map($('input[type=checkbox]:checked', this.$el), function (elem) {
				return $(elem).val();
			});
			var selectValues = selectedValuesArr.join(';');
			this.model.set({ "PickedAnswer": selectValues }, { validate: true });
		} else if (questionType == this.questionConstants.TYPE_NUMERIC) {
			var numericScaleAnswer = $('input[name=numeric-scale-answer' + this.model.get("Id") + ']:checked', this.$el).val();
			numericScaleAnswer = numericScaleAnswer != undefined ? numericScaleAnswer : Question.noValueAnswer;
			this.model.set({
				"PickedAnswer": numericScaleAnswer,
				"AdditionalInfo": $(".comment", this.$el) != undefined ? $(".comment", this.$el).val() : ""
			},
			{
				validate: true
			});
		}
		else {
			this.model.set(
					{
						"PickedAnswer": $(".answer", this.$el).val(),
						"AdditionalInfo": $(".comment", this.$el) != undefined ? $(".comment", this.$el).val() : ""
					},
					{
						validate: true
					});
		}
	},
	numericScaleSelected: function (event) {
		//DA 3 is again hardcoded - it should be configurable
		if ($(event.currentTarget).children("input").val() < 3) {
			$(".comment", this.$el).show();
		} else {
			$(".comment", this.$el).hide();
		}
	},
	keyPressListener: function(event) {
		 if (event.keyCode == this.keys.ENTER) {
			 event.preventDefault();
			 $(".comment", this.$el).blur();
		 }
	},
	close: function() {		
		_.each(this.childViews, function(childView) {
			childView.close();
		}, this);
		this.childViews.length = 0;
		this.remove();
	}
});

MobileSurvey.SurveyMobileView = Backbone.View.extend({
	initialize: function () {
		_.bindAll(this, "render", "saveSurvey", "close", "refresh");
		this.surveyPreviewModel = this.options.surveyPreviewModel;
		this.surveyMobileTemplate = _.template($("#mobileSurveyTemplate").html());
		this.el = $("#questions");
		this.surveyConstants = SurveyUtilities.Utilities.CONSTANTS_SURVEY;
		this.pageElements = {
				$DONE_BTN: $("#doneBtn", this.$el),
				$QUESTIONS_AREA: $("#questions", this.$el),
				$PAGE_TITLE: $("#pageTitle", this.$el),
				$INTRO_MESSAGE: $("#introMessageContent", this.$el)
		};
		this.pageElements.$PAGE_TITLE.html(this.model.get("Title"));
		this.pageElements.$INTRO_MESSAGE.html(this.model.get("IntroMessage"));
		this.pageEvents = {
				THANK_YOU_PAGE: "goToThankYouPageEvent"
		};
		this.questionsViews = [];
		
		//SubmitButton style
		this.doneBtn = new MobileSurvey.ButtonView({
			el: this.pageElements.$DONE_BTN
		});
		this.doneBtnTitle = $("#doneBtnTitle").val();
		this.doneBtn.enable();
		this.doneBtn.setTitle(this.doneBtnTitle);
				
		//SubmitButton functionality
		this.submitSurveyButton = new google.ui.FastButton($("#doneBtn", this.$el)[0],
				this.saveSurvey);
	},
	render: function () {
		var self = this;
		this.el.html(this.surveyMobileTemplate());
		
		var areaToAddContentTo = $(".questionsArea", this.$el);
		//http://ozkatz.github.io/avoiding-common-backbonejs-pitfalls.html?tagref=js
	    var questionsContainer = document.createDocumentFragment();
		_.each(this.model.getQuestionSetCollection(), function (question, index) {
			//DA now that the collection is sorted (due to the comparator on the collection) we can correctly set the QuestionNumber
			question.set("QuestionNumber", index + 1);
			var questionPreviewView = new MobileSurvey.QuestionMobileView({ model: question });
			self.questionsViews.push(questionPreviewView);
			questionsContainer.appendChild(questionPreviewView.render().el)		
		}, this);
		
		//once we have all the questions, append them to the DOM
		areaToAddContentTo.append(questionsContainer);
		
		//DA this may still be leaking handlers
		$('.numeric-radio').screwDefaultButtons({
			image: 'url("images/radioSmall77.png")',
			width: 78,
			height: 77
		});		
		return this;
	},	
	isSurveyComplete: function () {
		var answeredQuestions = 0;
		_.each(this.model.getQuestionSetCollection(), function (value, key, list) {
			if (value.get("ValidAnswer"))++answeredQuestions;
		}, this);
		return {
			isDone:
				answeredQuestions == this.model.getQuestionSet().length ? true : false,
						status: answeredQuestions + "/" + this.model.getQuestionSet().length
		};
	},
	saveSurvey: function () {
		var answeredQuestions = 0;
		_.each(this.questionsViews, function (value) {
			value.updateAnswer();
		}, this);
		var surveyStatus = this.isSurveyComplete();
		if (surveyStatus.isDone) {
			this.doneBtn.setTitle(
					this.doneBtnTitle + " (" +
							surveyStatus.status +
							")"
			);
			this.trigger(this.pageEvents.THANK_YOU_PAGE);
			//DA and now we should save in the database

		} else {
			this.doneBtn.setTitle(
					this.doneBtnTitle + " (" +
					surveyStatus.status +
					")"
			);
		}
	},
	getWidth: function () {
		return this.$el.outerWidth();
	},
	setWidth: function (value) {
		this.$el.css("width", value);
	},
	hide: function () {
		this.$el.hide();
	},
	show: function () {
		this.doneBtnTitle = $("#doneBtnTitle").val();
		this.doneBtn.enable();
		this.doneBtn.setTitle(this.doneBtnTitle);
		this.$el.show();
		$(document).scrollTop(0);
	},
	getHeight: function () {
		return this.$el.outerHeight();
	},
	refresh: function() {
		//clean up the inner views here
		_.each(this.questionsViews, function(questionView){
			questionView.close();
		}, this);
		this.questionsViews.length = 0;
		
		//and then move on with redrawing
		this.render();
		this.doneBtn.setTitle(this.doneBtnTitle);
	},
	close: function() {
		_.each(this.questionsViews, function(childView) {
			childView.close();
		}, this);
		this.questionsViews.length = 0;
		
		this.doneBtn.close();
		this.submitSurveyButton.reset();
	}
});

var isBlank = function (str) {
	return (!str || /^\s*$/.test(str));
}

MobileSurvey.ThankYouPageView = Backbone.View.extend({
	initialize: function () {
		_.bindAll(this, "setWidth", "show",
				"getHeight", "render", "sendPersonalInfo",
				"setSurveyResultId", "clearErrorsFromFields",
				"validationError", "retakeSurvey", "sendPersonalInfoResponse","close");
		this.template = _.template($("#thankyoupage-template").html());
		this.pageEvents = {
			RETAKE_SURVEY : "retakeSurveyEvent"	
		};
		this.storage = this.options.localStorage;
		this.thankYouMessage = this.options.thankYouMessage;
		this.surveyTitle = this.options.surveyTitle;
		
	},
	setSurveyResultId: function (surveyResultId) {
		this.surveyResultId = surveyResultId;
	},
	setWidth: function (value) {
		this.$el.css("width", value);
	},
	getWidth: function() {
		return this.$el.outerWidth();
	},
	show: function () {
		this.$el.show();
		$('#surveyUserInfo').show();
		this.sendBtn.setTitle($("#sendPersonalDetails", this.$el).val());
		this.sendBtn.enable();
		
		$('#name').val("");
		$("#surname").val("");
		$('#email').val("");
		$('#telephone').val("");
	},
	hide: function() {
		this.dom.$ALERT_BOX.hide();      
		this.$el.hide();		
	},
	enableSendBtn: function (event) {
		if (event.target.checked) {
			this.sendBtn.enable();
		} else {
			this.sendBtn.disable();
		}
	},
	validateData: function () {
		var self = this;
		this.dom.$ALERT_BOX.hide();
		var errors = [];
		var name = $('#name').val();
		var surname = $('#surname').val();
		var email = $('#email').val();

		var validData = true;
		if (isBlank(name)) {
			errors.push(MobileSurvey.PersonalInformationErrors.INVALID_NAME);
			validData = false;
		}
		if (isBlank(surname)) {
			errors.push(MobileSurvey.PersonalInformationErrors.INVALID_SURNAME);
			validData = false;
		}

		var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
		if (!filter.test(email)) {
			validData = false;
			errors.push(MobileSurvey.PersonalInformationErrors.INVALID_EMAIL);
		}

		//reset all "invalid fields" qualifiers
		this.clearErrorsFromFields();

		if (!validData) {
			self.validationError(errors);
			//var errorsText = "<span><strong>Check the following errors:</strong><span><br/>";
			//for (var i = 0; i<errors.length; ++i) {
			//   errorsText += "<span>" + (i+1) + ". " + errors[i] + "</span><br/>";
			//}
			//this.dom.$VALIDATION_BOX.html(errorsText)
			//this.dom.$ALERT_BOX.show();
		}
		return validData;
	},
	clearErrorsFromFields: function () {
		var nameField = $('#name', this.$el);
		var surnameField = $('#surname', this.$el);
		var emailField = $('#email', this.$el);
		nameField.removeClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
		surnameField.removeClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
		emailField.removeClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);

		this.dom.$VALIDATION_BOX.html("")
		this.dom.$ALERT_BOX.hide();
	},
	validationError: function (errorsArray) {
		var self = this;
		var nameField = $('#name', this.$el);
		var surnameField = $('#surname', this.$el);
		var emailField = $('#email', this.$el);

		for (var i = 0; i < errorsArray.length; ++i) {
			if (errorsArray[i] === MobileSurvey.PersonalInformationErrors.INVALID_NAME) {
				nameField.addClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
			} else if (errorsArray[i] === MobileSurvey.PersonalInformationErrors.INVALID_SURNAME) {
				surnameField.addClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
			} else if (errorsArray[i] === MobileSurvey.PersonalInformationErrors.INVALID_EMAIL) {
				emailField.addClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
			}
		}
		var errorMessage = errorsArray.join("\r\n");
		this.dom.$VALIDATION_BOX.html(errorMessage)           
		this.dom.$ALERT_BOX.removeClass("alert-success");
		this.dom.$ALERT_BOX.addClass("alert-error");
		this.dom.$ALERT_BOX.show();      
	},
	retakeSurvey: function(event) {
		event.preventDefault();
		this.trigger(this.pageEvents.RETAKE_SURVEY);
	},
	sendPersonalInfoResponse: function() {
		Timer.stopTimer();
		Timer.restartSurvey();
		
		$('#surveyUserInfo').slideToggle('slow');
		this.sendBtn.setTitle($("#personalInfoSubmitted", this.$el).val());
		this.sendBtn.disable();
		loader.hideLoader();
	},
	sendPersonalInfo: function (event) {
		var self = this;
		if (this.validateData()) {
			loader.showLoader();
			var personalInfo = {};
			personalInfo.Name = $('#name').val();
			personalInfo.Surname = $('#surname').val();
			personalInfo.Email = $('#email').val();
			personalInfo.Telephone = $('#telephone').val();
			var dataToSendObject = {
				info: personalInfo,
				surveyResultId: this.surveyResultId,
				LocalId: "noLocalId"
			};
			this.makeSendPersonalInfoRequest(dataToSendObject, this.sendPersonalInfoResponse, function() {
				var surveyResults = JSON.parse(self.storage.getItem("surveyResults"));
				if (surveyResults != null && surveyResults != "null"
					&& surveyResults != undefined && surveyResults != "undefined") {
					var resultsInStorage = false;
					for (var i=0; i<surveyResults.length; ++i) {
						if (surveyResults[i].Id == self.surveyResultId) {
							surveyResults[i].PersonalInfo = dataToSendObject;
							resultsInStorage = true;
						} 
					}
					if (!resultsInStorage) {
						var justPersonalInfo = {};
						dataToSendObject.LocalId = self.surveyResultId;
						justPersonalInfo.PersonalInfo = dataToSendObject;
						
						surveyResults.push(justPersonalInfo);
					}					
				} else {
					surveyResults = [];
					var justPersonalInfo = {};
					dataToSendObject.LocalId = self.surveyResultId;
					justPersonalInfo.PersonalInfo = dataToSendObject;
					surveyResults.push(justPersonalInfo);
				}
				self.storage.setItem("surveyResults",JSON.stringify(surveyResults));
				self.sendPersonalInfoResponse();
			});
			
		}
	},
	makeSendPersonalInfoRequest: function(dataToSendObject, successCallback, errorCallback) {
		var dataToSend = JSON.stringify(dataToSendObject);
		$.ajax({
			url: "http://dev.txtfeedback.net/MobileSurvey/SaveRespondentInfo1",
			data: dataToSend,
			type: 'post',
			cache: false,
			dataType: "json",
			contentType: 'application/json',
			traditional: true,
			crossDomain: true,
			success: successCallback,
			error: errorCallback,
			timeout: 5000
		});
	},
	getHeight: function () {
		return this.$el.outerHeight();
	},
	setTop: function (value) {
		this.$el.css("top", value);
	},
	render: function () {
		this._rendered = false;
		this.$el.html(this.template());
		this.dom = {
				$ALERT_BOX: $(".alert", this.$el),
				$VALIDATION_BOX: $(".personal-info-validation", this.$el),
				$THANK_YOU_MESSAGE: $("#thankYouMessageContent", this.$el),
				$PAGE_TITLE: $("#pageTitle", this.$el)
		}
		this.sendBtn = new MobileSurvey.ButtonView({ el: $("#sendPersonalDetailsBtn", this.$el) });
		this.dom.$THANK_YOU_MESSAGE.html(this.thankYouMessage);
		this.dom.$PAGE_TITLE.html(this.surveyTitle);
		this.sendBtn.enable();
		this.retakeSurveyButton = new google.ui.FastButton($(".retakeBtn", this.$el)[0],
				this.retakeSurvey);
		this.sendPersonalnfoButton = new google.ui.FastButton($("#sendPersonalDetailsBtn", this.$el)[0],
				this.sendPersonalInfo);
		this._rendered = true;
		return this;
	},
	close: function() {
		//DA could be that the view was never rendered (if reset is called on expired timer)
		if(this._rendered) {		
			this.sendBtn.close();
			this.retakeSurveyButton.reset();
			this.sendPersonalnfoButton.reset();
		}
		//this.remove();
	}
});

MobileSurvey.SurveyView = Backbone.View.extend({
	initialize: function () {
		_.bindAll(this, "goToThankYouPage", "saveSurvey", 
				"updateQuestionSet", "render", "goToQuestionsPage",
				"saveAListOfSurveys", "makeSaveRequest");
		this.storage = this.options.localStorage;
		this.questionsPage = new MobileSurvey.SurveyMobileView(
				{ 
					el: $("#questionsPage"),
					model: this.model 
				});
		this.thankYouPage = new MobileSurvey.ThankYouPageView({ 
			el: $("#thankYouPage"), 
			thankYouMessage: this.model.get("ThankYouMessage"),
			surveyTitle: this.model.get("Title"),
			localStorage: this.storage
		});
		Timer.startTimer();
		$(Timer).on(Timer.events.RESTART_SURVEY, this.goToQuestionsPage);
		this.questionsPage.on(this.questionsPage.pageEvents.THANK_YOU_PAGE,
				this.saveSurvey);
		this.thankYouPage.on(this.thankYouPage.pageEvents.RETAKE_SURVEY,
				this.goToQuestionsPage);
		this.dom = {
				$LOCATION_INPUT: $("#location", this.$el)
		}
	},
	goToQuestionsPage: function() {
		this.questionsPage.refresh();		
		$.mobile.changePage("#questionsPage");
		this.thankYouPage.close();
		Timer.startTimer();
	},
	goToThankYouPage: function (surveyResult) {
		//alert("success");
		if (surveyResult != undefined) {
			this.thankYouPage.setSurveyResultId(surveyResult.DbId);
		}
		this.thankYouPage.render();
		/*$("#loading-modal").modal("hide");*/
		loader.hideLoader();
		$.mobile.changePage("#thankYouPage");
	},
	setWidth: function (value) {
		this.$el.css("width", value);
	},
	render: function () {
		this.questionsPage.render();
	},
	updateQuestionSet: function () {
		this.model.set("QuestionSet", this.model.getQuestionSetCollectionAsJson(false));
	},
	saveSurvey: function (event) {
		/*$("#loading-modal").modal("show");
		$("#loading-modal").css("visibility", "visible");*/
		loader.showLoader();
		var self = this;
		this.updateQuestionSet();
		//DA now we have in this.model.get("QuestionSet") the required information
		var infoToUpload = this.model.getQuestionSetCollectionAsJson(false);
		var location = this.dom.$LOCATION_INPUT.val();
		var sendDataObject = {
				questions: infoToUpload,
				surveyResultId: this.model.get("SurveyResultId"),
				surveyTemplateId: this.model.get("SurveyTemplateId"),
				location: location
			};	
		this.makeSaveRequest(sendDataObject, this.goToThankYouPage,
		function() {	
			//alert("error");
			var surveyResults = JSON.parse(self.storage.getItem("surveyResults"));
			if (surveyResults == null || surveyResults == "null"
				|| surveyResults == undefined || surveyResults == "undefined") {
				var results = [];
				sendDataObject.Id = SurveyUtilities.Utilities.generateUUID();
				results.push(sendDataObject);
				self.storage.setItem("surveyResults",JSON.stringify(results));
			} else {
				sendDataObject.Id = SurveyUtilities.Utilities.generateUUID();
				surveyResults.push(sendDataObject);
				self.storage.setItem("surveyResults",JSON.stringify(surveyResults));
			}
			self.goToThankYouPage({DbId: sendDataObject.Id});				
		});
	},
	saveAListOfSurveys: function(surveys) {
		/* first save the results and then if everything worked just fine save
		 * the personal info
		 */
		var self = this;
		for (var i=0; i<surveys.length; ++i) {
			if (surveys[i].Id != undefined) {
				this.makeSaveRequest(surveys[i], function(result) {
					if (result.DbId > -1) {
						var surveyResults = JSON.parse(self.storage.getItem("surveyResults"));
						if (surveyResults != null && surveyResults != undefined &&
								surveyResults != "null" && surveyResults != "undefined") {
							var k=-1;
							for (var j=0; j<surveyResults.length; ++j) {
								if (surveyResults[j].Id == result.LocalId) {
									k=j;
								}
							}
							if (k>-1) {
								if (surveyResults[k].PersonalInfo != undefined) {
									var personalInfo = surveyResults[k].PersonalInfo;
									personalInfo.surveyResultId = result.DbId;
									personalInfo.LocalId = result.LocalId;
									self.thankYouPage.makeSendPersonalInfoRequest(personalInfo,	function(LocalId) {
										var surveyResults = JSON.parse(self.storage.getItem("surveyResults"));
										if (surveyResults != null && surveyResults != undefined &&
											surveyResults != "null" && surveyResults != "undefined") {
											var t=-1;
											for (var l=0; l<surveyResults.length; ++l) {
												if (surveyResults[l].Id == LocalId) {
													t=l;
												}
											}
											if (t>-1) {
												surveyResults.splice(t,1);
												self.storage.setItem("surveyResults",JSON.stringify(surveyResults));
											}
										}									
									}, function() {});
								} else {
									surveyResults.splice(k,1);
									self.storage.setItem("surveyResults",JSON.stringify(surveyResults));
								}
							}
						}
					}				
				}, function() {});
			} else {
				// it is just personal info
				self.thankYouPage.makeSendPersonalInfoRequest(surveys[i].PersonalInfo,	function(LocalId) {
					var surveyResults = JSON.parse(self.storage.getItem("surveyResults"));
					if (surveyResults != null && surveyResults != undefined &&
						surveyResults != "null" && surveyResults != "undefined") {
						var t=-1;
						for (var l=0; l<surveyResults.length; ++l) {
							if (surveyResults[l].PersonalInfo.LocalId == LocalId) {
								t=l;
							}
						}
						if (t>-1) {
							surveyResults.splice(t,1);
							self.storage.setItem("surveyResults",JSON.stringify(surveyResults));
						}
					}									
				}, function() {});
			}
		}
	},
	makeSaveRequest: function(sendDataObject, successCallback, errorCallback) {
		var sendData = JSON.stringify(sendDataObject);
		$.ajax({
			url: "http://dev.txtfeedback.net/MobileSurvey/SaveSurvey1",
			data: sendData,
			crossDomain: true,
			type: 'post',
			cache: false,
			dataType: "json",
			contentType: 'application/json',
			traditional: true,
			success: successCallback,
			error: errorCallback,
			timeout: 5000
		});		
	}
});