var uncrm = angular.module('uncrm', ['ngRoute', 'LocalStorageModule', 'ngFileUpload', 'ngclipboard']);

uncrm.factory('Auth', function($http, $rootScope, localStorageService, $window) {

  var authBusy = false;
  var isAuthenticated = true;

  return {
    isLoggedIn: function() {
      return !!localStorageService.get('authenticationToken') && isAuthenticated;
    },

    getToken: function() {
      return localStorageService.get('authenticationToken');
    },

    logout: function() {
      localStorageService.remove('authenticationToken');
      isAuthenticated = false;
      $window.location.reload();
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
        localStorageService.set('authenticationToken', response.AuthenticationToken);
        if (success) {
          success();
          isAuthenticated = true;
          $window.location.reload();
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
    }

  };

});

uncrm.config(function(localStorageServiceProvider) {
  localStorageServiceProvider
    .setPrefix('uncrm');
});

uncrm.factory('httpRequestInterceptor', function (localStorageService) {
  return {
    request: function (config) {
      config.headers['X-AUTH-TOKEN'] = localStorageService.get('authenticationToken');
      return config;
    }
  };
});

/*
uncrm.run(function($rootScope, $location, Auth) {
  $rootScope.$on('$routeChangeSuccess', function() {
    if (!Auth.isLoggedIn()) {
      $location.url("/");
    }
  });
});
*/

uncrm.config(function($routeProvider, $httpProvider) {

  $httpProvider.interceptors.push('httpRequestInterceptor');

  $routeProvider.when('/', {
    templateUrl: 'templates/index.html',
    controller: 'mainCtrl'
  });

  $routeProvider.when('/incident/:num', {
    templateUrl: 'templates/incident.html',
    controller: 'incidentCtrl'
  });

  $routeProvider.when('/search/incident', {
    templateUrl: 'templates/search.html',
    controller: 'searchCtrl'
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
      return '[' + x + '](#/incident/' + x + ')';
    });
  };
});

uncrm.filter('tfsify', function ($sce) {
  return function (input) {
    return (input || '').replace(/\bT([0-9]{4,6})\b/g, function(x) {
      return '[' + x + '](http://tfs.eni.local:8080/tfs/EnergyNavigator/AFENavigator/_workitems#id=' + x.substr(1) + '&triage=true&_a=edit)';
    });
  };
});

uncrm.filter('escape', function() {
  return window.encodeURIComponent;
});

uncrm.filter('marked', function ($sce) {
  return function (input) {
    try {
      return $sce.trustAsHtml(marked(input || ''));
    } catch (err) {
      return $sce.trustAsHtml(_.escape(input || ''));
    }
  };
});

uncrm.controller('mainCtrl', function($scope, $location) {
});

uncrm.controller('quickSearchCtrl', function($scope, $location, Auth) {
  $scope.isLoggedIn = Auth.isLoggedIn;
  $scope.searchQuery = $location.search().query;
  $scope.go = function() {
    var queryInvalid = typeof $scope.searchQuery !== 'string' || !$scope.searchQuery;
    $location.url( queryInvalid ? '/search/incident' : '/search/incident?query=' + $scope.searchQuery);
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
      $scope.password = '';
    });
  };
});
