uncrm.controller('searchCtrl', function ($scope, $routeParams, $http, localStorageService, Auth) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;


  var searchPath = '../search/incident';
  var searchParams = [];

  _.each($routeParams, function (param, paramName) {
    if (paramName === 'Query') {
      searchParams.unshift('Query=' + encodeURIComponent(param.trim()));
    }
    else {
      searchParams.push(paramName + '=' + encodeURIComponent(param.trim()));
    }
  });

  if (searchParams.length) {
    searchPath += '?' + searchParams.join('&');
  }


  $http({
    url: searchPath,
    dataType: 'json',
    method: 'GET',
    headers: {
      'Content-Type': 'application/json; charset=utf-8'
    }
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
