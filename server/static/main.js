var uncrm = angular.module('uncrm', ['ngRoute', 'LocalStorageModule']);

uncrm.config(function(localStorageServiceProvider) {
  localStorageServiceProvider
    .setPrefix('uncrm');
});

uncrm.config(function($routeProvider) {

  $routeProvider.when('/incident/search?', {
    templateUrl: 'templates/search.html',
    controller: 'searchCtrl'
  });

  $routeProvider.when('/', {
    templateUrl: 'templates/index.html',
    controller: 'mainCtrl'
  });

  $routeProvider.when('/incident/:num', {
    templateUrl: 'templates/incident.html',
    controller: 'incidentCtrl'
  });

});

// Autosize: https://gist.github.com/jclement/076a95c94bb52e61c407

  $.fn.getHiddenOffsetWidth = function () {
    // save a reference to a cloned element that can be measured
    var $hiddenElement = $(this).clone().appendTo($(this).parents(':visible').first());

    // calculate the width of the clone
    var width = $hiddenElement.outerWidth();

    // remove the clone from the DOM
    $hiddenElement.remove();

    return width;
  };

uncrm.directive('autogrow', function() {
  return function(scope, element, attr){
    var minHeight = element[0].offsetHeight,
      paddingLeft = element.css('paddingLeft'),
      paddingRight = element.css('paddingRight');

    var $shadow = angular.element('<div></div>').css({
      position: 'absolute',
      top: -10000,
      left: -10000,
      width: element.getHiddenOffsetWidth(),
      fontSize: element.css('fontSize'),
      fontFamily: element.css('fontFamily'),
      lineHeight: element.css('lineHeight'),
      resize:     'none'
    });
    angular.element(document.body).append($shadow);

    var update = function() {
      var times = function(string, number) {
        for (var i = 0, r = ''; i < number; i++) {
          r += string;
        }
        return r;
      }

      var val = element.val().replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/&/g, '&amp;')
        .replace(/\n$/, '<br/>&nbsp;')
        .replace(/\n/g, '<br/>')
        .replace(/\s{2,}/g, function(space) { return times('&nbsp;', space.length - 1) + ' ' });
      $shadow.html(val);

      element.css('height', Math.max($shadow[0].offsetHeight + 10 /* the "threshold" */, minHeight) + 'px');
    }

    if (attr.ngModel) {
        // update when the model changes
        scope.$watch(attr.ngModel, update);
    }

    element.bind('keyup keydown keypress change', update);
    update();
  }
});

uncrm.factory('Auth', function($http, $rootScope, localStorageService) {

  var authBusy = false;

  return {
    isLoggedIn: function() {
      return !!localStorageService.get('authenticationToken');
    },

    getLoggedInName: function() {
      return localStorageService.get('authenticationName');
    },

    getToken: function() {
      return localStorageService.get('authenticationToken');
    },

    logout: function() {
      localStorageService.remove('authenticationToken');
      localStorageService.remove('authenticationName');
    },

    isBusy: function() {
      return authBusy;
    },

    login: function(username, password, success, failure) {
      authBusy = true;
      $http({
        url: '../login',
        dataType: 'json',
        method: 'POST',
        data: {
          Username: username,
          Password: password
        },
        headers: {
          'Content-Type': 'application/json; charset=utf-8'
        }
      }).success(function(response){
        authBusy = false;
        console.log('Logged in');
        localStorageService.set('authenticationName', username);
        localStorageService.set('authenticationToken', response);
        if (success) {
          success();
        }
      }).error(function(response) {
        authBusy = false;
        noty({
          text: response.data.ResponseStatus.Message,
          timeout: 1000,
          type: 'error'
        });
        if (failure) {
          failure();
        }
      });
    },

    search: function (query, success, failure) {
      $http({
        url: '../incident/search/' + encodeURIComponent(query.trim()),
        dataType: 'json',
        method: 'POST',
        headers: {
          'Content-Type': 'application/json; charset=utf-8'
        },
        data: {
          Query: query.trim()
        }
      }).then(
        //success handler
        function (response) {
          console.log('success response:', response);
        },
        //failure handler
        function (response) {
          console.log('failure response:', response);

          noty({
            text: (response.data.ResponseStatus && response.data.ResponseStatus.Message) ? response.data.ResponseStatus.Message : response.data.toString(),
            timeout: 5000,
            type: 'error'
          });

          if (typeof failure === 'function') {
            failure(response);
          }
        }
      );
    }

  };

});

marked.setOptions({
  renderer: new marked.Renderer(),
  gfm: true,
  tables: true,
  breaks: false,
  pedantic: false,
  sanitize: true,
  smartLists: true,
  smartypants: false,
  highlight: function (code) {
    return hljs.highlightAuto(code).value;
  }
});

uncrm.filter('comment', function ($sce) {
  return function (input) {
    return $sce.trustAsHtml(_.escape(input).replace(/\n/g, '<br/>'));
  };
});

uncrm.filter('crmify', function ($sce) {
  return function (input) {
    return (input || '').replace(/\b(CAS-[0-9]{5}-[A-Z][0-9][A-Z][0-9][A-Z][0-9])\b/g, function(x) {
      return '[' + x + '](' + x + ')';
    });
  };
});

uncrm.filter('marked', function ($sce) {
  return function (input) {
    return $sce.trustAsHtml(marked(input || ''));
  };
});

uncrm.controller('mainCtrl', function($scope, $location) {
  $scope.go = function() {
    // TODO: An actual search by string implementation?
    if ($scope.incidentNumber) {
      $location.url('/incident/' + $scope.incidentNumber);
    }
  };
});

uncrm.controller('searchCtrl', function($scope, $location, Auth) {
  $scope.isLoggedIn = Auth.isLoggedIn;
  $scope.go = function() {
    // TODO: An actual search by string implementation?
    if ($scope.incidentNumber) {
      // $location.url('/incident/search/' + $scope.incidentNumber);

      Auth.search($scope.incidentNumber, function () {}, function () {});

      $scope.incidentNumber = '';
    }
  };
});


uncrm.controller('authCtrl', function($scope, $location, Auth) {
  $scope.isLoggedIn = Auth.isLoggedIn;
  $scope.getLoggedInName = Auth.getLoggedInName;
  $scope.logout = Auth.logout;
  $scope.isBusy = Auth.isBusy;
  $scope.login = function() {
    Auth.login($scope.username, $scope.password, function() {
      $scope.username = '';
      $scope.password='';
    });
  };
});
