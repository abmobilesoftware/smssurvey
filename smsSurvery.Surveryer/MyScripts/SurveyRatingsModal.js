SurveyModals.RatingsModalView = Backbone.View.extend({
   events: {
      "change #rating-select": "changeScaleSize"
   },
   initialize: function () {
      _.bindAll(this, "changeScaleSize", "render");
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
   }
});

SurveyModals.RatingsModalModel = Backbone.Model.extend({
   defaults: {
      Ratings: ""
   },
   initialize: function () {
      this.initializeRatingScale(this.get("ScaleSize"));
   },
   changeScaleSize: function (size) {
      this.initializeRatingScale(size);
      this.set("ScaleSize", size);
   },
   initializeRatingScale: function (scaleSize) {
      var ratingsSplitted = this.get("Ratings").split(";");
      var ratingsCollection = [];
      for (var i = 0; i < ratingsSplitted.length; ++i) {
         ratingsCollection.push(new SurveyModals.RatingModel({ RatingIdentifier: i, RatingLabel: ratingsSplitted[i]}));
      }
      this.surveyRatingsCollection = new SurveyModals.RatingsCollection(ratingsCollection);
   },
   getRatings: function () {
      return this.surveyRatingsCollection.models;
   }
});

SurveyModals.RatingModel = Backbone.Model.extend({
   defaults: {
      RatingIdentifier: "",
      RatingLabel: ""
   }
});

SurveyModals.RatingView = Backbone.View.extend({
   events: {
      "keyup .rating-label-input": "updateRatingLabel"
   },
   initialize: function () {
      this.template = _.template($("#rating-template").html());
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      return this.$el;
   },
   updateRatingLabel: function (event) {
      this.model.set("RatingLabel", event.currentTarget.value);
   }
})

SurveyModals.RatingsCollection = Backbone.Collection.extend({
   model: SurveyModals.RatingModel
});