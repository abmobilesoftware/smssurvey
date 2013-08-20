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
         "updateThankYouMessage", "saveSurvey", "confirmPageLeaving",
         "surveyLoaded", "validationResult", "updateSaveButton");
      this.template = _.template($("#survey-info-template").html());
      this.dom = {
         $SURVEY_INFO: $("#survey-info", this.$el),
         $SURVEY_BUILDER: $("#survey-builder", this.$el),
         $NOTIFICATION_TEXT: $(".notification-text", this.$el),
         $NOTIFICATION: $("#survey-notification", this.$el)
      }
      this.model.on("change:DisplayInfoTable", this.render);
      this.model.on("change:Id change:MobileWebsiteLocation", this.render);
      window.onbeforeunload = this.confirmPageLeaving;
      this.model.on(this.model.events.SURVEY_LOADED, this.surveyLoaded);
      this.model.on(this.model.events.VALIDATE, this.validationResult);
      this.model.on(this.model.events.UPDATE_SAVE_BUTTON, this.updateSaveButton);
      this.model.loadSurvey();
   },
   render: function () {
      this.dom.$SURVEY_INFO.html(this.template(this.model.toJSON()));
      this.dom.$EDIT_SURVEY_INFO = $(".edit-survey", this.$el);
      this.dom.$INFO_TABLE = $(".survey-info-data", this.$el);
      this.dom.$SURVEY_INFO_TITLE_TEXT = $(".survey-info-title-text", this.$el);
      this.dom.$SURVEY_DESCRIPTION_INPUT = $("#survey-description", this.$el);
      this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT = $("#survey-thank-you-message", this.$el);
      this.dom.$SAVE_SURVEY_BTN = $(".save-btn", this.$el);
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
      this.model.set("QuestionSet", this.model.getQuestionSetModel().getQuestionSetCollectionAsJson(true));
   },
   saveSurvey: function (event) {
      event.preventDefault();
      if (this.model.validateSurvey()) {
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
                        self.model.set("MobileWebsiteLocation", response.MobileWebsiteLocation);
                     }
                     setTimeout(function () {
                        self.model.set("DataChanged", false);
                     }, 1000);
                  } else if (response.Result == "error") {
                     self.dom.$NOTIFICATION_TEXT.text("Errors while saving.");
                     self.dom.$NOTIFICATION_TEXT.
                        removeClass("notification-success notification-error").addClass("notification-error");
                  }
                  self.dom.$NOTIFICATION.show();
               }
            });
      }
   },
   confirmPageLeaving: function () {
      if (this.model.get("DataChanged")) {
         return "On the page are unsaved fields and " +
            "this changes will be lost when you will leave the" +
            "page. Are you sure you want to do this?";
      }
   },
   surveyLoaded: function () {
      this.questionSetView = new Question.QuestionSetView({
         el: this.dom.$SURVEY_BUILDER,
         model: this.model.getQuestionSetModel()
      });
      this.render();
   },
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      this.dom.$SURVEY_DESCRIPTION_INPUT.removeClass(invalidFieldClass);
      this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT.removeClass(invalidFieldClass);

      for (var i = 0; i < result.length; ++i) {
         if (result[i] == this.model.errors.INVALID_DESCRIPTION) {
            this.dom.$SURVEY_DESCRIPTION_INPUT.addClass(invalidFieldClass);
         } else if (result[i] == this.model.errors.INVALID_THANK_YOU_MESSAGE) {
            this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT.addClass(invalidFieldClass);
         }
      }
   },
   updateSaveButton: function (noOfChanges) {
      if (noOfChanges == 0) {
         this.dom.$SAVE_SURVEY_BTN.prop('disabled', true);
         this.dom.$SAVE_SURVEY_BTN.addClass("disabled");
         this.dom.$SAVE_SURVEY_BTN.text("Save ( no changes )");
      } else {
         this.dom.$SAVE_SURVEY_BTN.prop('disabled', false);
         this.dom.$SAVE_SURVEY_BTN.text("Save ( unsaved changes )");
         this.dom.$SAVE_SURVEY_BTN.removeClass("disabled");
      }
   }
});

SurveyBuilder.SurveyModel = Backbone.Model.extend({
   events: {
      SURVEY_LOADED: "surveyLoadedEvent",
      VALIDATE: "validateEvent",
      UPDATE_SAVE_BUTTON: "updateSaveButtonEvent"
   },
   errors: {
      INVALID_DESCRIPTION: "invalid description",
      INVALID_THANK_YOU_MESSAGE: "invalid thank you message"
   },
   defaults: {
      Id: 7,
      Description: "no description",
      ThankYouMessage: "no thank you message",
      DisplayInfoTable: false,
      HasChanged: false,
      MobileWebsiteLocation: ""
   },
   initialize: function () {
      _.bindAll(this, "attributeChanged", "modelSynced");
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      this.on("change:Description change:ThankYouMessage", this.attributeChanged);
      this.on("sync", this.modelSynced);
      Backbone.on(attributeChangedEvent, this.attributeChanged);
      this.noOfAttributesChanged = 0;
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
      parsedMethod = method == "update" ? "create" : method;
      Backbone.sync(parsedMethod, model, options);
   },
   attributeChanged: function () {
      this.set("DataChanged", true);
      ++this.noOfAttributesChanged;
      this.trigger(this.events.UPDATE_SAVE_BUTTON, this.noOfAttributesChanged);
   },
   modelSynced: function () {
      this.set({ "DataChanged": false }, { silent: true });
      this.noOfAttributesChanged = 0;
      this.trigger(this.events.UPDATE_SAVE_BUTTON, this.noOfAttributesChanged);
   },
   validateSurvey: function () {
      var questionSetModelValidity = this.questionSetModel.validateQuestionSetModel();
      var descriptionValidity = true;
      var thankYouMessageValidity = true;
      this.result = [];
      if (this.get("Description").length == 0 || this.get("Description").length > 100) {
         this.result.push(this.errors.INVALID_DESCRIPTION)
         descriptionValidity = false;
      }
      if (this.get("ThankYouMessage").length == 0 || this.get("ThankYouMessage").length > 160) {
         this.result.push(this.errors.INVALID_THANK_YOU_MESSAGE);
         thankYouMessageValidity = false;
      }
      if (!descriptionValidity || !thankYouMessageValidity) {
         this.trigger(this.events.VALIDATE, this.result);
      } else {
         this.trigger(this.events.VALIDATE, [])
      }
      return questionSetModelValidity && descriptionValidity && thankYouMessageValidity;
   },
   loadSurvey: function () {
      self = this;
      if (this.get("Id") != SurveyUtilities.Utilities.CONSTANTS_MISC.NEW_SURVEY) {
         this.fetch({
            silent: true,
            data: "Id=" + this.get("Id"),
            success: function (model, response, options) {
               self.questionSetModel = new Question.QuestionSetModel({ jsonQuestions: model.get("QuestionSet") });
               self.trigger(self.events.SURVEY_LOADED);
            },
            error: function (model, response, options) {
               alert(response)
            }            
         });
      } else {
         this.set("Id", -1);
         this.questionSetModel = new Question.QuestionSetModel();
         self.trigger(self.events.SURVEY_LOADED);
      }
   },
   getQuestionSetModel: function () {
      return this.questionSetModel;
   }
});