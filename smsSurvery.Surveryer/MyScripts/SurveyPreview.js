var SurveyPreview = SurveyPreview || {};
SurveyPreview.QuestionPreviewWebsiteView = Backbone.View.extend({
   initialize: function () {
      this.questionPreviewTemplate = _.template($("#question-preview-website-template").html());
      this.questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
   },
   render: function () {
      this.$el.html(this.questionPreviewTemplate(this.model.toJSON()));
      if (this.model.get("Type") == this.questionConstants.TYPE_RATING) {
         var starBarView = new SurveyElements.StarBarView({ el: $(".answerArea", this.$el) });
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
         $PREVIEW_CONTENT: $(".preview-content2", this.$el)
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
         $SURVEY_PREVIEW_CONTENT: $("#questions", this.$el)
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
SurveyPreview.SurveyMobileView = Backbone.View.extend({
   events: {      
     
   },   
   initialize: function () {
      _.bindAll(this,"render");
      this.surveyPreviewModel = this.options.surveyPreviewModel;
      this.el = $("#questions");
      //this.dom = {
      //   $SURVEY_PREVIEW_CONTENT: $("#questions", this.$el)
      //}
      this.surveyConstants = SurveyUtilities.Utilities.CONSTANTS_SURVEY;

      var self = this;
      //_.bindAll(this, "render", "saveSurvey",
      //    "hide", "getHeight");
      this.pageElements = {
         $DONE_BTN: $("#doneBtn", this.$el),
         $QUESTIONS_AREA: $("#questions", this.$el),
         $PAGE_TITLE: $("#pageTitle", this.$el)
      };
      this.pageEvents = {
         THANK_YOU_PAGE: "goToThankYouPageEvent"
      };    
      this.doneBtn = new SurveyPreview.ButtonView({
         el: this.pageElements.$DONE_BTN
      });
      this.doneBtnTitle = $("#doneBtnTitle").val();
      this.doneBtn.enable();
      this.doneBtn.setTitle(this.doneBtnTitle);
      //this.questionSet.on("change:PickedAnswer", function () {
      //   if (self.isSurveyComplete()) {
      //      self.doneBtn.enable();
      //   } else {
      //      self.doneBtn.disable();
      //   }
      //});
      this.doneBtn.on("click", this.saveSurvey);
      this.render();

     
   },   
   render: function () {
      
      var surveyPreviewView = new SurveyPreview.SurveyPreviewWebsiteView({ model: this.model });
      this.el.append(surveyPreviewView.render());
   }  
});