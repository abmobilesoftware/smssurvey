(function () {
   /* Translation */
   var cAlertCommandFailed;
   var cAlertSetSurveySuccess;
   var cAlertRefreshSurveySuccess;
   $(document).ready(function () {
      cAlertCommandFailed = $("#alert-command-failed").val();
      cAlertSetSurveySuccess = $("#alert-set-survey-success").val();
      cAlertRefreshSurveySuccess = $("#alert-refresh-survey-success").val();
   });

   /* Alert blinking */
   var noOfBlinks = 3;
   var blinkTimer;
   var cBlinkInterval = 500;

   function startBlinking() {
      noOfBlinks = 3;
      blinkTimer = setInterval(blinking, cBlinkInterval);
   }
   function blinking() {
      if (noOfBlinks > 0) {
         if ($(".alert").hasClass("alert-visible")) {
            $(".alert").removeClass("alert-visible");
            $(".alert").addClass("alert-invisible");
         } else {
            $(".alert").addClass("alert-visible");
            $(".alert").removeClass("alert-invisible");
            noOfBlinks = noOfBlinks - 1;
         }
      } else {
         clearInterval(blinkTimer);
      }
   }

   $(".survey-link-select").on("change", function () {
      var newLink = $(this).val();
      $(this).parents(".device-row").attr("newSurvey", newLink);
      var $btnsGroup = $(this).parents(".device-row").children(".save-discard-btns");
      var $saveBtn = $(".save-device-btn", $btnsGroup);
      var $discardBtn = $(".discard-device-btn", $btnsGroup);
      $discardBtn.show();
      $saveBtn.removeAttr("disabled");
      /*var deviceId = $(this).attr("deviceId");
      if (newLink != "") {
         $.ajax({
            url: "/Devices/SendLinkToDevice",
            type: "POST",
            data: {
               deviceId: deviceId,
               link: newLink
            },
            success: function (response) {
               $(".alert").hide();
               if (response == "success") {
                  $(".alert-success").html(cAlertSetSurveySuccess);
                  $(".alert-success").show();
                  startBlinking();
               } else {
                  $(".alert-danger").html(cAlertCommandFailed);
                  $(".alert-danger").show();
                  startBlinking();
               }
            }
         });
      } else {
         alert("The new link cannot be empty... I think");
      }*/
   });

   $(".save-device-btn").click(function() {
      var $device = $(this).parents(".device-row");
      var newSurvey = $device.attr("newSurvey");
      var deviceId = $device.attr("deviceId");
      var $discardBtn = $(this).parent().children(".discard-device-btn");
      var $saveBtn = $(this);
         $.ajax({
            url: "/Devices/SendLinkToDevice",
            type: "POST",
            data: {
               deviceId: deviceId,
               link: newSurvey
            },
            success: function (response) {
               $(".alert").hide();
               if (response == "success") {
                  $(".alert-success").html(cAlertSetSurveySuccess);
                  $(".alert-success").show();
                  $saveBtn.attr("disabled", "disabled");
                  $discardBtn.hide();
                  startBlinking();
               } else {
                  $(".alert-danger").html(cAlertCommandFailed);
                  $(".alert-danger").show();
                  startBlinking();
               }
            }
         });      
   });

   $(".discard-device-btn").click(function () {
      var $device = $(this).parents(".device-row");
      var newSurvey = $device.attr("newSurvey");
      var currentSurvey = $device.attr("currentSurvey");
      var $surveyList = $device.children(".surveys-list-cell").children(".survey-link-select");
      $surveyList.val(currentSurvey);
      var $saveBtn = $(this).parent().children(".save-device-btn");
      $saveBtn.attr("disabled", "disabled");
      var $discardBtn = $(this);
      $discardBtn.hide();
   });

   $(".refresh-survey").click(function (event) {
      event.preventDefault();
      var deviceId = $(event.target).attr("deviceId");
      $.ajax({
         url: "/Devices/SendRefreshCommandToDevice",
         type: "POST",
         data: {
            deviceId: deviceId           
         },
         success: function (response) {
            var responseObject = JSON.parse(response);
            $(".alert").hide();
            if (responseObject.success == 1) {
               $(".alert-success").html(cAlertRefreshSurveySuccess);
               $(".alert-success").show();
               startBlinking();
            } else if (responseObject.failure == 1) {
               $(".alert-danger").html(cAlertCommandFailed);
               $(".alert-danger").show();
               startBlinking();
            }
         }
      });
   });

   $(".release-device").click(function (event) {
      event.preventDefault();
      var deviceId = $(event.target).attr("deviceId");
      $.ajax({
         url: "/Devices/ReleaseDevice",
         type: "POST",
         data: {
            deviceId: deviceId
         },
         success: function (response) {
            if (response == "success") {
               location.reload();
            } else {
               alert(response);
            }
         }
      });
   });
})();