(function () {
    "use strict";

    angular
        .module("app")
        .controller("monitorCtrl",
                    ["monitorSvc", "$scope", "$rootScope", monitorCtrl]);

    function monitorCtrl(monitorSvc, $scope, $rootScope) {
        var wdgt = this;

        $scope.monitors = new Array();
        $scope.Processor = new Array();
        $scope.FileOrderEntities = new Array();

        $scope.sendmonitor = function () {
            monitorSvc.sendRequest();
        };

        var addProcessor = function (data) {
            $scope.Processor = new Array();
            $scope.Processor.push(data);
            wdgt.Processor = $scope.Processor;
        };

        var addFileOrderEntity = function (data) {
            $scope.FileOrderEntities = new Array();
            $scope.FileOrderEntities.push(data);

            wdgt.FileOrderEntities = $scope.FileOrderEntities;
        };

        monitorSvc.initialize();

        $scope.$parent.$on("LoadBalance", function (e, data) {
            $scope.$apply(function () {
                addProcessor(data);
            });
        });

        $scope.$parent.$on("TransformFileToFileOrderEntity", function (e, data) {
            $scope.$apply(function () {
                addFileOrderEntity(data);
            });
        });
    }
})();