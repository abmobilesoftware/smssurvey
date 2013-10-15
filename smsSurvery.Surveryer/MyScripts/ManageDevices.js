(function () {
   $("link-modal").modal();
   $(".set-new-link-btn").click(function (event) {
      event.preventDefault();
      var deviceId = $(event.target).attr("deviceId");
      $(".link-modal").modal("show");
      $("#device-id-input").val(deviceId);
   });

   $("#update-link-btn").click(function () {
      var newLink = $("#link-input").val();
      var deviceId = $("#device-id-input").val();
      if (newLink != "") {
         $.ajax({
            url: "/Devices/SendLinkToDevice",
            type: "POST",
            data: {
               deviceId: deviceId,
               link: newLink
            },
            success: function (response) {
               if (response == "success") {
                  location.reload();
               } else {
                  alert(response);
               }
            }
         });
      } else {
         alert("The new link cannot be empty... I think");
      }
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
            alert(response);
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