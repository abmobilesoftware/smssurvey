var Question = Question || {};
Question.noValueAnswer = "noValue";
Question.QuestionModel = Backbone.Model.extend({
   errors: {
      INVALID_TEXT: "invalid text",
      VALID: "valid"
   },
   events: {
      ANSWERS_CHANGED: "answersChanged",
      ALERTS_CHANGED: "alertsChanged",
      VALIDATE: "validateEvent"
   },
   defaults: {
      Id: SurveyUtilities.Utilities.generateUUID(),
      Text: "",
      Type: SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_FREE_TEXT,
      Order: 0,
      QuestionNumber: 1,
      Answers: [],
      AlertOperatorsValues: [],
      AlertOperatorsLabels: [],
      QuestionAlertSet: [],
      PickedAnswer: Question.noValueAnswer,
      AdditionalInfo: "",
      ValidAnswer: true
   },
   initialize: function () {
      this.parseAttributes();
   },
   parseAttributes: function () {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      var miscConstants = SurveyUtilities.Utilities.CONSTANTS_MISC;
      if (this.get("Type") == questionConstants.TYPE_SELECT_ONE_FROM_MANY ||
         this.get("Type") == questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         var answersIdentifier = this.get("ValidAnswers") != null ?
            this.get("ValidAnswers").split(miscConstants.SEPARATOR_ANSWERS) : [];
         var answersLabel = this.get("ValidAnswersDetails") != null ?
            this.get("ValidAnswersDetails").split(miscConstants.SEPARATOR_ANSWERS) : [];
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
      this.set("Order", (newOrder + 1));
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateQuestionText: function (newText) {
      this.set("Text", newText);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateQuestionType: function (newType) {
      this.set("ValidAnswersDetails", "");
      this.set("Answers", []);
      this.set("Type", newType);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   setAnswersModalModel: function (answersModalModel) {
      this.answersModalModel = answersModalModel;
   },
   setAlertsModalModel: function (alertsModalModel) {
      this.alertsModalModel = alertsModalModel;
   },
   setRatingsModalModel: function (ratingsModalModel) {
      this.ratingsModalModel = ratingsModalModel;
   },
   setQuestionAlertSet: function () {
      this.set("QuestionAlertSet",
         this.alertsModalModel.getQuestionAlertsAsJson());
   },
   setAnswers: function () {
      if (this.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_MANY_FROM_MANY ||
         this.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_ONE_FROM_MANY) {
         if (this.answersModalModel != null) {
            var answersAsJson = this.answersModalModel.getAnswersAsJson();
            this.set("ValidAnswers", answersAsJson.ValidAnswers);
            this.set("ValidAnswersDetails", answersAsJson.ValidAnswersDetails);
         }
      }
      /* Attributes Answers (array) and ValidAnswers/ValidAnswersDetails(strings) keep the same information 
         and they must be kept synchronized. Answers is used on client and ValidAnswers(Details) is used on
         server-side.
         For better code clarity Answers(array) should be removed.
      */
      this.parseAttributes();
   },
   setRatings: function () {
      if (this.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_RATING) {
         if (this.ratingsModalModel != null) {
            var ratingsAsJson = this.ratingsModalModel.getRatingsAsJson();
            this.set("ValidAnswers", ratingsAsJson.ValidAnswers);
            this.set("ValidAnswersDetails", ratingsAsJson.ValidAnswersDetails);
         }
      }
   },
   setYesNo: function() {
      if (this.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_YES_NO) {
         this.set("ValidAnswers", "1;2");
         this.set("ValidAnswersDetails", "Yes;No");
      }
   },
   // Used for validation when building the survey
   validateQuestion: function () {
      if (this.get("Text").length == 0 || this.get("Text").length > 160) {
         this.trigger(this.events.VALIDATE, this.errors.INVALID_TEXT);
         return false;
      } else {
         this.trigger(this.events.VALIDATE, this.errors.VALID);
         return true;
      }
   },
   // Used for validation when taking the survey
   validate: function (attributes, options) {
      if (attributes.PickedAnswer === Question.noValueAnswer || attributes.PickedAnswer === "") {
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
      "keyup .question-input": "updateQuestion",
      "click .edit-answers-btn": "openAnswersModal",
      "click .edit-alerts-btn": "openAlertsModal",
      "click .edit-ratings-btn": "openRatingsModal"
   },
   initialize: function () {
      _.bindAll(this, "selectQuestionType", "deleteQuestion", "updateQuestion",
         "render", "initializeModals", "validationResult", "openAnswersModal",
         "openAlertsModal", "openRatingsModal");
      this.questionTemplate = _.template($("#question-template").html());
      this.model.on(this.model.events.ANSWERS_CHANGED, this.render);
      this.model.on(this.model.events.ALERTS_CHANGED, this.render);
      this.initializeModals();
      this.model.on("change:Type", this.initializeModals);
      this.model.on("change:Type", this.render);
      this.model.on(this.model.events.VALIDATE, this.validationResult)
   },
   render: function () {
      this.$el.html(this.questionTemplate(this.model.toJSON()));
      this.dom = {
         $TYPE_BUTTON: $(".type-btn-text", this.$el),
         $QUESTION_INPUT: $(".question-input", this.$el),
         $ANSWERS_TABLE: $(".answers-table", this.$el),
         $MULTIPLE_ANSWERS_MODAL: $("#multiple-answer-modal" + this.model.get("Id"), this.$el),
         $EDIT_ALERTS_MODAL: $("#edit-alerts-modal" + this.model.get("Id"), this.$el),
         $RATINGS_MODAL: $("#edit-rating-modal" + this.model.get("Id"), this.$el)
      };
      if (this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_ONE_FROM_MANY
         || this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_MANY_FROM_MANY) {
         this.answersModalView = new SurveyModals.AnswersModalView({
            el: this.dom.$MULTIPLE_ANSWERS_MODAL,
            model: this.answersModalModel
         });
         this.answersModalView.render();
      };
      if (this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_RATING) {
         this.ratingsModalView = new SurveyModals.RatingsModalView({
            el: this.dom.$RATINGS_MODAL,
            model: this.ratingsModalModel
         })
         this.ratingsModalView.render();
      };
      if (this.alertsModalView == null) {
         this.alertsModalView = new SurveyModals.AlertsModalView({
            el: this.dom.$EDIT_ALERTS_MODAL,
            model: this.alertsModalModel
         });
      };
      this.alertsModalView.render();

      return this.$el;
   },
   initializeModals: function () {
      if (this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_ONE_FROM_MANY
         || this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_SELECT_MANY_FROM_MANY) {
         this.answersModalModel = new SurveyModals.AnswersModalModel({ Answers: this.model.get("Answers") });
         this.model.setAnswersModalModel(this.answersModalModel);
         this.answersModalView = null;
      }
      if (this.model.get("Type") == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_RATING) {
         this.ratingsModalModel = new SurveyModals.RatingsModalModel({
            Ratings: this.model.get("ValidAnswersDetails") == null ? "" : this.model.get("ValidAnswersDetails"),
            ScaleSize: this.model.get("ValidAnswersDetails") == null ? 0 : this.model.get("ValidAnswersDetails").split(";").length == 1 ? 0 :
               this.model.get("ValidAnswersDetails").split(";").length
         });
         this.model.setRatingsModalModel(this.ratingsModalModel);
         this.ratingsModalView = null;
      }
      this.alertsModalModel = new SurveyModals.AlertsModalModel({
         QuestionAlertSet: this.model.get("QuestionAlertSet"),
         QuestionType: this.model.get("Type")
      });
      this.model.setAlertsModalModel(this.alertsModalModel);
      this.alertsModalView = null;
      this.alertsModalModel.updateAlertOperators(this.model.get("Type"));
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
   },
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      if (result == this.model.errors.INVALID_TEXT) {
         this.dom.$QUESTION_INPUT.addClass(invalidFieldClass);
      } else if (result == this.model.errors.VALID) {
         this.dom.$QUESTION_INPUT.removeClass(invalidFieldClass);
      }
   },
   openAnswersModal: function () {
      this.answersModalView.render();
      this.answersModalView.openModal();
   },
   openAlertsModal: function () {
      this.alertsModalView.render();
      this.alertsModalView.openModal();
   },
   openRatingsModal: function () {
      this.ratingsModalView.render();
      this.ratingsModalView.openModal();
   }
});

Question.QuestionSetCollection = Backbone.Collection.extend({
   events: {
      COLLECTION_CHANGED: "collectionChanged"
   },
   defaults: {
      SurveyResultId: "",
      SurveyPlanId: ""
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
      this.noQuestionsTemplate = _.template($("#no-questions-template").html());
      this.dom.$QUESTION_SET_CONTENT.on("sortupdate", this.listSorted);
      this.questionViewCollection = [];
      this.render();
   },
   render: function () {
      this.dom.$QUESTION_SET_CONTENT.empty();
      this.questionViewCollection.length = 0;
      var questionSetModels = this.model.getQuestionSetCollection();
      if (questionSetModels.length > 0) {
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
      } else {
         this.dom.$QUESTION_SET_CONTENT.append(this.noQuestionsTemplate());
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
      this.model.getQuestionSetCollectionAsJson(true);
      this.surveyPreviewView = new SurveyPreview.SurveyPreviewView({
         el: this.dom.$PREVIEW_MODAL,
         model: this.model,
         surveyPreviewModel: this.surveyPreviewModel
      });
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
            var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
            Backbone.trigger(attributeChangedEvent);
         }, this);
      this.questionTemporaryId = -10000;
   },
   getQuestionSetCollection: function () {
      return this.questionSetCollection.models;
   },
   getQuestionSet: function () {
      return this.questionSetCollection;
   },
   getQuestionSetCollectionAsJson: function (saveAlerts) {
      var collectionAsJson = [];
      for (var i = 0; i < this.questionSetCollection.models.length; ++i) {
         // set the last alerts changes
         if (saveAlerts) {
            this.questionSetCollection.models[i].setQuestionAlertSet();
         }
         this.questionSetCollection.models[i].setAnswers();
         this.questionSetCollection.models[i].setRatings();
         this.questionSetCollection.models[i].setYesNo();
         // set the last answers changes
         collectionAsJson.push(this.questionSetCollection.models[i].toJSON());
      }
      this.set("jsonQuestions", collectionAsJson);
      return collectionAsJson;
   },
   addQuestion: function () {
      ++this.questionTemporaryId;
      //DA before this, make sure that the changes are saved
      for (var i = 0; i < this.questionSetCollection.models.length; ++i) {
         // set the last alerts changes         
         this.questionSetCollection.models[i].setQuestionAlertSet();
         this.questionSetCollection.models[i].setAnswers();
         this.questionSetCollection.models[i].setRatings();
         this.questionSetCollection.models[i].setYesNo();
      }
      var questionModel = new Question.QuestionModel({ Id: this.questionTemporaryId });
      questionModel.updateOrder(this.questionSetCollection.models.length);
      this.questionSetCollection.add(questionModel);
   },
   sortQuestionCollection: function () {
      this.questionSetCollection.sort();
   },
   validateQuestionSetModel: function () {
      var isValid = true;
      _.each(this.questionSetCollection.models, function (question) {
         var questionValidity = question.validateQuestion();
         if (!questionValidity) {
            isValid = questionValidity;
         }
      });
      return isValid;
   }
});