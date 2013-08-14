var SurveyPreview = SurveyPreview || {};
SurveyPreview.QuestionPreviewWebsiteView = Backbone.View.extend({
   initialize: function () {
      this.questionPreviewTemplate = _.template($("#question-preview-website-template").html());
      this.questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
   },
   render: function () {
      this.$el.html(this.questionPreviewTemplate(this.model.toJSON()));
      if (this.model.get("Type") == this.questionConstants.TYPE_RATING) {
         var starBarView = new SurveyElements.StarBarView({ el: $(".website-answer-area-preview", this.$el) });
      }
      return this.$el;
   }
});

SurveyPreview.QuestionPreviewSmsView = Backbone.View.extend({
   initialize: function () {
      this.questionPreviewTemplate = _.template($("#question-preview-sms-template").html());
   },
   render: function () {
      this.$el.html(this.questionPreviewTemplate(this.model.toJSON()));
      return this.$el;
   }
});

SurveyPreview.SurveyPreviewSmsView = Backbone.View.extend({
   className: "survey-preview-content",
   initialize: function () {
      this.surveyPreviewTemplate = _.template($("#preview-sms-template").html());
   },
   render: function () {
      this.$el.html(this.surveyPreviewTemplate());
      this.dom = {
         $PREVIEW_CONTENT: $(".preview-content", this.$el)
      }
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         var questionPreviewView = new SurveyPreview.QuestionPreviewSmsView({ model: question });
         this.dom.$PREVIEW_CONTENT.append(questionPreviewView.render());
      }, this);
      return this.$el;
   }
});

SurveyPreview.SurveyPreviewWebsiteView = Backbone.View.extend({
   className: "survey-preview-content",
   initialize: function () {
      this.surveyPreviewTemplate = _.template($("#preview-website-template").html());
   },
   render: function () {
      this.$el.html(this.surveyPreviewTemplate());
      this.dom = {
         $PREVIEW_CONTENT: $(".preview-content", this.$el)
      }
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         var questionPreviewView = new SurveyPreview.QuestionPreviewWebsiteView({ model: question });
         this.dom.$PREVIEW_CONTENT.append(questionPreviewView.render());
      }, this);
      return this.$el;
   }
});

SurveyPreview.SurveyPreviewView = Backbone.View.extend({
   events: {
      "click .sms-preview-btn": "displaySmsPreview",
      "click .mobile-website-btn": "displayMobileWebsitePreview"
   },
   initialize: function () {
      _.bindAll(this, "displaySmsPreview", "displayMobileWebsitePreview", "render");
      this.surveyPreviewModel = this.options.surveyPreviewModel;
      this.surveyPreviewModel.on("change:PreviewType", this.render);
      this.dom = {
         $SURVEY_PREVIEW_CONTENT: $("#preview-content-modal", this.$el)
      }
      this.surveyConstants = SurveyUtilities.Utilities.CONSTANTS_SURVEY;
   },
   render: function () {
      this.dom.$SURVEY_PREVIEW_CONTENT.empty();
      var surveyPreviewView = (this.surveyPreviewModel.get("PreviewType") == this.surveyConstants.TYPE_MOBILE_WEBSITE) ?
         new SurveyPreview.SurveyPreviewWebsiteView({ model: this.model })
         : new SurveyPreview.SurveyPreviewSmsView({ model: this.model });
      this.dom.$SURVEY_PREVIEW_CONTENT.append(surveyPreviewView.render());
   },
   displaySmsPreview: function () {
      this.surveyPreviewModel.set("PreviewType", this.surveyConstants.TYPE_SMS)
   },   
   displayMobileWebsitePreview: function () {
      this.surveyPreviewModel.set("PreviewType", this.surveyConstants.TYPE_MOBILE_WEBSITE);
   }
});

SurveyPreview.SurveyPreviewModel = Backbone.Model.extend({
   defaults: {
      PreviewType: SurveyUtilities.Utilities.CONSTANTS_SURVEY.TYPE_SMS
   }
});


SurveyPreview.ButtonView = Backbone.View.extend({
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


SurveyPreview.QuestionMobileView = Backbone.View.extend({
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
      this.model.set(
          {
             "PickedAnswer": $(".answer", this.$el).val()
          },
          {
             validate: true
          });
   }
});

SurveyPreview.SurveyMobileView = Backbone.View.extend({
   events: {      
     
   },   
   initialize: function () {
      _.bindAll(this,"render","saveSurvey");
      this.surveyPreviewModel = this.options.surveyPreviewModel;
      this.surveyMobileTemplate = _.template($("#mobileSurveyTemplate").html());
      this.el = $("#questions");     
      this.surveyConstants = SurveyUtilities.Utilities.CONSTANTS_SURVEY;        
      this.pageElements = {
         $DONE_BTN: $("#doneBtn", this.$el),
         $QUESTIONS_AREA: $("#questions", this.$el),
         $PAGE_TITLE: $("#pageTitle", this.$el)
      };
      this.doneBtn = new SurveyPreview.ButtonView({
         el: this.pageElements.$DONE_BTN
      });
      this.doneBtnTitle = $("#doneBtnTitle").val();
      this.doneBtn.enable();
      this.doneBtn.setTitle(this.doneBtnTitle);     
      this.doneBtn.on("click", this.saveSurvey);
      this.questionsViews = [];
      this.thankYouPage = new SurveyPreview.ThankYouPageView({ el: $("#thankYouPage") });
   },   
   render: function () {
      var self = this;
      this.el.append(this.surveyMobileTemplate());
      var areaToAddContentTo = $(".questionsArea", this.$el);
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         //DA now that the collection is sorted (due to the comparator on the collection) we can correctly set the QuestionNumber
         question.set("QuestionNumber", index + 1);
         var questionPreviewView = new SurveyPreview.QuestionMobileView({ model: question });
         this.questionsViews.push(questionPreviewView);
         areaToAddContentTo.append(questionPreviewView.render());
      }, this);
      return this.$el;
   },
   isSurveyComplete: function () {
      var answeredQuestions = 0;
      _.each(this.model.getQuestionSetCollection(), function (value, key, list) {
         if (value.get("ValidAnswer")) ++answeredQuestions;
      }, this);
      return {
         isDone:
             answeredQuestions == this.model.getQuestionSet().length ? true : false,
         status: answeredQuestions + "/" +  this.model.getQuestionSet().length
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
      } else {
         this.doneBtn.setTitle(
             this.doneBtnTitle + " (" +
                 surveyStatus.status +
             ")"
             );
      }
   }
});

SurveyPreview.ThankYouPageView = Backbone.View.extend({
   events: {
      "click button": "sendEmail"
   },
   initialize: function () {
      _.bindAll(this, "setWidth", "show",
          "getHeight");
      this.sendBtn = new ButtonView({ el: $("#sendBtn", this.$el) });
      this.sendBtn.setTitle("Send");
      this.sendBtn.enable();
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
   sendEmail: function (event) {
      alert("Send email");
   },
   getHeight: function () {
      return this.$el.outerHeight();
   },
   setTop: function (value) {
      this.$el.css("top", value);
   }
});

var ButtonView = Backbone.View.extend({
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