﻿var SurveyModals = SurveyModals || {};
SurveyModals.NumericModalView = Backbone.View.extend({
   keys: {
      ENTER: 13,
      ESC: 27
   },
   events: {
      "click .close-numeric-modal-btn": "discardModalData",
      "click .save-numeric": "saveModal",
      "click .close-numeric-notifications": "closeAlertBox",
      "change .numeric-select": "changeScale"
   },
   initialize: function () {
      _.bindAll(this, "discardModalData", "saveModal", "closeAlertBox",
         "validationResult", "render", "processKeyStroke");
      this.dom = {
         $MODAL_CONTENT : $(".modal-content", this.$el),
         $NUMERIC_NOTIFICATIONS: $(".numeric-notifications", this.$el),
         $ALERT_BOX: $(".alert", this.$el)
      };
      this.template = _.template($("#numeric-modal-template").html());
      this.model.on(this.model.events.VALIDATE, this.validationResult);
      this.model.on(this.model.events.UPDATE_VIEW, this.render)
      this.render();
   },
   render: function () {
      this.dom.$MODAL_CONTENT.html(this.template(this.model.toJSON()));
      this.dom.$NUMERIC_TABLE= $(".numeric-table", this.$el);
      this.dom.$NUMERIC_TABLE.empty();
      _.each(this.model.getNumericScale(), function (numericEntry) {
         var numericEntryView = new SurveyModals.NumericEntryView({ model: numericEntry });
         this.dom.$NUMERIC_TABLE.append(numericEntryView.render());
      }, this);
   },
   saveModal: function () {
      if (this.model.validate()) {
         this.model.saveNumericScale();
         this.closeModal();
      }
   },
   closeAlertBox: function () {
      this.dom.$ALERT_BOX.hide();
   },
   openModal: function () {
      this.model.backupNumericScaleCollection();
      this.closeAlertBox();
      document.addEventListener("keydown",
         this.processKeyStroke, false);
   },
   closeModal: function () {
      this.$el.modal("hide");
      document.removeEventListener("keydown",
         this.processKeyStroke, false);
   },
   discardModalData: function() {
      this.model.restoreNumericScaleCollection();
      this.closeModal();
   },
   validationResult: function (result) {
      if (result == "otherErrors") {
         this.dom.$NUMERIC_NOTIFICATIONS.html("Check the fields marked with red");
         this.dom.$ALERT_BOX.show();
      } else if (result == "invalidScale") {
         this.dom.$NUMERIC_NOTIFICATIONS.html("Invalid scale. Choose a scale from the list.");
         this.dom.$ALERT_BOX.show();
      }
   },
   changeScale: function (event) {
      this.model.changeScale(event.currentTarget.value)
   },
   processKeyStroke: function (event) {
      if (event.keyCode == this.keys.ENTER) {
         event.preventDefault();
         this.saveModal();
      } else if (event.keyCode == this.keys.ESC) {
         event.preventDefault();
         this.discardModalData();
      }
   }
});

