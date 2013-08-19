SurveyModals.RatingsModalView = Backbone.View.extend({
   events: {
      "change #rating-select": "changeScaleSize",
      "click .close-rating-modal-btn": "closeModal",
      "click .save-rating": "saveModal"
   },
   initialize: function () {
      _.bindAll(this, "changeScaleSize", "render", "closeModal");
      this.template = _.template($("#rating-modal-content-template").html());
      this.dom = {
         $RATING_MODAL_BODY: $(".modal-body", this.$el)
      }
      this.model.on("change:ScaleSize", this.render);
   },
   render: function () {
      this.dom.$RATING_MODAL_BODY.html(this.template(this.model.toJSON()));
      this.dom = this.dom || {};
      this.dom.$RATING_TABLE = $(".rating-table", this.$el);
      _.each(this.model.getRatings(), function (rating) {
         var ratingView = new SurveyModals.RatingView({ model: rating });
         this.dom.$RATING_TABLE.append(ratingView.render());
      }, this);
   },
   changeScaleSize: function (event) {
      this.model.changeScaleSize(event.currentTarget.value);
   },
   closeModal: function () {
      this.model.emptyRatingsCollection();
   },
   saveModal: function () {
      var areRatingsValid = this.model.validate();
      if (areRatingsValid) {
         this.$el.modal("hide");
      }
   }
});

SurveyModals.RatingsModalModel = Backbone.Model.extend({
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
   getRatingsAsString: function () {
      var ratingsAsString = "";
      if (this.surveyRatingsCollection.models.length > 1) {
         ratingsAsString += this.surveyRatingsCollection.models[0].get("RatingLabel");
         for (var i = 1; i < this.surveyRatingsCollection.models.length; ++i) {
            ratingsAsString += ";" + this.surveyRatingsCollection.models[i].get("RatingLabel");
         }
      }
      return ratingsAsString;
   },
   emptyRatingsCollection: function () {
      for (var i = this.surveyRatingsCollection.models.length - 1; i > -1; --i) {
         this.surveyRatingsCollection.remove(this.surveyRatingsCollection.models[i]);
      }
      this.set("ScaleSize", 0);
   },
   validate: function () {
      var isValid = true;
      _.each(this.surveyRatingsCollection.models, function (rating) {
         var ratingValidity = rating.validate();
         if (!ratingValidity) {
            isValid = ratingValidity;
         }
      });
      return isValid;
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