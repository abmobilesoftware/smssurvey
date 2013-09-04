SurveyModals.AlertsModalView = Backbone.View.extend({
   events: {
      "click .add-alert-btn": "addAlert",
      "click .close-alerts-modal-btn": "closeModal",
      "click .save-alerts": "saveModal"
   },
   initialize: function () {
      _.bindAll(this, "render", "validationResult");
      this.template = _.template($("#no-alerts-template").html());
      this.dom = {
         $ALERTS_MODAL_CONTENT: $(".alerts-modal-content", this.$el),
         $ALERTS_NOTIFICATIONS: $(".alerts-notifications", this.$el)
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
         this.$el.modal("hide");
      }
   },
   openModal: function () {
      this.model.backupAlertsCollection();
      this.dom.$ALERTS_NOTIFICATIONS.hide();
   },
   validationResult: function (result) {
      if (result == this.model.errors.ERROR) {
         this.dom.$ALERTS_NOTIFICATIONS.html("Check the fields marked with red");
         this.dom.$ALERTS_NOTIFICATIONS.show();
      }
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
      QuestionType: ""
   },
   initialize: function () {
      _.bindAll(this, "updateAlertOperators");
      this.alertsCollection = new SurveyModals.AlertsCollection();
      _.each(this.get("QuestionAlertSet"), function (alert) {
         alert = alert || {};
         alert.AlertOperatorsValues = this.getAlertOperators(this.get("QuestionType"));
         alert.AlertOperatorsLabels = this.getAlertOperatorsLabels(this.get("QuestionType"));
         var alertModel = new SurveyModals.AlertModel(alert);
         this.alertsCollection.add(alertModel);
      }, this);
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
         }
      }));
   },
   updateAlertOperators: function (type) {
      this.set("QuestionType", type);
      _.each(this.alertsCollection.models, function (alert) {
         alert.set("AlertOperatorsValues", this.getAlertOperators(type));
         alert.set("AlertOperatorsLabels",this.getAlertOperatorsLabels(type));
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
   }
});

SurveyModals.AlertView = Backbone.View.extend({
   events: {
      "click .delete-alert-btn": "deleteAlert",
      "keyup .alert-trigger-answer-input": "updateTriggerAnswer",
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
         $ALERT_OPERATOR_SELECT: $(".alert-operator-select", this.$el)
      }
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
      this.dom.$ALERT_TRIGGER_ANSWER_INPUT.removeClass(invalidFieldClass);
      this.dom.$ALERT_DISTRIBUTION_LIST_INPUT.removeClass(invalidFieldClass);
      this.dom.$ALERT_OPERATOR_SELECT.removeClass(invalidFieldClass);
      if (result != "valid") {
         for (var i = 0; i < result.length; ++i) {
            if (result[i] == this.model.errors.INVALID_DESCRIPTION) {
               this.dom.$ALERT_DESCRIPTION_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_TRIGGER_ANSWER) {
               this.dom.$ALERT_TRIGGER_ANSWER_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_DISTRIBUTION_LIST) {
               this.dom.$ALERT_DISTRIBUTION_LIST_INPUT.addClass(invalidFieldClass);
            } else if (result[i] == this.model.errors.INVALID_OPERATOR) {
               this.dom.$ALERT_OPERATOR_SELECT.addClass(invalidFieldClass);
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