var isBlank = function (str) {
   return (!str || /^\s*$/.test(str));
}
SurveyModals.NumericModalModel = Backbone.Model.extend({
   events: {
      UPDATE_VIEW: "updateViewEvent"
   },
   initialize: function () {
      this.numericScaleCollection = new SurveyModals.NumericScaleCollection();
      if (this.get("ValidAnswers") != "") {
         var numericLabels = this.get("ValidAnswersDetails").split(";");
         var numericValues = this.get("ValidAnswers").split(";");
         if (numericLabels.length == numericValues.length) {
            for (var i = 0; i < numericLabels.length; ++i) {
               var numericEntryModel = new SurveyModals.NumericEntryModel(
                  {
                     NumericLabel: numericLabels[i],
                     NumericValue: numericValues[i]
                  });
               this.numericScaleCollection.add(numericEntryModel);
            }
         };
      }
      this.numericScaleCollection.on("add remove", function () {
         this.trigger(this.events.UPDATE_VIEW);
         var attributeChangedEvent =
            SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
         Backbone.trigger(attributeChangedEvent);
      }, this);
   },
   getNumericScale: function () {
      return this.numericScaleCollection.models;
   },
   getNumericScaleAsJson: function () {
      var numericScale = this.numericScaleCollection.models;
      var numericLabelsAsString = "";
      var numericValueAsString = "";
      if (numericScale.length > 0) {
         numericLabelsAsString = numericScale[0].get("NumericLabel");
         numericValueAsString = numericScale[0].get("NumericValue");
      }
      for (var i = 1; i < numericScale.length; ++i) {
         numericLabelsAsString += ";" + numericScale[i].get("NumericLabel");
         numericValueAsString += ";" + numericScale[i].get("NumericValue");
      }
      return {
         "ValidAnswers": numericValueAsString,
         "ValidAnswersDetails": numericLabelsAsString
      }
   },
   emptyNumericScaleCollection: function () {
      for (var i = this.numericScaleCollection.models.length - 1; i > -1; --i) {
         this.numericScaleCollection.remove(this.numericScaleCollection.models[i]);
      }
   },
   backupNumericScaleCollection: function () {
      this.numericScaleCollectionBackup = new SurveyModals.NumericScaleCollection();
      _.each(this.numericScaleCollection.models, function (numericEntry) {
         this.numericScaleCollectionBackup.add(
            new SurveyModals.NumericEntryModel(numericEntry.toJSON()));
      }, this);
   },
   restoreNumericScaleCollection: function () {
      this.emptyNumericScaleCollection();
      _.each(this.numericScaleCollectionBackup.models,
         function (numericEntry) {
            this.numericScaleCollection.add(
               new SurveyModals.NumericEntryModel(numericEntry.toJSON()));
         }, this);
      this.saveNumericScale();

   },
   saveNumericScale: function() {
      var numericScale = this.getNumericScaleAsJson();
      this.set("ValidAnswers", numericScale.ValidAnswers);
      this.set("ValidAnswersDetails", numericScale.ValidAnswersDetails);
   },
   validate: function () {
      var isValid = true;
      if (this.numericScaleCollection.models.length > 0) {
         _.each(this.numericScaleCollection.models, function (numericEntry) {
            var numericEntryValidity = numericEntry.validate();
            if (!numericEntryValidity) {
               isValid = numericEntryValidity;
            }
         });
         if (!isValid) this.trigger(this.events.VALIDATE, "otherErrors");
      } else {
         isValid = false;
         this.trigger(this.events.VALIDATE, "invalidScale");
      }
      return isValid;
   },
   changeScale: function (size) {
      this.emptyNumericScaleCollection();
      for (var i = 0; i < size; ++i) {
         this.numericScaleCollection.add(
               new SurveyModals.NumericEntryModel({ NumericValue: (i + 1) }));
      };
      this.saveNumericScale();
      this.trigger(this.events.UPDATE_VIEW);
   }
});

SurveyModals.NumericEntryView = Backbone.View.extend({
   events: {
      "keyup .numeric-value-input": "updateNumericValue",
      "keyup .numeric-label-input": "updateNumericLabel"
   },
   tagName: "tr",
   initialize: function () {
      _.bindAll(this, "updateNumericValue",
         "updateNumericLabel", "validationResult");
      this.template = _.template($("#numeric-entry-template").html());
      this.model.on(this.model.events.VALIDATE, this.validationResult)
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.dom = {
         $NUMERIC_VALUE_INPUT: $(".numeric-value-input", this.$el),
         $NUMERIC_LABEL_INPUT: $(".numeric-label-input", this.$el)
      }
      return this.$el;
   },
   updateNumericValue: function (event) {
      event.preventDefault();
      this.model.updateNumericValue(event.currentTarget.value);
   },
   updateNumericLabel: function (event) {
      event.preventDefault();
      this.model.updateNumericLabel(event.currentTarget.value);
   },
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      if (result == this.model.errors.INVALID_NUMERIC_VALUE) {
         this.dom.$NUMERIC_VALUE_INPUT.addClass(invalidFieldClass);
      } else if (result == this.model.errors.VALID_NUMERIC_VALUE) {
         this.dom.$NUMERIC_VALUE_INPUT.removeClass(invalidFieldClass);
      } else if (result == this.model.errors.INVALID_NUMERIC_LABEL) {
         this.dom.$NUMERIC_LABEL_INPUT.addClass(invalidFieldClass);
      } else if (result == this.model.errors.VALID_NUMERIC_LABEL) {
         this.dom.$NUMERIC_LABEL_INPUT.removeClass(invalidFieldClass);
      }
   }
});

SurveyModals.NumericEntryModel = Backbone.Model.extend({
   errors: {
      INVALID_NUMERIC_VALUE: "invalidNumericValue",
      INVALID_NUMERIC_LABEL: "invalidNumericLabel",
      VALID_NUMERIC_LABEL: "validNumericLabel",
      VALID_NUMERIC_VALUE: "validNumericValue"
   },
   events: {
      VALIDATE: "validateEvent"
   },
   defaults: {
      NumericValue: "",
      NumericLabel: ""
   },
   updateNumericValue: function (newNumericValue) {
      this.set("NumericValue", newNumericValue);
      var attributeChangedEvent =
         SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   updateNumericLabel: function (newNumericLabel) {
      this.set("NumericLabel", newNumericLabel);
      var attributeChangedEvent =
         SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   validate: function () {
      if (isBlank(this.get("NumericLabel"))) {
         this.trigger(this.events.VALIDATE, this.errors.INVALID_NUMERIC_LABEL);
         return false;
      }
      return true;
   }
});

SurveyModals.NumericScaleCollection = Backbone.Collection.extend({
   model: SurveyModals.NumericEntryModel
});