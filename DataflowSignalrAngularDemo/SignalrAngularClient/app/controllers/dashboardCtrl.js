(function () {
    "use strict";

    angular
        .module("app")
        .controller("dashboardCtrl",
                    ["dashboardSvc", "$scope", "$rootScope", dashboardCtrl]);

    function dashboardCtrl(dashboardSvc, $scope, $rootScope) {
        var wdgt = this;
   
        var addProcessor = function (data) {
            $scope.Processor = new Array();
            $scope.Processor.push(data);
            wdgt.Processor = $scope.Processor;
        };

        var addSalesOrderDetail = function (data) {
            $scope.SalesOrderDetail = new Array();
            $scope.SalesOrderDetail.push(data);
            wdgt.SalesOrderDetail = $scope.SalesOrderDetail;
        };

        dashboardSvc.initialize();

        $scope.$parent.$on("LoadBalance", function (e, data) {
            $scope.$apply(function () {
                addProcessor(data);
            });
        });

        $scope.$parent.$on("TransformSalesOrderDetailEntity", function (e, data) {
            $scope.$apply(function () {
                addSalesOrderDetail(data);
            });
        });
    }
})();