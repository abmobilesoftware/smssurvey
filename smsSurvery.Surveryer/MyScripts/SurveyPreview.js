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


