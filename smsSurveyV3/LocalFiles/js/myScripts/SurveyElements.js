var SurveyElements = SurveyElements || {};
SurveyElements.Star = Backbone.Model.extend({});
SurveyElements.StarView = Backbone.View.extend({
   className: "star",
   initialize: function () {
      _.bindAll(this, "click", "render","activeStatusChanged", "close");
      this.template = _.template($("#star-template").html());
      this.listenTo(this.model,"change:Active", this.activeStatusChanged);
      this.starButton = null;
   },
   render: function () {
      this.$el.html(this.template(this.model.toJSON()));
      this.starButton = new google.ui.FastButton($(".starImg", this.$el)[0], this.click);
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
      Backbone.trigger("touch");
   },
   close: function() {	
	   //to make sure we clean up all the handlers for the FastButton call reset
	   this.starButton.reset();
	   //all events declared in events {} and all events to which we binded with listenTo will be removed
	   this.remove();
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
      this.listenTo(this.starsCollection, "resultEvent", this.starClicked);
      this.result = -1;
      this.starViews = [];
      this.render();      
   },
   render: function () {
      this.$el.append(this.template());
      this.starViews.length = 0;
      //to avoid DOM reflows we append only the full starBar
      //http://ozkatz.github.io/avoiding-common-backbonejs-pitfalls.html?tagref=js
      var starsContainer = document.createDocumentFragment();
      _.each(this.starsCollection.models, function (value, index, list) {
         var starView = new SurveyElements.StarView({ model: value });
         starsContainer.appendChild(starView.render().el);
         this.starViews.push(starView);
      }, this);
      //reason for the following line: http://stackoverflow.com/questions/9284117/inserting-arbitrary-html-into-a-documentfragment
      var separator = document.createElement('tmp');
      document.body.appendChild(separator);
      separator.innerHTML = "<div class='clear'></div>";      
      starsContainer.appendChild(separator.firstChild); //no loop required as we only have 1 child
      document.body.removeChild(separator);
      $(".starsArea",this.$el).append(starsContainer);      
      
      this.dom = {
         $ADDITIONAL_INFO: $(".comment", this.$el),
         $ANSWER: $(".answer", this.$el)
      };
      if (this.dom.$ANSWER.val() == Question.noValueAnswer) {
         this.dom.$ADDITIONAL_INFO.hide();
      }
      return this;
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
   },
   close: function() {
	   //dispose of all the child views
	   _.each(this.starViews, function(starView) {
		   starView.close();		   
	   });
	   this.starViews.length = 0;
	 //all events declared in events {} and all events to which we binded with listenTo will be removed
	   this.remove();
   }
});

SurveyElements.Loader = Backbone.View.extend({
	initialize: function() {
		$('#loading-modal').modal({
			  backdrop: 'static',
			  keyboard: false
		});
		$('#loading-modal').modal("hide");
		var loadingAnimation = new imageLoader(cImageSrc, 'startAnimation()');
	},
	showLoader: function() {
		$('#loading-modal').modal("show");
		$("#loading-modal").css("visibility", "visible");
	},
	hideLoader: function() {
		$('#loading-modal').modal("hide");
	}
});
