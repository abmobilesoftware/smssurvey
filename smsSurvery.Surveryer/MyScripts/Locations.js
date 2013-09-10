window.Location = window.Location || {};

window.LocationModel = Backbone.Model.extend({
   events: {},
   defaults: function(){
      return {
         Name: "",
         Description: ""
      }
   },
   idAttribute: "Name",
   validate: function (attributes, options) {

   }
});

window.LocationView = Backbone.View.extend({
   tagName: "li",
   className: "location",
   events: {
      "click .save-location-btn": "saveLocation"
   },
   initialize: function () {
      _.bindAll(this, "render", "saveLocation", "updateView");
      this.template = _.template($("#location-template").html());
      this.model.on( {
         "change:Name" : this.updateView,
         "change:Description" : this.updateView
      });
   },
   render: function() {
      this.$el.html(this.template(this.model.toJSON()));
      return this.$el;
   },
   saveLocation: function () {

   },
   updateView: function () {
      //now the only required update is enabling the Save button
      $(".save-location-btn", this).prop("disabled", false);
   }
});

window.LocationSetCollection = Backbone.Collection.extend({ 
   model: window.LocationModel,
   url: "/api/LocationTags",
   initialize: function () {     
   }  
});

window.LocationSetView = Backbone.View.extend({
   tag: "div",
   className:"locationsViewContent",
   events: {
      "click .add-location-btn": "addNewLocation"
   },
   model: window.LocationSetCollection,
   initialize: function () {
      _.bindAll(this, "render", "addNewLocation", "locationAdded");
      this.template = _.template($("#location-set-template").html());      
      this._locationViews = [];
      var self = this;
      this.collection.each(self.locationAdded);
      this.collection.on("add", self.locationAdded);
   },
   render: function () {
      var self = this;
      self.$el.html(self.template());
      // We keep track of the rendered state of the view
      self._rendered = true;

      // Render each sub-view and append it to the parent view's element.
      _(self._locationViews).each(function (locView) {
         $(".locations-set-content",self.el).append(locView.render());
      });
      return this.$el;
   },
   locationAdded: function (location) {
      var self = this;
      var locView = new LocationView({
         model: location
      });      
      //if the view was rendered append the new element
      this._locationViews.push(locView);
      if (self._rendered) {
         $(".locations-set-content",self.el).append(locView.render());
      }
   },
   addNewLocation: function () {
      var newLocation =new window.LocationModel();
      this.collection.add(newLocation);
   }  
});

$(document).ready(function () {
   var locations = new window.LocationSetCollection();
   locations.fetch();
   var locView = new window.LocationSetView({
      collection: locations     
   });
   $('#contentGoesHerelocView').html(locView.render());
});