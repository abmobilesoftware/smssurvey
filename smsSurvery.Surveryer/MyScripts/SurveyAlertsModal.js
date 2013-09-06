SurveyModals.AlertsModalView = Backbone.View.extend({
   events: {
      "click .add-alert-btn": "addAlert",
      "click .close-alerts-modal-btn": "closeModal",
      "click .save-alerts": "saveModal",
      "click .close-alerts-notifications": "closeAlertBox"
   },
   initialize: function () {
      _.bindAll(this, "render", "validationResult", "closeAlertBox");
      this.template = _.template($("#no-alerts-template").html());
      this.dom = {
         $ALERTS_MODAL_CONTENT: $(".alerts-modal-content", this.$el),
         $ALERTS_NOTIFICATIONS: $(".alerts-notifications", this.$el),
         $ALERT_BOX: $(".alert", this.$el)
      };      
      this.model.on(this.model.events.UPDATE_VIEW, this.render);
      this.model.on(this.model.events.VALIDATE, this.validationResult)
   },
   render: function () {
      this.dom.$ALERTS_MODAL_CONTENT.empty();
      if (this.model.getQuestionAlerts().length > 0) {
         _.each(this.model.getQuestionAlerts(), function (alert) {
            var alertView = new SurveyModals.AlertView({ model: alert });
            this.dom.$ALERTS_MODAL_CONTENT.append(alertView.render());
         }, this);
      } else {
         this.dom.$ALERTS_MODAL_CONTENT.append(this.template());
      }
   },
   addAlert: function (event) {
      event.preventDefault();
      this.model.addAlert();
   },
   closeModal: function (event) {
      this.model.restoreAlertsCollection();
      this.$el.modal("hide");      
   },
   saveModal: function (event) {
      var isDataValid = this.model.validateAlerts();
      if (isDataValid) {
         this.model.saveAlertsCollection();
         this.$el.modal("hide");         
      }
   },
   openModal: function () {
      this.model.backupAlertsCollection();
      this.dom.$ALERT_BOX.hide();
      this.model.refreshAlerts(this.model.get("QuestionType"));
   },
   validationResult: function (result) {
      if (result == this.model.errors.ERROR) {
         this.dom.$ALERTS_NOTIFICATIONS.html("Check the fields marked with red");
         this.dom.$ALERT_BOX.show();
      }
   },
   closeAlertBox: function () {
      this.dom.$ALERT_BOX.hide();
   }
});

