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
      this.model.set(
          {
             "PickedAnswer": $(".answer", this.$el).val()
          },
          {
             validate: true
          });
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
      this.doneBtn = new MobileSurvey.ButtonView({
         el: this.pageElements.$DONE_BTN
      });
      this.doneBtnTitle = $("#doneBtnTitle").val();
      this.doneBtn.enable();
      this.doneBtn.setTitle(this.doneBtnTitle);
      this.doneBtn.on("click", this.saveSurvey);
      this.questionsViews = [];
      this.thankYouPage = new MobileSurvey.ThankYouPageView({ el: $("#thankYouPage") });
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
      } else {
         this.doneBtn.setTitle(
             this.doneBtnTitle + " (" +
                 surveyStatus.status +
             ")"
             );
      }
   }
});

MobileSurvey.ThankYouPageView = Backbone.View.extend({
   events: {
      "click button": "sendEmail"
   },
   initialize: function () {
      _.bindAll(this, "setWidth", "show",
          "getHeight");
      this.sendBtn = new MobileSurvey.ButtonView({ el: $("#sendBtn", this.$el) });
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