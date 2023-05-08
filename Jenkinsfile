pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                // Checkout the source code from Git
                checkout scmGit(branches: [[name: '*/feature/review-API']], extensions: [], userRemoteConfigs: [[credentialsId: 'github', url: 'https://github.com/Asynoo/Online-Retail-Project']])
                
                // Build the Docker image
                sh 'docker compose build'
            }
        }
       stage("Deliver") {
            steps {
                withCredentials([usernamePassword(credentialsId: 'DockerHub', passwordVariable: 'DH_PASSWORD', usernameVariable: 'DH_USERNAME')]) {
                    sh 'docker login -u $DH_USERNAME -p $DH_PASSWORD'
                    sh "docker compose push alisdair01:test"
                }
            }
        }
        stage("Deploy") {
            steps {
                build job: 'RPS-Deploy', parameters: [[$class: 'StringParameterValue', name: 'DEPLOY_NUMBER', value: "${BUILD_NUMBER}"]]
            }
        }
    }
}
