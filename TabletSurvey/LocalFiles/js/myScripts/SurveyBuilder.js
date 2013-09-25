var SurveyBuilder = SurveyBuilder || {};

SurveyBuilder.SurveyView = Backbone.View.extend({
   events: {
      "click .edit-survey": "editSurveyInfo",
      "keyup #survey-description": "updateDescription",
      "keyup #survey-intro": "updateIntroMessage",
      "keyup #survey-thank-you-message": "updateThankYouMessage",
      "click .save-btn": "saveSurvey",
      "click .languageSelect": "updateSurveyLanguage",
      "click .close-survey-notifications": "closeSurveyNotifications"
   },
   initialize: function () {
      var self = this;
      _.bindAll(this, "editSurveyInfo", "render", "updateDescription",
         "updateThankYouMessage", "saveSurvey", "confirmPageLeaving",
         "validationResult", "updateSaveButton", "updateIntroMessage",
         "updateSurveyLanguage", "closeSurveyNotifications");
      this.template = _.template($("#survey-info-template").html());
      this.dom = {
         $SURVEY_INFO: $("#survey-info", this.$el),
         $SURVEY_BUILDER: $("#survey-builder", this.$el),
         $NOTIFICATION: $(".survey-notification", this.$el),
         $ALERT_BOX: $(".save-alert", this.$el),
         $SURVEY_LOADER: $("#survey-loader"),
         $SURVEY_CONTENT: $("#survey-content")
      };
      this.model.on("change:DisplayInfoTable", this.render);
      this.model.on("change:Id change:MobileWebsiteLocation", this.render);

      window.onbeforeunload = this.confirmPageLeaving;
      this.model.on(this.model.events.VALIDATE, this.validationResult);
      this.model.on(this.model.events.UPDATE_SAVE_BUTTON, this.updateSaveButton);
      this.model.on(this.model.events.SURVEY_LOADED, this.render);
      //TODO - this should actually be a renderLanguage
      this.model.on("change:DefaultLanguage", this.render);
      this.questionSetView = new Question.QuestionSetView({
         el: this.dom.$SURVEY_BUILDER,
         model: this.model.getQuestionSetModel()
      });

      /* First load locations and then when the locations are loaded 
      load survey */
      this.model.loadLocations();
   },
   render: function () {
      this.dom.$SURVEY_INFO.html(this.template(this.model.toJSON()));
      this.dom.$EDIT_SURVEY_INFO = $(".edit-survey", this.$el);
      this.dom.$INFO_TABLE = $(".survey-info-data", this.$el);
      this.dom.$SURVEY_INFO_TITLE_TEXT = $(".survey-info-title-text", this.$el);
      this.dom.$SURVEY_DESCRIPTION_INPUT = $("#survey-description", this.$el);
      this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT = $("#survey-thank-you-message", this.$el);
      this.dom.$SURVEY_INTRO_MESSAGE_INPUT = $("#survey-intro", this.$el);
      this.dom.$SAVE_SURVEY_BTN = $(".save-btn", this.$el);
      this.updateSaveButton(this.model.getNoOfAttributesChanged());
      this.dom.$SURVEY_LOADER.hide();
      this.dom.$SURVEY_CONTENT.fadeIn();

   },
   editSurveyInfo: function (event) {
      event.preventDefault();
      if (this.model.get("DisplayInfoTable")) {
         this.model.set("DisplayInfoTable", false);
      } else {
         this.model.set("DisplayInfoTable", true);
      }
   },
   updateIntroMessage: function (event) {
      this.model.updateIntroMessage(event.currentTarget.value);
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
   updateSurveyLanguage: function (event) {
      event.preventDefault();
      var newSurveyLanguage = $(event.currentTarget).attr("value");
      this.model.updateSurveyLanguage(newSurveyLanguage);
   },
   saveSurvey: function (event) {
      event.preventDefault();
      if (this.model.validateSurvey()) {
         var self = this;
         this.updateQuestionSet();
         this.model.save(this.model.toJSON(),
            {
               success: function (model, response, options) {
                  if (response.Result == "error") {
                     self.dom.$NOTIFICATION.text("Errors while saving.");
                     self.dom.$ALERT_BOX.removeClass("alert-success");
                     self.dom.$ALERT_BOX.addClass("alert-error");
                  } else {
                     self.dom.$NOTIFICATION.text("Changes saved successfully.");
                     self.dom.$ALERT_BOX.removeClass("alert-error");
                     self.dom.$ALERT_BOX.addClass("alert-success");
                     self.model.updateQuestionSetModel();
                  }
                  self.dom.$ALERT_BOX.show();
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
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      this.dom.$SURVEY_DESCRIPTION_INPUT.removeClass(invalidFieldClass);
      this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT.removeClass(invalidFieldClass);
      this.dom.$SURVEY_INTRO_MESSAGE_INPUT.removeClass(invalidFieldClass);

      var surveyHasError = false;
      for (var i = 0; i < result.length; ++i) {
         if (result[i] == this.model.errors.INVALID_DESCRIPTION) {
            this.dom.$SURVEY_DESCRIPTION_INPUT.addClass(invalidFieldClass);
            surveyHasError = true;
         } else if (result[i] == this.model.errors.INVALID_THANK_YOU_MESSAGE) {
            this.dom.$SURVEY_THANK_YOU_MESSAGE_INPUT.addClass(invalidFieldClass);
            surveyHasError = true;
         } else if (result[i] == this.model.errors.INVALID_INTRO_MESSAGE) {
            this.dom.$SURVEY_INTRO_MESSAGE_INPUT.addClass(invalidFieldClass);
            surveyHasError = true;
         } else if (result[i] == this.model.errors.ERROR_IN_QUESTION_SET) {
            surveyHasError = true;
         }
      }
      if (surveyHasError) {
         this.dom.$NOTIFICATION.text("Check the fields marked with red");
         this.dom.$ALERT_BOX.addClass("alert-error");
         this.dom.$ALERT_BOX.show();
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
   },
   closeSurveyNotifications: function () {
      this.dom.$ALERT_BOX.hide();
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
      INVALID_THANK_YOU_MESSAGE: "invalid thank you message",
      INVALID_INTRO_MESSAGE: "invalid intro message",
      ERROR_IN_QUESTION_SET: "errorInQuestionSet"
   },
   defaults: {
      Id: 7,
      Description: "no description",
      IntroMessage: "no intro message",
      ThankYouMessage: "no thank you message",
      DisplayInfoTable: false,
      HasChanged: false,
      MobileWebsiteLocation: "",
      DefaultLanguage: ""
   },
   initialize: function () {
      _.bindAll(this, "attributeChanged", "modelSynced", "updateSurveyLanguage");
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      this.on("change:Description change:ThankYouMessage change:IntroMessage change:DefaultLanguage", this.attributeChanged);

      this.on("sync", this.modelSynced);
      Backbone.on(attributeChangedEvent, this.attributeChanged);
      this.noOfAttributesChanged = 0;
      this.questionSetModel = new Question.QuestionSetModel();
   },
   urlByMethod: {
      "read": "http://tablet.txtfeedback.net/MobileSurvey/ActiveSurveyAsJson?company=TestAbMobile&location=TestTableta",
      "update": "http://tablet.txtfeedback.net/SurveyTemplate/SaveSurvey"
   },
   idAttribute: "Id",
   updateThankYouMessage: function (thankYouMessage) {
      this.set("ThankYouMessage", thankYouMessage);
   },
   updateIntroMessage: function (introMessage) {
      this.set("IntroMessage", introMessage);
   },
   updateDescription: function (description) {
      this.set("Description", description);
   },
   updateSurveyLanguage: function (language) {
      this.set("DefaultLanguage", language);
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
      this.result = [];
      var descriptionValidity = true;
      var thankYouMessageValidity = true;
      var introMessageValidity = true;

      var questionSetModelValidity = this.questionSetModel.validateQuestionSetModel();
      if (!questionSetModelValidity) {
         this.result.push(this.errors.ERROR_IN_QUESTION_SET);
      }
      if (this.get("Description").length == 0 || this.get("Description").length > 100) {
         this.result.push(this.errors.INVALID_DESCRIPTION)
         descriptionValidity = false;
      }
      if (this.get("IntroMessage").length == 0 || this.get("IntroMessage").length > 160) {
         this.result.push(this.errors.INVALID_INTRO_MESSAGE);
         introMessageValidity = false;
      }
      if (this.get("ThankYouMessage").length == 0 || this.get("ThankYouMessage").length > 160) {
         this.result.push(this.errors.INVALID_THANK_YOU_MESSAGE);
         thankYouMessageValidity = false;
      }
      if (!descriptionValidity || !introMessageValidity || !thankYouMessageValidity || !questionSetModelValidity) {
         this.trigger(this.events.VALIDATE, this.result);
      } else {
         this.trigger(this.events.VALIDATE, [])
      }
      return questionSetModelValidity && descriptionValidity && introMessageValidity && thankYouMessageValidity;
   },
   loadSurvey: function () {
      self = this;
      if (this.get("Id") != SurveyUtilities.Utilities.CONSTANTS_MISC.NEW_SURVEY) {
         this.fetch({
            silent: true,
            data: "Id=" + this.get("Id"),
            success: function (model, response, options) {
               self.trigger(self.events.SURVEY_LOADED);
               self.updateQuestionSetModel();
            },
            error: function (model, response, options) {
               alert(response)
            }
         });
      } else {
         this.set("Id", -1);
         this.setLocationsOnQuestionSetModel(this.locations);
         self.trigger(self.events.SURVEY_LOADED);
      }
   },
   loadLocations: function () {
      var self = this;
      $.ajax({
         url: "/Reports/GetAllLocationTags",
         success: function (data, textStatus, jqXHR) {
            self.locations = data;
            return self.loadSurvey();
         },
         type: "GET",
         async: false
      });

   },
   getQuestionSetModel: function () {
      return this.questionSetModel;
   },
   updateQuestionSetModel: function () {
      this.questionSetModel.updateQuestionSetCollection(this.get("QuestionSet"), this.locations);
   },
   getNoOfAttributesChanged: function () {
      return this.noOfAttributesChanged;
   },
   setLocationsOnQuestionSetModel: function (locations) {
      this.questionSetModel.setLocations(locations);
   }
});