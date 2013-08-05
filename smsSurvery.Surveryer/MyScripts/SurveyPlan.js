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
   parseAttributes: function () {
      if (this.get("Type") == "Multiple answers") {
         var answersIdentifier = this.get("ValidAnswers").split(";");
         var answersLabel = this.get("ValidAnswersDetails").split(";");
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
      this.updateAnswerDbAttribute();
   },
   addAnswer: function () {
      var answers = this.get("Answers");
      answers.push({ AnswerLabel: "" });
      this.set("Answers", answers);
      this.trigger(this.events.ANSWERS_CHANGED);
      this.updateAnswerDbAttribute();
   },
   deleteAnswer: function (index) {
      var answers = this.get("Answers");
      answers.splice(index, 1);
      this.set("Answers", answers);
      this.trigger(this.events.ANSWERS_CHANGED);
      this.updateAnswerDbAttribute();
   },
   updateAnswerDbAttribute: function () {
      var answers = this.get("Answers");
      var answersLabelAsString = "";
      var answersIdentifierAsString = "";
      if (answers.length > 0) {
         answersLabelAsString = answers[0].AnswerLabel;
         answersIdentifierAsString = answers[0].AnswerIdentifier;
      }
      for (var i = 1; i < answers.length; ++i) {
         answersLabelAsString += ";" + answers[i].AnswerLabel;
         answersIdentifierAsString += ";" + answers[i].AnswerIdentifier;
      }
      this.set("ValidAnswers", answersIdentifierAsString);
      this.set("ValidAnswersDetails", answersLabelAsString);      
   },

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

var SurveyView = Backbone.View.extend({
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
      this.template = _.template($("#survey-info").html());
      this.dom = {
         $SURVEY_INFO: $("#survey-info", this.$el),
         $SURVEY_BUILDER: $("#survey-builder", this.$el),
         $NOTIFICATION_TEXT: $(".notification-text", this.$el),
         $NOTIFICATION: $("#survey-notification", this.$el)
      }
      var dom = this.dom;
      this.model.on("change:DisplayInfoTable", this.render);
      window.onbeforeunload = this.confirmPageLeaving;

      this.model.fetch({
         data: "Id=" + this.model.get("Id"),
         success: function (model, response, options) {
            self.questionSetModel = new QuestionSetModel({ jsonQuestions: model.get("QuestionSet") });
            self.questionSetView = new QuestionSetView({
               el: dom.$SURVEY_BUILDER,
               model: self.questionSetModel
            });            
            self.render();
            
         },
         error: function (model, response, options) {
            alert(response)
         }         
      });      
   },
   render: function () {
      this.dom.$SURVEY_INFO.html(this.template(this.model.toJSON()));
      this.dom.$EDIT_SURVEY_INFO = $(".edit-survey", this.$el);
      this.dom.$INFO_TABLE = $(".survey-info-data", this.$el);
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
               if (response == "success") {
                  self.dom.$NOTIFICATION_TEXT.text("Changes saved successfully.");
                  self.dom.$NOTIFICATION_TEXT.removeClass("notification-success notification-error").addClass("notification-success");                  
                  self.model.set("DataChanged", false);
               } else if (response == "error") {
                  self.dom.$NOTIFICATION_TEXT.text("Errors while saving.");
                  self.dom.$NOTIFICATION_TEXT.removeClass("notification-success notification-error").addClass("notification-error");                  
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

var SurveyModel = Backbone.Model.extend({
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

var QuestionSetModel = Backbone.View.extend({
   events: {
      UPDATE_VIEW: "updateView"
   },
   initialize: function () {
      _.bindAll(this, "getQuestionSetCollection");
      this.jsonQuestions = this.options.jsonQuestions;
      this.questionModels = [];
      for (var i = 0; i < this.jsonQuestions.length; ++i) {
         var question = new QuestionModel(this.jsonQuestions[i]);
         question.parseAttributes();
         this.questionModels.push(question);
      }
      this.questionSetCollection =
         new QuestionSetCollection(this.questionModels);
      this.questionSetCollection.on("remove add",
         function () {
            this.trigger(this.events.UPDATE_VIEW);
         }, this);
   },
   getQuestionSetCollection: function () {
      return this.questionSetCollection.models;
   },
   getQuestionSetCollectionAsJson: function () {
      var collectionAsJson = [];
      for (var i = 0; i < this.questionSetCollection.models.length; ++i) {
         collectionAsJson.push(this.questionSetCollection.models[i].toJSON());
      }
      return collectionAsJson;
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
   var urlParts = document.URL.split("/");
   var surveyId = urlParts[urlParts.length - 1];
   var surveyModel = new SurveyModel({
      Id: surveyId,
      Description: "Beer survey",
      ThankYouMessage: "Thank you!",
      StartDate: "15/6/2013",
      EndDate: "17/7/2013",
      IsRunning: false
   });
   var survey = new SurveyView({
      el: $("#survey"),
      model: surveyModel
   })
});

