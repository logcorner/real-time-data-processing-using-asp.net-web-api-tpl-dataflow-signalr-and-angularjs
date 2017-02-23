(function () {
    "use strict";

    angular
        .module("common.services",
                    [])
    	.constant("appSettings",
        {
            serverPath: "http://localhost:49918"
        });
}());