SurveyModals.AlertsModalModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent",
      VALIDATE: "validateEvent"
   },
   errors: {
      ERROR: "error"
   },
   defaults: {
      QuestionAlertSet: [],
      QuestionType: "",
      Modal: {}
   },
   initialize: function () {
      _.bindAll(this, "refreshAlerts");
      this.alertsCollection = new SurveyModals.AlertsCollection();
      this.refreshAlerts(this.get("QuestionType"));
      this.alertsCollection.on("add remove", function () {
         this.trigger(this.events.UPDATE_VIEW);
         var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
         Backbone.trigger(attributeChangedEvent);
      }, this);
      this.alertClientId = -10000;
   },
   getAlertOperators: function (type) {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      if (type == questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         return new Array("==", "!=");
      } else if (type == questionConstants.TYPE_RATING) {
         return new Array("==", "!=", "<", "<=", ">", ">=");
      } else if (type == questionConstants.TYPE_FREE_TEXT) {
         return new Array("contains");
      } else if (type == questionConstants.TYPE_YES_NO) {
         return new Array("==");
      } else if (type == questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         return new Array("any", "all");
      }
   },
   getAlertOperatorsLabels: function (type) {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      if (type == questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         return new Array("equal", "not equal");
      } else if (type == questionConstants.TYPE_RATING) {
         return new Array("equal", "not equal", "<", "<=", ">", ">=");
      } else if (type == questionConstants.TYPE_FREE_TEXT) {
         return new Array("contains");
      } else if (type == questionConstants.TYPE_YES_NO) {
         return new Array("equal");
      } else if (type == questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         return new Array("any", "all");
      }
   },
   getTriggerAnswerValues: function (questionType) {
      var triggerAnswerValues = null;
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      if (questionType == questionConstants.TYPE_RATING) {
         var ratings = this.get("Modal").getRatings();
         var ratingsTriggerValues = [];
         for (var i = 0; i < ratings.length; ++i) {
            ratingsTriggerValues.push({
               TriggerLabel: ratings[i].get("RatingLabel"),
               TriggerValue: ratings[i].get("RatingIdentifier")
            });
         };
         triggerAnswerValues = ratingsTriggerValues;
      } else if (questionType == questionConstants.TYPE_SELECT_ONE_FROM_MANY
         || questionType == questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         var answers = this.get("Modal").getAnswers();
         var answersTriggerValues = [];
         for (var i = 0; i < answers.length; ++i) {
            answersTriggerValues.push({
               TriggerLabel: answers[i].get("AnswerLabel"),
               TriggerValue: answers[i].get("AnswerIdentifier")
            })
         };
         triggerAnswerValues = answersTriggerValues;
      } else if (questionType == questionConstants.TYPE_YES_NO) {
         var yesNoTriggerValues = [];
         yesNoTriggerValues.push({ TriggerLabel: "Yes", TriggerValue: "1" });
         yesNoTriggerValues.push({ TriggerLabel: "No", TriggerValue: "2" });
         triggerAnswerValues = yesNoTriggerValues;
      } else if (questionType == questionConstants.TYPE_FREE_TEXT) {
         triggerAnswerValues = [];
      }
      return triggerAnswerValues;
   },
   addAlert: function () {
      ++this.alertClientId;
      this.alertsCollection.add(new SurveyModals.AlertModel({
         Id: this.alertClientId,
         Description: "",
         TriggerAnswer: "",
         Operator: "",
         AlertOperatorsValues: this.getAlertOperators(this.get("QuestionType")),
         AlertOperatorsLabels: this.getAlertOperatorsLabels(this.get("QuestionType")),
         AlertNotification: {
            DistributionList: "",
            Id: "",
            Type: "email"
         },
         QuestionType: this.get("QuestionType"),
         TriggerAnswerValues: this.getTriggerAnswerValues(this.get("QuestionType"))
      }));
   },
   refreshAlerts: function (questionType) {
      this.set("QuestionType", questionType);
      for (var i = this.alertsCollection.models.length - 1; i > -1; i = i - 1) {
         this.alertsCollection.remove(this.alertsCollection.models[i]);
      }
      _.each(this.get("QuestionAlertSet"), function (alert) {
         alert = alert || {};        
         alert.AlertOperatorsValues = this.getAlertOperators(questionType);
         alert.AlertOperatorsLabels = this.getAlertOperatorsLabels(questionType);
         alert.TriggerAnswerValues = this.getTriggerAnswerValues(questionType);
         alert.QuestionType = questionType;
         var alertModel = new SurveyModals.AlertModel(alert);
         this.alertsCollection.add(alertModel);
      }, this);
   },
   getQuestionAlerts: function () {
      return this.alertsCollection.models;
   },
   getQuestionAlertsAsJson: function () {
      var alertsAsJson = [];
      _.each(this.alertsCollection.models, function (alert) {
         alertsAsJson.push(alert.toJSON());
      });
      return alertsAsJson;
   },
   emptyAlertsCollection: function () {
      for (var i = this.alertsCollection.models.length - 1; i > -1; i = i - 1) {
         this.alertsCollection.remove(this.alertsCollection.models[i]);
      }
   },
   validateAlerts: function () {
      var isValid = true;
      _.each(this.alertsCollection.models, function (alert) {
         var isValidAlert = alert.validateAlert();
         if (!isValidAlert) {
            isValid = isValidAlert;
         }
      });
      if (!isValid) this.trigger(this.events.VALIDATE, this.errors.ERROR);
      return isValid;
   },
   backupAlertsCollection: function () {
      this.alertsCollectionBackup = new SurveyModals.AlertsCollection();
      _.each(this.alertsCollection.models, function (alert) {
         this.alertsCollectionBackup.add(new SurveyModals.AlertModel(alert.toJSON()));
      }, this);
   },
   restoreAlertsCollection: function () {
      this.emptyAlertsCollection();
      _.each(this.alertsCollectionBackup.models, function (alert) {
         this.alertsCollection.add(new SurveyModals.AlertModel(alert.toJSON()));
      }, this);
   },
   saveAlertsCollection: function () {
      this.set("QuestionAlertSet", this.getQuestionAlertsAsJson());
   }
});

