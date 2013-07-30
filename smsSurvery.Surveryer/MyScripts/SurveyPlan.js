/* STAR BAR */
var Star = Backbone.Model.extend({});
var StarView = Backbone.View.extend({
   className: "star",
   events: {
      "click a": "click"
   },
   initialize: function () {
      _.bindAll(this, "click", "render");
      this.template = _.template($("#star-template").html());
      this.model.on("change:Active", this.render);
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      return this;
   },
   click: function (event) {
      event.preventDefault();
      this.model.trigger("starClickedEvent", this.model);
   }
});

var StarsCollection = Backbone.Collection.extend({
   model: Star,
   initialize: function () {
      this.on("starClickedEvent", this.starClicked);
   },
   starClicked: function (model) {
      for (var i = 0; i <= model.get("id") ; ++i) {
         this.at(i).set("Active", true);
      }
      for (var i = model.get("id") + 1; i < this.models.length; ++i) {
         this.at(i).set("Active", false);
      }
      this.trigger("resultEvent", model.get("id"));
   }
});

var StarBarView = Backbone.View.extend({
   events: {
      "keyup .additionalInfo": "inputAdditionalInfo"
   },
   className: "starsBar",
   initialize: function () {
      _.bindAll(this, "starClicked", "inputAdditionalInfo");
      var starsArray = [];
      this.options.noOfElements = this.options.noOfElements != undefined
          ? this.options.noOfElements : 5;
      for (var i = 0; i < this.options.noOfElements; ++i) {
         starsArray.push({ id: i, Active: false });
      }
      this.starsCollection = new StarsCollection(starsArray);
      this.starsCollection.on("resultEvent", this.starClicked);
      this.result = -1;
      this.render();      
   },
   render: function () {
      _.each(this.starsCollection.models, function (value, index, list) {
         var starView = new StarView({ model: value });
         this.$el.append(starView.render().el);
      }, this);
      this.$el.append("<fieldset class='additionalInfo'>" +
              "<legend class='stars-additional-info'>Why?</legend><textarea class='website-comment'></textarea></fieldset>" +
              "<input type='hidden' class='answer' value='noValue' />");
      this.domElements = {
         $ADDITIONAL_INFO: $(".additionalInfo", this.$el),
         $ANSWER: $(".answer", this.$el)
      };
      if (this.domElements.$ANSWER.val() == "noValue") {
         this.domElements.$ADDITIONAL_INFO.hide();
      }
   },
   starClicked: function (value) {
      if (value < 2) {
         this.domElements.$ADDITIONAL_INFO.show();
         this.saveResult(value,
             this.domElements.$ADDITIONAL_INFO.val());
      } else {
         this.domElements.$ADDITIONAL_INFO.hide();
         this.saveResult(value, "");
      }
      this.result = value;
   },
   inputAdditionalInfo: function (event) {
      this.saveResult(this.result, event.target.value);
   },
   saveResult: function (pValue, pAdditionalInfo) {
      /* save the result in .answer input field value attribute */
      var result = {};
      result.additionalInfo = pAdditionalInfo;
      result.value = pValue;
      this.domElements.$ANSWER.val(JSON.stringify(result));
   }
});
/* END STAR BAR */

var QuestionModel = Backbone.Model.extend({
   events: {
      ANSWERS_CHANGED: "answers changed"
   },
   defaults: {
      Id: MobileSurvey.Utilities.generateUUID(),
      Text: "",
      Type: "Free text",
      Order: 0,
      Answers: [
         {
            AnswerIdentifier: "1",
            AnswerLabel: "Yes"
         }
      ],
      IsAnswersModalOpen: false
   },
   initialize: function () {

   },
   deleteQuestion: function () {
      /*
      Destroy the model and listen to sync*/
      this.trigger("delete", this);
   },
   updateOrder: function (order) {
      this.set("Order", order);
   },
   updateQuestionText: function (text) {
      this.set("Text", text);
   },
   updateQuestionType: function (type) {
      this.set("Type", type);
   },
   updateAnswerLabel: function (label, index) {
      var answers = this.get("Answers");
      answers[index].AnswerLabel = label;
      this.set("Answers", answers);
   },
   addAnswer: function () {
      var answers = this.get("Answers");
      answers.push({ AnswerLabel: "" });
      this.set("Answers", answers);
      this.trigger(this.events.ANSWERS_CHANGED);
   },
   deleteAnswer: function (index) {
      var answers = this.get("Answers");
      answers.splice(index, 1);
      this.set("Answers", answers);
      this.trigger(this.events.ANSWERS_CHANGED);
   }

});

