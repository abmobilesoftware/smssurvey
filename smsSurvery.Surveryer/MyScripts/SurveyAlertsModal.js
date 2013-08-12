SurveyModals.AlertsModalView = Backbone.View.extend({
   events: {
      "click .add-alert-btn": "addAlert"
   },
   initialize: function () {
      _.bindAll(this, "render");
      this.template = _.template($("#no-alerts-template").html());
      this.dom = {
         $ALERTS_MODAL_CONTENT: $(".alerts-modal-content", this.$el)
      };
      this.model.on(this.model.events.UPDATE_VIEW, this.render);      
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
   }
});

SurveyModals.AlertsModalModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent"
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
      this.alertsCollection.on("add remove", function() {
         this.trigger(this.events.UPDATE_VIEW);
      }, this);
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
      }
   },
   addAlert: function () {
      this.alertsCollection.add(new SurveyModals.AlertModel({
         Id: "",
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
   updateAlertOperators: function (model) {
      var type = model.get("Type");
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
      _.bindAll(this, "render", "deleteAlert");
      this.template = _.template($("#alert-template").html());
      this.model.on(this.model.events.UPDATE_VIEW,
         this.render);
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
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
   }
});

SurveyModals.AlertModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent",
      DELETE_ALERT: "deleteAlertEvent"
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