uncrm.controller('searchCtrl', function ($scope, $routeParams, $http, localStorageService, Auth) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;

  var extract = function(aStr) {
    var str, queryData = {};
    for(var i = 0, argCount = aStr.length; i < argCount; i++) {
      str = aStr[i];
      if ($routeParams[str]) {
        queryData[str] = $routeParams[str];
      }
    }
    return queryData;
  };

  $http({
    url: '../search/incident',
    dataType: 'json',
    method: 'GET',
    headers: {
      'Content-Type': 'application/json; charset=utf-8'
    },
    data: extract(['Query', 'OwnerId', 'StateCode'])
  }).then(
    // success handler
    function (response) {
      $scope.searchResults = response.data;
      $scope.loaded = true;
      return;
    },

    //failure handler
    function (response) {
      noty({
        text: response.ResponseStatus.Message,
        type: 'error'
      });
      $scope.searchResults = [];
      $scope.loaded = true;
      return;
    }
  );

});
