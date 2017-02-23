(function () {
    "use strict";

    angular
        .module("common.services")
        .factory("monitorSvc",
                  ["$rootScope", monitorSvc])

    function monitorSvc($rootScope) {
        var initialize = function () {
            // fetch connection object and create proxy
            var connection = $.hubConnection();
            this.proxy = connection.createHubProxy('monitors');

            // start connection
            connection.start();

            this.proxy.on('LoadBalance', function (data) {
                $rootScope.$emit("LoadBalance", data);
            });

            this.proxy.on('TransformSalesOrderDetailEntity', function (data) {
                $rootScope.$emit("TransformSalesOrderDetailEntity", data);
            });
        };

        return {
            initialize: initialize,
        };
    }
})();