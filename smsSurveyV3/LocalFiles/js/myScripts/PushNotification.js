var PushNotificationHandler = Backbone.Model.extend({
	events: {
		REFRESH: "refreshEvent",
		RELEASE: "releaseEvent",
		UPDATE_LINK: "updateLinkEvent"
	},
	initialize: function() {
		_.bindAll(this, 
				"pushNotificationListener",
				"pushNotificationRegisterSuccess",
				"pushNotificationRegisterError",
				"getGCMKey");
		this.storage = window.localStorage;
		this.pushNotificationManager = new PushNotificationManager();
		// GCM project id
		this.pushNotificationManager.accountID("92320531700");
		if (this.storage.getItem("GCMKey") == null || this.storage.getItem("GCMKey") == "null") {
			this.pushNotificationManager.register(
					this.pushNotificationRegisterSuccess,
					this.pushNotificationRegisterError);
		} else {
			this.pushNotificationManager.listener(
					this.pushNotificationListener);
		}
	},
	pushNotificationListener: function(notification) {
		alert("Notification received" + notification.message);
		if (notification.message == "refresh") {
			this.trigger(this.events.REFRESH);
		} else if (notification.message == "release") {
			this.trigger(this.events.RELEASE);
		} else {
			this.trigger(this.events.UPDATE_LINK,
					notification.message);
		}
	},
	pushNotificationRegisterSuccess: function(token) {
		this.storage.setItem("GCMKey", token);
	},
	pushNotificationRegisterError: function() {
		alert("PushNotification register error");
	},
	getGCMKey: function() {
		//return this.storage.getItem("GCMKey");
		return "test pe chrome";
	}
});