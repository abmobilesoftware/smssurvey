var SurveyBuilder = SurveyBuilder || {};

SurveyBuilder.SurveyView = Backbone.View.extend({
   events: {
      "click .edit-survey": "editSurveyInfo",
      "keyup #survey-description": "updateDescription",
      "keyup #survey-thank-you-message": "updateThankYouMessage",
      "click .save-btn": "saveSurvey"
   },
   initialize: function () {
      var self = this;
      _.bindAll(this, "editSurveyInfo", "render", "updateDescription",
         "updateThankYouMessage", "saveSurvey", "confirmPageLeaving");
      this.template = _.template($("#survey-info-template").html());
      this.dom = {
         $SURVEY_INFO: $("#survey-info", this.$el),
         $SURVEY_BUILDER: $("#survey-builder", this.$el),
         $NOTIFICATION_TEXT: $(".notification-text", this.$el),
         $NOTIFICATION: $("#survey-notification", this.$el)
      }
      var dom = this.dom;
      this.model.on("change:DisplayInfoTable", this.render);
      this.model.on("change:Id", this.render);
      window.onbeforeunload = this.confirmPageLeaving;

      if (this.model.get("Id") != SurveyUtilities.Utilities.CONSTANTS_MISC.NEW_SURVEY) {
         this.model.fetch({
            data: "Id=" + this.model.get("Id"),
            success: function (model, response, options) {
               self.questionSetModel = new Question.QuestionSetModel({ jsonQuestions: model.get("QuestionSet") });
               self.questionSetView = new Question.QuestionSetView({
                  el: dom.$SURVEY_BUILDER,
                  model: self.questionSetModel
               });
               self.render();

            },
            error: function (model, response, options) {
               alert(response)
            }
         });
      } else {
         this.model.set("Id", -1);
         this.questionSetModel = new Question.QuestionSetModel();
         this.questionSetView = new Question.QuestionSetView({
            el: dom.$SURVEY_BUILDER,
            model: this.questionSetModel
         });
         this.render();
      }
   },
   render: function () {
      this.dom.$SURVEY_INFO.html(this.template(this.model.toJSON()));
      this.dom.$EDIT_SURVEY_INFO = $(".edit-survey", this.$el);
      this.dom.$INFO_TABLE = $(".survey-info-data", this.$el);
      this.dom.$SURVEY_INFO_TITLE_TEXT = $(".survey-info-title-text", this.$el);
   },
   editSurveyInfo: function (event) {
      event.preventDefault();
      if (this.model.get("DisplayInfoTable")) {
         this.model.set("DisplayInfoTable", false);
      } else {
         this.model.set("DisplayInfoTable", true);
      }
   },
   updateDescription: function (event) {
      this.model.updateDescription(event.currentTarget.value);
      this.dom.$SURVEY_INFO_TITLE_TEXT.text(event.currentTarget.value);
   },
   updateThankYouMessage: function (event) {
      this.model.updateThankYouMessage(event.currentTarget.value);
   },
   updateQuestionSet: function () {
      this.model.set("QuestionSet", this.questionSetModel.getQuestionSetCollectionAsJson());
   },
   saveSurvey: function (event) {
      event.preventDefault();
      var self = this;
      this.updateQuestionSet();
      this.model.save(this.model.toJSON(),
         {
            success: function (model, response, options) {
               if (response.Result == "success") {
                  self.dom.$NOTIFICATION_TEXT.text("Changes saved successfully.");
                  self.dom.$NOTIFICATION_TEXT.
                     removeClass("notification-success notification-error").addClass("notification-success");
                  if (response.Operation == "create") {
                     self.model.set("Id", response.Details);
                  }
                  self.model.set("DataChanged", false);
               } else if (response.Result == "error") {
                  self.dom.$NOTIFICATION_TEXT.text("Errors while saving.");
                  self.dom.$NOTIFICATION_TEXT.
                     removeClass("notification-success notification-error").addClass("notification-error");
               }
               self.dom.$NOTIFICATION.show();
            }
         });
   },
   confirmPageLeaving: function () {
      this.updateQuestionSet();
      if (this.model.get("DataChanged")) {
         return "On the page are unsaved fields and " +
            "this changes will be lost when you will leave the" +
            "page. Are you sure you want to do this?";
      }
   }
});

SurveyBuilder.SurveyModel = Backbone.Model.extend({
   defaults: {
      Id: 7,
      Description: "no description",
      ThankYouMessage: "no thank you message",
      StartDate: "no start date",
      EndDate: "no end date",
      IsRunning: false,
      DisplayInfoTable: false,
      HasChanged: false
   },
   initialize: function () {
      _.bindAll(this, "attributeChanged", "modelSynced");
      this.on("change", this.attributeChanged);
      this.on("sync", this.modelSynced);
   },
   urlByMethod: {
      "read": "/SurveyPlan/GetSurvey",
      "update": "/SurveyPlan/SaveSurvey"
   },
   idAttribute: "Id",
   updateThankYouMessage: function (thankYouMessage) {
      this.set("ThankYouMessage", thankYouMessage);
   },
   updateDescription: function (description) {
      this.set("Description", description);
   },
   sync: function (method, model, options) {
      options = options || {};
      options.url = this.urlByMethod[method];
      Backbone.sync(method, model, options);
   },
   attributeChanged: function () {
      this.set("DataChanged", true);
   },
   modelSynced: function () {
      this.set("DataChanged", false);
   }
});

SurveyBuilder.QuestionSetModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateView"
   },
   initialize: function () {
      _.bindAll(this, "getQuestionSetCollection");
      this.jsonQuestions = this.get("jsonQuestions");
      this.questionModels = [];
      if (this.jsonQuestions != undefined) {
         for (var i = 0; i < this.jsonQuestions.length; ++i) {
            var question = new Question.QuestionModel(this.jsonQuestions[i]);
            this.questionModels.push(question);
         }
      }
      this.questionSetCollection =
         new Question.QuestionSetCollection(this.questionModels);
      this.questionSetCollection.on("remove add",
         function () {
            this.trigger(this.events.UPDATE_VIEW);
         }, this);
      this.questionId = 15000;
   },
   getQuestionSetCollection: function () {
      return this.questionSetCollection.models;
   },
   getQuestionSetCollectionAsJson: function () {
      var collectionAsJson = [];
      for (var i = 0; i < this.questionSetCollection.models.length; ++i) {
         // set the last alerts changes
         this.questionSetCollection.models[i].setQuestionAlertSet();
         this.questionSetCollection.models[i].setAnswers();
         this.questionSetCollection.models[i].setRatings();

         // set the last answers changes
         collectionAsJson.push(this.questionSetCollection.models[i].toJSON());
      }
      return collectionAsJson;
   },
   addQuestion: function () {
      ++this.questionId;
      var questionModel = new Question.QuestionModel({ Id: this.questionId });
      questionModel.updateOrder(this.questionSetCollection.models.length);
      this.questionSetCollection.add(questionModel);
   },
   sortQuestionCollection: function () {
      this.questionSetCollection.sort();
   }
});