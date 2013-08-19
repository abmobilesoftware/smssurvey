SurveyModals.AlertsModalView = Backbone.View.extend({
   events: {
      "click .add-alert-btn": "addAlert",
      "click .close-alerts-modal-btn": "closeModal",
      "click .save-alerts": "saveModal"
   },
   initialize: function () {
      _.bindAll(this, "render");
      this.template = _.template($("#no-alerts-template").html());
      this.dom = {
         $ALERTS_MODAL_CONTENT: $(".alerts-modal-content", this.$el)
      };
      this.model.on(this.model.events.UPDATE_VIEW, this.render);
      this.model.on(this.model.events.VALIDATE, function () {
         alert("Now close");
      });
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
      this.model.emptyAlertsCollection();
   },
   saveModal: function (event) {
      this.model.validateAlerts();
   }
});

SurveyModals.AlertsModalModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent",
      VALIDATE: "validateEvent"
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
         alert.AlertOperators = this.getAlertOperators(this.get("QuestionType"));
         var alertModel = new SurveyModals.AlertModel(alert);
         this.alertsCollection.add(alertModel);
      }, this);
      this.alertsCollection.on("add remove", function () {
         this.trigger(this.events.UPDATE_VIEW);
      }, this);
      this.alertClientId = -1300;
   },
   getAlertOperators: function (type) {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      if (type == questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         var alertOperators = new Array("==", "!=");
         return alertOperators;
      } else if (type == questionConstants.TYPE_RATING) {
         var alertOperators = new Array("==", "!=", "<", "<=", ">", ">=");
         return alertOperators;
      } else if (type == questionConstants.TYPE_FREE_TEXT) {
         var alertOperators = new Array("CONTAINS");
         return alertOperators;
      } else if (type == questionConstants.TYPE_YES_NO) {
         var alertOperators = new Array("==");
         return alertOperators;
      } else if (type == questionConstants.TYPE_SELECT_MANY_FROM_MANY) {
         var alertOperators = new Array("ANY", "ALL");
         return alertOperators;
      }
   },
   addAlert: function () {
      ++this.alertClientId;
      this.alertsCollection.add(new SurveyModals.AlertModel({
         Id: this.alertClientId,
         Description: "",
         TriggerAnswer: "",
         Operator: "",
         AlertOperators: this.getAlertOperators(this.get("QuestionType")),
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
         alert.set("AlertOperators", this.getAlertOperators(type));
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
         isValid = alert.validate();
      });
      //this.trigger(this.events.VALIDATE, isValid);
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
      _.bindAll(this, "render", "deleteAlert", "validateResult");
      this.template = _.template($("#alert-template").html());
      this.model.on(this.model.events.UPDATE_VIEW,
         this.render);
      this.model.on(this.model.events.VALIDATE, this.validateResult)
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.dom = {
         $ALERT_DESCRIPTION_INPUT: $(".alert-description-input", this.$el),
         $ALERT_TRIGGER_ANSWER_INPUT: $(".alert-trigger-answer-input", this.$el)
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
   validateResult: function (validateResult) {
      if (validateResult != "valid") {
         for (var i = 0; i < validateResult.length; ++i) {
            if (validateResult[i] == this.model.errors.INVALID_DESCRIPTION) {
               this.dom.$ALERT_DESCRIPTION_INPUT.addClass("invalidField");
            } else if (validateResult[i] == this.model.errors.INVALID_TRIGGER_ANSWER) {
               this.dom.$ALERT_TRIGGER_ANSWER_INPUT.addClass("invalidField");
            }
         }
      }
   }
});

SurveyModals.AlertModel = Backbone.Model.extend({
   errors: {
      INVALID_DESCRIPTION: "invalid description",
      INVALID_TRIGGER_ANSWER: "invalid trigger answer",
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
      AlertOperators: [],
      AlertNotification: {}
   },
   updateTriggerAnswer: function (newTriggerAnswer) {
      this.set("TriggerAnswer", newTriggerAnswer);
   },
   updateDistributionList: function (newDistributionList) {
      var alertNotification = this.get("AlertNotification");
      alertNotification.DistributionList = newDistributionList;
      this.set("AlertNotification", alertNotification);
   },
   updateDescription: function (newDescription) {
      this.set("Description", newDescription);
   },
   updateOperator: function (newOperator) {
      this.set("Operator", newOperator);
   },
   validate: function () {
      var errors = [];
      var hasErrors = false;
      if (this.get("TriggerAnswer").length == 0) {
         hasErrors = true;
         errors.push(this.errors.INVALID_TRIGGER_ANSWER);
      }
      if (this.get("Description").length == 0) {
         hasErrors = true;
         errors.push(this.errors.INVALID_DESCRIPTION);
      }
      var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
      //if 
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