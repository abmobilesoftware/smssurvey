var SurveyElements = SurveyElements || {};
SurveyElements.Star = Backbone.Model.extend({});
var StarView = Backbone.View.extend({
   className: "star",
   events: {
      "click a": "click"
   },
   initialize: function () {
      _.bindAll(this, "click", "render");
      this.template = _.template($("#star-template").html());
      this.model.on("change:Active", this.render);
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      return this;
   },
   click: function (event) {
      event.preventDefault();
      this.model.trigger("starClickedEvent", this.model);
   }
});

SurveyElements.StarsCollection = Backbone.Collection.extend({
   model: SurveyElements.Star,
   initialize: function () {
      this.on("starClickedEvent", this.starClicked);
   },
   starClicked: function (model) {
      for (var i = 0; i <= model.get("id") ; ++i) {
         this.at(i).set("Active", true);
      }
      for (var i = model.get("id") + 1; i < this.models.length; ++i) {
         this.at(i).set("Active", false);
      }
      this.trigger("resultEvent", model.get("id"));
   }
});

SurveyElements.StarBarView = Backbone.View.extend({
   events: {
      "keyup .additionalInfo": "inputAdditionalInfo"
   },
   className: "starsBar",
   initialize: function () {
      _.bindAll(this, "starClicked", "inputAdditionalInfo");
      this.template = _.template($("#star-bar-template").html());
      var starsArray = [];
      this.options.noOfElements = this.options.noOfElements != undefined
          ? this.options.noOfElements : 5;
      for (var i = 0; i < this.options.noOfElements; ++i) {
         starsArray.push({ id: i, Active: false });
      }
      this.starsCollection = new SurveyElements.StarsCollection(starsArray);
      this.starsCollection.on("resultEvent", this.starClicked);
      this.result = -1;
      this.render();
   },
   render: function () {
      _.each(this.starsCollection.models, function (value, index, list) {
         var starView = new SurveyElements.StarView({ model: value });
         this.$el.append(starView.render().el);
      }, this);

      this.$el.append(this.template());
      this.dom = {
         $ADDITIONAL_INFO: $(".additionalInfo", this.$el),
         $ANSWER: $(".answer", this.$el)
      };
      if (this.dom.$ANSWER.val() == "noValue") {
         this.dom.$ADDITIONAL_INFO.hide();
      }
   },
   //TODO DA - don't hardcode the 2 value - it should be configurable (we should move this at db level as WhyThreshold)
   starClicked: function (value) {
      if (value < 2) {
         this.domElements.$ADDITIONAL_INFO.show();
         this.saveResult(value,
             this.domElements.$ADDITIONAL_INFO.val());
      } else {
         this.domElements.$ADDITIONAL_INFO.hide();
         this.saveResult(value, "");
      }
      this.result = value;
   },
   inputAdditionalInfo: function (event) {
      this.saveResult(this.result, event.target.value);
   },
   saveResult: function (pValue, pAdditionalInfo) {
      /* save the result in .answer input field value attribute */
      var result = {};
      result.additionalInfo = pAdditionalInfo;
      result.value = pValue;
      this.domElements.$ANSWER.val(JSON.stringify(result));
   }
});
