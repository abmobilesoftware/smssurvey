var MobileSurvey = MobileSurvey || {};

MobileSurvey.ButtonView = Backbone.View.extend({
   events: {
      "click": "click"
   },
   initialize: function () {
      _.bindAll(this, "click");
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
   }
});

MobileSurvey.QuestionMobileView = Backbone.View.extend({
   initialize: function () {
      this.questionMobileTemplate = _.template($("#question-mobile-template").html());
      this.questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;

      _.bindAll(this, "render", "toggleDisplay", "updateQuestionDisplay", "updateAnswer");
      this.model.on("change:ValidAnswer", this.updateQuestionDisplay);
   },
   render: function () {
      this.$el.html(this.questionMobileTemplate(this.model.toJSON()));
      if (this.model.get("Type") == this.questionConstants.TYPE_RATING) {
         var starBarView = new SurveyElements.StarBarView({ el: $(".answerArea", this.$el) });
      }
      return this.$el;
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
      var questionType= this.model.get("Type");
      if (questionType == this.questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         var selectedOption = $("select[name='answer']", this.$el).find(":selected").val();
         this.model.set({ "PickedAnswer": selectedOption }, { validate: true });
      } else if (questionType == this.questionConstants.TYPE_YES_NO) {
         var yesNoAnswer = $('input[name=yes-no-answer]:checked', this.$el).val();
         yesNoAnswer = yesNoAnswer != undefined ? yesNoAnswer : Question.noValueAnswer;         
         this.model.set({ "PickedAnswer": yesNoAnswer }, { validate: true });
      } else if (questionType == this.questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         var selectedValuesArr = $.map($('input[type=checkbox]:checked', this.$el), function (elem) {
            return $(elem).val();
         });
         var selectValues = selectedValuesArr.join(';');
         this.model.set({ "PickedAnswer": selectValues }, { validate: true });
      }
      else {
         this.model.set(
          {
             "PickedAnswer": $(".answer", this.$el).val(),
             "AdditionalInfo": $(".additionalInfo", this.$el) != undefined ? $(".additionalInfo", this.$el).val() : ""
          },
          {
             validate: true
          });
      }
   }
});

MobileSurvey.SurveyMobileView = Backbone.View.extend({
   events: {

   },
   initialize: function () {
      _.bindAll(this, "render", "saveSurvey");
      this.surveyPreviewModel = this.options.surveyPreviewModel;
      this.surveyMobileTemplate = _.template($("#mobileSurveyTemplate").html());
      this.el = $("#questions");
      this.surveyConstants = SurveyUtilities.Utilities.CONSTANTS_SURVEY;
      this.pageElements = {
         $DONE_BTN: $("#doneBtn", this.$el),
         $QUESTIONS_AREA: $("#questions", this.$el),
         $PAGE_TITLE: $("#pageTitle", this.$el)
      };
      this.pageEvents = {
         THANK_YOU_PAGE: "goToThankYouPageEvent"
      };
      this.doneBtn = new MobileSurvey.ButtonView({
         el: this.pageElements.$DONE_BTN
      });
      this.doneBtnTitle = $("#doneBtnTitle").val();
      this.doneBtn.enable();
      this.doneBtn.setTitle(this.doneBtnTitle);
      this.doneBtn.on("click", this.saveSurvey);
      this.questionsViews = [];     
   },
   render: function () {
      var self = this;
      this.el.append(this.surveyMobileTemplate());
      var areaToAddContentTo = $(".questionsArea", this.$el);
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         //DA now that the collection is sorted (due to the comparator on the collection) we can correctly set the QuestionNumber
         question.set("QuestionNumber", index + 1);
         var questionPreviewView = new MobileSurvey.QuestionMobileView({ model: question });
         this.questionsViews.push(questionPreviewView);
         areaToAddContentTo.append(questionPreviewView.render());
      }, this);
      return this.$el;
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
      this.$el.show();
   },
   getHeight: function () {
      return this.$el.outerHeight();
   }
});
MobileSurvey.ThankYouPageView = Backbone.View.extend({
   events: {
      "click #sendPersonalDetailsBtn": "sendPersonalInfo"

   },
   initialize: function () {
      _.bindAll(this, "setWidth", "show",
          "getHeight", "render","sendPersonalInfo",
          "setSurveyResultId");
      this.template = _.template($("#thankyoupage-template").html());
    
      this.render();
   },
   setSurveyResultId: function(surveyResultId) {
      this.surveyResultId = surveyResultId;
   },
   setWidth: function (value) {
      this.$el.css("width", value);
   },
   show: function () {
      this.$el.show();   
   },
   enableSendBtn: function (event) {
      if (event.target.checked) {
         this.sendBtn.enable();
      } else {
         this.sendBtn.disable();
      }
   },
   sendPersonalInfo: function (event) {
      $('#surveyUserInfo').slideToggle('slow');
      this.sendBtn.setTitle("Information submitted. Thank you!");
      this.sendBtn.disable();
      var personalInfo = {};
      personalInfo.Name = $('#name').val();
      personalInfo.Surname = $('#surname').val();
      personalInfo.Email = $('#email').val();
      personalInfo.Telephone = $('#telephone').val();
      var dataToSend = JSON.stringify({
         info: personalInfo,
         surveyResultId: this.surveyResultId         
      });
      $.ajax({
         url: "/MobileSurvey/SaveRespondentInfo",
         data: dataToSend,
         type: 'post',
         cache: false,
         dataType: "json",
         contentType: 'application/json',
         traditional: true      
      });
   },
   getHeight: function () {
      return this.$el.outerHeight();
   },
   setTop: function (value) {
      this.$el.css("top", value);
   },
   render: function () {
      this.$el.append(this.template());
      this.sendBtn = new MobileSurvey.ButtonView({ el: $("#sendPersonalDetailsBtn", this.$el) });
     
      this.sendBtn.enable();
      return this.$el;
   }
});

