SurveyModals.RatingsModalView = Backbone.View.extend({
   keys: {
      ENTER: 13,
      ESC: 27
   },
   events: {
      "change #rating-select": "changeScaleSize",
      "click .close-rating-modal-btn": "discardModalData",
      "click .save-rating": "saveModal",
      "click .close-ratings-notifications": "closeAlertBox"
   },
   initialize: function () {
      _.bindAll(this, "changeScaleSize", "render", "discardModalData",
         "validationResult", "closeAlertBox", "processKeyStroke");
      this.template = _.template($("#rating-modal-content-template").html());
      this.dom = {
         $RATING_MODAL_BODY: $(".modal-body", this.$el)
      }
      this.model.on("change:ScaleSize", this.render);
      this.model.on(this.model.events.VALIDATE, this.validationResult);
   },
   render: function () {
      this.dom.$RATING_MODAL_BODY.html(this.template(this.model.toJSON()));
      this.dom = this.dom || {};
      this.dom.$RATING_TABLE = $(".rating-table", this.$el);
      this.dom.$RATINGS_NOTIFICATIONS = $(".ratings-notifications", this.$el);
      this.dom.$RATINGS_SELECT = $("#rating-select", this.$el);
      this.dom.$ALERT_BOX = $(".alert", this.$el);
      _.each(this.model.getRatings(), function (rating) {
         var ratingView = new SurveyModals.RatingView({ model: rating });
         this.dom.$RATING_TABLE.append(ratingView.render());
      }, this);
   },
   changeScaleSize: function (event) {
      this.model.changeScaleSize(event.currentTarget.value);
   },
   closeModal: function () {
      this.$el.modal("hide");
      document.removeEventListener("keydown", this.processKeyStroke, false);
   },
   discardModalData: function() {
      this.model.restoreRatingsCollection();
      this.closeModal();
   },
   saveModal: function () {
      var areRatingsValid = this.model.validate();
      if (areRatingsValid) {
         this.closeModal();
      }
   },
   openModal: function () {
      this.model.backupRatingsCollection();
      document.addEventListener("keydown", this.processKeyStroke, false);
   },
   validationResult: function(result) {
      if (result == "noRatingsDefined") {
         this.dom.$RATINGS_NOTIFICATIONS.html("No ratings defined. Choose a scale using the selector below.");
         this.dom.$ALERT_BOX.show();
         this.dom.$RATINGS_SELECT.addClass("invalidField");
      } else if (result == "otherErrors") {
         this.dom.$RATINGS_NOTIFICATIONS.html("Check the fields marked with red");
         this.dom.$ALERT_BOX.show();
         this.dom.$RATINGS_SELECT.removeClass("invalidField");
      }
   },
   closeAlertBox: function () {
      this.dom.$ALERT_BOX.hide();
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

SurveyModals.RatingsModalModel = Backbone.Model.extend({
   events: {
      VALIDATE: "validateEvent"
   },
   defaults: {
      Ratings: "",
      ScaleSize: "",
   },
   initialize: function () {
      this.initializeRatingScale(this.get("ScaleSize"));
   },
   changeScaleSize: function (size) {
      this.set("Ratings", "");
      this.initializeRatingScale(size);      
      this.set("ScaleSize", size);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   initializeRatingScale: function (scaleSize) {
      var ratingsSplitted = this.get("Ratings").split(";");
      var ratingsCollection = [];
      for (var i = 0; i < scaleSize; ++i) {
         ratingsCollection.push(new SurveyModals.RatingModel({
            RatingIdentifier: i+1,
            RatingLabel: i < ratingsSplitted.length ? ratingsSplitted[i] : ""
         }));
      }
      this.surveyRatingsCollection = new SurveyModals.RatingsCollection(ratingsCollection);
   },
   getRatings: function () {
      return this.surveyRatingsCollection.models;
   },
   getRatingsAsJson: function () {
      var ratingsLabels = "";
      var ratingsIdentifiers = "";
      if (this.surveyRatingsCollection.models.length > 1) {
         ratingsLabels += this.surveyRatingsCollection.models[0].get("RatingLabel");
         ratingsIdentifiers += 1;
         for (var i = 1; i < this.surveyRatingsCollection.models.length; ++i) {
            ratingsLabels += ";" + this.surveyRatingsCollection.models[i].get("RatingLabel");
            ratingsIdentifiers += ";" + (i + 1);
         }
      }
      return {
         ValidAnswersDetails: ratingsLabels,
         ValidAnswers: ratingsIdentifiers
      };
   },
   emptyRatingsCollection: function () {
      for (var i = this.surveyRatingsCollection.models.length - 1; i > -1; --i) {
         this.surveyRatingsCollection.remove(this.surveyRatingsCollection.models[i]);
      }      
   },
   validate: function () {
      var isValid = true;
      if (this.surveyRatingsCollection.models.length > 0) {
         _.each(this.surveyRatingsCollection.models, function (rating) {
            var ratingValidity = rating.validate();
            if (!ratingValidity) {
               isValid = ratingValidity;
            }
         });
         if (!isValid) this.trigger(this.events.VALIDATE, "otherErrors");
      } else {
         isValid = false;
         this.trigger(this.events.VALIDATE, "noRatingsDefined");
      };
      return isValid;
   },
   backupRatingsCollection: function () {
      this.ratingsCollectionBackup = new SurveyModals.RatingsCollection();
      _.each(this.surveyRatingsCollection.models, function (rating) {
         this.ratingsCollectionBackup.add(new SurveyModals.RatingModel(rating.toJSON()));
      }, this);
      this.scaleSizeBackup = this.get("ScaleSize");
   },
   restoreRatingsCollection: function () {
      this.emptyRatingsCollection();
      _.each(this.ratingsCollectionBackup.models, function (rating) {
         this.surveyRatingsCollection.add(new SurveyModals.RatingModel(rating.toJSON()));
      }, this);
      this.set("ScaleSize", this.scaleSizeBackup);
   }
});

SurveyModals.RatingModel = Backbone.Model.extend({
   events: {
      VALIDATE: "validateEvent"
   },
   errors: {
      INVALID_RATING_LABEL: "invalid rating label",
      VALID: "valid"
   },
   defaults: {
      RatingIdentifier: "",
      RatingLabel: ""
   },
   validate: function () {
      if (this.get("RatingLabel").length == 0) {
         this.trigger(this.events.VALIDATE, this.errors.INVALID_RATING_LABEL);
         return false;
      }
      this.trigger(this.events.VALIDATE, this.errors.VALID);
      return true;
   }

});

SurveyModals.RatingView = Backbone.View.extend({
   tagName: "tr",
   events: {
      "keyup .rating-label-input": "updateRatingLabel"
   },
   initialize: function () {
      _.bindAll(this, "validationResult");
      this.template = _.template($("#rating-template").html());
      this.model.on(this.model.events.VALIDATE, this.validationResult);
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.dom = {
         $RATING_LABEL_INPUT: $(".rating-label-input", this.$el)
      };
      return this.$el;
   },
   updateRatingLabel: function (event) {
      this.model.set("RatingLabel", event.currentTarget.value);
      var attributeChangedEvent = SurveyUtilities.Utilities.GLOBAL_EVENTS.ATTRIBUTE_CHANGED;
      Backbone.trigger(attributeChangedEvent);
   },
   validationResult: function (result) {
      var invalidFieldClass = SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD;
      if (result == this.model.errors.INVALID_RATING_LABEL) {
         this.dom.$RATING_LABEL_INPUT.addClass(invalidFieldClass);
      } else if (result == this.model.errors.VALID) {
         this.dom.$RATING_LABEL_INPUT.removeClass(invalidFieldClass);
      }
   }
})

SurveyModals.RatingsCollection = Backbone.Collection.extend({
   model: SurveyModals.RatingModel
});