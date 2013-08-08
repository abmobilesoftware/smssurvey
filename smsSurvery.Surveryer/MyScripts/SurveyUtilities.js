var SurveyUtilities = SurveyUtilities || {};
SurveyUtilities.Utilities = (function () {
   var innerClass = {};
   innerClass.generateUUID = function () {
      var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
         var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
         return v.toString(16);
      });
      return uuid;
   }
   innerClass.CONSTANTS_SURVEY = {
      TYPE_SMS: "sms",
      TYPE_MOBILE_WEBSITE: "mobile website"
   },
   innerClass.CONSTANTS_QUESTION = {
      TYPE_RATING: "Rating",
      TYPE_FREE_TEXT: "FreeText",
      TYPE_SELECT_ONE_FROM_MANY: "SelectOneFromMany",
      TYPE_SELECT_MANY_FROM_MANY: "SelectManyFromMany",
      TYPE_YES_NO: "YesNo"
   }
   innerClass.CONSTANTS_MISC = {
      SEPARATOR_ANSWERS: ";",
      NEW_SURVEY: "new_survey"
   }
   return innerClass;
})();