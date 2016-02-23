uncrm.controller('searchCtrl', function ($scope, $routeParams, $http, localStorageService, Auth, $location) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;

  $scope.hasUserList = false;
  $scope.userList = [];

  if (!$routeParams.OwnerId) {
    $scope.owner = null;
  }

  $scope.onlyInProgress = $routeParams.StateCode ? true : false;

  // loading
  document.getElementById('spinnerPlaceholder').innerHTML = spinner.innerHTML;

  var evaluateSearchUrl = function(routeParams) {
    var searchPath = '/search/incident';
    var searchParams = [];

    var safeEncodeParam = function(param, paramName) {
      if (typeof param === 'string') {
        searchParams.push(paramName + '=' + encodeURIComponent(param.trim()));
      }
    };

    _.each(routeParams, function (param, paramName) {
      if (paramName === 'Query' && param) {
        searchParams.unshift('Query=' + encodeURIComponent(param.trim()));
      }
      else {
        safeEncodeParam(param, paramName);
      }
    });

    if (searchParams.length) {
      searchPath += '?' + searchParams.join('&');
    }

    return searchPath;
  };


  var getUserList = function() {
    $http({
      url: '../users/',
      dataType: 'json',
      method: 'GET'
    }).then(
      function (response) {
        $scope.userList = _.sortBy(response.data, 'Item1');
        $scope.hasUserList = true;

        if ($routeParams.OwnerId) {
          $scope.owner = _.find($scope.userList, function (listedUser) {
            return listedUser.Item2 === $routeParams.OwnerId;
          });
        }
      },

      function (response) {
        noty({
          text: response.ResponseStatus.Message,
          type: 'error'
        });
      }
    );
  };

  var refilter = function(filterArg) {
    var newFilter = _.extend({}, $routeParams, filterArg || {});
    $location.url(evaluateSearchUrl(newFilter));
  };

  $scope.clearOwner = function() {
    refilter({OwnerId: null});
  };
  $scope.newOwnerFilter = function() {
    refilter({OwnerId: $scope.owner.Item2});
  };
  $scope.toggleInProgress = function () {
    refilter({StateCode: $scope.onlyInProgress ? '1' : null});
  };

  $http({
    url: '..' + evaluateSearchUrl($routeParams),
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

      getUserList();
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
