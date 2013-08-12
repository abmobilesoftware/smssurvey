var SurveyModals = SurveyModals || {};
SurveyModals.AnswersModalView = Backbone.View.extend({
   events: {
      "click .add-answer-btn": "addAnswer"
   },
   initialize: function () {
      _.bindAll(this, "render", "addAnswer", "deleteAnswer");
      this.template = _.template($("#no-answers-template").html());
      this.dom = {
         $ANSWERS_TABLE: $(".answers-table", this.$el)
      };
      this.answersCollection = new SurveyModals.AnswersCollection();
      _.each(this.model.get("Answers"), function (answer, index) {
         var answerModel = new SurveyModals.AnswerModel(answer);
         this.answersCollection.add(answer);
      }, this);
      this.answersCollection.on("add remove", this.render);
      this.answersCollection.on("deleteAnswerEvent", this.deleteAnswer)
      this.render();
   },
   render: function () {
      this.dom.$ANSWERS_TABLE.empty();
      if (this.answersCollection.models.length > 0) {
         _.each(this.answersCollection.models, function (answer, index) {
            var answerView = new SurveyModals.AnswerView({ model: answer });
            this.dom.$ANSWERS_TABLE.append(answerView.render());
         }, this);
      } else {
         this.dom.$ANSWERS_TABLE.append(this.template());
      }
   },
   addAnswer: function () {
      this.answersCollection.add(new SurveyModals.AnswerModel());
   },
   deleteAnswer: function (answer) {
      this.answersCollection.remove(answer);
   },
   getAnswers: function () {
      var answers = this.answersCollection.models;
      var answersLabelAsString = "";
      var answersIdentifierAsString = "";
      if (answers.length > 0) {
         answersLabelAsString = answers[0].get("AnswerLabel");
         answersIdentifierAsString = answers[0].get("AnswerIdentifier");
      }
      for (var i = 1; i < answers.length; ++i) {
         answersLabelAsString += ";" + answers[i].get("AnswerLabel");
         answersIdentifierAsString += ";" + answers[i].get("AnswerIdentifier");
      }
      return {
         "ValidAnswers": answersIdentifierAsString,
         "ValidAnswersDetails": answersLabelAsString
      };
   }
});

SurveyModals.AnswersModalModel = Backbone.Model.extend({
   defaults: {
      Answers: []
   }
});

SurveyModals.AnswerView = Backbone.View.extend({
   events: {
      "click .delete-answer": "deleteAnswer",
      "keyup .answer-label-input": "updateAnswer"
   },
   initialize: function () {
      _.bindAll(this, "deleteAnswer", "updateAnswer");
      this.template = _.template($("#answer-template").html());
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      return this.$el;
   },
   deleteAnswer: function (event) {
      event.preventDefault();
      this.model.trigger("deleteAnswerEvent", this.model);
   },
   updateAnswer: function (event) {
      this.set("AnswerLabel", event.currentTarget.value);
   }
});

SurveyModals.AnswerModel = Backbone.Model.extend({
   defaults: {
      AnswerIdentifier: "",
      AnswerLabel: ""
   }
});
SurveyModals.AnswersCollection = Backbone.Collection.extend({
   model: SurveyModals.AnswerModel
});