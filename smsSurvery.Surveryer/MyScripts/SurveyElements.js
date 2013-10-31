var SurveyElements = SurveyElements || {};
SurveyElements.Star = Backbone.Model.extend({});
SurveyElements.StarView = Backbone.View.extend({
   className: "star",  
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
      "keyup .comment": "inputAdditionalInfo"
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

      
      this.dom = {
         $ADDITIONAL_INFO: $(".comment", this.$el),
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

//#region Loader
var cSpeed = 9;
var cWidth = 150;
var cHeight = 75;
var cTotalFrames = 7;
var cFrameWidth = 150;
var cImageSrc = '/Images/sprites.gif';

var cImageTimeout = false;
var cIndex = 0;
var cXpos = 0;
var cPreloaderTimeout = false;
var SECONDS_BETWEEN_FRAMES = 0;

function startAnimation() {

   document.getElementById('loaderImage').style.backgroundImage = 'url(' + cImageSrc + ')';
   document.getElementById('loaderImage').style.width = cWidth + 'px';
   document.getElementById('loaderImage').style.height = cHeight + 'px';

   //FPS = Math.round(100/(maxSpeed+2-speed));
   FPS = Math.round(100 / cSpeed);
   SECONDS_BETWEEN_FRAMES = 1 / FPS;

   cPreloaderTimeout = setTimeout('continueAnimation()', SECONDS_BETWEEN_FRAMES / 1000);

}

function continueAnimation() {

   cXpos += cFrameWidth;
   //increase the index so we know which frame of our animation we are currently on
   cIndex += 1;

   //if our cIndex is higher than our total number of frames, we're at the end and should restart
   if (cIndex >= cTotalFrames) {
      cXpos = 0;
      cIndex = 0;
   }

   if (document.getElementById('loaderImage'))
      document.getElementById('loaderImage').style.backgroundPosition = (-cXpos) + 'px 0';

   cPreloaderTimeout = setTimeout('continueAnimation()', SECONDS_BETWEEN_FRAMES * 1000);
}

function stopAnimation() {//stops animation
   clearTimeout(cPreloaderTimeout);
   cPreloaderTimeout = false;
}

function imageLoader(s, fun)//Pre-loads the sprites image
{
   clearTimeout(cImageTimeout);
   cImageTimeout = 0;
   genImage = new Image();
   genImage.onload = function () { cImageTimeout = setTimeout(fun, 0) };
   genImage.onerror = new Function('alert(\'Could not load the image\')');
   genImage.src = s;
}
//#endregion

SurveyElements.Loader = Backbone.View.extend({
   initialize: function () {
      $('#loading-modal').modal({
         backdrop: 'static',
         keyboard: false
      });
      $('#loading-modal').modal("hide");
      var loadingAnimation = new imageLoader(cImageSrc, 'startAnimation()');
   },
   showLoader: function () {
      $('#loading-modal').modal("show");
      $("#loading-modal").css("visibility", "visible");
   },
   hideLoader: function () {
      $('#loading-modal').modal("hide");
   }
});
