var Question = Question || {};

Question.QuestionModel = Backbone.Model.extend({
   events: {
      ANSWERS_CHANGED: "answers changed",
      ALERTS_CHANGED: "alerts changed"
   },
   defaults: {
      Id: SurveyUtilities.Utilities.generateUUID(),
      Text: "",
      Type: SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_FREE_TEXT,
      Order: 0,
      QuestionNumber: 1,
      Answers: [],
      AlertOperators: [],
      QuestionAlertSet: [],
      PickedAnswer: "noValue",
      ValidAnswer: true
   },
   initialize: function () {
      this.parseAttributes();
   },
   parseAttributes: function () {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      var miscConstants = SurveyUtilities.Utilities.CONSTANTS_MISC;
      if (this.get("Type") == questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         var answersIdentifier = this.get("ValidAnswers").split(miscConstants.SEPARATOR_ANSWERS);
         var answersLabel = this.get("ValidAnswersDetails").split(miscConstants.SEPARATOR_ANSWERS);
         var answers = [];
         for (var i = 0; i < answersLabel.length; ++i) {
            answers.push({
               AnswerIdentifier: answersIdentifier[i],
               AnswerLabel: answersLabel[i]
            });
         }
         this.set("Answers", answers);
      }
   },
   deleteQuestion: function () {
      this.trigger("delete", this);
   },
   updateOrder: function (newOrder) {
      this.set("Order", newOrder);
   },
   updateQuestionText: function (newText) {
      this.set("Text", newText);
   },
   updateQuestionType: function (newType) {
      this.set("Type", newType);
   },
   setAnswersModalModel: function (answerModalModel) {
      this.answerModalModel = answerModalModel;
   },
   setAlertsModalModel: function (alertsModalModel) {
      this.alertsModalModel = alertsModalModel;
   },
   setQuestionAlertSet: function () {
      this.set("QuestionAlertSet",
         this.alertsModalModel.getQuestionAlertsAsJson());
   },
   validate: function (attributes, options) {
      if (attributes.PickedAnswer === "noValue" || attributes.PickedAnswer === "") {
         this.set({ "ValidAnswer": false }, { silent: false });
      } else {
         this.set({ "ValidAnswer": true }, { silent: false });
      }
   }
});

Question.QuestionView = Backbone.View.extend({
   tagName: "li",
   className: "question",
   events: {
      "click .question-type": "selectQuestionType",
      "click .delete-btn": "deleteQuestion",
      "keyup .question-input": "updateQuestion"
   },
   initialize: function () {
      _.bindAll(this, "selectQuestionType", "deleteQuestion", "updateQuestion",
         "render");
      this.questionTemplate = _.template($("#question-template").html());
      this.model.on(this.model.events.ANSWERS_CHANGED, this.render);
      this.model.on(this.model.events.ALERTS_CHANGED, this.render);

      this.answersModalModel = new SurveyModals.AnswersModalModel({ Answers: this.model.get("Answers") });
      this.model.setAnswersModalModel(this.answersModalModel);
      this.answersModalView = null;
      this.alertsModalModel = new SurveyModals.AlertsModalModel({
         QuestionAlertSet: this.model.get("QuestionAlertSet"),
         QuestionType: this.model.get("Type")
      });
      this.model.setAlertsModalModel(this.alertsModalModel);
      this.alertsModalView = null;
      this.model.on("change:Type", this.alertsModalModel.updateAlertOperators);
      this.model.on("change:Type", this.render);
   },
   render: function () {
      this.$el.html(this.questionTemplate(this.model.toJSON()));
      this.dom = {
         $TYPE_BUTTON: $(".type-btn-text", this.$el),
         $QUESTION_INPUT: $(".question-input", this.$el),
         $ANSWERS_TABLE: $(".answers-table", this.$el),
         $MULTIPLE_ANSWERS_MODAL: $("#multiple-answer-modal" + this.model.get("Id"), this.$el),
         $EDIT_ALERTS_MODAL: $("#edit-alerts-modal" + this.model.get("Id"), this.$el)
      };
      if (this.answersModalView == null) {
         this.answersModalView = new SurveyModals.AnswersModalView({
            el: this.dom.$MULTIPLE_ANSWERS_MODAL,
            model: this.answersModalModel
         });
      };
      if (this.alertsModalView == null) {
         this.alertsModalView = new SurveyModals.AlertsModalView({
            el: this.dom.$EDIT_ALERTS_MODAL,
            model: this.alertsModalModel
         });
      };
      this.answersModalView.render();
      this.alertsModalView.render();
      return this.$el;
   },
   selectQuestionType: function (event) {
      event.preventDefault();
      var newQuestionType = $(event.currentTarget).attr("value");
      this.model.updateQuestionType(newQuestionType);
   },
   deleteQuestion: function (event) {
      event.preventDefault();
      this.model.deleteQuestion();
   },
   updateOrder: function (parentOffset) {
      this.model.updateOrder(
         Math.round((this.$el.offset().top - parentOffset) /
         this.$el.innerHeight()));
   },
   updateQuestion: function (event) {
      this.model.updateQuestionText(event.currentTarget.value)
   }
});

