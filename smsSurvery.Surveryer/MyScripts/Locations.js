window.Location = window.Location || {};

var isBlank = function(str) {
   return (!str || /^\s*$/.test(str));
}
window.LocationErrors = {
   INVALID_NAME: "The Name cannot be empty or whitespace only",
   INVALID_DESCRIPTION: "The Description cannot be empty or whitespace only"
};

window.LocationModel = Backbone.Model.extend({
   events: {},
   defaults: function(){
      return {
         Name: "",
         Description: "",
         ActiveSurveyDescription: "",
         ActiveSurveyId: 0,
         ActiveSurveyPerLocationLink: ""
      }
   },   
   idAttribute: "Id",
   validate: function (attrs, options) {
      //DA will return an array of errors
      var errorsArray = [];
      //can't have no name && name must be unique
      if (isBlank(attrs.Name)) {
         errorsArray.push(window.LocationErrors.INVALID_NAME);
      }
      //TODO check for uniqueness
      //can't have no description
      if (isBlank(attrs.Description)) {         
         errorsArray.push(window.LocationErrors.INVALID_DESCRIPTION);
      }
      if (errorsArray.length != 0) {
         return errorsArray;
      }
   },
   backup: function () {
      this._backup = this.clone();      
   },
   restore: function () {
      var self = this;
      //DA for some reason the discard is not working correctly (maybe the Id? )
      this.set(
         {
            Name: self._backup.get("Name"),
            Description: self._backup.get("Description"),
            ActiveSurveyId: self._backup.get("ActiveSurveyId"),
            ActiveSurveyDescription: self._backup.get("ActiveSurveyDescription")
         }, { silent: true });
      //this.set(self._backup, { silent: true });
   }
});

//#region Detect if event is supported
var isEventSupported = (function () {
   var TAGNAMES = {
      'select': 'input', 'change': 'input',
      'submit': 'form', 'reset': 'form',
      'error': 'img', 'load': 'img', 'abort': 'img'
   };
   function isEventSupported(eventName) {
      var el = document.createElement(TAGNAMES[eventName] || 'div');
      eventName = 'on' + eventName;
      var isSupported = (eventName in el);
      if (!isSupported) {
         el.setAttribute(eventName, 'return;');
         isSupported = typeof el[eventName] == 'function';
      }
      el = null;
      return isSupported;
   }
   return isEventSupported;
})();
//#endregion
window.Location.activeSurveyOptions = {};

