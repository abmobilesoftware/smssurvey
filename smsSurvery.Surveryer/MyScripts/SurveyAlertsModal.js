SurveyModals.AlertsModalView = Backbone.View.extend({
   initialize: function () {
      _.bindAll(this, "render");
      this.alertsCollection = new SurveyModals.AlertsCollection();
      _.each(this.model.get("QuestionAlertSet"), function (alert) {
         var alertModel = new SurveyModals.AlertModel(alert);
         this.alertsCollection.add(alertModel);
      }, this);
      this.dom = {
         $ALERTS_MODAL_CONTENT: $(".alerts-modal-content", this.$el)
      }
      this.render();
   },
   render: function () {
      this.dom.$ALERTS_MODAL_CONTENT.empty();
      _.each(this.alertsCollection.models, function (alert) {
         var alertView = new SurveyModals.AlertView({ model: alert });
         this.dom.$ALERTS_MODAL_CONTENT.append(alertView.render());
      }, this);
   },
   initializeAlertOperators: function (type) {
      _.each(this.alertsCollection.models, function (alert) {
         alert.initializeAlertOperators(type);
      }, this);
   }
});

SurveyModals.AlertsModalModel = Backbone.Model.extend({
   defaults: {
      QuestionAlertSet: []
   }
});

SurveyModals.AlertView = Backbone.View.extend({
   initialize: function () {
      _.bindAll(this, "render");
      this.template = _.template($("#alert-template").html());
      this.model.on(this.model.events.UPDATE_VIEW,
         this.render);
   },
   render: function () {
      this.$el.html(this.template(this.model));
      return this.$el;
   }
});

SurveyModals.AlertModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent"
   },
   defaults: {
      Id: "",
      Description: "",
      TriggerAnswer: "",
      AlertOperators: []
   },
   initializeAlertOperators: function (type) {
      var questionConstants = SurveyUtilities.Utilities.CONSTANTS_QUESTION;
      if (type == questionConstants.TYPE_SELECT_ONE_FROM_MANY) {
         var alertOperators = new Array("==", "!=");
         this.set("AlertOperators", alertOperators);
      } else if (type == questionConstants.TYPE_RATING) {
         var alertOperators = new Array("==", "!=", "<", "<=", ">", ">=");
         this.set("AlertOperators", alertOperators);
      } else if (type == questionConstants.TYPE_FREE_TEXT) {
         var alertOperators = new Array("CONTAINS");
         this.set("AlertOperators", alertOperators);
      } else if (type == questionConstants.TYPE_YES_NO) {
         var alertOperators = new Array("==");
         this.set("AlertOperators", alertOperators);
      }
      this.trigger(this.events.UPDATE_VIEW);
   }
});

SurveyModals.AlertsCollection = Backbone.Collection.extend({
   model: SurveyModals.AlertModel
});