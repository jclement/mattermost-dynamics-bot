uncrm.controller('incidentCtrl', function($scope, $routeParams, $http, localStorageService, Auth, Upload, $timeout) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;

  $http({
    url: '../incident/' + $routeParams.num,
    dataType: 'json',
    method: 'GET',
    headers: {
      'Content-Type': 'application/json; charset=utf-8'
    }
  }).success(function(response){
      console.log('incident controller running');

      if (!response) {
        $scope.error = 'Incident Not Found';
        $scope.loaded = true;
        return;
      }

      var processNote = function(note) {
        note.Created = new Date(parseInt(note.Created.substr(6)));
        note.Modified = new Date(parseInt(note.Modified.substr(6)));
        note.isEditMode = false;
        return note;
      };
      var processAttachment = function(attachment) {
        attachment.Created = new Date(parseInt(attachment.Created.substr(6)));
        attachment.Modified = new Date(parseInt(attachment.Modified.substr(6)));
        return attachment;
      };
      _.each(response.Notes, processNote);
      _.each(response.NetworkAttachments, processAttachment);
      $scope.incident = response;
      $scope.incident.CreatedOn = new Date(parseInt($scope.incident.CreatedOn.substr(6)));
      $scope.loaded = true;
      $scope.commentPosting = false;
      $scope.isChangingOwner = false;
      $scope.hasUserList = false;
      $scope.savingNewOwner = false;
      $scope.userList = [];
      $scope.downloadNoteAttachment = function(attachmentId, filename) {
        document.getElementById('download_iframe').src = '../attachment/getfile/'+attachmentId+'/' + filename;
      };
      $scope.toggleEditMode = function(note) {
        note.isEditMode = !note.isEditMode;
      };
      $scope.deleteNote = function(note) {
        note.doomed = true;
        $http({
          url: '../notes/' + note.Id + '/delete',
          dataType: 'json',
          data: {
            'authenticationToken': Auth.getToken()
          },
          method: 'POST',
          headers: {
            'Content-Type': 'application/json; charset=utf-8'
          }
        }).success(function(response) {
          var i = $scope.incident.Notes.indexOf(note);
          if(i != -1) {
            $scope.incident.Notes.splice(i, 1);
          }
        }).error(function(response) {
          note.doomed = false;
          noty({
            text: response.ResponseStatus.Message,
            type: 'error'
          });
        });
      };
      $scope.addNote = function() {
        if (!$scope.newCommentBody && !$scope.newCommentTitle)
          return;

        $scope.commentPosting = true;
        $http({
          url: '../incident/' + $routeParams.num + '/notes/add',
          dataType: 'json',
          data: {
            'body': $scope.newCommentBody,
            'title': $scope.newCommentTitle,
            'authenticationToken': Auth.getToken()
          },
          method: 'POST',
          headers: {
            'Content-Type': 'application/json; charset=utf-8'
          }
        }).success(function(response) {
          $scope.incident.Notes.push(processNote(response));
          $scope.newCommentBody = '';
          $scope.newCommentTitle = '';
          $scope.commentPosting = false;
        }).error(function(response) {
          noty({
            text: response.ResponseStatus.Message,
            type: 'error'
          });
        });
      };
      $scope.updateNote = function(note) {
          note.isUpdating = true;
          $http({
              url: '../notes/' + note.Id + '/',
              dataType: "json",
              data: {
                  'body': note.Body,
                  'title': note.Title,
                  'authenticationToken': Auth.getToken()
              },
              method: "POST",
              headers: {
                  "Content-Type": "application/json; charset=utf-8"
              }
          }).success(function(response) {
              $scope.toggleEditMode(note);
              note.isUpdating = false;
          }).error(function(response) {
              note.isUpdating = false;
              noty({
                  text: response.ResponseStatus.Message,
                  type: "error"
              });
          });
      };
      $scope.startChangeOwner = function () {
        $scope.isChangingOwner = true;
        $scope.hasUserList = false;
        $scope.userList = [];
        // make http request to get user list
        $http({
          url: '../users/',
          dataType: 'json',
          method: 'GET'
        }).success(function(response) {
          $scope.userList = _.sortBy(response, 'Item1');
          $scope.hasUserList = true;
        }).error(function(response) {
          noty({
            text: response.ResponseStatus.Message,
            type: 'error'
          });
        });
      };
      $scope.saveNewOwner = function() {
        if (!_.isUndefined($scope.newOwner)) {
          $scope.isChangingOwner = true;
          $scope.savingNewOwner = true;
          $http({
            url: '../incident/' + $routeParams.num + '/changeOwner',
            dataType: 'json',
            method: 'POST',
            data: {
              'authenticationToken': Auth.getToken(),
              'OwnerId': $scope.newOwner.Item2
            }
          }).success(function(response) {
            _.extend($scope.incident, response);
            $scope.isChangingOwner = false;
            $scope.savingNewOwner = false;
          }).error(function(response) {
            noty({
              text: response.ResponseStatus.Message,
              type: 'error'
            });
          });
        }
      };
      $scope.cancelNewOwner = function() {
        $scope.isChangingOwner = false;
      };
      $scope.changeOwnerDisabled = function() {
        return _.isUndefined($scope.newOwner);
      };

    $scope.uploadFiles = function (files) {
        $scope.files = files;
        if (files && files.length) {
            $scope.uploadInProgress = true;
            $scope.uploadProgressPercent = 0.0;
            Upload.upload({
                url: "../incident/" + $routeParams.num + "/uploadFiles",
                data: {
                  authenticationToken: Auth.getToken(),
                  files: files
                }
            }).then(function (response) {
                $timeout(function () {
                    $scope.uploadInProgress = false;
                    _.each(response.data, processAttachment);
                    $scope.incident.NetworkAttachments = response.data;
                });
            }, function (response) {
                $scope.uploadInProgress = false;
                if (response.status > 0) {
                  noty({
                    text: response.data.ResponseStatus.Message,
                    type: 'error'
                  });
                }
            }, function (evt) {
                $scope.uploadProgressPercent = 
                    Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
            });
        }
    };
    
  }).error(function(error){
      $scope.error = error;
  });
  $scope.incidentNumber = $routeParams.num;
});
