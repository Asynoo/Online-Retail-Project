pipeline {
    agent any
    triggers {
        pollSCM("* * * * *")
    }
    environment {
        DEPLOY_NUMBER = "${BUILD_NUMBER}"
    }
    stages {
	  stage("Build") {
            steps {
                sh "docker compose build"
            }
        }
    }
  }
}