SurveyModals.AlertView = Backbone.View.extend({
   events: {
      "click .delete-alert-btn": "deleteAlert",
      "keyup .alert-trigger-answer-input": "updateTriggerAnswer",
      "change .alert-trigger-answer-select": "updateTriggerAnswer",
      "change .alert-operator-select": "updateOperator",
      "keyup .alert-distribution-list-input": "updateDistributionList",
      "keyup .alert-description-input": "updateDescription"
   },
   initialize: function () {
      _.bindAll(this, "render", "deleteAlert", "validationResult");
      this.template = _.template($("#alert-template").html());
      this.model.on(this.model.events.UPDATE_VIEW,
         this.render);
      this.model.on(this.model.events.VALIDATE, this.validationResult)
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.dom = {
         $ALERT_DESCRIPTION_INPUT: $(".alert-description-input", this.$el),
         $ALERT_TRIGGER_ANSWER_INPUT: $(".alert-trigger-answer-input", this.$el),
         $ALERT_DISTRIBUTION_LIST_INPUT: $(".alert-distribution-list-input", this.$el),
         $ALERT_OPERATOR_SELECT: $(".alert-operator-select", this.$el),
         $ALERT_TRIGGER_ANSWER_SELECT: $(".alert-trigger-answer-select", this.$el)         
      }
      this.dom.$ALERT_TRIGGER_ANSWER_SELECT.chosen({
         width: "45%",
         disable_search_threshold: 10,
         inherit_select_classes: true
      });
      this.dom.$ALERT_OPERATOR_SELECT.chosen({
         width: "45%",
         disable_search_threshold: 10,
         inherit_select_classes: true
      });
      this.dom.$ALERT_TRIGGER_ANSWER_SELECT_DIV = $("div.alert-trigger-answer-select", this.$el);
      this.dom.$ALERT_OPERATOR_SELECT_DIV= $("div.alert-operator-select", this.$el);
      return this.$el;
   },
   deleteAlert: function (event) {
      event.preventDefault();
      this.model.trigger(this.model.events.DELETE_ALERT, this.model);
   },
   updateTriggerAnswer: function (event) {
      this.model.updateTriggerAnswer(event.currentTarget.value);
   },
   updateDistributionList: function (event) {
      this.model.updateDistributionList(event.currentTarget.value);
   },
   updateDescription: function (event) {
      this.model.updateDescription(event.currentTarget.value);
   },
   updateOperator: function (event) {
      this.model.updateOperator(event.currentTarget.value);
   },   
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      this.dom.$ALERT_DESCRIPTION_INPUT.removeClass(invalidFieldClass);
      this.dom.$ALERT_TRIGGER_ANSWER_SELECT_DIV.removeClass(invalidFieldClass);
      this.dom.$ALERT_TRIGGER_ANSWER_INPUT.removeClass(invalidFieldClass);
      this.dom.$ALERT_DISTRIBUTION_LIST_INPUT.removeClass(invalidFieldClass);
      this.dom.$ALERT_OPERATOR_SELECT_DIV.removeClass(invalidFieldClass);
      if (result != "valid") {
         for (var i = 0; i < result.length; ++i) {
            if (result[i] == this.model.errors.INVALID_DESCRIPTION) {
               this.dom.$ALERT_DESCRIPTION_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_TRIGGER_ANSWER) {
               this.dom.$ALERT_TRIGGER_ANSWER_SELECT_DIV.addClass(invalidFieldClass);
               this.dom.$ALERT_TRIGGER_ANSWER_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_DISTRIBUTION_LIST) {
               this.dom.$ALERT_DISTRIBUTION_LIST_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_OPERATOR) {
               this.dom.$ALERT_OPERATOR_SELECT_DIV.addClass(invalidFieldClass);
            }
         }
      }
   }
});

SurveyModals.AlertModel = Backbone.Model.extend({
   errors: {
      INVALID_DESCRIPTION: "invalid description",
      INVALID_TRIGGER_ANSWER: "invalid trigger answer",
      INVALID_DISTRIBUTION_LIST: "invalid distribution list",
      INVALID_OPERATOR: "invalid operator",
      VALID: "valid"
   },
   events: {
      UPDATE_VIEW: "updateViewEvent",
      DELETE_ALERT: "deleteAlertEvent",
      VALIDATE: "validateEvent"
   },
   defaults: {
      Id: "",
      Description: "",
      TriggerAnswer: "",
      AlertOperatorsValues: [],
      AlertOperatorsLabels: [],
      AlertNotification: {}
   },
   updateTriggerAnswer: function (newTriggerAnswer) {
      this.set("TriggerAnswer", newTriggerAnswer);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateDistributionList: function (newDistributionList) {
      var alertNotification = this.get("AlertNotification");
      alertNotification.DistributionList = newDistributionList;
      this.set("AlertNotification", alertNotification);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateDescription: function (newDescription) {
      this.set("Description", newDescription);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateOperator: function (newOperator) {
      this.set("Operator", newOperator);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   validateAlert: function () {
      var errors = [];
      var hasErrors = false;
      if (this.get("TriggerAnswer").length == 0) {
         hasErrors = true;
         errors.push(this.errors.INVALID_TRIGGER_ANSWER);
      }
      if (this.get("Description").length == 0 || this.get("Description").length > 160) {
         hasErrors = true;
         errors.push(this.errors.INVALID_DESCRIPTION);
      }
      var areEmailsValid = true;
      var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
      var emailAddresses = this.get("AlertNotification").DistributionList.split(",");
      for (var i = 0; i < emailAddresses.length; ++i) {
         if (!filter.test(SurveyUtilities.Utilities.trim(emailAddresses[i]))) {
            areEmailsValid = false;
         }
      }
      if (this.get("Operator") == "" || this.get("Operator") == Question.noValueAnswer) {
         hasErrors = true;
         errors.push(this.errors.INVALID_OPERATOR);
      }
      if (!areEmailsValid) {
         hasErrors = true;
         errors.push(this.errors.INVALID_DISTRIBUTION_LIST);
      }
      if (hasErrors) {
         this.trigger(this.events.VALIDATE, errors);
         return false;
      } else {
         this.trigger(this.events.VALIDATE, "valid");
         return true;
      }
   }
});

SurveyModals.AlertsCollection = Backbone.Collection.extend({
   model: SurveyModals.AlertModel,
   initialize: function () {
      _.bindAll(this, "deleteAlert");
      this.on("deleteAlertEvent", this.deleteAlert)
   },
   deleteAlert: function (alert) {
      this.remove(alert);
   }
});