window.LocationView = Backbone.View.extend({
   tagName: "li",
   className: "location",
   events: {
      "click .save-location-btn": "saveLocation",
      "click .close-location-notifications": "closeAlertBox",
      "click .discard-location-btn": "discardChanges",
      "mousedown .location-select-active-survey": "loadSurveyTemplateOptions",
      "change .location-select-active-survey": "inputDataChanged"
   },
   initialize: function () {
      _.bindAll(this, "render", "saveLocation", "inputDataChanged",
         "closeAlertBox", "validationError", "clearErrorsFromFields",
         "discardChanges", "modelModified", "loadSurveyTemplateOptions",
         "renderActiveSurveyPerLocationInfo");
      this.template = _.template($("#location-template").html());
      var self = this;
      this.model.on("invalid", self.validationError);
      this.model.on("change:Name change:Description change:ActiveSurveyId", self.modelModified);
   },
   renderActiveSurveyPerLocationInfo: function () {
      //DA depending on whether we have an activeSurvey or not show the link or not
      if (this.model.get("ActiveSurveyId") != 0) {
         var activeSurveyLocation = this.model.get("ActiveSurveyPerLocationLink");
         this.$(".activeSurveyPerLocationLink a").attr("href", activeSurveyLocation);
         this.$(".activeSurveyPerLocationLink a").text(activeSurveyLocation);
         this.$(".activeSurveyPerLocationLink").show();
      } else {
         this.$(".activeSurveyPerLocationLink").hide();
      }
   },
   render: function() {
      this.$el.html(this.template(this.model.toJSON()));
      var self = this;
      if (isEventSupported('input')) {
         $('input[name="LocationName"]', this.$el).off('input');
         $('input[name="LocationName"]', this.$el).on('input', self.inputDataChanged);
         $('input[name="Description"]', this.$el).unbind('input');
         $('input[name="Description"]', this.$el).on('input', self.inputDataChanged);
      }
      else {
         $('input[name="LocationName"]', this.$el).off('change');
         $('input[name="LocationName"]', this.$el).on('change', self.inputDataChanged);
         $('input[name="Description"]', this.$el).unbind('input');
         $('input[name="Description"]', this.$el).on('input', self.inputDataChanged);
      }
      this.model.backup();
      this.renderActiveSurveyPerLocationInfo();
      return this;
   },
   saveLocation: function () {
      var self = this;
      this.closeAlertBox();
      this.clearErrorsFromFields();
      
      var name = $('input[name="LocationName"]', this.$el).val();
      this.model.set({ "Name": name }, { validate: false, silent:true });
      var description = $('input[name="Description"]', this.$el).val();
      this.model.set({ "Description": description }, { validate: false, silent: true });
      var activeSurveyElem = $("option:selected", this.$el);
      var activeSurveyId = activeSurveyElem.length != 0 ? activeSurveyElem.val() : 0;
      var activeSurveyDescription = activeSurveyElem.text();
      this.model.set(
         {
            "ActiveSurveyId": activeSurveyId,
            "ActiveSurveyDescription" : activeSurveyDescription
         }, { validate: false, silent: true });

      //hide to discard button, we will bring it up if validation fails
      $(".discard-location-btn", this.$el).hide();
      this.model.save(this.model.toJSON(),
            {
               success: function (model, response, options) {                 
                     $(".location-notifications", self.$el).text("Changes saved successfully.");
                     $(".alert", self.$el).removeClass("alert-error");
                     $(".alert", self.$el).addClass("alert-success");
                     $(".alert", self.$el).show();
                     self.renderActiveSurveyPerLocationInfo();
                     self.model.backup();
               }
            });
      $(".save-location-btn", this.$el).prop("disabled", true);
 },
   discardChanges: function() {
      this.model.restore();
      this.modelModified();
      this.closeAlertBox();
      this.clearErrorsFromFields();
      $(".save-location-btn", this.$el).prop("disabled", true);
   },
   modelModified: function() {
      $(".discard-location-btn", this.$el).hide();
      var nameField = $('input[name="LocationName"]', this.$el);
      nameField.val(this.model.get("Name"));
      var descriptionField = $('input[name="Description"]', this.$el);
      descriptionField.val(this.model.get("Description"));
      var activeSurveyField = $(".location-select-active-survey", this.$el);
      var selectedValue = this.model.get("ActiveSurveyId");
      $("option", this.$el).filter(function () {
         return $(this).val() == selectedValue;
      })[0].selected = true;
      this.model.backup();
   },
   inputDataChanged: function (model) {
      //now the only required update is enabling the Save button
      $(".save-location-btn", this.$el).prop("disabled", false);
      $(".discard-location-btn", this.$el).show();           
   },
   clearErrorsFromFields: function () {     
      var nameField = $('input[name="LocationName"]', this.$el);
      nameField.removeClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
      var descriptionField = $('input[name="Description"]', this.$el);
      descriptionField.removeClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
   },
   validationError: function (model, errorsArray) {
      var self = this;
   
      //reset all "invalid fields" qualifiers
      this.clearErrorsFromFields();
      var nameField = $('input[name="LocationName"]', this.$el);     
      var descriptionField = $('input[name="Description"]', this.$el);
      
      for (var i = 0; i < errorsArray.length; ++i) {
         if (errorsArray[i] === window.LocationErrors.INVALID_NAME) {
            nameField.addClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
         } else if (errorsArray[i] === window.LocationErrors.INVALID_DESCRIPTION) {
            descriptionField.addClass(SurveyUtilities.Utilities.CONSTANTS_CLASS.INVALID_FIELD);
         }
      }
      var errorMessage = errorsArray.join("\r\n");
      $(".location-notifications", self.$el).html(errorMessage);
      $(".alert", self.$el).removeClass("alert-success");
      $(".alert", self.$el).addClass("alert-error");
      $(".alert", self.$el).show();
      $(".discard-location-btn", this.$el).show();
   },
   closeAlertBox: function () {
      var self = this;
      $(".location-notifications", self.$el).html("");
      $(".alert", self.$el).hide();
   },
   loadSurveyTemplateOptions: function () {
      if (this._optionsLoaded) return;
      var self = this;
      if ($.isEmptyObject(window.Location.activeSurveyOptions)) {
         //we need to load the options and cache them
         $.ajax({
            url: "/Locations/SurveyTemplateList",
            type: 'get',
            cache: false,
            dataType: "json",
            async: false,
            contentType: 'application/json',
            success: function (data) {
               //bring the option list up to date and make sure that the default option is selected
               var activeSurveyId = $("option:selected", self.$el).val();
               var optElem = $(".location-select-active-survey", self.$el);
               optElem.html("");
               var noSelectionElem = '<option value="0"></option>';
               optElem.append(noSelectionElem);
               window.Location.activeSurveyOptions["0"] = "";
               _.each(data, function (item) {
                  var elem = '<option value="' + item.value + '">' + item.label + '</option>';
                  optElem.append(elem);
                  window.Location.activeSurveyOptions[item.value] = item.label;
               });
               var selectedCandidate = $("option", self.$el).filter(function () {
                  return $(this).val() == activeSurveyId;
               });
               if (selectedCandidate.length === 1) {
                  selectedCandidate[0].selected = true;
               }              
            }
         });
      }
      else {
         var optElem = $(".location-select-active-survey", this.$el);
         optElem.html("");
         for (var opt in window.Location.activeSurveyOptions) {
            var elem = '<option value="' + opt + '">' + window.Location.activeSurveyOptions[opt] + '</option>';
            optElem.append(elem);
         };
         var activeSurveyId = $("option:selected", self.$el).val();
         var selectedCandidate = $("option", self.$el).filter(function () {
            return $(this).val() == activeSurveyId;
         });
         if (selectedCandidate.length === 1) {
            selectedCandidate[0].selected = true;
         }
      }     
      this._optionsLoaded = true;
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
         $(".locations-set-content",self.el).append(locView.render().el);
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
         $(".locations-set-content",self.el).append(locView.render().el);
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