Question.QuestionSetCollection = Backbone.Collection.extend({
   events: {
      COLLECTION_CHANGED: "collectionChanged"
   },
   model: Question.QuestionModel,
   initialize: function () {
      _.bindAll(this, "deleteModel");
      this.on("delete", this.deleteModel);
   },
   deleteModel: function (model) {
      this.remove(model);
   },
   comparator: function (question) {
      return question.get("Order");
   }
});

Question.QuestionSetView = Backbone.View.extend({
   events: {
      "click .add-question-btn": "addQuestion",
      "click .preview-btn": "previewSurvey"
   },
   initialize: function () {
      _.bindAll(this, "render", "addQuestion", "listSorted", "previewSurvey");
      this.model.on(this.model.events.UPDATE_VIEW, this.render)

      this.dom = {
         $ADD_QUESTION_BTN: $(".add-question-btn", this.$el),
         $QUESTION_SET_CONTENT: $("#question-set-content", this.$el),
         $PREVIEW_MODAL: $("#preview-modal", this.$el)
      };
      this.surveyPreviewModel = new SurveyPreview.SurveyPreviewModel();
      this.surveyPreviewView = new SurveyPreview.SurveyPreviewView({
         el: this.dom.$PREVIEW_MODAL,
         model: this.model,
         surveyPreviewModel: this.surveyPreviewModel
      });
      this.dom.$QUESTION_SET_CONTENT.on("sortupdate", this.listSorted);
      this.questionViewCollection = [];
      this.render();
   },
   render: function () {
      this.dom.$QUESTION_SET_CONTENT.empty();
      this.questionViewCollection.length = 0;
      var questionSetModels = this.model.getQuestionSetCollection();
      _.each(questionSetModels, function (question) {
         var questionView = new Question.QuestionView({ model: question });
         this.questionViewCollection.push(questionView);
         this.dom.$QUESTION_SET_CONTENT.append(questionView.render());
      }, this);
      this.dom.$QUESTION_SET_CONTENT.sortable({ axis: "y", handle: ".grip", cursor: "move" });
      if (questionSetModels.length < 5) {
         this.dom.$ADD_QUESTION_BTN.show();
      } else {
         this.dom.$ADD_QUESTION_BTN.hide();
      }
   },
   addQuestion: function (event) {
      event.preventDefault();
      this.model.addQuestion();
   },
   listSorted: function (event, ui) {
      _.each(this.questionViewCollection, function (questionView) {
         questionView.updateOrder(this.dom.$QUESTION_SET_CONTENT.offset().top);
      }, this);
      this.model.sortQuestionCollection();
   },
   previewSurvey: function () {
      this.surveyPreviewView.render();
   }
});

Question.QuestionSetModel = Backbone.Model.extend({
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
   },
   getQuestionSetCollection: function () {
      return this.questionSetCollection.models;
   },
   getQuestionSet: function () {
      return this.questionSetCollection;
   },
   getQuestionSetCollectionAsJson: function () {
      var collectionAsJson = [];
      for (var i = 0; i < this.questionSetCollection.models.length; ++i) {
         // set the last alerts changes
         this.questionSetCollection.models[i].setQuestionAlertSet();
         // set the last answers changes
         collectionAsJson.push(this.questionSetCollection.models[i].toJSON());
      }
      return collectionAsJson;
   },
   addQuestion: function () {
      var questionModel = new Question.QuestionModel();
      questionModel.updateOrder(this.questionSetCollection.models.length);
      this.questionSetCollection.add(questionModel);
   }
});