var QuestionView = Backbone.View.extend({
   tagName: "li",
   className: "question",
   events: {
      "click .question-type": "selectQuestionType",
      "click .delete-btn": "deleteQuestion",
      "keyup .question-input": "updateQuestion",
      "keyup .answer-label-input": "updateAnswer",
      "click .add-answer-btn": "addAnswer",
      "click .delete-answer": "deleteAnswer",
      "click .open-modal-btn": "openModal",
      "click .close-modal-btn": "closeModal"
   },
   initialize: function () {
      _.bindAll(this, "selectQuestionType", "deleteQuestion", "updateQuestion",
         "updateAnswer", "render", "deleteAnswer", "openModal", "closeModal");
      this.questionTemplate = _.template($("#question-template").html());
      this.model.on(this.model.events.ANSWERS_CHANGED, this.render);
      this.model.on("change:Type", this.render);
   },
   render: function () {
      this.$el.html(this.questionTemplate(this.model.toJSON()));
      this.dom = {
         TYPE_BUTTON: $(".type-btn-text", this.$el),
         QUESTION_INPUT: $(".question-input", this.$el),
         ANSWERS_TABLE: $(".answers-table", this.$el),
         MULTIPLE_ANSWERS_MODAL: $("#multiple-answer-modal" + this.model.get("Id"), this.$el)
      };

      this.dom.MULTIPLE_ANSWERS_MODAL.modal({
         "backdrop": false,
         "show": this.model.get("IsAnswersModalOpen") ? true : false
      });
      return this.$el;
   },
   selectQuestionType: function (event) {
      event.preventDefault();
      var questionType = event.currentTarget.innerHTML;
      this.dom.TYPE_BUTTON.text(questionType);
      this.model.updateQuestionType(questionType);
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
   updateAnswer: function (event) {
      this.model.updateAnswerLabel(event.currentTarget.value,
         $(event.currentTarget).parents("tr").attr("index"));
   },
   addAnswer: function (event) {
      event.preventDefault();
      this.model.addAnswer();
   },
   deleteAnswer: function (event) {
      event.preventDefault();
      this.model.deleteAnswer($(event.currentTarget).parents("tr").attr("index"));
   },
   openModal: function (event) {
      event.preventDefault();
      this.model.set("IsAnswersModalOpen", true);
   },
   closeModal: function (event) {
      event.preventDefault();
      this.model.set("IsAnswersModalOpen", false);
   }
});

var QuestionSetCollection = Backbone.Collection.extend({
   events: {
      COLLECTION_CHANGED: "collectionChanged"
   },
   model: QuestionModel,
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

var QuestionSetView = Backbone.View.extend({
   events: {
      "click .add-question-btn": "addQuestion",
      "click .preview-btn": "previewSurvey"
   },
   initialize: function () {
      _.bindAll(this, "render", "addQuestion", "listSorted", "previewSurvey");
      this.model.on(this.model.events.UPDATE_VIEW, this.render)

      this.dom = {
         ADD_QUESTION_BTN: $(".add-question-btn", this.$el),
         QUESTION_SET_CONTENT: $("#question-set-content", this.$el),
         PREVIEW_MODAL: $("#preview-modal", this.$el)
      };
      this.surveyPreviewModel = new SurveyPreviewModel();
      this.surveyPreviewView = new SurveyPreviewView({
         el: this.dom.PREVIEW_MODAL,
         model: this.model,
         surveyPreviewModel: this.surveyPreviewModel
      });
      this.dom.QUESTION_SET_CONTENT.on("sortupdate", this.listSorted);
      this.questionViewCollection = [];
      this.render();
   },
   render: function () {
      this.dom.QUESTION_SET_CONTENT.empty();
      this.questionViewCollection.length = 0;
      var questionSetModels = this.model.getQuestionSetCollection();
      _.each(questionSetModels, function (question) {
         var questionView = new QuestionView({ model: question });
         this.questionViewCollection.push(questionView);
         this.dom.QUESTION_SET_CONTENT.append(questionView.render());
      }, this);
      this.dom.QUESTION_SET_CONTENT.sortable();
      if (questionSetModels.length < 5) {
         this.dom.ADD_QUESTION_BTN.show();
      } else {
         this.dom.ADD_QUESTION_BTN.hide();
      }
   },
   addQuestion: function (event) {
      event.preventDefault();
      this.model.addQuestion();
   },
   listSorted: function (event, ui) {
      _.each(this.questionViewCollection, function (questionView) {
         questionView.updateOrder(this.dom.QUESTION_SET_CONTENT.offset().top);
      }, this);
      this.model.sortQuestionCollection();
   },
   previewSurvey: function () {
      this.surveyPreviewView.render();
   }
});

var SurveyView = new Backbone.View.extend({
   initialize: function () {

   },
   render: function () {

   }
});

var SurveyModel = new Backbone.Model.extend({
   defaults: {
      Description: "no description",
      ThankYouMessage: "no thank you message",
      StartDate: "no start date",
      EndDate: "no end date",
      IsRunning: false
   }
});

var QuestionSetModel = Backbone.View.extend({
   events: {
      UPDATE_VIEW: "updateView"
   },
   initialize: function () {
      _.bindAll(this, "getQuestionSetCollection");
      var questionModel1 = new QuestionModel({
         Id: 0,
         Text: "What's your favorite beer?",
         Type: "Multiple answers",
         Order: 0,
         Answers: [
            {
               AnswerLabel: "Ursus"
            },
            {
               AnswerLabel: "Becks",
            },
            {
               AnswerLabel: "Silva",
            }
         ]
      });
      var questionModel2 = new QuestionModel({
         Id: 1,
         Text: "Rate the movie Sherlock Holmes",
         Type: "Rating",
         Order: 1,
         Answers: [
            {
               AnswerLabel: "Red"
            },
            {
               AnswerLabel: "Blue",
            },
            {
               AnswerLabel: "Green",
            }
         ]
      });
      var questionModel3 = new QuestionModel({
         Id: 2,
         Text: "Do you have iPhone?",
         Type: "Yes/No",
         Order: 2,
         Answers: [
            {
               AnswerLabel: "Red"
            },
            {
               AnswerLabel: "Blue",
            },
            {
               AnswerLabel: "Green",
            }
         ]
      });
      var questionModels = [questionModel1, questionModel2, questionModel3];
      this.questionSetCollection =
         new QuestionSetCollection(questionModels);
      this.questionSetCollection.on("remove add",
         function () {
            this.trigger(this.events.UPDATE_VIEW);
         }, this);
   },
   getQuestionSetCollection: function () {
      return this.questionSetCollection.models;
   },
   addQuestion: function () {
      var questionModel = new QuestionModel();
      questionModel.updateOrder(this.questionSetCollection.models.length);
      this.questionSetCollection.add(questionModel);
   },
   sortQuestionCollection: function () {
      this.questionSetCollection.sort();
   }
});

var QuestionPreviewWebsiteView = Backbone.View.extend({
   initialize: function () {
      this.questionPreviewTemplate = _.template($("#question-preview-website-template").html());
   },
   render: function () {
      this.$el.html(this.questionPreviewTemplate(this.model.toJSON()));
      if (this.model.get("Type") == "Rating") {
         var starBarView = new StarBarView({ el: $(".website-answer-area-preview", this.$el) });
      }
      return this.$el;
   }
});

var QuestionPreviewSmsView = Backbone.View.extend({
   initialize: function () {
      this.questionPreviewTemplate = _.template($("#question-preview-sms-template").html());
   },
   render: function () {
      this.$el.html(this.questionPreviewTemplate(this.model.toJSON()));
      return this.$el;
   }
});

var SurveyPreviewSmsView = Backbone.View.extend({
   className: "survey-preview-content",
   initialize: function () {
      this.surveyPreviewTemplate = _.template($("#preview-sms-template").html());      
   },
   render: function () {
      this.$el.html(this.surveyPreviewTemplate());
      this.dom = {
         PREVIEW_CONTENT: $(".preview-content", this.$el)
      }
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         var questionPreviewView = new QuestionPreviewSmsView({ model: question });
         this.dom.PREVIEW_CONTENT.append(questionPreviewView.render());
      }, this);
      return this.$el;
   }
});

var SurveyPreviewWebsiteView = Backbone.View.extend({
   className: "survey-preview-content",
   initialize: function () {
      this.surveyPreviewTemplate = _.template($("#preview-website-template").html());
   },
   render: function () {
      this.$el.html(this.surveyPreviewTemplate());
      this.dom = {
         PREVIEW_CONTENT: $(".preview-content", this.$el)
      }
      _.each(this.model.getQuestionSetCollection(), function (question, index) {
         var questionPreviewView = new QuestionPreviewWebsiteView({ model: question });
         this.dom.PREVIEW_CONTENT.append(questionPreviewView.render());
      }, this);
      return this.$el;
   }
});

var SurveyPreviewView = Backbone.View.extend({
   events: {
      "click .sms-preview-btn": "displaySmsPreview",
      "click .mobile-website-btn": "displayMobileWebsitePreview"
   },
   initialize: function () {
      _.bindAll(this, "displaySmsPreview", "displayMobileWebsitePreview", "render");
      this.surveyPreviewModel = this.options.surveyPreviewModel;
      this.surveyPreviewModel.on("change:PreviewType", this.render);
      this.dom = {
         SURVEY_PREVIEW_CONTENT: $("#preview-content-modal", this.$el)
      }
   },
   render: function () {
      this.dom.SURVEY_PREVIEW_CONTENT.empty();
      var surveyPreviewView = (this.surveyPreviewModel.get("PreviewType") == "mobile website") ?
         new SurveyPreviewWebsiteView({ model: this.model })
         : new SurveyPreviewSmsView({ model: this.model });
      this.dom.SURVEY_PREVIEW_CONTENT.append(surveyPreviewView.render());
   },
   displaySmsPreview: function () {
      this.surveyPreviewModel.set("PreviewType", "sms")
   },
   displayMobileWebsitePreview: function () {
      this.surveyPreviewModel.set("PreviewType", "mobile website");
   }
});

var SurveyPreviewModel = Backbone.Model.extend({
   defaults: {
      PreviewType: "sms"
   }
});

$(document).ready(function () {
   var questionSetModel = new QuestionSetModel();
   var questionSetView = new QuestionSetView({ el: $("#survey-builder"), model: questionSetModel });
});

