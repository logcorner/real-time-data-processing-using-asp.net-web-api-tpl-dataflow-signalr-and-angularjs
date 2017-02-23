(function () {
    "use strict";

    angular
        .module("common.services")
        .factory("dashboardSvc",
                  ["$rootScope","appSettings", dashboardSvc])

    function dashboardSvc($rootScope, appSettings) {
        var initialize = function () {
           
            var connection = $.hubConnection(appSettings.serverPath, { useDefaultPath: false });
            this.proxy = connection.createHubProxy('dashboards');

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