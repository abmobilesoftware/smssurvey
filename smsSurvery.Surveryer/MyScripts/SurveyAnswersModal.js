var SurveyModals = SurveyModals || {};
SurveyModals.AnswersModalView = Backbone.View.extend({
   events: {
      "click .add-answer-btn": "addAnswer",
      "click .close-answers-modal-btn": "closeModal",
      "click .save-answers": "saveModal"
   },
   initialize: function () {
      _.bindAll(this, "render", "addAnswer", "closeModal",
         "saveModal", "openModal", "validationResult");
      this.template = _.template($("#no-answers-template").html());
      this.dom = {
         $ANSWERS_TABLE: $(".answers-table", this.$el),
         $ANSWERS_NOTIFICATIONS: $(".answers-notifications", this.$el)
      };
      this.model.on(this.model.events.VALIDATE, this.validationResult)
      this.model.on(this.model.events.UPDATE_VIEW, this.render);
      this.render();
   },
   render: function () {
      this.dom.$ANSWERS_TABLE.empty();
      if (this.model.getAnswers().length > 0) {
         _.each(this.model.getAnswers(), function (answer, index) {
            var answerView = new SurveyModals.AnswerView({ model: answer });
            this.dom.$ANSWERS_TABLE.append(answerView.render());
         }, this);
      } else {
         this.dom.$ANSWERS_TABLE.append(this.template());
      }
   },
   addAnswer: function () {
      this.model.addAnswer();
   },
   closeModal: function () {
      this.model.restoreAnswersCollection();
      this.$el.modal("hide");
   },
   saveModal: function () {
      if (this.model.validate()) {
         this.$el.modal("hide");
      }
   },
   openModal: function () {
      this.dom.$ANSWERS_NOTIFICATIONS.hide();
      this.model.backupAnswersCollection();
   },
   validationResult: function(result) {
      if (result == "noAnswersDefined") {
         this.dom.$ANSWERS_NOTIFICATIONS.html("No answers defined. Add at least one answer.");
         this.dom.$ANSWERS_NOTIFICATIONS.show();
      } else if (result == "otherErrors") {
         this.dom.$ANSWERS_NOTIFICATIONS.html("Check the fields marked with red");
         this.dom.$ANSWERS_NOTIFICATIONS.show();
      }
   }
});

SurveyModals.AnswersModalModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent",
      VALIDATE: "validateEvent"
   },
   defaults: {
      Answers: []
   },
   initialize: function () {
      this.answersCollection = new SurveyModals.AnswersCollection();
      _.each(this.get("Answers"), function (answer, index) {
         var answerModel = new SurveyModals.AnswerModel(answer);
         this.answersCollection.add(answer);
      }, this);
      this.answersCollection.on("add remove", function () {
         this.trigger(this.events.UPDATE_VIEW);
         var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
         Backbone.trigger(attributeChangedEvent);
      }, this);
   },
   getAnswers: function () {
      return this.answersCollection.models;
   },
   getAnswersAsJson: function () {
      var answers = this.answersCollection.models;
      var answersLabelAsString = "";
      var answersIdentifierAsString = "";
      if (answers.length > 0) {
         answersLabelAsString = answers[0].get("AnswerLabel");
         answersIdentifierAsString = "1";
      }
      for (var i = 1; i < answers.length; ++i) {
         answersLabelAsString += ";" + answers[i].get("AnswerLabel");
         answersIdentifierAsString += ";" + (i+1);
      }
      return {
         "ValidAnswers": answersIdentifierAsString,
         "ValidAnswersDetails": answersLabelAsString
      };
   },
   addAnswer: function () {
      this.answersCollection.add(new SurveyModals.AnswerModel());
   },
   emptyAnswersCollection: function() {
      for (var i=this.answersCollection.models.length - 1; i>-1; --i) {
         this.answersCollection.remove(this.answersCollection.models[i]);
      }
   },
   validate: function () {
      var isValid = true;
      if (this.answersCollection.models.length > 0) {
         _.each(this.answersCollection.models, function (answer) {
            var answerValidity = answer.validate();
            if (!answerValidity) {
               isValid = answerValidity;
            }
         });
         if (!isValid) this.trigger(this.events.VALIDATE, "otherErrors");
      } else {
         isValid = false;
         this.trigger(this.events.VALIDATE, "noAnswersDefined");
      }
      return isValid;
   },
   backupAnswersCollection: function () {
      this.answersCollectionBackup = new SurveyModals.AnswersCollection();
      _.each(this.answersCollection.models, function (answer) {
         this.answersCollectionBackup.add(new SurveyModals.AnswerModel(answer.toJSON()));
      }, this);         
   },
   restoreAnswersCollection: function () {
      this.emptyAnswersCollection();
      _.each(this.answersCollectionBackup.models, function (answer) {
         this.answersCollection.add(new SurveyModals.AnswerModel(answer.toJSON()));
      }, this);      
   }
});

SurveyModals.AnswerView = Backbone.View.extend({
   tagName: "tr",
   events: {
      "click .delete-answer": "deleteAnswer",
      "keyup .answer-label-input": "updateAnswer"
   },
   initialize: function () {
      _.bindAll(this, "deleteAnswer", "updateAnswer", "validationResult");
      this.template = _.template($("#answer-template").html());
      this.model.on(this.model.events.VALIDATE, this.validationResult)
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.dom = {
         $ANSWER_LABEL_INPUT : $(".answer-label-input", this.$el)
      }
      return this.$el;
   },
   deleteAnswer: function (event) {
      event.preventDefault();
      this.model.trigger("deleteAnswerEvent", this.model);
   },
   updateAnswer: function (event) {
      this.model.updateAnswer(event.currentTarget.value);
   },
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      if (result == this.model.errors.INVALID_ANSWER_LABEL) {
         this.dom.$ANSWER_LABEL_INPUT.addClass(invalidFieldClass);
      } else if (result == this.model.errors.VALID) {
         this.dom.$ANSWER_LABEL_INPUT.removeClass(invalidFieldClass);
      }
   }
});

SurveyModals.AnswerModel = Backbone.Model.extend({
   events: {
      VALIDATE: "validateEvent"
   },
   errors: {
      INVALID_ANSWER_LABEL: "invalid answer label",
      VALID: "valid"
   },
   defaults: {
      AnswerIdentifier: "",
      AnswerLabel: ""
   },
   validate: function () {
      if (this.get("AnswerLabel").length == 0) {
         this.trigger(this.events.VALIDATE, this.errors.INVALID_ANSWER_LABEL)
         return false;
      } else {
         this.trigger(this.events.VALIDATE, this.errors.VALID);
         return true;
      }
   },
   updateAnswer: function (newAnswerLabel) {
      this.set("AnswerLabel", newAnswerLabel);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   }
});
SurveyModals.AnswersCollection = Backbone.Collection.extend({
   model: SurveyModals.AnswerModel,
   initialize: function () {
      _.bindAll(this, "deleteAnswer");
      this.on("deleteAnswerEvent", this.deleteAnswer);
   },
   deleteAnswer: function (answer) {
      this.remove(answer);
   }
});