MobileSurvey.SurveyView = Backbone.View.extend({
   initialize: function () {
      _.bindAll(this, "goToThankYouPage","saveSurvey","updateQuestionSet", "render");
      this.questionsPage = new MobileSurvey.SurveyMobileView({ el: $("#questionsPage"), model: this.model });
      this.thankYouPage = new MobileSurvey.ThankYouPageView({ el: $("#thankYouPage") });      
      //this.questionsPage.on(this.questionsPage.pageEvents.THANK_YOU_PAGE,
      //    this.goToThankYouPage);
      this.questionsPage.on(this.questionsPage.pageEvents.THANK_YOU_PAGE,
         this.saveSurvey);
   },
   goToThankYouPage: function (surveyResultId) {
      var self = this;
      var pageWidthInPixels = this.questionsPage.getWidth() + "px";
      var expandedWidthPercent = "200%"; // width of two pages side by side
      var normalWidthPercent = "100%";
      this.questionsPage.setWidth(pageWidthInPixels);
      this.setWidth(expandedWidthPercent);
      this.thankYouPage.setSurveyResultId(surveyResultId);
      this.thankYouPage.show();
      this.thankYouPage.setWidth(pageWidthInPixels);
      /*
          thankYouPage.setTop - make thank you page visible during the transition.
          For devices with small display, where the survey doesn't fit entirely 
          on the screen and you have to scroll down.
      */
      var topPadding = this.questionsPage.getHeight() - window.innerHeight;
      this.thankYouPage.setTop(topPadding > 0 ? topPadding + "px" : 0);
      this.$el.animate({
         right: this.questionsPage.getWidth()
      }, {
         duration: 600,
         complete: function () {
            self.questionsPage.hide();
            self.thankYouPage.setTop(0);
            self.$el.css("right", "0px");
            self.thankYouPage.setWidth(normalWidthPercent);
            self.setWidth(normalWidthPercent);
         }
      });
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
      var self = this;
      this.updateQuestionSet();
      //DA now we have in this.model.get("QuestionSet") the required information
      var infoToUpload = this.model.getQuestionSetCollectionAsJson(false);
      var sendData = JSON.stringify({
         questions: infoToUpload,
         surveyResultId: this.model.get("SurveyResultId"),
         surveyPlanId: this.model.get("SurveyPlanId")
      });
      $.ajax({
         url: "/MobileSurvey/SaveSurvey",        
         data: sendData,
         type: 'post',
         cache: false,
         dataType: "json",
         contentType: 'application/json',
         traditional: true,
         success: function (surveyResultId) {
            self.goToThankYouPage(surveyResultId);
         }
      });
      //this.model.save(this.model.toJSON(),
      //   {
      //      success: function (model, response, options) {
      //         if (response.Result == "success") {
      //            self.dom.$NOTIFICATION_TEXT.text("Changes saved successfully.");
      //            self.dom.$NOTIFICATION_TEXT.
      //               removeClass("notification-success notification-error").addClass("notification-success");
      //            if (response.Operation == "create") {
      //               self.model.set("Id", response.Details);
      //            }
      //            self.model.set("DataChanged", false);
      //         } else if (response.Result == "error") {
      //            self.dom.$NOTIFICATION_TEXT.text("Errors while saving.");
      //            self.dom.$NOTIFICATION_TEXT.
      //               removeClass("notification-success notification-error").addClass("notification-error");
      //         }
      //         self.dom.$NOTIFICATION.show();
      //      }
      //   });
   }
});