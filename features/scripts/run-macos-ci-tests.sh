#!/bin/bash -e
pushd features/fixtures/mazerunner/
  unzip mazerunner_macos_$UNITY_PERFORMANCE_VERSION.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/mazerunner/mazerunner_macos_$UNITY_PERFORMANCE_VERSION.app --os=macos features 
