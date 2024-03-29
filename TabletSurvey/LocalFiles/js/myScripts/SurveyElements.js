﻿var SurveyElements = SurveyElements || {};
SurveyElements.Star = Backbone.Model.extend({});
SurveyElements.StarView = Backbone.View.extend({
   className: "star",
   /*events: {
      "click .starImg": "click"
   },*/
   initialize: function () {
      _.bindAll(this, "click", "render","activeStatusChanged");
      this.template = _.template($("#star-template").html());
      this.model.on("change:Active", this.activeStatusChanged);      
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      var starButton = new google.ui.FastButton($(".starImg", this.$el)[0], this.click)
      return this;
   },
   activeStatusChanged: function()
   {
      var activeStatus = this.model.get("Active");
      if (activeStatus) {
         $(".starImg", this.$el).addClass("starActive");
      } else {
         $(".starImg", this.$el).removeClass("starActive");
      }
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
      this.trigger("resultEvent", model.get("id") + 1);
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
      this.$el.append(this.template());
      _.each(this.starsCollection.models, function (value, index, list) {
         var starView = new SurveyElements.StarView({ model: value });
         $(".starsArea",this.$el).append(starView.render().el);         
      }, this);
      $(".starsArea", this.$el).append("<div class='clear'></div>");

      
      this.dom = {
         $ADDITIONAL_INFO: $(".additionalInfo", this.$el),
         $ANSWER: $(".answer", this.$el)
      };
      if (this.dom.$ANSWER.val() == Question.noValueAnswer) {
         this.dom.$ADDITIONAL_INFO.hide();
      }
   },
   //TODO DA - don't hardcode the 2 value - it should be configurable (we should move this at db level as WhyThreshold)
   starClicked: function (value) {
      if (value <= 2) {
         this.dom.$ADDITIONAL_INFO.show();
         this.saveResult(value,
             this.dom.$ADDITIONAL_INFO.val());
      } else {
         this.dom.$ADDITIONAL_INFO.hide();
         this.saveResult(value, "");
      }
      this.result = value;
   },
   inputAdditionalInfo: function (event) {
      this.saveResult(this.result, event.target.value);
   },
   saveResult: function (pValue, pAdditionalInfo) {
      /* save the result in .answer input field value attribute */      
      this.dom.$ANSWER.val(pValue);      
      this.dom.$ADDITIONAL_INFO.val(pAdditionalInfo);
   }
});
